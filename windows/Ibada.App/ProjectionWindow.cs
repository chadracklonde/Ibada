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

    private readonly bool _windowed;

    /// <param name="windowed">Un seul écran : projection dans une fenêtre ordinaire, sinon elle cacherait la fenêtre de l'opérateur.</param>
    public ProjectionWindow(System.Drawing.Rectangle physicalBounds, bool windowed = false)
    {
        _bounds = physicalBounds;
        _windowed = windowed;
        WindowStyle = windowed ? WindowStyle.SingleBorderWindow : WindowStyle.None;
        ResizeMode = windowed ? ResizeMode.CanResize : ResizeMode.NoResize;
        ShowInTaskbar = windowed;
        Topmost = !windowed;
        Cursor = windowed ? Cursors.Arrow : Cursors.None;
        Background = System.Windows.Media.Brushes.Black;
        Title = "Ibada — projection";
        Content = _view;
        // Position provisoire ; la vraie position est posée en pixels physiques dans OnSourceInitialized.
        WindowStartupLocation = WindowStartupLocation.Manual;
        Left = windowed ? 20 : physicalBounds.Left; Top = windowed ? 20 : physicalBounds.Top; Width = windowed ? 960 : 800; Height = windowed ? 540 : 450;
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
        Place();
        // WPF peut réappliquer sa propre taille après l'initialisation : on replace la fenêtre une fois affichée.
        ContentRendered += (_, _) => Place();
    }

    private void Place()
    {
        if (_windowed) return;
        var hwnd = new WindowInteropHelper(this).Handle;
        SetWindowPos(hwnd, new IntPtr(-1), _bounds.Left, _bounds.Top, _bounds.Width, _bounds.Height, 0x0040 /* SWP_SHOWWINDOW */);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);
}
