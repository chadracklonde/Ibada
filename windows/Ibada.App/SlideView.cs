using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ibada.Core.Live;
using Ibada.Core.Rendering;
using Ibada.Core.Slides;
using Ibada.Core.Themes;

namespace Ibada.App;

/// <summary>
/// Dessine une diapositive selon son thème, à n'importe quelle taille (THM-03) : aperçu, direct et projection
/// utilisent ce même élément, donc ce que l'opérateur voit est ce qui sera projeté.
/// </summary>
public sealed class SlideView : FrameworkElement
{
    private static readonly Dictionary<string, ImageSource?> ImageCache = new();

    public Slide? Slide { get => _slide; set { _slide = value; InvalidateVisual(); } }
    public Theme Theme { get => _theme; set { _theme = value; InvalidateVisual(); } }
    public LiveMode Mode { get => _mode; set { _mode = value; InvalidateVisual(); } }
    public string? LogoPath { get => _logo; set { _logo = value; InvalidateVisual(); } }
    public string ChurchName { get => _church; set { _church = value; InvalidateVisual(); } }

    private Slide? _slide;
    private Theme _theme = new();
    private LiveMode _mode = LiveMode.Slide;
    private string? _logo;
    private string _church = "";

    public SlideView() { ClipToBounds = true; SnapsToDevicePixels = true; }

    protected override void OnRender(DrawingContext dc)
    {
        var w = (int)Math.Round(ActualWidth);
        var h = (int)Math.Round(ActualHeight);
        if (w < 2 || h < 2) return;
        var full = new Rect(0, 0, w, h);

        if (Mode == LiveMode.Black) { dc.DrawRectangle(Brushes.Black, null, full); return; }
        if (Mode == LiveMode.Logo) { DrawLogo(dc, full); return; }

        var st = ThemeScaler.Scale(Theme, w, h);
        DrawBackground(dc, full);
        if (Mode == LiveMode.Clear || Slide is null) return;

        if (Slide.Kind == SlideKind.Image && !string.IsNullOrWhiteSpace(Slide.ImagePath))
        {
            var img = LoadImage(Slide.ImagePath);
            if (img is not null) { DrawContained(dc, img, full); return; }
        }

        if (Theme.Header.Show && !string.IsNullOrWhiteSpace(Slide.Label))
            DrawBand(dc, Slide.Label, st.HeaderArea, st.HeaderSizePx, Theme.Header.Color, Theme.Align);
        if (Theme.Footer.Show && !string.IsNullOrWhiteSpace(Slide.Footer))
            DrawBand(dc, Slide.Footer, st.FooterArea, st.FooterSizePx, Theme.Footer.Color, Theme.Align);

        DrawMainText(dc, Slide.Text, st);
    }

    private void DrawBackground(DrawingContext dc, Rect full)
    {
        var bg = Theme.Background;
        if (bg.Type == BackgroundType.Image)
        {
            var img = LoadImage(bg.Value);
            if (img is not null)
            {
                dc.DrawRectangle(Brushes.Black, null, full);
                DrawCovering(dc, img, full);
                return;
            }
        }
        // Une vidéo de fond sera ajoutée avec MED-01 (lot L6) ; en attendant, on garde un fond uni lisible.
        var color = bg.Type == BackgroundType.Color ? ParseColor(bg.Value, Colors.Black) : Colors.Black;
        dc.DrawRectangle(new SolidColorBrush(color), null, full);
    }

    private void DrawMainText(DrawingContext dc, string text, ScaledTheme st)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        var area = st.TextArea;
        var size = st.FontSizePx;
        FormattedText ft = Format(text, size, area.Width, Theme.Font.Color);
        // Réduit la taille jusqu'à ce que le texte tienne dans la zone (plancher : 30 % de la taille voulue).
        while (ft.Height > area.Height && size > st.FontSizePx * 0.3)
        {
            size *= 0.94;
            ft = Format(text, size, area.Width, Theme.Font.Color);
        }

        var y = Theme.VAlign switch
        {
            VAlign.Top => area.Y,
            VAlign.Bottom => area.Y + area.Height - ft.Height,
            _ => area.Y + (area.Height - ft.Height) / 2,
        };
        var origin = new Point(area.X, Math.Max(area.Y, y));

        if (st.ShadowX != 0 || st.ShadowY != 0 || st.ShadowBlur > 0)
        {
            var shadow = Format(text, size, area.Width, Theme.Shadow.Color);
            dc.DrawText(shadow, new Point(origin.X + st.ShadowX, origin.Y + st.ShadowY));
        }

