// Bible : références (« Luc 12:10-17 »), recherche plein texte, verset suivant (BIB-02, BIB-03, BIB-04).
import type { Bible, BibleRef } from './types';
import { normalize } from './text';

/** Noms et abréviations usuels du canon à 66 livres, en français. */
const CANON_FR: Array<[string, string[]]> = [
  ['Genèse', ['gn', 'gen']], ['Exode', ['ex', 'exo']], ['Lévitique', ['lv', 'lev']],
  ['Nombres', ['nb', 'nom']], ['Deutéronome', ['dt', 'deut']], ['Josué', ['jos']],
  ['Juges', ['jg', 'jug']], ['Ruth', ['rt']], ['1 Samuel', ['1s', '1sam']],
  ['2 Samuel', ['2s', '2sam']], ['1 Rois', ['1r', '1rois']], ['2 Rois', ['2r', '2rois']],
  ['1 Chroniques', ['1ch', '1chr']], ['2 Chroniques', ['2ch', '2chr']], ['Esdras', ['esd']],
  ['Néhémie', ['ne', 'neh']], ['Esther', ['est']], ['Job', ['jb']],
  ['Psaumes', ['ps', 'psaume', 'pse']], ['Proverbes', ['pr', 'prov']], ['Ecclésiaste', ['ec', 'eccl']],
  ['Cantique des Cantiques', ['ct', 'cant']], ['Ésaïe', ['es', 'esaie', 'isaie', 'is']], ['Jérémie', ['jr', 'jer']],
  ['Lamentations', ['lm', 'lam']], ['Ézéchiel', ['ez', 'ezech']], ['Daniel', ['dn', 'dan']],
  ['Osée', ['os']], ['Joël', ['jl']], ['Amos', ['am']], ['Abdias', ['ab', 'abd']],
  ['Jonas', ['jon']], ['Michée', ['mi', 'mic']], ['Nahum', ['na', 'nah']],
  ['Habacuc', ['ha', 'hab']], ['Sophonie', ['so', 'soph']], ['Aggée', ['ag']],
  ['Zacharie', ['za', 'zac']], ['Malachie', ['ml', 'mal']], ['Matthieu', ['mt', 'mat']],
  ['Marc', ['mc', 'mar']], ['Luc', ['lc']], ['Jean', ['jn']], ['Actes', ['ac', 'act']],
  ['Romains', ['rm', 'rom']], ['1 Corinthiens', ['1co', '1cor']], ['2 Corinthiens', ['2co', '2cor']],
  ['Galates', ['ga', 'gal']], ['Éphésiens', ['ep', 'eph']], ['Philippiens', ['ph', 'phil']],
  ['Colossiens', ['col']], ['1 Thessaloniciens', ['1th', '1thes']], ['2 Thessaloniciens', ['2th', '2thes']],
  ['1 Timothée', ['1tm', '1tim']], ['2 Timothée', ['2tm', '2tim']], ['Tite', ['tt']],
  ['Philémon', ['phm']], ['Hébreux', ['he', 'heb']], ['Jacques', ['jc', 'jac']],
  ['1 Pierre', ['1p', '1pi']], ['2 Pierre', ['2p', '2pi']], ['1 Jean', ['1jn']],
  ['2 Jean', ['2jn']], ['3 Jean', ['3jn']], ['Jude', ['jud']], ['Apocalypse', ['ap', 'apo']],
];

export function canonName(canon: number): string | undefined {
  return CANON_FR[canon]?.[0];
}

function cle(s: string): string {
  return normalize(s).replace(/[\s.]/g, '');
}

/** Table clé -> index de livre pour une Bible donnée. */
export function bookIndex(bible: Bible): Map<string, number> {
  const m = new Map<string, number>();
  const add = (k: string, i: number) => {
    if (k && !m.has(k)) m.set(k, i);
  };
  bible.books.forEach((b, i) => {
    add(cle(b.name), i);
    for (const a of b.abbreviations ?? []) add(cle(a), i);
    if (b.canon !== undefined && CANON_FR[b.canon]) {
      const [nom, abbr] = CANON_FR[b.canon];
      add(cle(nom), i);
      for (const a of abbr) add(cle(a), i);
    }
  });
  return m;
}

function findBook(bible: Bible, texte: string): number | null {
  const idx = bookIndex(bible);
  const k = cle(texte);
  if (idx.has(k)) return idx.get(k)!;
  if (k.length >= 3) {
    const trouves = new Set<number>();
    for (const [nom, i] of idx) if (nom.startsWith(k)) trouves.add(i);
    if (trouves.size === 1) return [...trouves][0];
  }
  return null;
}

