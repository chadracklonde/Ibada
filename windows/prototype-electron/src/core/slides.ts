// Transformation des contenus en diapositives : chants, versets, annonces, images.
import type { Bible, BibleRef, Slide, Song } from './types';
import { sectionOrder } from './songs';
import { refLabel, refVerses } from './bible';
import { newId } from './text';

/** Coupe une section trop longue en blocs de `maxLines` lignes au plus, en équilibrant les blocs. */
export function chunkLines(text: string, maxLines: number): string[] {
  const lignes = text.split('\n');
  if (maxLines < 1 || lignes.length <= maxLines) return [text];
  const nbBlocs = Math.ceil(lignes.length / maxLines);
  const taille = Math.ceil(lignes.length / nbBlocs);
  const blocs: string[] = [];
  for (let i = 0; i < lignes.length; i += taille) blocs.push(lignes.slice(i, i + taille).join('\n'));
  return blocs;
}

export function songToSlides(song: Song, opts: { arrangement?: string; maxLines?: number } = {}): Slide[] {
  const maxLines = opts.maxLines ?? 4;
  const slides: Slide[] = [];
  sectionOrder(song, opts.arrangement).forEach((section, i) => {
    chunkLines(section.text, maxLines).forEach((bloc, j) => {
      slides.push({
        id: `${song.id}#${i}.${j}`,
        kind: 'song',
        title: song.title,
        label: section.label,
        text: bloc,
        footer: song.number !== undefined ? `${song.title} · n° ${song.number}` : song.title,
      });
    });
  });
  return slides;
}

/** Regroupe les versets d'une plage en diapositives de `maxChars` caractères au plus. */
export function bibleToSlides(bible: Bible, ref: BibleRef, opts: { maxChars?: number } = {}): Slide[] {
  const maxChars = opts.maxChars ?? 260;
  const versets = refVerses(bible, ref);
  const groupes: Array<Array<{ n: number; text: string }>> = [];
  let courant: Array<{ n: number; text: string }> = [];
  let taille = 0;
  for (const v of versets) {
    if (courant.length && taille + v.text.length > maxChars) {
      groupes.push(courant);
      courant = [];
      taille = 0;
    }
    courant.push(v);
    taille += v.text.length;
  }
  if (courant.length) groupes.push(courant);
  return groupes.map((g, i) => {
    const sousRef: BibleRef = { ...ref, verseStart: g[0].n, verseEnd: g[g.length - 1].n };
    const seul = g.length === 1;
    return {
      id: `bible:${bible.id}:${ref.book}.${ref.chapter}.${g[0].n}#${i}`,
      kind: 'bible' as const,
      text: g.map((v) => (seul ? v.text : `${v.n} ${v.text}`)).join('\n'),
      footer: refLabel(bible, sousRef),
    };
  });
}

/** Diapositive personnalisée : plusieurs diapositives séparées par une ligne « --- ». */
export function customToSlides(title: string, text: string): Slide[] {
  const base = newId('custom');
  return text
    .replace(/\r\n?/g, '\n')
    .split(/\n\s*---+\s*\n/)
    .map((t) => t.trim())
    .filter((t) => t.length > 0)
    .map((t, i) => ({ id: `${base}#${i}`, kind: 'custom' as const, title, text: t }));
}

export function imageToSlide(title: string, path: string): Slide {
  return { id: `image:${path}`, kind: 'image', title, text: '', imagePath: path };
}
