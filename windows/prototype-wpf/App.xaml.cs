using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace IbadaPrototype;

public partial class App : Application
{
    public static readonly Stopwatch StartupWatch = Stopwatch.StartNew();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        StartupWatch.Stop();
        try
        {
            File.AppendAllText(
                Path.Combine(AppContext.BaseDirectory, "startup-log.txt"),
                $"{DateTime.Now:O} démarrage en {StartupWatch.ElapsedMilliseconds} ms{Environment.NewLine}");
        }
        catch { /* ne jamais faire échouer le démarrage pour un log */ }
    }
}
