export interface ImportDepenseRequestDto {
  includeEngagement: boolean;
  engagementAvecValidation: boolean;
  includeLivraison: boolean;
  livraisonAvecValidation: boolean;
  includeReception: boolean;
  receptionAvecValidation: boolean;
  includeFacture: boolean;
  factureAvecValidation: boolean;
}

export interface ExportDepenseRequestDto extends ImportDepenseRequestDto {
  typeDepense: string;
}

export interface ImportErrorDto {
  sheet: string;
  rowNumber: number;
  column: string;
  message: string;
}

export interface DatabaseErrorDto extends ImportErrorDto {
  errorType: string;
  sqlErrorCode: string;
  technicalMessage: string;
  entityName: string;
  timestamp: string;
}

export interface ImportResultDto {
  success: boolean;
  totalRows: number;
  insertedRows: number;
  errors: ImportErrorDto[];
}