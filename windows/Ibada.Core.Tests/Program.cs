// Tests de la bibliothèque Ibada.Core. Programme console sans dépendance externe :
// code de sortie 0 si tout passe, 1 sinon. Lancer : dotnet run --project Ibada.Core.Tests
using System.IO.Compression;
using Ibada.Core.Input;
using Ibada.Core.Localization;
using Ibada.Core.Settings;
using Ibada.Core.Storage;

var failures = new List<string>();
var passed = 0;

void Test(string name, Action body)
{
    try { body(); passed++; Console.WriteLine($"  ok   {name}"); }
    catch (Exception e) { failures.Add(name); Console.WriteLine($"  ÉCHEC {name} : {e.Message}"); }
}
void Check(bool condition, string message = "condition fausse") { if (!condition) throw new Exception(message); }
void Equal<T>(T expected, T actual) { if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new Exception($"attendu « {expected} », obtenu « {actual} »"); }
void Throws<TEx>(Action a) where TEx : Exception
{
    try { a(); } catch (TEx) { return; }
    throw new Exception($"{typeof(TEx).Name} attendue");
}

JsonStore NewStore()
{
    var d = Path.Combine(Path.GetTempPath(), "ibada-test-" + Guid.NewGuid().ToString("N"));
    return new JsonStore(d);
}

Console.WriteLine("JsonStore");
Test("écriture puis lecture, accents conservés", () =>
{
    var s = NewStore();
    s.Write("chants/a.json", new Dictionary<string, string> { ["titre"] = "Grâce infinie — Éternel" });
    Equal("Grâce infinie — Éternel", s.Read("chants/a.json", new Dictionary<string, string>())["titre"]);
    Check(File.ReadAllText(s.Full("chants/a.json")).Contains("Grâce"), "accents non lisibles dans le fichier");
});
Test("aucun fichier temporaire laissé après écriture", () =>
{
    var s = NewStore();
    s.Write("x.json", new[] { 1, 2, 3 });
    s.Write("x.json", new[] { 4 });
    Check(!File.Exists(s.Full("x.json.tmp")));
    Equal(1, s.Read("x.json", new int[0]).Length);
});
Test("fichier absent ou corrompu : valeur de secours", () =>
{
    var s = NewStore();
    Equal("secours", s.Read("absent.json", "secours"));
    File.WriteAllText(s.Full("casse.json"), "{ pas du json");
    Equal("secours", s.Read("casse.json", "secours"));
});
Test("chemin hors du dossier refusé", () =>
{
    var s = NewStore();
    Throws<InvalidOperationException>(() => s.Write("../dehors.json", 1));
    Throws<InvalidOperationException>(() => s.Read("../../etc/passwd", ""));
});
Test("liste triée et suppression", () =>
{
    var s = NewStore();
    s.Write("d/b.json", 1); s.Write("d/a.json", 1);
    Equal("a.json,b.json", string.Join(",", s.List("d")));
    Check(s.Remove("d/a.json")); Check(!s.Remove("d/a.json"));
    Equal(0, s.List("inexistant").Count);
});

