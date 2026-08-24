using System.ComponentModel.DataAnnotations.Schema;

namespace VectisBackend.Entities.Fk;

[Table("BDG_STRUCTUREBUDGETAIRE_GEN_H")]
public class BdgStructureBudgetaireGenH
{
    [Column("REF_UTIL")] public string? RefUtil { get; set; }
    [Column("CODE_FILS")] public string? CodeFils { get; set; }
    [Column("LIBELLE")] public string? Libelle { get; set; }
    [Column("CODE_PERE")] public string? CodePere { get; set; }
    [Column("CODE_H")] public string? CodeH { get; set; }
    [Column("CODIFICATION")] public string? Codification { get; set; }
    [Column("ANNEE")] public string? Annee { get; set; }
    [Column("CATEGORIE_BUDGET")] public decimal? CategorieBudget { get; set; }
    [Column("ACTIVE_LIGNE")] public decimal? ActiveLigne { get; set; }
    [Column("CODE_LIGNE_BDG")] public decimal? CodeLigneBdg { get; set; }
    [Column("BOP")] public decimal? Bop { get; set; }
    [Column("Type_Categorie_budget")] public decimal? TypeCategorieBudget { get; set; }
    [Column("typebudget")] public int? TypeBudget { get; set; }

}