        if (st.OutlineWidthPx > 0)
        {
            var pen = new Pen(new SolidColorBrush(ParseColor(Theme.Outline.Color, Colors.Black)), st.OutlineWidthPx * 2)
            { LineJoin = PenLineJoin.Round };
            var geometry = ft.BuildGeometry(origin);
            dc.DrawGeometry(null, pen, geometry);
            dc.DrawGeometry(new SolidColorBrush(ParseColor(Theme.Font.Color, Colors.White)), null, geometry);
        }
        else
        {
            dc.DrawText(ft, origin);
        }
    }

    private FormattedText Format(string text, double size, double maxWidth, string color)
    {
        var f = Theme.Font;
        var typeface = new Typeface(new FontFamily(f.Family),
            f.Italic ? FontStyles.Italic : FontStyles.Normal,
            f.Bold ? FontWeights.Bold : FontWeights.Normal, FontStretches.Normal);
        var ft = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, size,
            new SolidColorBrush(ParseColor(color, Colors.White)), VisualTreeHelper.GetDpi(this).PixelsPerDip)
        {
            MaxTextWidth = Math.Max(1, maxWidth),
            TextAlignment = Theme.Align switch { HAlign.Left => TextAlignment.Left, HAlign.Right => TextAlignment.Right, _ => TextAlignment.Center },
        };
        if (f.Underline) ft.SetTextDecorations(TextDecorations.Underline);
        return ft;
    }

    private void DrawBand(DrawingContext dc, string text, LayoutRect area, double size, string color, HAlign align)
    {
        if (area.Height <= 0 || size <= 0) return;
        var typeface = new Typeface(new FontFamily(Theme.Font.Family), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        var ft = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, size,
            new SolidColorBrush(ParseColor(color, Colors.LightGray)), VisualTreeHelper.GetDpi(this).PixelsPerDip)
        {
            MaxTextWidth = Math.Max(1, area.Width),
            MaxTextHeight = Math.Max(1, area.Height),
            Trimming = TextTrimming.CharacterEllipsis,
            TextAlignment = align switch { HAlign.Left => TextAlignment.Left, HAlign.Right => TextAlignment.Right, _ => TextAlignment.Center },
        };
        dc.DrawText(ft, new Point(area.X, area.Y + Math.Max(0, (area.Height - ft.Height) / 2)));
    }

    private void DrawLogo(DrawingContext dc, Rect full)
    {
        dc.DrawRectangle(Brushes.Black, null, full);
        var img = string.IsNullOrWhiteSpace(LogoPath) ? null : LoadImage(LogoPath);
        if (img is not null)
        {
            DrawContained(dc, img, new Rect(full.Width * 0.2, full.Height * 0.2, full.Width * 0.6, full.Height * 0.6));
            return;
        }
        // Sans image de logo, le nom de l'église sert de logo.
        var name = string.IsNullOrWhiteSpace(ChurchName) ? "Ibada" : ChurchName;
        var ft = new FormattedText(name, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            new Typeface(new FontFamily(Theme.Font.Family), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
            full.Height * 0.09, Brushes.White, VisualTreeHelper.GetDpi(this).PixelsPerDip)
        { MaxTextWidth = full.Width * 0.8, TextAlignment = TextAlignment.Center };
        dc.DrawText(ft, new Point(full.Width * 0.1, (full.Height - ft.Height) / 2));
    }

    private static void DrawContained(DrawingContext dc, ImageSource img, Rect box)
    {
        var k = Math.Min(box.Width / img.Width, box.Height / img.Height);
        var w = img.Width * k; var h = img.Height * k;
        dc.DrawImage(img, new Rect(box.X + (box.Width - w) / 2, box.Y + (box.Height - h) / 2, w, h));
    }

    private static void DrawCovering(DrawingContext dc, ImageSource img, Rect box)
    {
        var k = Math.Max(box.Width / img.Width, box.Height / img.Height);
        var w = img.Width * k; var h = img.Height * k;
        dc.DrawImage(img, new Rect(box.X + (box.Width - w) / 2, box.Y + (box.Height - h) / 2, w, h));
    }

    /// <summary>Image chargée une seule fois puis gardée en mémoire ; un fichier absent ou illisible donne null, sans erreur.</summary>
    private static ImageSource? LoadImage(string path)
    {
        if (ImageCache.TryGetValue(path, out var cached)) return cached;
        ImageSource? result = null;
        try
        {
            if (File.Exists(path))
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri(Path.GetFullPath(path));
                bmp.EndInit();
                bmp.Freeze();
                result = bmp;
            }
        }
        catch { result = null; }
        ImageCache[path] = result;
        return result;
    }

    private static Color ParseColor(string? hex, Color fallback)
    {
        try { return (Color)ColorConverter.ConvertFromString(hex ?? ""); }
        catch { return fallback; }
    }
}
