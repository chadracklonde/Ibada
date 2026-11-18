using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Ibada.Core.Live;
using Ibada.Core.Slides;
using Ibada.Core.Themes;

namespace Ibada.App;

/// <summary>
/// Fenêtre de projection : sans bordure, au premier plan, sans curseur, hors de la barre des tâches.
/// Elle n'affiche jamais de boîte de dialogue. Placée en pixels physiques avec SetWindowPos pour éviter
/// les erreurs d'échelle DPI entre l'écran de l'opérateur et celui du projecteur.
/// </summary>
public sealed class ProjectionWindow : Window
{
    private readonly System.Drawing.Rectangle _bounds;
    private readonly SlideView _view = new();

    public ProjectionWindow(System.Drawing.Rectangle physicalBounds)
    {
        _bounds = physicalBounds;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        Topmost = true;
        Cursor = Cursors.None;
        Background = System.Windows.Media.Brushes.Black;
        Title = "Ibada — projection";
        Content = _view;
        // Position provisoire ; la vraie position est posée en pixels physiques dans OnSourceInitialized.
        WindowStartupLocation = WindowStartupLocation.Manual;
        Left = physicalBounds.Left; Top = physicalBounds.Top; Width = 800; Height = 450;
    }

    public void Show(ProjectionState state, Theme theme, string? logoPath, string churchName)
    {
        _view.Theme = theme;
        _view.LogoPath = logoPath;
        _view.ChurchName = churchName;
        _view.Slide = state.Slide;
        _view.Mode = state.Mode;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var hwnd = new WindowInteropHelper(this).Handle;
        SetWindowPos(hwnd, new IntPtr(-1), _bounds.Left, _bounds.Top, _bounds.Width, _bounds.Height, 0x0040 /* SWP_SHOWWINDOW */);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);
}
