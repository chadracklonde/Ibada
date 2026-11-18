using Ibada.Core.Slides;

namespace Ibada.Core.Hosting;

/// <summary>
/// Diapositives d'essai affichées au premier démarrage, pour essayer l'aperçu, le direct, les modes et les thèmes.
/// Textes originaux, sans droits : ni chant protégé ni verset d'une traduction sous licence.
/// </summary>
public static class SampleDeck
{
    public static IReadOnlyList<Slide> Build()
    {
        var slides = new List<Slide>
        {
            new() { Kind = SlideKind.Custom, Label = "Accueil", Text = "Bienvenue à Ibada\nVersion 1 — essai" },
            new() { Kind = SlideKind.Song, Label = "Essai 4 lignes", Footer = "Texte d'essai",
                    Text = "Nous chantons pour te louer\nTon amour ne change pas\nAu matin comme au soir\nTu restes fidèle" },
            new() { Kind = SlideKind.Custom, Label = "Mot court", Text = "Alléluia" },
            new() { Kind = SlideKind.Custom, Label = "Texte long",
                    Text = "Ceci est un texte volontairement long pour vérifier que la mise en page reste lisible, " +
                           "que les lignes passent à la ligne sans déborder et que la taille se réduit toute seule " +
                           "quand il n'y a plus assez de place sur l'écran de projection, quelle que soit la résolution." },
        };
        slides.AddRange(SlideBuilder.FromCustomText("Annonces",
            "Réunion de prière\nmercredi à 17 h\n---\nRépétition de la chorale\nsamedi après-midi"));
        return slides;
    }
}
