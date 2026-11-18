using System.Text.Encodings.Web;
using System.Text.Json;

namespace Ibada.Core.Storage;

/// <summary>
/// Stockage en fichiers JSON dans un dossier de données (cahier : section 5, « Données » ; ERG-03).
/// L'écriture est atomique : un fichier temporaire est écrit puis renommé, si bien qu'une coupure
/// de courant ne laisse jamais un fichier tronqué.
/// </summary>
public sealed class JsonStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // accents lisibles dans les fichiers
    };

    public string Dir { get; }

    public JsonStore(string dir)
    {
        Dir = Path.GetFullPath(dir);
        Directory.CreateDirectory(Dir);
    }

    /// <summary>Chemin complet d'un fichier du dépôt de données ; refuse toute sortie du dossier.</summary>
    public string Full(string relative)
    {
        var p = Path.GetFullPath(Path.Combine(Dir, relative));
        if (p != Dir && !p.StartsWith(Dir + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new InvalidOperationException($"Chemin hors du dossier de données : {relative}");
        return p;
    }

    public bool Exists(string relative) => File.Exists(Full(relative));

    /// <summary>Lit un fichier ; renvoie <paramref name="fallback"/> s'il manque ou s'il est illisible.</summary>
    public T Read<T>(string relative, T fallback)
    {
        try
        {
            var text = File.ReadAllText(Full(relative));
            return JsonSerializer.Deserialize<T>(text, Options) ?? fallback;
        }
        catch (Exception e) when (e is IOException or JsonException or UnauthorizedAccessException)
        {
            return fallback;
        }
    }

    public void Write<T>(string relative, T data)
    {
        var path = Full(relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var tmp = path + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(data, Options));
        File.Move(tmp, path, overwrite: true);
    }

    /// <summary>Noms des fichiers d'un sous-dossier, triés.</summary>
    public IReadOnlyList<string> List(string relativeDir, string extension = ".json")
    {
        var dir = Full(relativeDir);
        if (!Directory.Exists(dir)) return Array.Empty<string>();
        return Directory.GetFiles(dir, "*" + extension)
            .Select(Path.GetFileName)
            .OfType<string>()
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();
    }

    public bool Remove(string relative)
    {
        var p = Full(relative);
        if (!File.Exists(p)) return false;
        File.Delete(p);
        return true;
    }
}
