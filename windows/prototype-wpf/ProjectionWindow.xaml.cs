using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace IbadaPrototype;

public enum ProjectionMode { Slide, Black, Logo }

public partial class ProjectionWindow : Window
{
    private readonly System.Drawing.Rectangle _bounds;

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int x, int y, int cx, int cy, uint flags);

    private static readonly IntPtr HWND_TOPMOST = new(-1);
    private const uint SWP_NOACTIVATE = 0x0010;

    public ProjectionWindow(System.Drawing.Rectangle bounds)
    {
        _bounds = bounds;
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.Manual;
        WindowState = WindowState.Normal;
        Loaded += OnLoaded;
    }

    // Placement en pixels physiques : évite l'erreur due à la mise à l'échelle Windows
    // (125 %, 150 %…) sur le second écran. Les coordonnées de Screen.Bounds et de
    // SetWindowPos sont dans le même espace.
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var hwnd = new WindowInteropHelper(this).Handle;
        SetWindowPos(hwnd, HWND_TOPMOST, _bounds.Left, _bounds.Top,
            _bounds.Width, _bounds.Height, SWP_NOACTIVATE);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var videoPath = Path.Combine(AppContext.BaseDirectory, "assets", "test-video.mp4");
        if (File.Exists(videoPath))
        {
            BackgroundVideo.Source = new Uri(videoPath);
            BackgroundVideo.Play();
            BackgroundVideo.MediaEnded += (_, _) =>
            {
                BackgroundVideo.Position = TimeSpan.Zero;
                BackgroundVideo.Play();
            };
        }
    }

    public void SetSlide(string text)
    {
        SetMode(ProjectionMode.Slide);
        SlideText.Text = text;
    }

    public void SetMode(ProjectionMode mode)
    {
        SlideText.Visibility = mode == ProjectionMode.Slide ? Visibility.Visible : Visibility.Collapsed;
        LogoText.Visibility = mode == ProjectionMode.Logo ? Visibility.Visible : Visibility.Collapsed;
        BackgroundVideo.Visibility = mode == ProjectionMode.Black ? Visibility.Collapsed : Visibility.Visible;
    }
}
