using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Ibada.Core.Hosting;
using Ibada.Core.Input;
using Ibada.Core.Live;
using Forms = System.Windows.Forms;

namespace Ibada.App;

/// <summary>
/// Fenêtre de l'opérateur (L1 et L2). Interface construite en code : aucun fichier XAML à garder en accord
/// avec le code, et chaque texte vient du Localizer. La logique est dans AppHost ; ici, uniquement de l'affichage.
/// </summary>
public sealed class MainWindow : Window
{
    private static AppHost H => App.Host;

    private readonly ComboBox _screens = new() { MinWidth = 220 };
    private readonly ComboBox _themes = new() { MinWidth = 140, DisplayMemberPath = "Name" };
    private readonly Button _openClose = new();
    private readonly ListBox _list = new() { MinWidth = 220 };
    private readonly SlideView _preview = new();
    private readonly SlideView _live = new();
    private readonly TextBlock _status = new();
    private readonly Dictionary<LiveMode, Button> _modeButtons = new();
    private Button _prev = null!, _next = null!, _golive = null!, _save = null!, _restore = null!;
    private TextBlock _slidesTitle = null!, _previewTitle = null!, _liveTitle = null!, _screenLabel = null!, _themeLabel = null!;
    private ProjectionWindow? _projection;
    private bool _updating;

    public MainWindow()
    {
        Width = 1200; Height = 720; MinWidth = 900; MinHeight = 560;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Content = BuildLayout();
        FillScreens();
        FillThemes();
        ApplyTexts();
        H.Live.Changed += Refresh;
        _list.SelectionChanged += (_, _) => { if (!_updating && _list.SelectedIndex >= 0) H.Live.Select(_list.SelectedIndex); };
        PreviewKeyDown += OnKey;
        Closed += (_, _) => CloseProjection();
        Loaded += (_, _) => { RebuildList(); Refresh(); };
    }

