using Ibada.Core.Storage;

namespace Ibada.Core.Settings;

/// <summary>Réglages de l'application (ERG-01, ERG-02, THM, PRJ). Valeurs par défaut sûres pour un premier démarrage.</summary>
public sealed class AppSettings
{
    public string Language { get; set; } = "fr";
    public string ThemeId { get; set; } = "nuit";
    /// <summary>Numéro de l'écran de projection ; null = automatique (le second écran s'il existe).</summary>
    public int? ProjectionDisplay { get; set; }
    public string? LogoPath { get; set; }
    /// <summary>Dossier de la bibliothèque VideoPsalm, lu en lecture seule (VPS-04).</summary>
    public string? VideoPsalmFolder { get; set; }
    public int SongMaxLines { get; set; } = 4;
    public int BibleMaxChars { get; set; } = 260;
    public string ChurchName { get; set; } = "";
    /// <summary>Raccourcis modifiés par l'utilisateur : geste → nom de commande (vide = valeurs par défaut).</summary>
    public Dictionary<string, string> Shortcuts { get; set; } = new();

    public const string FileName = "reglages.json";

    public static AppSettings Load(JsonStore store)
    {
        var s = store.Read(FileName, new AppSettings());
        // Valeurs hors limites (fichier modifié à la main) : retour à des valeurs raisonnables.
        if (s.SongMaxLines is < 1 or > 12) s.SongMaxLines = 4;
        if (s.BibleMaxChars is < 60 or > 2000) s.BibleMaxChars = 260;
        if (string.IsNullOrWhiteSpace(s.Language)) s.Language = "fr";
        if (string.IsNullOrWhiteSpace(s.ThemeId)) s.ThemeId = "nuit";
        s.Shortcuts ??= new();
        return s;
    }

    public void Save(JsonStore store) => store.Write(FileName, this);
}
