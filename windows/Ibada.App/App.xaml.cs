using System.Windows;
using Ibada.Core.Hosting;

namespace Ibada.App;

public partial class App : Application
{
    public static AppHost Host { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Host = new AppHost(AppHost.DefaultDataDir());

        // Une erreur inattendue est écrite dans le journal et ne ferme jamais l'application en plein culte.
        DispatcherUnhandledException += (_, args) =>
        {
            Host.LogError("interface", args.Exception);
            args.Handled = true;
        };
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception ex) Host.LogError("fatal", ex);
        };

        Host.LoadSample();
        new MainWindow().Show();
    }
}
