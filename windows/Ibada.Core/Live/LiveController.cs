using Ibada.Core.Input;
using Ibada.Core.Slides;

namespace Ibada.Core.Live;

public enum LiveMode { Slide, Black, Logo, Clear }

/// <summary>Ce que la fenêtre de projection doit afficher à l'instant : un mode et, en mode diapositive, la diapositive.</summary>
public sealed record ProjectionState(LiveMode Mode, Slide? Slide, int Index);

/// <summary>
/// Aperçu et direct (PRJ-02, PRJ-03). L'opérateur sélectionne (aperçu) sans toucher à la projection ;
/// « projeter » envoie la sélection au direct ; suivant et précédent avancent le direct.
/// Au chargement et en cas de doute, la projection est à l'écran noir : jamais d'élément affiché par surprise.
/// </summary>
public sealed class LiveController
{
    private List<Slide> _deck = new();
    private int _live = -1;

    public IReadOnlyList<Slide> Deck => _deck;
    public int Selected { get; private set; }
    public LiveMode Mode { get; private set; } = LiveMode.Black;

    public event Action? Changed;

    public Slide? PreviewSlide => Selected >= 0 && Selected < _deck.Count ? _deck[Selected] : null;

    public ProjectionState Projection
        => Mode == LiveMode.Slide && _live >= 0 && _live < _deck.Count
            ? new ProjectionState(LiveMode.Slide, _deck[_live], _live)
            : new ProjectionState(Mode == LiveMode.Slide ? LiveMode.Black : Mode, null, -1);

    /// <summary>Remplace les diapositives. La projection passe à l'écran noir.</summary>
    public void Load(IEnumerable<Slide> slides)
    {
        _deck = slides.ToList();
        Selected = 0;
        _live = -1;
        Mode = LiveMode.Black;
        Changed?.Invoke();
    }

    public bool Select(int index)
    {
        if (index < 0 || index >= _deck.Count) return false;
        Selected = index;
        Changed?.Invoke();
        return true;
    }

    public bool SelectNext() => Select(Selected + 1);
    public bool SelectPrevious() => Select(Selected - 1);

    /// <summary>Envoie la diapositive sélectionnée à la projection.</summary>
    public bool GoLive()
    {
        if (PreviewSlide is null) return false;
        _live = Selected;
        Mode = LiveMode.Slide;
        Changed?.Invoke();
        return true;
    }

    /// <summary>Diapositive suivante au direct. Depuis le noir, le logo ou le vide, projette la sélection.</summary>
    public bool Next() => Step(+1);

    public bool Previous() => Step(-1);

    private bool Step(int delta)
    {
        if (Mode != LiveMode.Slide || _live < 0) return GoLive();
        var n = _live + delta;
        if (n < 0 || n >= _deck.Count) return false;
        _live = n;
        Selected = n;
        Changed?.Invoke();
        return true;
    }

    /// <summary>
    /// Change le mode de la projection. Redemander le mode déjà actif revient à la diapositive
    /// (la touche « Noir » deux fois : noir puis retour), sauf s'il n'y a pas encore de diapositive au direct.
    /// </summary>
    public void SetMode(LiveMode mode)
    {
        if (mode == LiveMode.Slide)
        {
            if (_live >= 0 && _live < _deck.Count) { Mode = LiveMode.Slide; Changed?.Invoke(); }
            else GoLive();
            return;
        }
        if (Mode == mode && _live >= 0 && _live < _deck.Count) Mode = LiveMode.Slide;
        else Mode = mode;
        Changed?.Invoke();
    }

    /// <summary>Exécute une commande du clavier. Renvoie false si elle ne concerne pas le direct.</summary>
    public bool Execute(Command command)
    {
        switch (command)
        {
            case Command.NextSlide: Next(); return true;
            case Command.PreviousSlide: Previous(); return true;
            case Command.GoLive: GoLive(); return true;
            case Command.ShowSlide: SetMode(LiveMode.Slide); return true;
            case Command.BlackScreen: SetMode(LiveMode.Black); return true;
            case Command.Logo: SetMode(LiveMode.Logo); return true;
            case Command.ClearText: SetMode(LiveMode.Clear); return true;
            default: return false;
        }
    }
}
