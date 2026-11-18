// Rendu HTML d'une diapositive. Toutes les tailles sont en unités u = 1/1080 de la hauteur du conteneur
// (unité CSS cqh), ce qui rend la mise en page identique à toute résolution (THM-03).
import type { LiveMode, Slide, Theme } from './types';
import { escapeHtml } from './text';

const U = (n: number) => `calc(var(--u) * ${n})`;

function fileUrl(chemin: string): string {
  if (/^(file|https?|data):/i.test(chemin)) return chemin;
  const c = chemin.replace(/\\/g, '/');
  return encodeURI(c.startsWith('/') ? `file://${c}` : `file:///${c}`).replace(/#/g, '%23');
}

function cssFond(theme: Theme): string {
  const bg = theme.background;
  if (bg.type === 'image') return `background:#000 url('${fileUrl(bg.value)}') center/cover no-repeat;`;
  return `background:${bg.value};`;
}

function styleTexte(theme: Theme): string {
  const f = theme.font;
  const parts = [
    `font-family:${f.family}`,
    `font-size:${U(f.size)}`,
    `color:${f.color}`,
    `font-weight:${f.bold ? 700 : 400}`,
    `font-style:${f.italic ? 'italic' : 'normal'}`,
    `text-decoration:${f.underline ? 'underline' : 'none'}`,
    `text-align:${theme.align}`,
    'line-height:1.18',
  ];
  if (theme.shadow.enabled) {
    const s = theme.shadow;
    parts.push(`text-shadow:${U(s.x)} ${U(s.y)} ${U(s.blur)} ${s.color}`);
  }
  if (theme.outline.enabled) {
    parts.push(`-webkit-text-stroke:${U(theme.outline.width)} ${theme.outline.color}`, 'paint-order:stroke fill');
  }
  return parts.join(';');
}

const JUSTIFY: Record<string, string> = { top: 'flex-start', middle: 'center', bottom: 'flex-end' };

export interface StageOptions {
  mode?: LiveMode;
  logoPath?: string;
  churchName?: string;
}

/**
 * Renvoie le HTML de la scène (à placer dans un conteneur `container-type: size`).
 * `slide` vaut null quand rien n'est à afficher (écran vide, logo, noir).
 */
export function renderStage(slide: Slide | null, theme: Theme, opts: StageOptions = {}): string {
  const mode = opts.mode ?? 'slide';
  const racine = `class="stage" style="--u:calc(100cqh / 1080);position:relative;width:100cqw;height:100cqh;overflow:hidden;`;
  if (mode === 'black') return `<div ${racine}background:#000"></div>`;

  const fond = cssFond(theme);
  const video =
    theme.background.type === 'video'
      ? `<video class="bgvideo" src="${escapeHtml(fileUrl(theme.background.value))}" autoplay loop muted playsinline style="position:absolute;inset:0;width:100%;height:100%;object-fit:cover"></video>`
      : '';
  const debut = `<div ${racine}${fond}">${video}`;

  if (mode === 'logo') {
    const logo = opts.logoPath
      ? `<img src="${escapeHtml(fileUrl(opts.logoPath))}" alt="" style="max-width:60%;max-height:60%;object-fit:contain">`
      : `<div style="${styleTexte(theme)};text-align:center">${escapeHtml(opts.churchName ?? '')}</div>`;
    return `${debut}<div style="position:absolute;inset:0;display:flex;align-items:center;justify-content:center">${logo}</div></div>`;
  }
  if (mode === 'clear' || !slide) return `${debut}</div>`;

  if (slide.kind === 'image' && slide.imagePath) {
    return `${debut}<div style="position:absolute;inset:0;display:flex;align-items:center;justify-content:center;background:#000"><img src="${escapeHtml(fileUrl(slide.imagePath))}" alt="" style="max-width:100%;max-height:100%;object-fit:contain"></div></div>`;
  }

  const pad = U(theme.padding);
  const entete =
    theme.header.show && slide.title
      ? `<div class="entete" style="position:absolute;left:${pad};right:${pad};top:${U(30)};text-align:${theme.align};font-size:${U(theme.header.size)};color:${theme.header.color};font-family:${theme.font.family}">${escapeHtml(slide.title)}</div>`
      : '';
  const pied =
    theme.footer.show && slide.footer
      ? `<div class="pied" style="position:absolute;left:${pad};right:${pad};bottom:${U(26)};text-align:${theme.align};font-size:${U(theme.footer.size)};color:${theme.footer.color};font-family:${theme.font.family}">${escapeHtml(slide.footer)}</div>`
      : '';
  const corps = escapeHtml(slide.text).replace(/\n/g, '<br>');
  return (
    `${debut}${entete}` +
    `<div class="corps" style="position:absolute;inset:${U(70)} ${pad};display:flex;flex-direction:column;justify-content:${JUSTIFY[theme.valign]};${styleTexte(theme)}"><div style="width:100%">${corps}</div></div>` +
    `${pied}</div>`
  );
}
