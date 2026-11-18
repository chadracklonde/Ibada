// Types partagés du cœur d'Ibada (sans dépendance à Electron).

export type SectionType = 'verse' | 'chorus' | 'bridge' | 'intro' | 'outro' | 'other';

export interface SongSection {
  label: string; // ex. « Couplet 1 », « Refrain »
  type: SectionType;
  text: string; // lignes séparées par \n
}

export interface Song {
  id: string;
  title: string;
  author?: string;
  composer?: string;
  number?: number;
  ccli?: string;
  copyright?: string;
  songbookId?: string;
  language?: string;
  sections: SongSection[];
  /** Plusieurs ordres de couplets pour un même chant (CHT-03) : nom -> libellés de sections. */
  arrangements?: Record<string, string[]>;
  defaultArrangement?: string;
  source?: 'ibada' | 'videopsalm';
}

export interface Songbook {
  id: string;
  name: string;
  songs: Song[];
  source?: 'ibada' | 'videopsalm';
}

export interface BibleBook {
  name: string;
  abbreviations?: string[];
  /** Index dans le canon protestant à 66 livres (0 = Genèse), s'il est connu. */
  canon?: number;
  /** chapters[c - 1][v - 1] = texte du verset. */
  chapters: string[][];
}

export interface Bible {
  id: string;
  name: string;
  abbreviation: string;
  language: string;
  copyright?: string;
  books: BibleBook[];
  source?: 'ibada' | 'videopsalm';
}

export interface BibleRef {
  bibleId: string;
  book: number; // index dans bible.books
  chapter: number; // à partir de 1
  verseStart: number;
  verseEnd: number;
}

export type SlideKind = 'song' | 'bible' | 'custom' | 'image';

export interface Slide {
  id: string;
  kind: SlideKind;
  title?: string; // en-tête (titre du chant)
  label?: string; // ex. « Refrain »
  text: string;
  footer?: string; // ex. « Jean 3:16 (LSG) »
  imagePath?: string;
}

export type Align = 'left' | 'center' | 'right';
export type VAlign = 'top' | 'middle' | 'bottom';

export interface Theme {
  id: string;
  name: string;
  background: { type: 'color' | 'image' | 'video'; value: string };
  font: {
    family: string;
    /** Taille en pixels pour une hauteur de référence de 1080 (THM-03). */
    size: number;
    color: string;
    bold: boolean;
    italic: boolean;
    underline: boolean;
  };
  align: Align;
  valign: VAlign;
  shadow: { enabled: boolean; x: number; y: number; blur: number; color: string };
  outline: { enabled: boolean; width: number; color: string };
  padding: number;
  header: { show: boolean; size: number; color: string };
  footer: { show: boolean; size: number; color: string };
}

export type AgendaItem =
  | { id: string; type: 'song'; title: string; songId: string; arrangement?: string }
  | { id: string; type: 'bible'; title: string; ref: BibleRef }
  | { id: string; type: 'custom'; title: string; text: string }
  | { id: string; type: 'image'; title: string; path: string };

export interface Agenda {
  id: string;
  name: string;
  items: AgendaItem[];
  isTemplate?: boolean;
  updatedAt: string;
}

export type LiveMode = 'slide' | 'logo' | 'black' | 'clear';

export interface Settings {
  language: 'fr';
  themeId: string;
  projectionDisplayId: number | 'auto';
  logoPath?: string;
  videoPsalmFolder?: string;
  songMaxLines: number;
  bibleMaxChars: number;
  churchName: string;
}
