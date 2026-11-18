using System.Windows;
using System.Windows.Controls;
using Ibada.Core.Localization;

namespace Ibada.App;

/// <summary>Choix d'une sauvegarde à restaurer, avec confirmation. Fenêtre construite en code.</summary>
public sealed class RestoreDialog : Window
{
    private readonly ListBox _list = new() { MinHeight = 180 };
    private readonly Localizer _t;
    public string? Chosen { get; private set; }

    public RestoreDialog(IReadOnlyList<string> backups, Localizer text)
    {
        _t = text;
        Title = text.T("dialog.restore.title");
        Width = 460; Height = 340;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        foreach (var b in backups.Reverse()) _list.Items.Add(b);
        _list.SelectedIndex = 0;

        var ok = new Button { Content = text.T("dialog.ok"), IsDefault = true, MinWidth = 90, Margin = new Thickness(0, 0, 8, 0) };
        var cancel = new Button { Content = text.T("dialog.cancel"), IsCancel = true, MinWidth = 90 };
        ok.Click += (_, _) => Confirm();
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
        buttons.Children.Add(ok); buttons.Children.Add(cancel);

        var panel = new DockPanel { Margin = new Thickness(12) };
        DockPanel.SetDock(buttons, Dock.Bottom);
        panel.Children.Add(buttons);
        panel.Children.Add(_list);
        Content = panel;
    }

    private void Confirm()
    {
        if (_list.SelectedItem is not string name) return;
        var answer = MessageBox.Show(this, _t.T("dialog.restore.confirm", name), Title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (answer != MessageBoxResult.Yes) return;
        Chosen = name;
        DialogResult = true;
    }
}
