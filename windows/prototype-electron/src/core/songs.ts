// Chants : texte d'édition, recherche instantanée (CHT-01, CHT-02, CHT-03).
import type { Song, SongSection, SectionType } from './types';
import { normalize, newId } from './text';

const TYPES: Array<[RegExp, SectionType]> = [
  [/^refrain/i, 'chorus'],
  [/^(couplet|strophe|verset)/i, 'verse'],
  [/^pont/i, 'bridge'],
  [/^intro/i, 'intro'],
  [/^(fin|outro|conclusion)/i, 'outro'],
];

export function sectionTypeFromLabel(label: string): SectionType {
  for (const [re, t] of TYPES) if (re.test(label.trim())) return t;
  return 'other';
}

/**
 * Convertit le texte d'édition d'un chant en sections.
 * Format : sections séparées par une ligne vide ; une première ligne « [Refrain] » nomme la section.
 */
export function parseSongText(text: string): SongSection[] {
  const blocs = text.replace(/\r\n?/g, '\n').split(/\n\s*\n/);
  const sections: SongSection[] = [];
  let numeroCouplet = 0;
  for (const bloc of blocs) {
    const lignes = bloc.split('\n').map((l) => l.replace(/\s+$/, ''));
    while (lignes.length && lignes[0].trim() === '') lignes.shift();
    while (lignes.length && lignes[lignes.length - 1].trim() === '') lignes.pop();
    if (!lignes.length) continue;
    let label = '';
    const m = /^\[(.+)\]\s*$/.exec(lignes[0].trim());
    if (m) {
      label = m[1].trim();
      lignes.shift();
    }
    if (!lignes.length) continue;
    if (!label) {
      numeroCouplet += 1;
      label = `Couplet ${numeroCouplet}`;
    } else if (/^couplet/i.test(label)) {
      numeroCouplet += 1;
    }
    sections.push({ label, type: sectionTypeFromLabel(label), text: lignes.join('\n') });
  }
  return sections;
}

export function songToText(sections: SongSection[]): string {
  return sections.map((s) => `[${s.label}]\n${s.text}`).join('\n\n');
}

export function newSong(partial: Partial<Song> & { title: string }): Song {
  return {
    id: partial.id ?? newId('song'),
    sections: [],
    source: 'ibada',
    ...partial,
  };
}

/** Ordre des sections à afficher, selon l'arrangement choisi (CHT-03). */
export function sectionOrder(song: Song, arrangement?: string): SongSection[] {
  const nom = arrangement ?? song.defaultArrangement;
  const ordre = nom && song.arrangements ? song.arrangements[nom] : undefined;
  if (!ordre || !ordre.length) return song.sections;
  const parLabel = new Map(song.sections.map((s) => [normalize(s.label), s]));
  const resultat: SongSection[] = [];
  for (const label of ordre) {
    const s = parLabel.get(normalize(label));
    if (s) resultat.push(s);
  }
  return resultat.length ? resultat : song.sections;
}

interface Entry {
  song: Song;
  title: string;
  meta: string;
  lyrics: string;
  number: string;
}

/** Index de recherche instantanée sur titres, paroles, auteurs, compositeurs, numéros et CCLI. */
export class SongIndex {
  private entries: Entry[] = [];

  constructor(songs: Song[] = []) {
    this.rebuild(songs);
  }

  rebuild(songs: Song[]): void {
    this.entries = songs.map((song) => ({
      song,
      title: normalize(song.title),
      meta: normalize([song.author, song.composer, song.ccli, song.copyright].filter(Boolean).join(' ')),
      lyrics: normalize(song.sections.map((s) => s.text).join(' ')),
      number: song.number !== undefined ? String(song.number) : '',
    }));
  }

  get size(): number {
    return this.entries.length;
  }

  search(query: string, limit = 50): Song[] {
    const q = normalize(query);
    if (!q) return this.entries.slice(0, limit).map((e) => e.song);
    const mots = q.split(' ');
    const scored: Array<{ song: Song; score: number }> = [];
    for (const e of this.entries) {
      let score = 0;
      let ok = true;
      for (const mot of mots) {
        if (e.number && e.number === mot) score += 0;
        else if (e.title.startsWith(mot)) score += 1;
        else if (e.title.includes(mot)) score += 2;
        else if (e.meta.includes(mot)) score += 4;
        else if (e.lyrics.includes(mot)) score += 6;
        else {
          ok = false;
          break;
        }
      }
      if (ok) scored.push({ song: e.song, score });
    }
    scored.sort((a, b) => a.score - b.score || a.song.title.localeCompare(b.song.title, 'fr'));
    return scored.slice(0, limit).map((s) => s.song);
  }
}
