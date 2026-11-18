namespace Ibada.Core.Slides;

public enum SlideKind { Song, Bible, Custom, Image }

/// <summary>
/// Une diapositive : du contenu, jamais de style. Le style vient du thème désigné par
/// <see cref="ThemeId"/> (ou du thème par défaut si null), voir THM-02.
/// </summary>
public sealed record Slide
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..12];
    public SlideKind Kind { get; init; } = SlideKind.Custom;
    public string Text { get; init; } = "";
    /// <summary>Étiquette courte affichée dans la liste de l'opérateur (« Couplet 1 », « Refrain »).</summary>
    public string? Label { get; init; }
    /// <summary>Ligne de pied de page (titre du chant, référence biblique).</summary>
    public string? Footer { get; init; }
    public string? ImagePath { get; init; }
    public string? ThemeId { get; init; }
}