Console.WriteLine("Backups");
Test("la sauvegarde contient les données mais pas les sauvegardes", () =>
{
    var s = NewStore();
    s.Write("reglages.json", new AppSettings { ChurchName = "Église de Kindu" });
    s.Write("chants/a.json", "x");
    var b1 = Backups.Create(s, "test");
    var b2 = Backups.Create(s, "test");
    Check(b1 != b2, "deux sauvegardes dans la même seconde doivent avoir des noms distincts");
    using var zip = ZipFile.OpenRead(Path.Combine(s.Dir, "sauvegardes", b2));
    var names = zip.Entries.Select(e => e.FullName).OrderBy(n => n).ToList();
    Equal("chants/a.json,reglages.json", string.Join(",", names));
    Equal(2, Backups.List(s).Count);
});
Test("restauration : fichiers modifiés, supprimés et ajoutés", () =>
{
    var s = NewStore();
    s.Write("chants/a.json", "version 1");
    s.Write("reglages.json", "R1");
    var b = Backups.Create(s, "avant");
    s.Write("chants/a.json", "version 2");   // modifié
    s.Remove("reglages.json");                // supprimé
    s.Write("chants/nouveau.json", "N");      // ajouté après la sauvegarde
    var safety = Backups.Restore(s, b);
    Equal("version 1", s.Read("chants/a.json", ""));
    Equal("R1", s.Read("reglages.json", ""));
    Check(!s.Exists("chants/nouveau.json"), "un fichier créé après la sauvegarde doit disparaître");
    Check(safety.Contains("avant-restauration"));
    Check(Backups.List(s).Contains(safety));
});
Test("on peut annuler une restauration avec la copie de sécurité", () =>
{
    var s = NewStore();
    s.Write("a.json", "ancien");
    var b = Backups.Create(s, "ancienne");
    s.Write("a.json", "récent");
    var safety = Backups.Restore(s, b);
    Equal("ancien", s.Read("a.json", ""));
    Backups.Restore(s, safety);
    Equal("récent", s.Read("a.json", ""));
});
Test("nom de sauvegarde invalide ou inconnu refusé", () =>
{
    var s = NewStore();
    Throws<ArgumentException>(() => Backups.Restore(s, "../x.zip"));
    Throws<ArgumentException>(() => Backups.Restore(s, "sauvegarde.txt"));
    Throws<FileNotFoundException>(() => Backups.Restore(s, "inconnue.zip"));
});
Test("archive piégée (chemin ../) refusée sans rien détruire", () =>
{
    var s = NewStore();
    s.Write("a.json", "précieux");
    var dir = Path.Combine(s.Dir, "sauvegardes");
    Directory.CreateDirectory(dir);
    using (var z = ZipFile.Open(Path.Combine(dir, "piege.zip"), ZipArchiveMode.Create))
    {
        var e = z.CreateEntry("../dehors.txt");
        using var w = new StreamWriter(e.Open()); w.Write("mauvais");
    }
    Throws<InvalidOperationException>(() => Backups.Restore(s, "piege.zip"));
    Equal("précieux", s.Read("a.json", ""));
    Check(!File.Exists(Path.Combine(s.Dir, "..", "dehors.txt")));
});
Test("étiquette nettoyée dans le nom du fichier", () =>
{
    var s = NewStore();
    var n = Backups.Create(s, "a/b\\c: d");
    Check(!n.Contains('/') && !n.Contains('\\') && !n.Contains(':'));
    Check(Backups.Create(s, "   ").Contains("manuelle"));
});

Console.WriteLine("Localizer");
Test("français par défaut, clé inconnue renvoyée telle quelle", () =>
{
    var l = new Localizer();
    Equal("Écran noir", l.T("mode.black"));
    Equal("cle.inconnue", l.T("cle.inconnue"));
});
Test("traduction partielle complétée par le français", () =>
{
    var l = new Localizer("sw", new Dictionary<string, string> { ["mode.black"] = "Skrini nyeusi" });
    Equal("Skrini nyeusi", l.T("mode.black"));
    Equal("Logo", l.T("mode.logo"));
});
Test("paramètres de format, tolérant aux erreurs", () =>
{
    var l = new Localizer();
    Equal("Sauvegarde créée : x.zip", l.T("backup.done", "x.zip"));
    Equal("Écran noir", l.T("mode.black", "inutile"));
    var cassee = new Localizer("xx", new Dictionary<string, string> { ["backup.done"] = "{0 {1}" });
    Equal("{0 {1}", cassee.T("backup.done", "a"));
});
Test("clés manquantes d'une traduction", () =>
{
    var missing = Localizer.MissingKeys(new Dictionary<string, string> { ["mode.black"] = "x", ["mode.logo"] = " " });
    Check(!missing.Contains("mode.black")); Check(missing.Contains("mode.logo")); Check(missing.Contains("app.title"));
});

