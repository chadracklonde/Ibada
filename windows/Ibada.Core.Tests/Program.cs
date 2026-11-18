// Tests de la bibliothèque Ibada.Core. Programme console sans dépendance externe :
// code de sortie 0 si tout passe, 1 sinon. Lancer : dotnet run --project Ibada.Core.Tests
using System.IO.Compression;
using Ibada.Core.Input;
using Ibada.Core.Live;
using Ibada.Core.Localization;
using Ibada.Core.Rendering;
using Ibada.Core.Settings;
using Ibada.Core.Slides;
using Ibada.Core.Storage;
using Ibada.Core.Themes;

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


Console.WriteLine("HexColor et Theme");
Test("couleurs valides et invalides", () =>
{
    Equal(new HexColor(255, 255, 0, 0), HexColor.Parse("#FF0000")!.Value);
    Equal(new HexColor(128, 1, 2, 3), HexColor.Parse("#80010203")!.Value);
    foreach (var bad in new[] { "", "FF0000", "#FFF", "#GG0000", "#12345678A", null })
        Check(HexColor.Parse(bad) is null, $"« {bad} » devrait être refusée");
});
Test("thème invalide corrigé sans exception", () =>
{
    var t = new Theme { Font = new FontSpec { Size = -5, Color = "rouge", Family = " " }, Padding = double.NaN,
        Background = new BackgroundSpec { Type = BackgroundType.Color, Value = "pas une couleur" } }.Sanitized();
    Equal(72.0, t.Font.Size); Equal("#FFFFFF", t.Font.Color); Equal("Segoe UI", t.Font.Family);
    Equal(60.0, t.Padding); Equal("#000000", t.Background.Value);
});
Test("fond image : le chemin est conservé", () =>
{
    var t = new Theme { Background = new BackgroundSpec { Type = BackgroundType.Image, Value = @"D:\fonds\croix.jpg" } }.Sanitized();
    Equal(@"D:\fonds\croix.jpg", t.Background.Value);
});

Console.WriteLine("ThemeCatalog (THM-01, THM-02)");
Test("quatre thèmes fournis, thème inconnu : repli sur nuit", () =>
{
    var c = new ThemeCatalog();
    Equal(4, c.All().Count);
    Equal("nuit", c.Get("inconnu").Id); Equal("nuit", c.Get(null).Id);
    Equal("contraste", c.Get("contraste").Id);
    Check(c.Get("contraste").Outline.Enabled);
});
Test("tous les thèmes fournis sont valides", () =>
{
    foreach (var t in ThemeCatalog.BuiltIn) Equal(t, t.Sanitized());
});
Test("thème enregistré, rechargé avec accents et énumérations lisibles", () =>
{
    var st = NewStore();
    new ThemeCatalog(st).Save(new Theme { Id = "fete", Name = "Fête de Noël", Align = HAlign.Left, VAlign = VAlign.Top,
        Font = new FontSpec { Italic = true, Underline = true, Color = "#00FF00" } });
    var json = File.ReadAllText(st.Full("themes/fete.json"));
    Check(json.Contains("\"Left\"") && json.Contains("Fête"), "fichier de thème peu lisible");
    var back = new ThemeCatalog(st).Get("fete");
    Equal("Fête de Noël", back.Name); Equal(HAlign.Left, back.Align); Equal(VAlign.Top, back.VAlign);
    Check(back.Font.Italic && back.Font.Underline); Equal("#00FF00", back.Font.Color);
});
Test("THM-02 : modifier un thème change toutes les diapositives qui l'utilisent", () =>
{
    var c = new ThemeCatalog(NewStore());
    var slides = SlideBuilder.FromCustomText("Annonces", "A\n---\nB\n---\nC", "chaud");
    Equal(3, slides.Count);
    Check(slides.All(sl => c.Get(sl.ThemeId).Font.Color == "#FFF1D6"));
    c.Save(c.Get("chaud") with { Font = c.Get("chaud").Font with { Color = "#112233" } });
    Check(slides.All(sl => c.Get(sl.ThemeId).Font.Color == "#112233"), "toutes les diapositives doivent suivre la modification");
    Check(c.Get("nuit").Font.Color == "#FFFFFF", "les autres thèmes ne changent pas");
});
Test("un thème fourni modifié puis supprimé revient à l'origine", () =>
{
    var st = NewStore(); var c = new ThemeCatalog(st);
    c.Save(c.Get("clair") with { Padding = 10 });
    Equal(10.0, c.Get("clair").Padding); Equal(4, c.All().Count);
    Check(c.Delete("clair"));
    Equal(60.0, c.Get("clair").Padding); Check(!st.Exists("themes/clair.json"));
});
Test("identifiant dangereux : le fichier reste dans le dossier des thèmes", () =>
{
    var st = NewStore();
    new ThemeCatalog(st).Save(new Theme { Id = "../../evil", Name = "x" });
    Check(st.List("themes").Count == 1 && !File.Exists(Path.Combine(st.Dir, "..", "evil.json")));
});

