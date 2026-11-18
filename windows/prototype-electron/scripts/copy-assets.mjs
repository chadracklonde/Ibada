// Copie les fichiers statiques (HTML, CSS) du dossier src vers dist après la compilation TypeScript.
import { cpSync, mkdirSync, readdirSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const racine = join(dirname(fileURLToPath(import.meta.url)), '..');
const source = join(racine, 'src', 'renderer');
const cible = join(racine, 'dist', 'renderer');
mkdirSync(cible, { recursive: true });
for (const f of readdirSync(source)) {
  if (f.endsWith('.html') || f.endsWith('.css')) cpSync(join(source, f), join(cible, f));
}
console.log('Fichiers statiques copiés vers', cible);
