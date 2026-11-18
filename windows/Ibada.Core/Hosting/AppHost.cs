using Ibada.Core.Input;
using Ibada.Core.Live;
using Ibada.Core.Localization;
using Ibada.Core.Settings;
using Ibada.Core.Slides;
using Ibada.Core.Storage;
using Ibada.Core.Themes;

namespace Ibada.Core.Hosting;

/// <summary>
/// Point d'entrée de la logique : réunit stockage, réglages, thèmes, textes, raccourcis et direct.
/// L'interface WPF ne contient que de l'affichage ; tout le reste passe par cet objet, donc se teste sans Windows.
/// </summary>
public sealed class AppHost
{
    public JsonStore Store { get; }
    public AppSettings Settings { get; private set; } = new();
    public ThemeCatalog Themes { get; private set; }
    public Localizer Text { get; private set; } = new();
    public ShortcutMap Shortcuts { get; private set; } = new();
    public LiveController Live { get; } = new();

    public AppHost(string dataDir)
    {
        Store = new JsonStore(dataDir);
        Themes = new ThemeCatalog(Store);
        Reload();
    }

    /// <summary>Dossier des données : la variable IBADA_DATA si elle existe, sinon « Ibada » dans AppData.</summary>
    public static string DefaultDataDir()
    {
        var custom = Environment.GetEnvironmentVariable("IBADA_DATA");
        return !string.IsNullOrWhiteSpace(custom)
            ? custom
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ibada");
    }

    /// <summary>Relit réglages, thèmes, textes et raccourcis depuis le disque (après une restauration, par exemple).</summary>
    public void Reload()
    {
        Settings = AppSettings.Load(Store);
        Themes = new ThemeCatalog(Store);
        Shortcuts = ShortcutMap.FromSettings(Settings.Shortcuts);
        Text = new Localizer(Settings.Language, LoadTranslation(Settings.Language));
    }

    private Dictionary<string, string>? LoadTranslation(string language)
        => language == "fr" ? null : Store.Read<Dictionary<string, string>?>($"langues/{language}.json", null);

    public void SaveSettings()
    {
        Settings.Shortcuts = Shortcuts.ToSettings();
        Settings.Save(Store);
    }

    /// <summary>Thème d'une diapositive : celui qu'elle désigne, sinon le thème choisi dans les réglages.</summary>
    public Theme ThemeFor(Slide? slide) => Themes.Get(slide?.ThemeId ?? Settings.ThemeId);

    public void SetTheme(string themeId)
    {
        Settings.ThemeId = Themes.Get(themeId).Id;
        SaveSettings();
    }

    public void SetProjectionDisplay(int? index)
    {
        Settings.ProjectionDisplay = index;
        SaveSettings();
    }

    public string CreateBackup(string label = "manuelle") => Backups.Create(Store, label);

    /// <summary>Restaure une sauvegarde puis relit les données. Renvoie le nom de la copie de sécurité.</summary>
    public string Restore(string backupName)
    {
        var safety = Backups.Restore(Store, backupName);
        Reload();
        return safety;
    }

    public void LoadSample() => Live.Load(SampleDeck.Build());

    public void LogError(string context, Exception e) => ErrorLog.Write(Store, context, e);

    /// <summary>
    /// Traite un geste du clavier. Renvoie la commande exécutée, ou null si le geste ne correspond à rien
    /// (la fenêtre laisse alors la touche à Windows). La sauvegarde est renvoyée sans être exécutée :
    /// elle demande un retour à l'écran, donc c'est la fenêtre qui s'en charge.
    /// </summary>
    public Command? HandleGesture(string gesture)
    {
        var command = Shortcuts.Resolve(gesture);
        if (command is null) return null;
        Live.Execute(command.Value);
        return command;
    }
}
