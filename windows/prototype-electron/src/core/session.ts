// Agenda en cours de culte : sélection, direct, navigation, reprise après plantage (AGD-01, AGD-03, PRJ-03).
import type { Agenda, AgendaItem, LiveMode, Settings, Slide } from './types';
import type { Library } from './library';
import { bibleToSlides, customToSlides, imageToSlide, songToSlides } from './slides';
import { newId } from './text';

export interface Position {
  item: number;
  slide: number;
}

export interface LiveState extends Position {
  mode: LiveMode;
}

export interface SessionSnapshot {
  agenda: Agenda;
  selected: Position;
  live: LiveState;
}

export function newAgenda(name = 'Culte'): Agenda {
  return { id: newId('agenda'), name, items: [], updatedAt: new Date().toISOString() };
}

export class Session {
  agenda: Agenda;
  selected: Position = { item: 0, slide: 0 };
  live: LiveState = { item: 0, slide: 0, mode: 'black' };

  constructor(
    private library: Library,
    private settings: () => Settings,
    agenda?: Agenda,
  ) {
    this.agenda = agenda ?? newAgenda();
  }

  slidesOf(item: AgendaItem): Slide[] {
    const s = this.settings();
    switch (item.type) {
      case 'song': {
        const song = this.library.getSong(item.songId);
        return song ? songToSlides(song, { arrangement: item.arrangement, maxLines: s.songMaxLines }) : [];
      }
      case 'bible': {
        const bible = this.library.getBible(item.ref.bibleId);
        return bible ? bibleToSlides(bible, item.ref, { maxChars: s.bibleMaxChars }) : [];
      }
      case 'custom':
        return customToSlides(item.title, item.text);
      case 'image':
        return [imageToSlide(item.title, item.path)];
    }
  }

  allSlides(): Slide[][] {
    return this.agenda.items.map((i) => this.slidesOf(i));
  }

  private touch(): void {
    this.agenda.updatedAt = new Date().toISOString();
  }

  addItem(item: AgendaItem, index?: number): void {
    const i = index ?? this.agenda.items.length;
    this.agenda.items.splice(i, 0, item);
    this.touch();
    if (this.agenda.items.length === 1) this.selected = { item: 0, slide: 0 };
  }

  removeItem(index: number): void {
    if (index < 0 || index >= this.agenda.items.length) return;
    this.agenda.items.splice(index, 1);
    this.touch();
    const n = this.agenda.items.length;
    this.selected.item = Math.min(this.selected.item, Math.max(0, n - 1));
    this.selected.slide = 0;
    if (this.live.item >= n) this.live = { item: 0, slide: 0, mode: 'black' };
  }

  moveItem(index: number, delta: -1 | 1): void {
    const j = index + delta;
    const items = this.agenda.items;
    if (index < 0 || index >= items.length || j < 0 || j >= items.length) return;
    [items[index], items[j]] = [items[j], items[index]];
    this.touch();
    if (this.selected.item === index) this.selected.item = j;
    if (this.live.item === index) this.live.item = j;
  }

  select(item: number, slide = 0): void {
    const all = this.allSlides();
    if (item < 0 || item >= all.length) return;
    const nb = all[item].length;
    this.selected = { item, slide: Math.max(0, Math.min(slide, Math.max(0, nb - 1))) };
  }

  private slideAt(p: Position): Slide | null {
    const item = this.agenda.items[p.item];
    if (!item) return null;
    return this.slidesOf(item)[p.slide] ?? null;
  }

  selectedSlide(): Slide | null {
    return this.slideAt(this.selected);
  }

  liveSlide(): Slide | null {
    return this.live.mode === 'slide' ? this.slideAt(this.live) : null;
  }

  /** Envoie la diapositive sélectionnée au direct. */
  goLive(): void {
    if (!this.selectedSlide()) return;
    this.live = { ...this.selected, mode: 'slide' };
  }

  setMode(mode: LiveMode): void {
    if (mode === 'slide') {
      if (this.slideAt(this.live)) this.live.mode = 'slide';
      else this.goLive();
      return;
    }
    this.live.mode = mode;
  }

  private neighbour(p: Position, delta: 1 | -1): Position | null {
    const all = this.allSlides();
    let { item, slide } = p;
    slide += delta;
    for (;;) {
      if (item < 0 || item >= all.length) return null;
      if (slide >= 0 && slide < all[item].length) return { item, slide };
      if (delta === 1) {
        item += 1;
        slide = 0;
      } else {
        item -= 1;
        slide = item >= 0 && item < all.length ? all[item].length - 1 : 0;
      }
    }
  }

  /** Diapositive suivante au direct. Depuis le logo, le noir ou le vide, reprend la sélection. */
  liveNext(): void {
    if (this.live.mode !== 'slide' || !this.slideAt(this.live)) {
      this.goLive();
      return;
    }
    const n = this.neighbour(this.live, 1);
    if (!n) return;
    this.live = { ...n, mode: 'slide' };
    this.selected = { ...n };
  }

  livePrev(): void {
    if (this.live.mode !== 'slide' || !this.slideAt(this.live)) {
      this.goLive();
      return;
    }
    const n = this.neighbour(this.live, -1);
    if (!n) return;
    this.live = { ...n, mode: 'slide' };
    this.selected = { ...n };
  }

  /** Avance la sélection (aperçu) sans toucher au direct. */
  selectNext(): void {
    const n = this.neighbour(this.selected, 1);
    if (n) this.selected = n;
  }

  selectPrev(): void {
    const n = this.neighbour(this.selected, -1);
    if (n) this.selected = n;
  }

  snapshot(): SessionSnapshot {
    return {
      agenda: this.agenda,
      selected: { ...this.selected },
      live: { ...this.live },
    };
  }

  /**
   * Reprise après plantage ou fermeture : l'agenda et la position sont retrouvés,
   * mais la projection reste noire tant que l'opérateur n'agit pas (AGD-03).
   */
  restore(snap: SessionSnapshot): void {
    this.agenda = snap.agenda;
    const n = this.agenda.items.length;
    const clamp = (p: Position): Position => ({
      item: Math.max(0, Math.min(p.item, Math.max(0, n - 1))),
      slide: Math.max(0, p.slide),
    });
    this.selected = clamp(snap.selected);
    this.live = { ...clamp(snap.live), mode: 'black' };
  }
}
