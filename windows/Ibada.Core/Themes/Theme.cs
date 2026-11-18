namespace Ibada.Core.Themes;

public enum BackgroundType { Color, Image, Video }
public enum HAlign { Left, Center, Right }
public enum VAlign { Top, Middle, Bottom }

public sealed record BackgroundSpec
{
    public BackgroundType Type { get; init; } = BackgroundType.Color;
    /// <summary>Couleur « #RRGGBB », ou chemin du fichier image ou vidéo.</summary>
    public string Value { get; init; } = "#000000";
}

public sealed record FontSpec
{
    public string Family { get; init; } = "Segoe UI";
    /// <summary>Taille sur une hauteur de référence de 1080 : elle est mise à l'échelle selon la résolution (THM-03).</summary>
    public double Size { get; init; } = 72;
    public string Color { get; init; } = "#FFFFFF";
    public bool Bold { get; init; } = true;
    public bool Italic { get; init; }
    public bool Underline { get; init; }
}

public sealed record ShadowSpec
{
    public bool Enabled { get; init; } = true;
    public double X { get; init; } = 3;
    public double Y { get; init; } = 3;
    public double Blur { get; init; } = 6;
    public string Color { get; init; } = "#000000";
}

public sealed record OutlineSpec
{
    public bool Enabled { get; init; }
    public double Width { get; init; } = 2;
    public string Color { get; init; } = "#000000";
}

public sealed record BandSpec
{
    public bool Show { get; init; } = true;
    public double Size { get; init; } = 32;
    public string Color { get; init; } = "#CCCCCC";
}

/// <summary>
/// Thème de projection (THM-01). Toutes les longueurs sont exprimées pour une hauteur de 1080
/// et mises à l'échelle au moment du rendu (THM-03). Les diapositives ne contiennent aucun style :
/// elles désignent un thème, ce qui permet de modifier toutes celles qui l'utilisent d'un coup (THM-02).
/// </summary>
public sealed record Theme
{
    public string Id { get; init; } = "nuit";
    public string Name { get; init; } = "Nuit";
    public BackgroundSpec Background { get; init; } = new();
    public FontSpec Font { get; init; } = new();
    public HAlign Align { get; init; } = HAlign.Center;
    public VAlign VAlign { get; init; } = VAlign.Middle;
    public ShadowSpec Shadow { get; init; } = new();
    public OutlineSpec Outline { get; init; } = new();
    public double Padding { get; init; } = 60;
    public BandSpec Header { get; init; } = new() { Show = false };
    public BandSpec Footer { get; init; } = new();

    /// <summary>Remplace les valeurs invalides (couleur illisible, taille absurde) par des valeurs par défaut sûres.</summary>
    public Theme Sanitized()
    {
        var d = new Theme();
        static string Col(string? c, string fallback) => HexColor.IsValid(c) ? c! : fallback;
        static double Rng(double v, double min, double max, double fallback)
            => double.IsFinite(v) && v >= min && v <= max ? v : fallback;
        var bgColorOk = Background.Type != BackgroundType.Color || HexColor.IsValid(Background.Value);
        return this with
        {
            Id = string.IsNullOrWhiteSpace(Id) ? d.Id : Id.Trim(),
            Name = string.IsNullOrWhiteSpace(Name) ? Id : Name,
            Background = bgColorOk && !string.IsNullOrWhiteSpace(Background.Value) ? Background : d.Background,
            Font = Font with
            {
                Family = string.IsNullOrWhiteSpace(Font.Family) ? d.Font.Family : Font.Family,
                Size = Rng(Font.Size, 8, 400, d.Font.Size),
                Color = Col(Font.Color, d.Font.Color),
            },
            Shadow = Shadow with
            {
                X = Rng(Shadow.X, -100, 100, d.Shadow.X),
                Y = Rng(Shadow.Y, -100, 100, d.Shadow.Y),
                Blur = Rng(Shadow.Blur, 0, 100, d.Shadow.Blur),
                Color = Col(Shadow.Color, d.Shadow.Color),
            },
            Outline = Outline with { Width = Rng(Outline.Width, 0, 50, d.Outline.Width), Color = Col(Outline.Color, d.Outline.Color) },
            Padding = Rng(Padding, 0, 500, d.Padding),
            Header = Header with { Size = Rng(Header.Size, 8, 200, d.Header.Size), Color = Col(Header.Color, d.Header.Color) },
            Footer = Footer with { Size = Rng(Footer.Size, 8, 200, d.Footer.Size), Color = Col(Footer.Color, d.Footer.Color) },
        };
    }
}
