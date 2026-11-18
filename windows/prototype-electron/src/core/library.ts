// Bibliothèque : chants et Bibles d'Ibada, plus les sources externes en lecture seule (VideoPsalm, L3).
import type { Bible, Song, Songbook } from './types';
import { JsonStore } from './storage';
import { SongIndex, newSong } from './songs';
import { newId } from './text';

export const PERSO_ID = 'perso';

export class Library {
  private own: Songbook[] = [];
  private ownBibles: Bible[] = [];
  private extSongbooks: Songbook[] = [];
  private extBibles: Bible[] = [];
  private index = new SongIndex();
  private byId = new Map<string, Song>();

  constructor(private store: JsonStore) {}

  load(): void {
    this.own = this.store
      .list('bibliotheque/recueils')
      .map((f) => this.store.read<Songbook | null>(`bibliotheque/recueils/${f}`, null))
      .filter((x): x is Songbook => !!x && Array.isArray(x.songs));
    this.ownBibles = this.store
      .list('bibliotheque/bibles')
      .map((f) => this.store.read<Bible | null>(`bibliotheque/bibles/${f}`, null))
      .filter((x): x is Bible => !!x && Array.isArray(x.books));
    this.reindex();
  }

  /** Remplace les sources externes (lecture seule). */
  setExternal(songbooks: Songbook[], bibles: Bible[]): void {
    this.extSongbooks = songbooks;
    this.extBibles = bibles;
    this.reindex();
  }

  private reindex(): void {
    const songs: Song[] = [];
    this.byId.clear();
    for (const sb of [...this.own, ...this.extSongbooks]) {
      for (const s of sb.songs) {
        s.songbookId ??= sb.id;
        s.source ??= sb.source ?? 'ibada';
        songs.push(s);
        this.byId.set(s.id, s);
      }
    }
    this.index.rebuild(songs);
  }

  get songCount(): number {
    return this.index.size;
  }

  songbooks(): Array<{ id: string; name: string; count: number; source: string }> {
    return [...this.own, ...this.extSongbooks].map((b) => ({
      id: b.id,
      name: b.name,
      count: b.songs.length,
      source: b.source ?? 'ibada',
    }));
  }

  searchSongs(q: string, limit = 60): Song[] {
    return this.index.search(q, limit);
  }

  getSong(id: string): Song | undefined {
    return this.byId.get(id);
  }

  bibles(): Array<{ id: string; name: string; abbreviation: string; source: string }> {
    return [...this.ownBibles, ...this.extBibles].map((b) => ({
      id: b.id,
      name: b.name,
      abbreviation: b.abbreviation,
      source: b.source ?? 'ibada',
    }));
  }

  getBible(id: string): Bible | undefined {
    return [...this.ownBibles, ...this.extBibles].find((b) => b.id === id);
  }

  /** Enregistre un chant dans le recueil personnel d'Ibada. Les chants externes ne sont jamais modifiés. */
  saveSong(song: Song): Song {
    if (song.source === 'videopsalm') throw new Error('Un chant de la bibliothèque VideoPsalm est en lecture seule.');
    let perso = this.own.find((b) => b.id === PERSO_ID);
    if (!perso) {
      perso = { id: PERSO_ID, name: 'Mes chants', songs: [], source: 'ibada' };
      this.own.push(perso);
    }
    const complet = newSong({ ...song, songbookId: PERSO_ID, source: 'ibada' });
    const i = perso.songs.findIndex((s) => s.id === complet.id);
    if (i >= 0) perso.songs[i] = complet;
    else perso.songs.push(complet);
    this.store.write(`bibliotheque/recueils/${PERSO_ID}.json`, perso);
    this.reindex();
    return complet;
  }

  deleteSong(id: string): boolean {
    const perso = this.own.find((b) => b.id === PERSO_ID);
    if (!perso) return false;
    const n = perso.songs.length;
    perso.songs = perso.songs.filter((s) => s.id !== id);
    if (perso.songs.length === n) return false;
    this.store.write(`bibliotheque/recueils/${PERSO_ID}.json`, perso);
    this.reindex();
    return true;
  }

  importSongbook(sb: Songbook): void {
    sb.source = 'ibada';
    for (const s of sb.songs) {
      s.id ||= newId('song');
      s.songbookId = sb.id;
      s.source = 'ibada';
    }
    this.store.write(`bibliotheque/recueils/${sb.id}.json`, sb);
    this.own = this.own.filter((b) => b.id !== sb.id).concat(sb);
    this.reindex();
  }

  importBible(b: Bible): void {
    b.source = 'ibada';
    this.store.write(`bibliotheque/bibles/${b.id}.json`, b);
    this.ownBibles = this.ownBibles.filter((x) => x.id !== b.id).concat(b);
  }

  get isEmpty(): boolean {
    return this.own.length === 0 && this.ownBibles.length === 0;
  }
}
