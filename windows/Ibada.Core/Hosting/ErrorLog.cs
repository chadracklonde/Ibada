using System.Text;
using Ibada.Core.Storage;

namespace Ibada.Core.Hosting;

/// <summary>
/// Journal d'erreurs lisible par l'administrateur (cahier, section 5, « Maintenabilité »).
/// Écrire dans le journal ne doit jamais provoquer d'erreur : en cas d'échec, on se tait.
/// </summary>
public static class ErrorLog
{
    public const string RelativePath = Backups.JournalFolderName + "/erreurs.log";
    private const long MaxBytes = 512 * 1024;

    public static void Write(JsonStore store, string context, Exception exception)
    {
        try
        {
            var path = store.Full(RelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            if (File.Exists(path) && new FileInfo(path).Length > MaxBytes)
                File.Move(path, path + ".old", overwrite: true);
            var sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString("O")).Append(" [").Append(context).AppendLine("]");
            sb.AppendLine(exception.ToString());
            sb.AppendLine();
            File.AppendAllText(path, sb.ToString(), Encoding.UTF8);
        }
        catch
        {
            // Le journal ne doit jamais faire planter l'application.
        }
    }
}