Console.WriteLine("ThemeScaler (THM-03)");
Test("même mise en page de 1280×720 à 3840×2160", () =>
{
    var th = new ThemeCatalog().Get("contraste") with { Header = new BandSpec { Show = true, Size = 30 } };
    var a = ThemeScaler.Scale(th, 1280, 720);
    var b = ThemeScaler.Scale(th, 3840, 2160);
    Equal(3.0, Math.Round(b.FontSizePx / a.FontSizePx, 9));
    Equal(3.0, Math.Round(b.PaddingPx / a.PaddingPx, 9));
    Equal(3.0, Math.Round(b.OutlineWidthPx / a.OutlineWidthPx, 9));
    Equal(3.0, Math.Round(b.TextArea.Height / a.TextArea.Height, 9));
    Equal(3.0, Math.Round(b.TextArea.Width / a.TextArea.Width, 9));
    // Part de la hauteur occupée par le texte : identique à toute résolution.
    Equal(Math.Round(a.TextArea.Height / a.Height, 9), Math.Round(b.TextArea.Height / b.Height, 9));
});
Test("référence 1080 : taille inchangée ; 720 donne les deux tiers", () =>
{
    var th = new Theme();
    Equal(72.0, ThemeScaler.Scale(th, 1920, 1080).FontSizePx);
    Equal(48.0, ThemeScaler.Scale(th, 1280, 720).FontSizePx);
});
Test("zones de texte, en-tête et pied sans chevauchement", () =>
{
    var th = new Theme { Header = new BandSpec { Show = true, Size = 30 } };
    foreach (var (w, h) in new[] { (1280, 720), (1920, 1080), (3840, 2160), (1024, 768) })
    {
        var s = ThemeScaler.Scale(th, w, h);
        Check(s.HeaderArea.Y + s.HeaderArea.Height <= s.TextArea.Y + 1e-9, $"en-tête/texte {w}x{h}");
        Check(s.TextArea.Y + s.TextArea.Height <= s.FooterArea.Y + 1e-9, $"texte/pied {w}x{h}");
        Check(s.FooterArea.Y + s.FooterArea.Height <= h + 1e-9, $"pied dans l'écran {w}x{h}");
        Check(s.TextArea.Width > 0 && s.TextArea.Height > 0);
    }
});
Test("ombre et contour désactivés : valeurs nulles ; résolution invalide refusée", () =>
{
    var s = ThemeScaler.Scale(new Theme { Shadow = new ShadowSpec { Enabled = false } }, 1920, 1080);
    Equal(0.0, s.ShadowBlur); Equal(0.0, s.OutlineWidthPx);
    Throws<ArgumentOutOfRangeException>(() => ThemeScaler.Scale(new Theme(), 0, 1080));
});

Console.WriteLine("SlideBuilder");
Test("découpage équilibré des lignes", () =>
{
    Equal("a\nb\nc|d\ne", string.Join("|", SlideBuilder.ChunkLines("a\nb\nc\nd\ne", 4)));
    Equal("a\nb\nc\nd", string.Join("|", SlideBuilder.ChunkLines("a\nb\nc\nd", 4)));
    Equal("a|b", string.Join("|", SlideBuilder.ChunkLines("a\nb", 1)));
    Equal(0, SlideBuilder.ChunkLines("  \n\n", 4).Count);
    Throws<ArgumentOutOfRangeException>(() => SlideBuilder.ChunkLines("a", 0));
});
Test("fins de ligne Windows et lignes vides aux extrémités", () =>
{
    Equal("a\nb", string.Join("|", SlideBuilder.ChunkLines("\r\n\r\na\r\nb\r\n\r\n", 4)));
});
Test("diapositives personnalisées séparées par ---", () =>
{
    var sl = SlideBuilder.FromCustomText("Accueil", "Bienvenue\r\nà tous\r\n---\n\n---\nPriorités\n  ---  \nFin");
    Equal(3, sl.Count);
    Equal("Bienvenue\nà tous", sl[0].Text); Equal("Priorités", sl[1].Text); Equal("Fin", sl[2].Text);
    Check(sl.All(x => x.Kind == SlideKind.Custom && x.Label == "Accueil"));
    Check(sl.Select(x => x.Id).Distinct().Count() == 3, "identifiants distincts");
});
Test("diapositive image", () =>
{
    var sl = SlideBuilder.FromImage("Logo", @"C:\img\a.png");
    Equal(SlideKind.Image, sl.Kind); Equal(@"C:\img\a.png", sl.ImagePath);
});

