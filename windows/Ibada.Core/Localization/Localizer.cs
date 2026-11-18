namespace Ibada.Core.Localization;

/// <summary>
/// Textes de l'interface (ERG-02). Le français est intégré ; les autres langues (swahili, lingala,
/// anglais, V2) sont des dictionnaires clé → texte chargés depuis des fichiers et complétés par le français.
/// </summary>
public sealed class Localizer
{
    public static readonly IReadOnlyDictionary<string, string> French = new Dictionary<string, string>
    {
        ["app.title"] = "Ibada",
        ["operator.title"] = "Ibada — Opérateur",
        ["screen.label"] = "Écran de projection :",
        ["screen.primary"] = "Principal",
        ["screen.secondary"] = "Secondaire",
        ["projection.open"] = "Ouvrir la projection",
        ["projection.close"] = "Fermer la projection",
        ["mode.slide"] = "Diapositive",
        ["mode.black"] = "Écran noir",
        ["mode.logo"] = "Logo",
        ["mode.clear"] = "Texte vide",
        ["nav.previous"] = "Précédent",
        ["nav.next"] = "Suivant",
        ["nav.golive"] = "Projeter",
        ["status.ready"] = "Prêt.",
        ["backup.create"] = "Sauvegarder maintenant",
        ["backup.restore"] = "Restaurer…",
        ["backup.done"] = "Sauvegarde créée : {0}",
        ["backup.restored"] = "Sauvegarde restaurée. Une copie de sécurité a été faite : {0}",
        ["backup.none"] = "Aucune sauvegarde.",
        ["settings.title"] = "Réglages",
        ["settings.church"] = "Nom de l'église",
        ["settings.language"] = "Langue",
        ["settings.shortcuts"] = "Raccourcis clavier",
        ["slides.title"] = "Diapositives",
        ["preview.title"] = "Aperçu",
        ["live.title"] = "Direct",
        ["theme.label"] = "Thème :",
        ["theme.edit"] = "Modifier…",
        ["theme.new"] = "Nouveau…",
        ["theme.editor.title"] = "Modifier le thème",
        ["dialog.restore.title"] = "Restaurer une sauvegarde",
        ["dialog.restore.confirm"] = "Restaurer « {0} » ? Les données actuelles seront remplacées. Une copie de sécurité est faite d'abord.",
        ["dialog.ok"] = "OK",
        ["dialog.cancel"] = "Annuler",
        ["projection.opened"] = "Projection ouverte sur {0}.",
        ["projection.closed"] = "Projection fermée.",
        ["projection.noscreen"] = "Aucun écran de projection disponible.",
        ["error.generic"] = "Une erreur s'est produite. Le détail est dans le journal.",
    };

    private readonly IReadOnlyDictionary<string, string> _overrides;

    public string Language { get; }

    public Localizer(string language = "fr", IReadOnlyDictionary<string, string>? translations = null)
    {
        Language = language;
        _overrides = translations ?? new Dictionary<string, string>();
    }

    /// <summary>Texte pour une clé ; à défaut, le français ; à défaut, la clé elle-même (jamais d'exception).</summary>
    public string T(string key)
    {
        if (_overrides.TryGetValue(key, out var v) && !string.IsNullOrEmpty(v)) return v;
        return French.TryGetValue(key, out var fr) ? fr : key;
    }

    /// <summary>Texte avec paramètres, par exemple « Sauvegarde créée : {0} ».</summary>
    public string T(string key, params object[] args)
    {
        var text = T(key);
        try { return string.Format(text, args); }
        catch (FormatException) { return text; }
    }

    /// <summary>Clés du français absentes d'une traduction : sert à guider le traducteur.</summary>
    public static IReadOnlyList<string> MissingKeys(IReadOnlyDictionary<string, string> translation)
        => French.Keys.Where(k => !translation.ContainsKey(k) || string.IsNullOrWhiteSpace(translation[k]))
            .OrderBy(k => k, StringComparer.Ordinal).ToList();
}
