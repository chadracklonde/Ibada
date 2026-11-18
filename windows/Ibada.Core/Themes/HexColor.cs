namespace Ibada.Core.Themes;

/// <summary>Couleur « #RRGGBB » ou « #AARRGGBB » (ordre de WPF). Sert à valider les thèmes sans dépendre de WPF.</summary>
public readonly record struct HexColor(byte A, byte R, byte G, byte B)
{
    public static HexColor? Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var t = text.Trim();
        if (t[0] != '#' || (t.Length != 7 && t.Length != 9)) return null;
        if (!uint.TryParse(t.AsSpan(1), System.Globalization.NumberStyles.AllowHexSpecifier,
                System.Globalization.CultureInfo.InvariantCulture, out var v)) return null;
        return t.Length == 7
            ? new HexColor(255, (byte)(v >> 16), (byte)(v >> 8), (byte)v)
            : new HexColor((byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)v);
    }

    public static bool IsValid(string? text) => Parse(text) is not null;
}
