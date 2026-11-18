using System;
using System.IO;
using System.Windows;

namespace IbadaPrototype;

public partial class ProjectionWindow : Window
{
    public ProjectionWindow()
    {
        InitializeComponent();
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

    public void SetSlide(string text) => SlideText.Text = text;
}
