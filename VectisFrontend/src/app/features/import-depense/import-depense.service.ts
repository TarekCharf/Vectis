import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import {
  ImportDepenseRequestDto,
  ImportResultDto,
  ImportErrorDto,
  DatabaseErrorDto
} from '../models/import-depense.models';

@Injectable({
  providedIn: 'root'
})
export class ImportDepenseService {
  private readonly baseUrl = 'api';

  constructor(private http: HttpClient) {}

  /** Télécharge le modèle d'export Excel */
  exportExcel(options: ImportDepenseRequestDto): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/export-depense/excel`, options, {
      responseType: 'blob'
    });
  }

  /** Importe le fichier Excel */
  importExcel(file: File, options: ImportDepenseRequestDto): Observable<ImportResultDto> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    formData.append('includeEngagement', String(options.includeEngagement));
    formData.append('engagementAvecValidation', String(options.engagementAvecValidation));
    formData.append('includeLivraison', String(options.includeLivraison));
    formData.append('livraisonAvecValidation', String(options.livraisonAvecValidation));
    formData.append('includeReception', String(options.includeReception));
    formData.append('receptionAvecValidation', String(options.receptionAvecValidation));
    formData.append('includeFacture', String(options.includeFacture));
    formData.append('factureAvecValidation', String(options.factureAvecValidation));

    return this.http.post<ImportResultDto>(
      `${this.baseUrl}/import-depense/excel`,
      formData
    ).pipe(
      catchError((error: HttpErrorResponse) => throwError(this.normalizeError(error)))
    );
  }

  /**
   * Reconnaît TOUTES les formes de réponse d'erreur possibles côté backend et les ramène à
   * un format unique { isImportError | isDatabaseError | isSystemError, error }. Testé dans
   * cet ordre du plus spécifique au plus générique — le générique (isSystemError) ne doit
   * plus jamais être le seul cas atteint en pratique.
   */
  private normalizeError(error: HttpErrorResponse): {
    status: number;
    error: ImportResultDto | DatabaseErrorDto;
    isImportError?: boolean;
    isDatabaseError?: boolean;
    isSystemError?: boolean;
  } {
    const body = error.error;

    // Cas 0 : le corps n'a pas pu être parsé en JSON (page HTML de la Developer Exception
    // Page, texte brut, etc.) — `body` est alors une string, pas un objet.
    if (typeof body === 'string' || body == null) {
      return {
        status: error.status,
        error: this.buildDatabaseError(
          'Erreur serveur',
          error.status === 0
            ? 'Impossible de contacter le serveur (réseau, CORS, ou serveur arrêté).'
            : `Le serveur a renvoyé une erreur non structurée (HTTP ${error.status}).`,
          typeof body === 'string' ? body.slice(0, 2000) : error.message
        ),
        isDatabaseError: true
      };
    }

    // Cas 1 : notre forme "combinaison invalide" -> { message, details }
    if (typeof body.message === 'string' && Array.isArray(body.details)) {
      return {
        status: error.status,
        error: this.buildDatabaseError(
          'Erreur de configuration',
          body.message,
          body.details.join('\n')
        ),
        isDatabaseError: true
      };
    }

    // Cas 2 : ImportResultDto -> { success, totalRows, insertedRows, errors: ImportErrorDto[] }
    if (Array.isArray(body.errors)) {
      const result: ImportResultDto = {
        success: false,
        totalRows: body.totalRows ?? 0,
        insertedRows: body.insertedRows ?? 0,
        errors: body.errors
      };
      return { status: error.status, error: result, isImportError: true };
    }

    // Cas 3 : ValidationProblemDetails standard d'ASP.NET Core (échec de binding/validation
    // automatique du [ApiController], déclenché AVANT d'atteindre le corps de l'action) ->
    // { title, status, errors: { "NomChamp": ["message1", "message2"] } }
    if (body.errors && typeof body.errors === 'object' && !Array.isArray(body.errors)) {
      const fieldErrors: ImportErrorDto[] = [];
      Object.keys(body.errors as Record<string, string[]>).forEach(field => {
        const messages: string[] = (body.errors as Record<string, string[]>)[field] || [];
        messages.forEach(msg => {
          fieldErrors.push({
            sheet: 'Requête',
            rowNumber: 0,
            column: field,
            message: msg
          });
        });
      });
      const result: ImportResultDto = {
        success: false,
        totalRows: 0,
        insertedRows: 0,
        errors: fieldErrors.length > 0
          ? fieldErrors
          : [{ sheet: 'Requête', rowNumber: 0, column: '-', message: body.title || 'Requête invalide.' }]
      };
      return { status: error.status, error: result, isImportError: true };
    }

    // Cas 4 : ProblemDetails générique renvoyé par le handler d'exceptions global
    // (voir Program.cs) -> { title, detail, status }
    if (typeof body.title === 'string' || typeof body.detail === 'string') {
      return {
        status: error.status,
        error: this.buildDatabaseError(
          'Erreur serveur',
          body.title || 'Une erreur est survenue.',
          body.detail || ''
        ),
        isDatabaseError: true
      };
    }

    // Cas 5 : vraiment rien de reconnaissable — dernier recours
    return {
      status: error.status,
      error: this.buildDatabaseError(
        'Erreur inattendue',
        'Une erreur inattendue est survenue.',
        error.message || JSON.stringify(body).slice(0, 500)
      ),
      isSystemError: true
    };
  }

  private buildDatabaseError(errorType: string, message: string, technicalMessage: string): DatabaseErrorDto {
    return {
      sheet: 'Système',
      rowNumber: 0,
      column: '-',
      message,
      errorType,
      sqlErrorCode: '',
      technicalMessage,
      entityName: '',
      timestamp: new Date().toISOString()
    };
  }
}