Console.WriteLine("LiveController (PRJ-02, PRJ-03)");
LiveController Deck(int n)
{
    var c = new LiveController();
    c.Load(Enumerable.Range(1, n).Select(i => new Slide { Text = "S" + i }));
    return c;
}
Test("au chargement : projection noire, aperçu sur la première", () =>
{
    var c = Deck(3);
    Equal(LiveMode.Black, c.Projection.Mode); Check(c.Projection.Slide is null);
    Equal("S1", c.PreviewSlide!.Text);
});
Test("sélectionner ne change pas la projection", () =>
{
    var c = Deck(3);
    c.GoLive(); c.Select(2);
    Equal("S3", c.PreviewSlide!.Text); Equal("S1", c.Projection.Slide!.Text);
    Check(!c.Select(3) && !c.Select(-1)); Equal(2, c.Selected);
});
Test("projeter la sélection", () =>
{
    var c = Deck(3);
    c.Select(1); Check(c.GoLive());
    Equal(LiveMode.Slide, c.Projection.Mode); Equal("S2", c.Projection.Slide!.Text); Equal(1, c.Projection.Index);
});
Test("suivant et précédent avancent le direct et la sélection, avec limites", () =>
{
    var c = Deck(3);
    c.Next();                                  // depuis le noir : projette la sélection (S1)
    Equal("S1", c.Projection.Slide!.Text);
    c.Next(); c.Next();
    Equal("S3", c.Projection.Slide!.Text); Equal(2, c.Selected);
    Check(!c.Next(), "pas de diapositive après la dernière");
    Equal("S3", c.Projection.Slide!.Text);
    c.Previous(); c.Previous();
    Equal("S1", c.Projection.Slide!.Text); Check(!c.Previous());
});
Test("depuis le logo, suivant projette la sélection et ne saute pas", () =>
{
    var c = Deck(3);
    c.GoLive(); c.Next();                      // direct sur S2
    c.SetMode(LiveMode.Logo);
    Equal(LiveMode.Logo, c.Projection.Mode);
    c.Select(2); c.Next();                     // reprend la sélection : S3
    Equal("S3", c.Projection.Slide!.Text);
});
Test("noir deux fois : retour à la diapositive", () =>
{
    var c = Deck(2);
    c.GoLive();
    c.SetMode(LiveMode.Black); Equal(LiveMode.Black, c.Projection.Mode); Check(c.Projection.Slide is null);
    c.SetMode(LiveMode.Black); Equal(LiveMode.Slide, c.Projection.Mode); Equal("S1", c.Projection.Slide!.Text);
    c.SetMode(LiveMode.Clear); Equal(LiveMode.Clear, c.Projection.Mode);
    c.SetMode(LiveMode.Slide); Equal("S1", c.Projection.Slide!.Text);
});
Test("noir sans rien au direct : reste noir", () =>
{
    var c = Deck(2);
    c.SetMode(LiveMode.Black); Equal(LiveMode.Black, c.Projection.Mode);
    c.SetMode(LiveMode.Black); Equal(LiveMode.Black, c.Projection.Mode);
});
Test("paquet vide : aucune erreur, projection noire", () =>
{
    var c = Deck(0);
    Check(!c.GoLive() && !c.Next() && !c.Previous() && !c.Select(0));
    Check(c.PreviewSlide is null); Equal(LiveMode.Black, c.Projection.Mode);
    c.SetMode(LiveMode.Slide); Equal(LiveMode.Black, c.Projection.Mode);
});
Test("recharger remet la projection au noir", () =>
{
    var c = Deck(3); c.GoLive();
    c.Load(new[] { new Slide { Text = "Nouveau" } });
    Equal(LiveMode.Black, c.Projection.Mode); Equal(0, c.Selected);
});
Test("commandes du clavier", () =>
{
    var c = Deck(3); var keys = new ShortcutMap();
    Check(c.Execute(keys.Resolve("Space")!.Value)); Equal("S1", c.Projection.Slide!.Text);
    c.Execute(keys.Resolve("Right")!.Value); Equal("S2", c.Projection.Slide!.Text);
    c.Execute(keys.Resolve("Left")!.Value); Equal("S1", c.Projection.Slide!.Text);
    c.Execute(keys.Resolve("B")!.Value); Equal(LiveMode.Black, c.Projection.Mode);
    c.Execute(keys.Resolve("L")!.Value); Equal(LiveMode.Logo, c.Projection.Mode);
    c.Execute(keys.Resolve("D")!.Value); Equal(LiveMode.Slide, c.Projection.Mode);
    Check(!c.Execute(Command.CreateBackup), "la sauvegarde ne concerne pas le direct");
});
Test("l'événement Changed est signalé à chaque changement", () =>
{
    var c = new LiveController(); var n = 0; c.Changed += () => n++;
    c.Load(new[] { new Slide(), new Slide() });
    c.Select(1); c.GoLive(); c.Previous(); c.SetMode(LiveMode.Logo);
    Equal(5, n);
});

Console.WriteLine();
Console.WriteLine(failures.Count == 0 ? $"{passed} tests réussis." : $"{failures.Count} échec(s) sur {passed + failures.Count} : {string.Join(", ", failures)}");
return failures.Count == 0 ? 0 : 1;
