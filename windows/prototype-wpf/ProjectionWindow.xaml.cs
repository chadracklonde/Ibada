using System;
using System.IO;
using System.Windows;

namespace IbadaPrototype;

public enum ProjectionMode { Slide, Black, Logo }

public partial class ProjectionWindow : Window
{
    public ProjectionWindow(System.Drawing.Rectangle bounds)
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.Manual;
        Left = bounds.Left;
        Top = bounds.Top;
        Width = bounds.Width;
        Height = bounds.Height;
        WindowState = WindowState.Normal;
        Loaded += OnLoaded;
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
