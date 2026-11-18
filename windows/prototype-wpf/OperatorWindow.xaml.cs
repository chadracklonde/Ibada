using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;

namespace IbadaPrototype;

public partial class OperatorWindow : Window
{
    private static readonly string[] Slides =
    {
        "Bienvenue à Ibada\nPrototype V0",
        "Texte net\navec ombre et contour",
        "Vidéo en boucle\nen arrière-plan",
        "Second écran\nplein écran",
        "Une heure\nde projection",
    };

    private int _current = 0;
    private ProjectionWindow? _projection;

    public OperatorWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        ContentRendered += OnFirstRender;
    }

    private bool _firstRenderDone;

    // Mesure jusqu'au premier affichage réel (cible : moins de 5 s, cahier section 5).
    private void OnFirstRender(object? sender, EventArgs e)
    {
        if (_firstRenderDone) return;
        _firstRenderDone = true;
        App.LogFirstRender();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Remplit la liste des écrans détectés
        var screens = WinForms.Screen.AllScreens;
        ScreenCombo.Items.Clear();
        foreach (var s in screens)
        {
            var label = $"{(s.Primary ? "Principal" : "Secondaire")} — {s.Bounds.Width}×{s.Bounds.Height} ({s.DeviceName})";
            ScreenCombo.Items.Add(label);
        }
        if (screens.Length > 1)
            ScreenCombo.SelectedIndex = 1;
        else if (screens.Length == 1)
            ScreenCombo.SelectedIndex = 0;

        // Charge la vidéo de fond
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
        else
        {
            StatusText.Text = "Vidéo de test introuvable (assets/test-video.mp4).";
        }

        UpdateSlide();
        StatusText.Text = $"Prêt. Démarrage mesuré : {App.StartupWatch.ElapsedMilliseconds} ms. "
                        + $"{screens.Length} écran(s) détecté(s).";
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

    private void ScreenCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

    private void OpenProjection_Click(object sender, RoutedEventArgs e)
    {
        if (ScreenCombo.SelectedIndex < 0) return;
        var screen = WinForms.Screen.AllScreens[ScreenCombo.SelectedIndex];

        if (_projection is { IsVisible: true })
        {
            _projection.Close();
            _projection = null;
        }
        _projection = new ProjectionWindow(screen.Bounds);
        _projection.SetSlide(Slides[_current]);
        _projection.Show();
        StatusText.Text = $"Projection ouverte sur {screen.DeviceName}.";
    }

    private void ShowSlide_Click(object sender, RoutedEventArgs e)
        => _projection?.SetSlide(Slides[_current]);

    private void BlackScreen_Click(object sender, RoutedEventArgs e)
        => _projection?.SetMode(ProjectionMode.Black);

    private void Logo_Click(object sender, RoutedEventArgs e)
        => _projection?.SetMode(ProjectionMode.Logo);
}
