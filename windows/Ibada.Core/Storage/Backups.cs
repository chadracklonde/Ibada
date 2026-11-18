using System.IO.Compression;

namespace Ibada.Core.Storage;

/// <summary>
/// Sauvegarde et restauration de toutes les données (bibliothèques, agendas, réglages) : ERG-03.
/// Les sauvegardes sont des archives ZIP rangées dans le sous-dossier « sauvegardes ».
/// </summary>
public static class Backups
{
    public const string FolderName = "sauvegardes";

    private static string BackupDir(JsonStore store)
    {
        var d = Path.Combine(store.Dir, FolderName);
        Directory.CreateDirectory(d);
        return d;
    }

    private static string Clean(string label)
    {
        var chars = label.Trim().Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' ? c : '-').ToArray();
        var s = new string(chars).Trim('-');
        return s.Length == 0 ? "manuelle" : s;
    }

    /// <summary>Crée une sauvegarde et renvoie son nom de fichier (sans dossier).</summary>
    public static string Create(JsonStore store, string label = "manuelle")
    {
        var dir = BackupDir(store);
        var baseName = $"{DateTime.Now:yyyyMMdd-HHmmss}-{Clean(label)}";
        var name = baseName + ".zip";
        for (var i = 2; File.Exists(Path.Combine(dir, name)); i++) name = $"{baseName}-{i}.zip";

        var tmp = Path.Combine(dir, name + ".tmp");
        using (var zip = ZipFile.Open(tmp, ZipArchiveMode.Create))
        {
            foreach (var file in Directory.EnumerateFiles(store.Dir, "*", SearchOption.AllDirectories))
            {
                var rel = Path.GetRelativePath(store.Dir, file).Replace('\\', '/');
                if (rel.StartsWith(FolderName + "/", StringComparison.Ordinal) || rel.EndsWith(".tmp", StringComparison.Ordinal))
                    continue;
                zip.CreateEntryFromFile(file, rel, CompressionLevel.Optimal);
            }
        }
        File.Move(tmp, Path.Combine(dir, name));
        return name;
    }

    /// <summary>Noms des sauvegardes, de la plus récente à la plus ancienne.</summary>
    public static IReadOnlyList<string> List(JsonStore store)
        => Directory.GetFiles(BackupDir(store), "*.zip")
            .Select(Path.GetFileName).OfType<string>()
            .OrderByDescending(n => n, StringComparer.Ordinal).ToList();

    /// <summary>
    /// Restaure une sauvegarde. Une sauvegarde « avant-restauration » est faite d'abord,
    /// pour pouvoir revenir en arrière. Renvoie le nom de cette sauvegarde de sécurité.
    /// </summary>
    public static string Restore(JsonStore store, string backupName)
    {
        if (backupName != Path.GetFileName(backupName) || !backupName.EndsWith(".zip", StringComparison.Ordinal))
            throw new ArgumentException("Nom de sauvegarde invalide.", nameof(backupName));
        var archive = Path.Combine(BackupDir(store), backupName);
        if (!File.Exists(archive)) throw new FileNotFoundException("Sauvegarde introuvable.", backupName);

        // Lire l'archive avant de toucher aux données : une archive illisible ne doit rien détruire.
        using var zip = ZipFile.OpenRead(archive);
        foreach (var entry in zip.Entries)
            if (!entry.FullName.EndsWith('/')) store.Full(entry.FullName); // refuse tout chemin qui sort du dossier, avant toute suppression
        var safety = Create(store, "avant-restauration");

        foreach (var file in Directory.EnumerateFiles(store.Dir, "*", SearchOption.AllDirectories).ToList())
        {
            var rel = Path.GetRelativePath(store.Dir, file).Replace('\\', '/');
            if (!rel.StartsWith(FolderName + "/", StringComparison.Ordinal)) File.Delete(file);
        }

        foreach (var entry in zip.Entries)
        {
            if (entry.FullName.EndsWith('/')) continue;
            var target = store.Full(entry.FullName); // refuse les chemins qui sortent du dossier
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            entry.ExtractToFile(target, overwrite: true);
        }
        return safety;
    }
}
