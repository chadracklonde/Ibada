using Ibada.Core.Storage;

namespace Ibada.Core.Themes;

/// <summary>
/// Thèmes disponibles : quatre thèmes fournis plus les thèmes de l'utilisateur (dossier « themes »).
/// Un thème modifié ou enregistré est utilisé tout de suite par toutes les diapositives qui le désignent (THM-02).
/// </summary>
public sealed class ThemeCatalog
{
    public const string FolderName = "themes";
    public const string FallbackId = "nuit";

    public static IReadOnlyList<Theme> BuiltIn { get; } = new Theme[]
    {
        new() { Id = "nuit", Name = "Nuit" },
        new()
        {
            Id = "contraste", Name = "Fort contraste",
            Background = new() { Value = "#000000" },
            Font = new() { Color = "#FFE600", Size = 80 },
            Outline = new() { Enabled = true, Width = 3, Color = "#000000" },
            Shadow = new() { Enabled = false },
        },
        new()
        {
            Id = "clair", Name = "Clair",
            Background = new() { Value = "#F7F5EE" },
            Font = new() { Color = "#1B1B1B", Bold = false },
            Shadow = new() { Enabled = false },
            Footer = new() { Color = "#555555" },
        },
        new()
        {
            Id = "chaud", Name = "Chaud",
            Background = new() { Value = "#2B1B12" },
            Font = new() { Color = "#FFF1D6", Family = "Georgia" },
            Footer = new() { Color = "#D9B98A" },
        },
    };

    private readonly JsonStore? _store;
    private readonly Dictionary<string, Theme> _custom = new(StringComparer.Ordinal);

    public ThemeCatalog(JsonStore? store = null)
    {
        _store = store;
        if (store is null) return;
        foreach (var file in store.List(FolderName))
        {
            var t = store.Read<Theme?>($"{FolderName}/{file}", null);
            if (t is not null && !string.IsNullOrWhiteSpace(t.Id)) _custom[t.Id] = t.Sanitized();
        }
    }

    public IReadOnlyList<Theme> All()
        => BuiltIn.Where(b => !_custom.ContainsKey(b.Id)).Concat(_custom.Values)
            .OrderBy(t => t.Name, StringComparer.CurrentCulture).ToList();

    public bool IsBuiltIn(string id) => BuiltIn.Any(b => b.Id == id);

    /// <summary>Thème demandé ; à défaut, « nuit » (le rendu ne doit jamais échouer faute de thème).</summary>
    public Theme Get(string? id)
    {
        if (id is not null)
        {
            if (_custom.TryGetValue(id, out var c)) return c;
            var b = BuiltIn.FirstOrDefault(x => x.Id == id);
            if (b is not null) return b;
        }
        return BuiltIn.First(x => x.Id == FallbackId);
    }

    /// <summary>Enregistre ou remplace un thème. Modifier un thème fourni crée une copie personnelle de même identifiant.</summary>
    public Theme Save(Theme theme)
    {
        var t = theme.Sanitized();
        _custom[t.Id] = t;
        _store?.Write($"{FolderName}/{Safe(t.Id)}.json", t);
        return t;
    }

    /// <summary>Supprime un thème personnel. Pour un thème fourni, revient à sa version d'origine.</summary>
    public bool Delete(string id)
    {
        var removed = _custom.Remove(id);
        _store?.Remove($"{FolderName}/{Safe(id)}.json");
        return removed;
    }

    private static string Safe(string id)
        => new(id.Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' ? c : '-').ToArray());
}
