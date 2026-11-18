// Thèmes fournis (THM-01). Les tailles sont données pour une hauteur de référence de 1080 pixels (THM-03).
import type { Theme } from './types';

const base: Omit<Theme, 'id' | 'name' | 'background'> = {
  font: { family: "'Segoe UI', Arial, sans-serif", size: 84, color: '#ffffff', bold: true, italic: false, underline: false },
  align: 'center',
  valign: 'middle',
  shadow: { enabled: true, x: 3, y: 3, blur: 8, color: 'rgba(0,0,0,0.85)' },
  outline: { enabled: false, width: 3, color: '#000000' },
  padding: 90,
  header: { show: false, size: 38, color: '#cfd8ff' },
  footer: { show: true, size: 36, color: '#cfd8ff' },
};

export const DEFAULT_THEMES: Theme[] = [
  { id: 'nuit', name: 'Nuit bleue', background: { type: 'color', value: '#0b1c4d' }, ...base },
  {
    id: 'contraste',
    name: 'Contraste (noir)',
    background: { type: 'color', value: '#000000' },
    ...base,
    outline: { enabled: true, width: 3, color: '#000000' },
    footer: { show: true, size: 36, color: '#ffd24a' },
  },
  {
    id: 'clair',
    name: 'Clair',
    background: { type: 'color', value: '#f6f3ea' },
    ...base,
    font: { ...base.font, color: '#1d2433', bold: true },
    shadow: { enabled: false, x: 0, y: 0, blur: 0, color: 'transparent' },
    footer: { show: true, size: 36, color: '#55607a' },
    header: { show: false, size: 38, color: '#55607a' },
  },
  {
    id: 'chaud',
    name: 'Chaleur (bordeaux)',
    background: { type: 'color', value: '#4a0f1a' },
    ...base,
    font: { ...base.font, color: '#fff4de' },
    footer: { show: true, size: 36, color: '#ffd9a0' },
  },
];

export function findTheme(themes: Theme[], id: string): Theme {
  return themes.find((t) => t.id === id) ?? themes[0] ?? DEFAULT_THEMES[0];
}

/** Fusionne une modification partielle dans un thème (éditeur de thème, THM-02). */
export function mergeTheme(theme: Theme, patch: Partial<Theme>): Theme {
  return {
    ...theme,
    ...patch,
    background: { ...theme.background, ...(patch.background ?? {}) },
    font: { ...theme.font, ...(patch.font ?? {}) },
    shadow: { ...theme.shadow, ...(patch.shadow ?? {}) },
    outline: { ...theme.outline, ...(patch.outline ?? {}) },
    header: { ...theme.header, ...(patch.header ?? {}) },
    footer: { ...theme.footer, ...(patch.footer ?? {}) },
  };
}
