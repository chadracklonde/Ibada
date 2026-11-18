using Ibada.Core.Themes;

namespace Ibada.Core.Rendering;

/// <summary>Un thème mis à l'échelle pour une résolution donnée : toutes les longueurs sont en pixels.</summary>
public sealed record ScaledTheme(
    int Width, int Height, double Scale,
    double FontSizePx, double PaddingPx,
    double ShadowX, double ShadowY, double ShadowBlur,
    double OutlineWidthPx, double HeaderSizePx, double FooterSizePx,
    LayoutRect TextArea, LayoutRect HeaderArea, LayoutRect FooterArea);

public readonly record struct LayoutRect(double X, double Y, double Width, double Height);

/// <summary>
/// Rendu indépendant de la résolution (THM-03). La référence est une hauteur de 1080 : un thème de
/// taille 72 donne 48 px à 720 de haut et 144 px à 2160 de haut, donc la même mise en page à toutes les résolutions.
/// L'échelle suit la hauteur, ce qui garde les proportions sur un écran 16:9, 16:10 ou 4:3.
/// </summary>
public static class ThemeScaler
{
    public const double ReferenceHeight = 1080;

    public static ScaledTheme Scale(Theme theme, int width, int height)
    {
        if (width < 1 || height < 1) throw new ArgumentOutOfRangeException(nameof(width), "Résolution invalide.");
        var k = height / ReferenceHeight;
        var pad = theme.Padding * k;
        var header = theme.Header.Show ? theme.Header.Size * k * 1.6 : 0;
        var footer = theme.Footer.Show ? theme.Footer.Size * k * 1.6 : 0;
        var inner = Math.Max(1, width - 2 * pad);
        return new ScaledTheme(
            width, height, k,
            theme.Font.Size * k, pad,
            theme.Shadow.Enabled ? theme.Shadow.X * k : 0,
            theme.Shadow.Enabled ? theme.Shadow.Y * k : 0,
            theme.Shadow.Enabled ? theme.Shadow.Blur * k : 0,
            theme.Outline.Enabled ? theme.Outline.Width * k : 0,
            theme.Header.Size * k, theme.Footer.Size * k,
            TextArea: new LayoutRect(pad, pad + header, inner, Math.Max(1, height - 2 * pad - header - footer)),
            HeaderArea: new LayoutRect(pad, pad, inner, header),
            FooterArea: new LayoutRect(pad, height - pad - footer, inner, footer));
    }
}
