import { Component } from '@angular/core';
import { ImportDepenseService } from './import-depense.service';
import { 
  ExportDepenseRequestDto,
  ImportResultDto,
  ImportErrorDto,
  DatabaseErrorDto
} from '../models/import-depense.models';

@Component({
  selector: 'app-import-depense',
  templateUrl: './import-depense.component.html',
})
export class ImportDepenseComponent {
  selectedType: string | null = null;
  statusMessage = 'Sélectionnez un type de dépense pour commencer.';
  isExporting = false;
  isImporting = false;

  // État de la popup / modale
  showModal = false;
  modalTitle = '';
  modalType: 'success' | 'warning' | 'error' = 'success';
  
  // Résultats de l'import
  importSummary: { total: number; inserted: number } | null = null;
  importErrors: ImportErrorDto[] = [];
  databaseError: DatabaseErrorDto | null = null;
  systemError: string | null = null;

  // Configuration du formulaire
  config: ExportDepenseRequestDto = {
    typeDepense: "Marché/BC/CDC",
    includeEngagement: false,
    engagementAvecValidation: true,
    includeLivraison: false,
    livraisonAvecValidation: true,
    includeReception: false,
    receptionAvecValidation: true,
    includeFacture: false,
    factureAvecValidation: true
  };

  // Variables de gestion de l'état d'activation des contrôles UI
  livraisonOuiDisabled = false;
  receptionOuiDisabled = false;
  factureDisabled = false;
  receptionDisabled = false;

  constructor(private importService: ImportDepenseService) {}

  selectType(type: string): void {
    this.selectedType = type;
    this.applyConstraints();
    this.updateStatus();
  }

  onToggleChange(): void {
    this.applyConstraints();
    this.updateStatus();
  }

  onValidationChange(): void {
    this.applyConstraints();
    this.updateStatus();
  }

  applyConstraints(): void {
    const engagementValide = this.config.includeEngagement && this.config.engagementAvecValidation;
    if (!engagementValide) {
      this.config.livraisonAvecValidation = false;
      this.livraisonOuiDisabled = true;
    } else {
      this.livraisonOuiDisabled = false;
    }

    const livraisonValide = this.config.includeLivraison && this.config.livraisonAvecValidation;
    if (!this.config.includeLivraison || !this.config.livraisonAvecValidation) {
      this.config.includeFacture = false;
      this.factureDisabled = true;
    } else {
      this.factureDisabled = false;
    }

    if (!livraisonValide) {
      this.config.receptionAvecValidation = false;
      this.receptionOuiDisabled = true;
    } else {
      this.receptionOuiDisabled = false;
    }

    if (!this.config.includeLivraison) {
      this.config.includeReception = false;
      this.receptionDisabled = true;
    } else {
      this.receptionDisabled = false;
    }
  }

  get activeOptionsCount(): number {
    let count = 0;
    if (this.config.includeEngagement) count++;
    if (this.config.includeLivraison) count++;
    if (this.config.includeReception) count++;
    if (this.config.includeFacture) count++;
    return count;
  }

  updateStatus(): void {
    if (!this.selectedType) {
      this.statusMessage = 'Sélectionnez un type de dépense pour commencer.';
      return;
    }
    const label = this.selectedType === 'marche' ? 'Marché/BC/CDC' : 'Dépense diverse';
    const count = this.activeOptionsCount;
    this.statusMessage = `Type : <strong>${label}</strong> — ${count} étape${count > 1 ? 's' : ''} activée${count > 1 ? 's' : ''}.`;
  }

  exportExcel(): void {
    if (!this.selectedType || this.isExporting) return;
    this.isExporting = true;
    this.importService.exportExcel(this.config).subscribe(
      (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        const dateStr = new Date().toLocaleDateString('fr-FR').replace(/\//g, '-');
        a.download = `import-depense_${this.selectedType}_${dateStr}.xlsx`;
        a.click();
        window.URL.revokeObjectURL(url);
        this.isExporting = false;
      },
      (err: any) => {
        console.error(err);
        this.statusMessage = `<span style="color:red;">Échec de l'export Excel.</span>`;
        this.isExporting = false;
      }
    );
  }

  // Import Excel & ouverture de la modale
// Import Excel & ouverture de la modale
onFileSelected(event: Event): void {
  const input = event.target as HTMLInputElement;
  if (!input.files || input.files.length === 0) return;
  const file = input.files[0];

  this.isImporting = true;
  this.importService.importExcel(file, this.config).subscribe(
    (res: ImportResultDto) => {
      // HTTP 200 — import réussi
      this.isImporting = false;
      this.databaseError = null;
      this.importErrors = res.errors || [];
      this.importSummary = {
        total: res.totalRows,
        inserted: res.insertedRows,
      };
      if (res.success && this.importErrors.length === 0) {
        this.modalTitle = 'Importation réussie';
        this.modalType = 'success';
      } else if (res.insertedRows > 0) {
        this.modalTitle = 'Importation partielle';
        this.modalType = 'warning';
      } else {
        this.modalTitle = 'Échec de l\'importation';
        this.modalType = 'error';
      }
      this.showModal = true;
    },
    (err: any) => {
      console.log('[DEBUG] err reçu:', err);   // ← ajoute ça
      this.isImporting = false;
      this.importErrors = [];
      this.importSummary = null;
      this.databaseError = null;

      // ✅ TEST 1 : Erreurs de validation du backend (ImportResultDto avec tableau errors)
      if (err.isImportError) {
        const result = err.error as ImportResultDto;
        this.importErrors = result.errors || [];
        this.importSummary = {
          total: result.totalRows,
          inserted: result.insertedRows,
        };
        this.modalTitle = 'Erreurs de validation';
        this.modalType = 'error';
      }
      // ✅ TEST 2 : Contraintes invalides / erreur DB / erreur serveur non structurée —
      // normalizeError() du service renvoie TOUJOURS un DatabaseErrorDto correctement rempli
      // ici (message + technicalMessage), qu'elle vienne d'un vrai souci DB, d'une page HTML
      // de la Developer Exception Page, ou d'un ProblemDetails générique.
      else if (err.isDatabaseError || err.isSystemError) {
        this.databaseError = err.error as DatabaseErrorDto;
        this.modalTitle = err.isSystemError ? 'Erreur inattendue' : 'Erreur de configuration';
        this.modalType = 'error';
      }
      // Dernier recours théorique (ne devrait plus être atteint avec normalizeError())
      else {
        this.modalType = 'error';
        this.modalTitle = 'Erreur système';
        this.databaseError = {
          sheet: 'Système',
          rowNumber: 0,
          column: '-',
          entityName: '',
          sqlErrorCode: '',
          timestamp: '',
          message: err.error?.message || 'Une erreur inattendue est survenue.',
          errorType: 'ServerException',
          technicalMessage: err.message || ''
        };
      }
      this.showModal = true;
    }
  );
}

  closeModal(): void {
    this.showModal = false;
    this.databaseError = null;
    this.importErrors = [];
    this.importSummary = null;
    this.systemError = null;
  }
}