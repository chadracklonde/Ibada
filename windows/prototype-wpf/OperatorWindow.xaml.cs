using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace IbadaPrototype;

public partial class OperatorWindow : Window
{
    private static readonly string[] Slides =
    {
        "Bienvenue à Ibada\nPrototype V0",
        "Texte net\navec ombre et contour",
        "Vidéo en boucle\nen arrière-plan",
        "Second écran\nplein écran",
    };

    private int _current = 0;
    private ProjectionWindow? _projection;

    public OperatorWindow()
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
        UpdateSlide();
    }

    private void UpdateSlide()
    {
        SlideText.Text = Slides[_current];
        _projection?.SetSlide(Slides[_current]);
    }

    private void Prev_Click(object sender, RoutedEventArgs e)
    {
        _current = (_current - 1 + Slides.Length) % Slides.Length;
        UpdateSlide();
    }

    private void Next_Click(object sender, RoutedEventArgs e)
    {
        _current = (_current + 1) % Slides.Length;
        UpdateSlide();
    }

    private void OpenProjection_Click(object sender, RoutedEventArgs e)
    {
        if (_projection is { IsVisible: true }) return;
        _projection = new ProjectionWindow();
        _projection.SetSlide(Slides[_current]);
        _projection.Show();
    }
}