    private UIElement BuildLayout()
    {
        var root = new DockPanel { Margin = new Thickness(8) };

        // Barre du haut
        var top = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        _screenLabel = new TextBlock { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0) };
        top.Children.Add(_screenLabel);
        top.Children.Add(_screens);
        _openClose.Margin = new Thickness(8, 0, 16, 0); _openClose.Padding = new Thickness(12, 4, 12, 4);
        _openClose.Click += (_, _) => ToggleProjection();
        top.Children.Add(_openClose);
        _themeLabel = new TextBlock { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0) };
        top.Children.Add(_themeLabel);
        _themes.SelectionChanged += (_, _) =>
        {
            if (_updating || _themes.SelectedItem is not Ibada.Core.Themes.Theme t) return;
            H.SetTheme(t.Id); Refresh();
        };
        top.Children.Add(_themes);
        _save = Make("backup.create", (_, _) => DoBackup()); _save.Margin = new Thickness(16, 0, 0, 0);
        _restore = Make("backup.restore", (_, _) => DoRestore()); _restore.Margin = new Thickness(6, 0, 0, 0);
        top.Children.Add(_save); top.Children.Add(_restore);
        DockPanel.SetDock(top, Dock.Top); root.Children.Add(top);

        // Barre du bas : état
        _status.Margin = new Thickness(0, 8, 0, 0);
        DockPanel.SetDock(_status, Dock.Bottom); root.Children.Add(_status);

        // Commandes
        var controls = new WrapPanel { Margin = new Thickness(0, 8, 0, 0) };
        _prev = Make("nav.previous", (_, _) => H.Live.Previous());
        _next = Make("nav.next", (_, _) => H.Live.Next());
        _golive = Make("nav.golive", (_, _) => H.Live.GoLive());
        controls.Children.Add(_prev); controls.Children.Add(_next); controls.Children.Add(_golive);
        foreach (var (mode, key) in new[] { (LiveMode.Slide, "mode.slide"), (LiveMode.Black, "mode.black"), (LiveMode.Logo, "mode.logo"), (LiveMode.Clear, "mode.clear") })
        {
            var m = mode;
            var b = Make(key, (_, _) => H.Live.SetMode(m));
            if (mode == LiveMode.Slide) b.Margin = new Thickness(24, 0, 6, 0);
            _modeButtons[mode] = b; controls.Children.Add(b);
        }
        DockPanel.SetDock(controls, Dock.Bottom); root.Children.Add(controls);

        // Liste à gauche
        var left = new DockPanel { Margin = new Thickness(0, 0, 8, 0) };
        _slidesTitle = new TextBlock { FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 4) };
        DockPanel.SetDock(_slidesTitle, Dock.Top); left.Children.Add(_slidesTitle);
        left.Children.Add(_list);
        DockPanel.SetDock(left, Dock.Left); root.Children.Add(left);

        // Aperçu et direct côte à côte
        var views = new UniformGrid { Columns = 2 };
        views.Children.Add(Framed(out _previewTitle, _preview));
        views.Children.Add(Framed(out _liveTitle, _live));
        root.Children.Add(views);
        return root;
    }

    private static UIElement Framed(out TextBlock title, SlideView view)
    {
        title = new TextBlock { FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 4) };
        var box = new Border { Background = System.Windows.Media.Brushes.Black, BorderBrush = System.Windows.Media.Brushes.Gray, BorderThickness = new Thickness(1), Child = new Viewbox { Stretch = System.Windows.Media.Stretch.Uniform, Child = Fixed(view) } };
        var panel = new DockPanel { Margin = new Thickness(4) };
        DockPanel.SetDock(title, Dock.Top); panel.Children.Add(title); panel.Children.Add(box);
        return panel;
    }

    /// <summary>Taille fixe 16:9 dans une Viewbox : l'aperçu garde les proportions d'un écran de projection.</summary>
    private static UIElement Fixed(SlideView view) { view.Width = 960; view.Height = 540; return view; }

    private Button Make(string key, RoutedEventHandler click)
    {
        var b = new Button { Tag = key, Padding = new Thickness(12, 6, 12, 6), Margin = new Thickness(0, 0, 6, 0) };
        b.Click += click; return b;
    }

    // ---------- Données

    private void FillScreens()
    {
        _updating = true;
        _screens.Items.Clear();
        var all = Forms.Screen.AllScreens;
        for (var i = 0; i < all.Length; i++)
            _screens.Items.Add($"{i + 1} — {(all[i].Primary ? H.Text.T("screen.primary") : H.Text.T("screen.secondary"))} ({all[i].Bounds.Width}×{all[i].Bounds.Height})");
        // Par défaut : l'écran enregistré s'il existe, sinon le dernier (souvent le projecteur), sinon le seul.
        var wanted = H.Settings.ProjectionDisplay;
        _screens.SelectedIndex = wanted is int w && w >= 0 && w < all.Length ? w : all.Length - 1;
        _updating = false;
    }

    private void FillThemes()
    {
        _updating = true;
        _themes.ItemsSource = H.Themes.All();
        _themes.SelectedItem = H.Themes.All().FirstOrDefault(t => t.Id == H.Settings.ThemeId);
        _updating = false;
    }

    private void ApplyTexts()
    {
        Title = H.Text.T("operator.title");
        _screenLabel.Text = H.Text.T("screen.label");
        _themeLabel.Text = H.Text.T("theme.label");
        _slidesTitle.Text = H.Text.T("slides.title");
        _previewTitle.Text = H.Text.T("preview.title");
        _liveTitle.Text = H.Text.T("live.title");
        foreach (var b in new[] { _prev, _next, _golive, _save, _restore }.Concat(_modeButtons.Values))
            b.Content = H.Text.T((string)b.Tag);
        UpdateOpenCloseText();
    }

    private void RebuildList()
    {
        _updating = true;
        _list.Items.Clear();
        for (var i = 0; i < H.Live.Deck.Count; i++)
        {
            var s = H.Live.Deck[i];
            var first = (s.Text ?? "").Split('\n')[0];
            _list.Items.Add($"{i + 1}. {(string.IsNullOrWhiteSpace(s.Label) ? first : s.Label)}");
        }
        _updating = false;
    }

    private void Refresh()
    {
        _updating = true;
        _list.SelectedIndex = H.Live.Selected;
        _updating = false;

        var theme = H.ThemeFor(H.Live.PreviewSlide);
        _preview.Theme = theme; _preview.LogoPath = H.Settings.LogoPath; _preview.ChurchName = H.Settings.ChurchName;
        _preview.Mode = LiveMode.Slide; _preview.Slide = H.Live.PreviewSlide;

        var state = H.Live.Projection;
        var liveTheme = H.ThemeFor(state.Slide);
        _live.Theme = liveTheme; _live.LogoPath = H.Settings.LogoPath; _live.ChurchName = H.Settings.ChurchName;
        _live.Mode = state.Mode; _live.Slide = state.Slide;
        _projection?.Show(state, liveTheme, H.Settings.LogoPath, H.Settings.ChurchName);

        foreach (var (mode, b) in _modeButtons)
            b.FontWeight = state.Mode == mode ? FontWeights.Bold : FontWeights.Normal;
    }

    // ---------- Projection

    private void ToggleProjection()
    {
        try
        {
            if (_projection is not null) { CloseProjection(); SetStatus(H.Text.T("projection.closed")); return; }
            var all = Forms.Screen.AllScreens;
            var idx = _screens.SelectedIndex;
            if (idx < 0 || idx >= all.Length) { SetStatus(H.Text.T("projection.noscreen")); return; }
            H.SetProjectionDisplay(idx);
            var windowed = all.Length < 2;
            _projection = new ProjectionWindow(all[idx].Bounds, windowed);
            _projection.Closed += (_, _) => { _projection = null; UpdateOpenCloseText(); };
            _projection.Show();
            Refresh();
            UpdateOpenCloseText();
            SetStatus(windowed ? H.Text.T("projection.windowed") : H.Text.T("projection.opened", _screens.SelectedItem?.ToString() ?? ""));
            Activate(); // le focus reste à l'opérateur, jamais à la fenêtre de projection
        }
        catch (Exception e) { Fail("projection", e); }
    }

    private void CloseProjection()
    {
        var p = _projection; _projection = null;
        try { p?.Close(); } catch { }
        UpdateOpenCloseText();
    }

    private void UpdateOpenCloseText()
        => _openClose.Content = H.Text.T(_projection is null ? "projection.open" : "projection.close");

    // ---------- Sauvegardes

    private void DoBackup()
    {
        try { SetStatus(H.Text.T("backup.done", H.CreateBackup())); }
        catch (Exception e) { Fail("sauvegarde", e); }
    }

    private void DoRestore()
    {
        try
        {
            var names = Ibada.Core.Storage.Backups.List(H.Store);
            if (names.Count == 0) { SetStatus(H.Text.T("backup.none")); return; }
            var dlg = new RestoreDialog(names, H.Text) { Owner = this };
            if (dlg.ShowDialog() != true || dlg.Chosen is null) return;
            var safety = H.Restore(dlg.Chosen);
            FillThemes(); ApplyTexts(); RebuildList(); Refresh();
            SetStatus(H.Text.T("backup.restored", safety));
        }
        catch (Exception e) { Fail("restauration", e); }
    }

    // ---------- Clavier

    private void OnKey(object sender, KeyEventArgs e)
    {
        // Une liste déroulante ouverte garde ses touches ; fermée, elle ne doit pas bloquer les commandes du culte.
        if (Keyboard.FocusedElement is ComboBox { IsDropDownOpen: true }) return;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or Key.LeftAlt or Key.RightAlt) return;
        var gesture = Keyboard.Modifiers.HasFlag(ModifierKeys.Control) ? $"Ctrl+{key}" : key.ToString();
        try
        {
            var command = H.HandleGesture(gesture);
            if (command is null) return;
            if (command == Command.CreateBackup) DoBackup();
            e.Handled = true;
        }
        catch (Exception ex) { Fail("clavier", ex); }
    }

    // ---------- Erreurs

    private void SetStatus(string text) => _status.Text = text;

    private void Fail(string context, Exception e)
    {
        H.LogError(context, e);
        SetStatus(H.Text.T("error.generic"));
    }
}
