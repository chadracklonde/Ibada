namespace Ibada.Core.Input;

public enum Command
{
    NextSlide,
    PreviousSlide,
    GoLive,
    ShowSlide,
    BlackScreen,
    Logo,
    ClearText,
    CreateBackup,
}

/// <summary>
/// Raccourcis clavier configurables (ERG-01). Un geste (« Ctrl+S », « Space », « Right ») correspond
/// à une commande ; l'interface ne connaît que les commandes, pas les touches.
/// </summary>
public sealed class ShortcutMap
{
    private readonly Dictionary<string, Command> _map = new(StringComparer.Ordinal);

    public static IReadOnlyDictionary<string, Command> Defaults { get; } = new Dictionary<string, Command>
    {
        ["Space"] = Command.NextSlide,
        ["Right"] = Command.NextSlide,
        ["PageDown"] = Command.NextSlide,
        ["Left"] = Command.PreviousSlide,
        ["PageUp"] = Command.PreviousSlide,
        ["Enter"] = Command.GoLive,
        ["D"] = Command.ShowSlide,
        ["B"] = Command.BlackScreen,
        ["L"] = Command.Logo,
        ["C"] = Command.ClearText,
        ["Ctrl+S"] = Command.CreateBackup,
    };

    public ShortcutMap()
    {
        foreach (var (g, c) in Defaults) _map[g] = c;
    }

    /// <summary>Met les gestes sous une forme unique : « shift+ctrl+x » devient « Ctrl+Shift+X ».</summary>
    public static string Normalize(string gesture)
    {
        var parts = gesture.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "";
        bool ctrl = false, alt = false, shift = false;
        string key = "";
        foreach (var p in parts)
        {
            switch (p.ToLowerInvariant())
            {
                case "ctrl" or "control": ctrl = true; break;
                case "alt": alt = true; break;
                case "shift" or "maj": shift = true; break;
                default: key = CanonicalKey(p); break;
            }
        }
        if (key.Length == 0) return "";
        return (ctrl ? "Ctrl+" : "") + (alt ? "Alt+" : "") + (shift ? "Shift+" : "") + key;
    }

    private static readonly Dictionary<string, string> KnownKeys = new[]
    {
        "Space", "Enter", "Left", "Right", "Up", "Down", "PageUp", "PageDown", "Home", "End",
        "Escape", "Tab", "Back", "Delete", "Insert",
    }.Concat(Enumerable.Range(1, 24).Select(i => "F" + i))
     .ToDictionary(k => k, k => k, StringComparer.OrdinalIgnoreCase);

    // Les noms de touches suivent ceux de WPF (Key.PageDown, Key.Space…) : la casse doit être exacte.
    private static string CanonicalKey(string key)
    {
        if (KnownKeys.TryGetValue(key, out var known)) return known;
        return key.Length == 1 ? key.ToUpperInvariant() : char.ToUpperInvariant(key[0]) + key[1..];
    }

    public Command? Resolve(string gesture)
        => _map.TryGetValue(Normalize(gesture), out var c) ? c : null;

    /// <summary>Associe un geste à une commande. Si ce geste servait déjà, il est réaffecté.</summary>
    public void Bind(string gesture, Command command)
    {
        var g = Normalize(gesture);
        if (g.Length == 0) throw new ArgumentException("Geste vide.", nameof(gesture));
        _map[g] = command;
    }

    public bool Unbind(string gesture) => _map.Remove(Normalize(gesture));

    public IReadOnlyList<string> GesturesFor(Command command)
        => _map.Where(kv => kv.Value == command).Select(kv => kv.Key).OrderBy(g => g, StringComparer.Ordinal).ToList();

    /// <summary>Tout le tableau sous la forme geste → nom de commande, pour l'enregistrer dans les réglages.</summary>
    public Dictionary<string, string> ToSettings()
        => _map.ToDictionary(kv => kv.Key, kv => kv.Value.ToString());

    /// <summary>
    /// Recharge depuis les réglages. Si le tableau est vide, ce sont les valeurs par défaut.
    /// Les entrées inconnues (commande disparue, geste vide) sont ignorées sans erreur.
    /// </summary>
    public static ShortcutMap FromSettings(IReadOnlyDictionary<string, string>? saved)
    {
        var map = new ShortcutMap();
        if (saved is null || saved.Count == 0) return map;
        map._map.Clear();
        foreach (var (g, name) in saved)
        {
            var n = Normalize(g);
            if (n.Length > 0 && Enum.TryParse<Command>(name, out var c) && Enum.IsDefined(c)) map._map[n] = c;
        }
        return map;
    }
}
