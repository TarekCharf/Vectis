namespace VectisBackend.Services;

/// <summary>
/// Reproduit EXACTEMENT applyConstraints() de import-depense.html : les dépendances entre
/// étapes (Engagement / Livraison / Réception / Facture) et leur mode de validation.
/// Règles (dans cet ordre, chacune peut modifier l'entrée de la suivante — comme en JS) :
///   1. Sans Engagement "Avec validation" -> Livraison ne peut pas être "Avec validation".
///   2. Sans Livraison "Avec validation" -> Facture ne peut pas être incluse.
///   3. Sans Livraison "Avec validation" -> Réception ne peut pas être "Avec validation".
///   4. Sans Livraison incluse -> Réception ne peut pas être incluse.
/// </summary>
public static class StepSelectionConstraints
{
    public sealed record Input(
        bool EngagementIncluded, bool EngagementAvecValidation,
        bool LivraisonIncluded, bool LivraisonAvecValidation,
        bool ReceptionIncluded, bool ReceptionAvecValidation,
        bool FactureIncluded, bool FactureAvecValidation);

    public sealed record Result(
        bool EngagementIncluded, bool EngagementAvecValidation,
        bool LivraisonIncluded, bool LivraisonAvecValidation,
        bool ReceptionIncluded, bool ReceptionAvecValidation,
        bool FactureIncluded, bool FactureAvecValidation,
        List<string> Adjustments)
    {
        /// <summary>True si le résultat corrigé diffère de ce qui a été soumis (au moins une règle a dû s'appliquer).</summary>
        public bool WasAdjusted => Adjustments.Count > 0;
    }

    public static Result Apply(Input input)
    {
        var adjustments = new List<string>();

        var engagementIncluded = input.EngagementIncluded;
        var engagementAvecValidation = input.EngagementAvecValidation;

        var livraisonIncluded = input.LivraisonIncluded;
        var livraisonAvecValidation = input.LivraisonAvecValidation;

        var receptionIncluded = input.ReceptionIncluded;
        var receptionAvecValidation = input.ReceptionAvecValidation;

        var factureIncluded = input.FactureIncluded;
        var factureAvecValidation = input.FactureAvecValidation;

        // Règle 1 : sans Engagement "Avec validation", Livraison ne peut pas être "Avec validation"
        var engagementValide = engagementIncluded && engagementAvecValidation;
        if (!engagementValide && livraisonAvecValidation)
        {
            livraisonAvecValidation = false;
            adjustments.Add("Livraison forcée en \"Sans validation\" (Engagement absent ou sans validation).");
        }

        // Règle 2 : sans Livraison "Avec validation", Facture ne peut pas être incluse
        var livraisonValide = livraisonIncluded && livraisonAvecValidation;
        if (!livraisonValide && factureIncluded)
        {
            factureIncluded = false;
            adjustments.Add("Facture désactivée (Livraison absente ou sans validation).");
        }

        // Règle 3 : sans Livraison "Avec validation", Réception ne peut pas être "Avec validation"
        if (!livraisonValide && receptionAvecValidation)
        {
            receptionAvecValidation = false;
            adjustments.Add("Réception forcée en \"Sans validation\" (Livraison absente ou sans validation).");
        }

        // Règle 4 : sans Livraison incluse, Réception ne peut pas être incluse
        if (!livraisonIncluded && receptionIncluded)
        {
            receptionIncluded = false;
            adjustments.Add("Réception désactivée (Livraison non incluse).");
        }

        return new Result(
            engagementIncluded, engagementAvecValidation,
            livraisonIncluded, livraisonAvecValidation,
            receptionIncluded, receptionAvecValidation,
            factureIncluded, factureAvecValidation,
            adjustments);
    }
}