Console.WriteLine("AppSettings");
Test("valeurs par défaut au premier démarrage", () =>
{
    var s = AppSettings.Load(NewStore());
    Equal("fr", s.Language); Equal("nuit", s.ThemeId); Equal(4, s.SongMaxLines); Equal(260, s.BibleMaxChars);
    Check(s.ProjectionDisplay is null);
});
Test("enregistrement et relecture", () =>
{
    var st = NewStore();
    new AppSettings { ChurchName = "Église méthodiste", ProjectionDisplay = 1, VideoPsalmFolder = @"D:\VideoPsalm" }.Save(st);
    var s = AppSettings.Load(st);
    Equal("Église méthodiste", s.ChurchName); Equal(1, s.ProjectionDisplay); Equal(@"D:\VideoPsalm", s.VideoPsalmFolder);
});
Test("valeurs hors limites corrigées", () =>
{
    var st = NewStore();
    st.Write(AppSettings.FileName, new AppSettings { SongMaxLines = 99, BibleMaxChars = 5, Language = " ", ThemeId = "" });
    var s = AppSettings.Load(st);
    Equal(4, s.SongMaxLines); Equal(260, s.BibleMaxChars); Equal("fr", s.Language); Equal("nuit", s.ThemeId);
});
Test("fichier de réglages corrompu : valeurs par défaut", () =>
{
    var st = NewStore();
    File.WriteAllText(st.Full(AppSettings.FileName), "[[[");
    Equal("nuit", AppSettings.Load(st).ThemeId);
});

Console.WriteLine("ShortcutMap");
Test("gestes normalisés", () =>
{
    Equal("Ctrl+Shift+X", ShortcutMap.Normalize("shift + ctrl + x"));
    Equal("Space", ShortcutMap.Normalize("space"));
    Equal("PageDown", ShortcutMap.Normalize("PAGEDOWN"));
    Equal("", ShortcutMap.Normalize("Ctrl+"));
});
Test("raccourcis par défaut", () =>
{
    var m = new ShortcutMap();
    Equal(Command.NextSlide, m.Resolve("space")); Equal(Command.NextSlide, m.Resolve("Right"));
    Equal(Command.PreviousSlide, m.Resolve("Left")); Equal(Command.BlackScreen, m.Resolve("b"));
    Equal(Command.CreateBackup, m.Resolve("control+s"));
    Check(m.Resolve("F12") is null);
});
Test("modification : le geste réaffecté ne sert plus à l'ancienne commande", () =>
{
    var m = new ShortcutMap();
    m.Bind("B", Command.Logo);
    Equal(Command.Logo, m.Resolve("B"));
    Check(!m.GesturesFor(Command.BlackScreen).Contains("B"));
    Check(m.Unbind("Space")); Check(m.Resolve("Space") is null);
    Throws<ArgumentException>(() => m.Bind("Ctrl+", Command.Logo));
});
Test("enregistrement dans les réglages et rechargement", () =>
{
    var m = new ShortcutMap();
    m.Bind("F5", Command.GoLive); m.Unbind("Enter");
    var back = ShortcutMap.FromSettings(m.ToSettings());
    Equal(Command.GoLive, back.Resolve("F5")); Check(back.Resolve("Enter") is null);
    Equal(Command.NextSlide, back.Resolve("Space"));
});
Test("réglages vides : défauts ; entrées inconnues ignorées", () =>
{
    Equal(Command.NextSlide, ShortcutMap.FromSettings(null).Resolve("Space"));
    Equal(Command.NextSlide, ShortcutMap.FromSettings(new Dictionary<string, string>()).Resolve("Space"));
    var m = ShortcutMap.FromSettings(new Dictionary<string, string> { ["X"] = "CommandeDisparue", [""] = "Logo", ["Y"] = "Logo" });
    Check(m.Resolve("X") is null); Equal(Command.Logo, m.Resolve("Y"));
});
Test("les raccourcis survivent à l'enregistrement du fichier de réglages", () =>
{
    var st = NewStore();
    var m = new ShortcutMap(); m.Bind("F6", Command.ClearText);
    new AppSettings { Shortcuts = m.ToSettings() }.Save(st);
    Equal(Command.ClearText, ShortcutMap.FromSettings(AppSettings.Load(st).Shortcuts).Resolve("F6"));
});

Console.WriteLine();
Console.WriteLine(failures.Count == 0 ? $"{passed} tests réussis." : $"{failures.Count} échec(s) sur {passed + failures.Count} : {string.Join(", ", failures)}");
return failures.Count == 0 ? 0 : 1;
