using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.EntityFrameworkCore;
using VectisBackend.Entities;
using VectisBackend.Entities.Fk;
using VectisBackend.Models;

namespace VectisBackend.Data;

public class VectisDbContext : DbContext
{
    public VectisDbContext(DbContextOptions<VectisDbContext> options) : base(options) { }

    public DbSet<DepMarche> DepMarches => Set<DepMarche>();
    public DbSet<DepMarchesH> DepMarchesH => Set<DepMarchesH>();
    public DbSet<DepOrdreService> DepOrdreServices => Set<DepOrdreService>();
    public DbSet<DepMarcheCentre> DepMarcheCentres => Set<DepMarcheCentre>();
    public DbSet<DepChargeSuivi> DepChargeSuivis => Set<DepChargeSuivi>();
    public DbSet<DepEtapeMarche> DepEtapeMarches => Set<DepEtapeMarche>();
    public DbSet<GenTarifFournisseur> GenTarifFournisseurs => Set<GenTarifFournisseur>();
    public DbSet<CptEngagement> CptEngagements => Set<CptEngagement>();
    public DbSet<CptEngagementLigne> CptEngagementLignes => Set<CptEngagementLigne>();
    public DbSet<DepReleveMarche> DepReleveMarches => Set<DepReleveMarche>();
    public DbSet<DepLigneReleveMarche> DepLigneReleveMarches => Set<DepLigneReleveMarche>();
    public DbSet<DepVentilQuantiteReleve> DepVentilQuantiteReleves => Set<DepVentilQuantiteReleve>();
    public DbSet<DepReception> DepReceptions => Set<DepReception>();
    public DbSet<DepReceptionReleve> DepReceptionReleves => Set<DepReceptionReleve>();
    public DbSet<DepFacture> DepFactures => Set<DepFacture>();
    public DbSet<DepFactureReleve> DepFactureReleves => Set<DepFactureReleve>();
    public DbSet<DepAvancementFacture> DepAvancementFactures => Set<DepAvancementFacture>();
    public DbSet<DepVentilMntsFacture> DepVentilMntsFactures => Set<DepVentilMntsFacture>();
    public DbSet<DepMontantsFacture> DepMontantsFactures => Set<DepMontantsFacture>();
    public DbSet<DepAllMontantsFacture> DepAllMontantsFactures => Set<DepAllMontantsFacture>();
    public DbSet<InstanceProcessus> InstanceProcessuses => Set<InstanceProcessus>();
    public DbSet<InstanceEtape> InstanceEtapes => Set<InstanceEtape>();
    public DbSet<Journal> Journals => Set<Journal>();

    //Fks

    public DbSet<BdgMaitreOuvrage> BdgMaitreOuvrages => Set<BdgMaitreOuvrage>();
    public DbSet<DepTypeContrat> DepTypeContrats => Set<DepTypeContrat>();
    public DbSet<GenProduit> GenProduits => Set<GenProduit>();
    public DbSet<GenProduitBrd> GenProduitBrds => Set<GenProduitBrd>();
    public DbSet<BdgExercice> BdgExercices => Set<BdgExercice>();
    public DbSet<BdgTypeBudget> BdgTypeBudgets => Set<BdgTypeBudget>();
    public DbSet<DepNaturePrestation> DepNaturePrestations => Set<DepNaturePrestation>();
    public DbSet<DepTypePrestation> DepTypePrestations => Set<DepTypePrestation>();
    public DbSet<GenBanque> GenBanques => Set<GenBanque>();
    public DbSet<GenCompteBancaire> GenCompteBancaires => Set<GenCompteBancaire>();
    public DbSet<GenCentreResponsabilite> GenCentreResponsabilites => Set<GenCentreResponsabilite>();
    public DbSet<GenCentreResponsabiliteH> GenCentreResponsabiliteHs => Set<GenCentreResponsabiliteH>();
    public DbSet<GenDevise> GenDevises => Set<GenDevise>();
    public DbSet<GenUnite> GenUnites => Set<GenUnite>();
    public DbSet<GenFournisseur> GenFournisseurs => Set<GenFournisseur>();
    public DbSet<GenTypeReglement> GenTypeReglements => Set<GenTypeReglement>();
    public DbSet<RefAgent> RefAgents => Set<RefAgent>();
    public DbSet<CptTypeCreditSupp> CptTypeCreditSupps => Set<CptTypeCreditSupp>();
    public DbSet<BdgStructureBudgetaireGenH> BdgStructureBudgetaireGenHs => Set<BdgStructureBudgetaireGenH>();
    public DbSet<BdgDotationCreditSupp> BdgDotationCreditSupps => Set<BdgDotationCreditSupp>();
    public DbSet<BdgDotationLigneBudgetaire> BdgDotationLigneBudgetaires => Set<BdgDotationLigneBudgetaire>();
    public DbSet<GenCentreRespContact> GenCentreRespContacts => Set<GenCentreRespContact>();
    public DbSet<GenMagasin> GenMagasins => Set<GenMagasin>();
    public DbSet<StkMagasinier> StkMagasiniers => Set<StkMagasinier>();
    public DbSet<StkMagasinMagasinier> StkMagasinMagasiniers => Set<StkMagasinMagasinier>();
    public DbSet<GenVille> GenVilles => Set<GenVille>();
    public DbSet<DepNatureReception> DepNatureReceptions => Set<DepNatureReception>();
    public DbSet<DepTypeReception> DepTypeReceptions => Set<DepTypeReception>();
    public DbSet<DepPeriode> DepPeriodes => Set<DepPeriode>();
    public DbSet<EtapeValidation> EtapeValidations => Set<EtapeValidation>();
    public DbSet<GenEtape> GenEtapes => Set<GenEtape>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DepMarchesH>().HasKey(x => x.CodeFils);
        modelBuilder.Entity<GenCentreResponsabiliteH>().HasNoKey();
        modelBuilder.Entity<GenProduit>(entity =>
        {
            entity.Property(e => e.DateFermeture)
                .HasColumnName("DATE_FERMETURE")
                .HasColumnType("datetime2");  // ou "date" selon votre base
                                              // .HasConversion(v => v, v => v); // Si nécessaire
        });
        modelBuilder.Entity<BdgStructureBudgetaireGenH>().HasNoKey();
        modelBuilder.Entity<CptTypeCreditSupp>().HasNoKey();
        modelBuilder.Entity<DepLigneReleveMarche>().HasKey(x => new { x.CodeReleve, x.CodeFilsProduit });
        modelBuilder.Entity<StkMagasinMagasinier>().HasNoKey();
        modelBuilder.Entity<DepReceptionReleve>().HasKey(x => new { x.CodeReleve, x.CodeReception });
        modelBuilder.Entity<DepFactureReleve>().HasKey(x => new { x.CodeFacture, x.CodeReleve });
        modelBuilder.Entity<DepAvancementFacture>().HasKey(x => new { x.CodeAvancement, x.CodeFacture, x.CodeFilsProduit });
        modelBuilder.Entity<DepMontantsFacture>().HasKey(x => x.CodeFacture);
        modelBuilder.Entity<DepMontantsFacture>().Property(x => x.CodeFacture).ValueGeneratedNever();
        modelBuilder.Entity<DepAllMontantsFacture>().HasKey(x => x.CodeFacture);
        modelBuilder.Entity<DepAllMontantsFacture>().Property(x => x.CodeFacture).ValueGeneratedNever();
        modelBuilder.Entity<Journal>().HasNoKey();
        base.OnModelCreating(modelBuilder);
    }
}