namespace Ibada.Core.Slides;

public static class SlideBuilder
{
    public static string NormalizeNewlines(string text) => text.Replace("\r\n", "\n").Replace('\r', '\n');

    /// <summary>
    /// Découpe un texte en groupes d'au plus <paramref name="maxLines"/> lignes. Les groupes sont équilibrés :
    /// cinq lignes avec un maximum de quatre donnent 3 + 2, pas 4 + 1, pour éviter une diapositive avec une ligne seule.
    /// </summary>
    public static IReadOnlyList<string> ChunkLines(string text, int maxLines)
    {
        if (maxLines < 1) throw new ArgumentOutOfRangeException(nameof(maxLines));
        var lines = NormalizeNewlines(text).Split('\n').Select(l => l.TrimEnd()).ToList();
        while (lines.Count > 0 && lines[^1].Length == 0) lines.RemoveAt(lines.Count - 1);
        while (lines.Count > 0 && lines[0].Length == 0) lines.RemoveAt(0);
        if (lines.Count == 0) return Array.Empty<string>();

        var groups = (int)Math.Ceiling(lines.Count / (double)maxLines);
        var size = (int)Math.Ceiling(lines.Count / (double)groups);
        var result = new List<string>();
        for (var i = 0; i < lines.Count; i += size)
            result.Add(string.Join("\n", lines.Skip(i).Take(size)));
        return result;
    }

    /// <summary>
    /// Diapositives personnalisées (annonces, liturgie) : une ligne contenant seulement « --- » sépare deux diapositives.
    /// Les diapositives vides sont ignorées.
    /// </summary>
    public static IReadOnlyList<Slide> FromCustomText(string title, string text, string? themeId = null)
    {
        var slides = new List<Slide>();
        var current = new List<string>();
        void Flush()
        {
            var body = string.Join("\n", current).Trim('\n', ' ', '\t');
            if (body.Length > 0)
                slides.Add(new Slide { Kind = SlideKind.Custom, Text = body, Label = title, ThemeId = themeId });
            current.Clear();
        }
        foreach (var raw in NormalizeNewlines(text).Split('\n'))
        {
            if (raw.Trim() == "---") Flush(); else current.Add(raw.TrimEnd());
        }
        Flush();
        return slides;
    }

    public static Slide FromImage(string title, string path, string? themeId = null)
        => new() { Kind = SlideKind.Image, Label = title, ImagePath = path, ThemeId = themeId };
}
