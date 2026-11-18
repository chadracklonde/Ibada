// Utilitaires de texte : normalisation, échappement, identifiants.

/** Minuscules, sans accents, espaces réduits : base de toute recherche (CHT-02, BIB-03). */
export function normalize(s: string): string {
  return s
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .replace(/[œŒ]/g, 'oe')
    .replace(/[æÆ]/g, 'ae')
    .toLowerCase()
    .replace(/[’‘`´]/g, "'")
    .replace(/\s+/g, ' ')
    .trim();
}

export function escapeHtml(s: string): string {
  return s
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
}

let compteur = 0;
export function newId(prefix = 'id'): string {
  compteur += 1;
  const alea = Math.random().toString(36).slice(2, 8);
  return `${prefix}-${Date.now().toString(36)}-${compteur.toString(36)}${alea}`;
}