/** Analyse « Luc 12:10-17 », « Jn 3.16 », « Psaume 23 », « 1 Jean 4 :8 ». */
export function parseReference(bible: Bible, saisie: string): BibleRef | null {
  const s = normalize(saisie).replace(/\s*([:.,;])\s*/g, '$1').replace(/[–—]/g, '-');
  const m = /^(\d?\s*[a-z][a-z' ]*?)\s*(\d+)(?:[:.,;v](\d+)(?:-(\d+))?)?$/.exec(s);
  if (!m) return null;
  const book = findBook(bible, m[1]);
  if (book === null) return null;
  const chapter = parseInt(m[2], 10);
  const chapitres = bible.books[book].chapters;
  if (chapter < 1 || chapter > chapitres.length) return null;
  const nbVersets = chapitres[chapter - 1].length;
  let start = 1;
  let end = nbVersets;
  if (m[3]) {
    start = parseInt(m[3], 10);
    end = m[4] ? parseInt(m[4], 10) : start;
  }
  if (start < 1 || start > nbVersets) return null;
  if (end < start) end = start;
  if (end > nbVersets) end = nbVersets;
  return { bibleId: bible.id, book, chapter, verseStart: start, verseEnd: end };
}

export function refLabel(bible: Bible, ref: BibleRef, avecBible = true): string {
  const livre = bible.books[ref.book]?.name ?? '?';
  const plage = ref.verseStart === ref.verseEnd ? `${ref.verseStart}` : `${ref.verseStart}-${ref.verseEnd}`;
  return `${livre} ${ref.chapter}:${plage}${avecBible ? ` (${bible.abbreviation})` : ''}`;
}

export function refVerses(bible: Bible, ref: BibleRef): Array<{ n: number; text: string }> {
  const chap = bible.books[ref.book]?.chapters[ref.chapter - 1] ?? [];
  const res: Array<{ n: number; text: string }> = [];
  for (let n = ref.verseStart; n <= ref.verseEnd; n++) {
    if (chap[n - 1] !== undefined) res.push({ n, text: chap[n - 1] });
  }
  return res;
}

/** Passe au verset suivant (delta = 1) ou précédent (delta = -1), en franchissant les chapitres et les livres. */
export function shiftVerse(bible: Bible, ref: BibleRef, delta: 1 | -1): BibleRef | null {
  let { book, chapter } = ref;
  let verse = delta === 1 ? ref.verseEnd + 1 : ref.verseStart - 1;
  const nb = (b: number, c: number) => bible.books[b]?.chapters[c - 1]?.length ?? 0;
  if (delta === 1 && verse > nb(book, chapter)) {
    if (chapter < bible.books[book].chapters.length) {
      chapter += 1;
      verse = 1;
    } else if (book + 1 < bible.books.length) {
      book += 1;
      chapter = 1;
      verse = 1;
    } else return null;
  } else if (delta === -1 && verse < 1) {
    if (chapter > 1) {
      chapter -= 1;
      verse = nb(book, chapter);
    } else if (book > 0) {
      book -= 1;
      chapter = bible.books[book].chapters.length;
      verse = nb(book, chapter);
    } else return null;
  }
  if (verse < 1) return null;
  return { bibleId: bible.id, book, chapter, verseStart: verse, verseEnd: verse };
}

export interface BibleHit {
  ref: BibleRef;
  label: string;
  text: string;
}

/** Recherche plein texte (tous les mots, sans accents ni casse) dans une Bible. */
export function searchBible(bible: Bible, requete: string, limit = 100): BibleHit[] {
  const mots = normalize(requete).split(' ').filter(Boolean);
  if (!mots.length) return [];
  const hits: BibleHit[] = [];
  for (let b = 0; b < bible.books.length; b++) {
    const chapitres = bible.books[b].chapters;
    for (let c = 0; c < chapitres.length; c++) {
      for (let v = 0; v < chapitres[c].length; v++) {
        const t = normalize(chapitres[c][v]);
        if (mots.every((m) => t.includes(m))) {
          const ref: BibleRef = { bibleId: bible.id, book: b, chapter: c + 1, verseStart: v + 1, verseEnd: v + 1 };
          hits.push({ ref, label: refLabel(bible, ref), text: chapitres[c][v] });
          if (hits.length >= limit) return hits;
        }
      }
    }
  }
  return hits;
}
