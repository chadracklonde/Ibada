// Stockage en fichiers JSON, écriture atomique, sauvegarde et restauration (ERG-03).
import * as fs from 'node:fs';
import * as path from 'node:path';

export class JsonStore {
  constructor(public readonly dir: string) {
    fs.mkdirSync(dir, { recursive: true });
  }

  private full(rel: string): string {
    const p = path.resolve(this.dir, rel);
    if (!p.startsWith(path.resolve(this.dir))) throw new Error(`Chemin hors du dossier de données : ${rel}`);
    return p;
  }

  exists(rel: string): boolean {
    return fs.existsSync(this.full(rel));
  }

  read<T>(rel: string, secours: T): T {
    try {
      return JSON.parse(fs.readFileSync(this.full(rel), 'utf8')) as T;
    } catch {
      return secours;
    }
  }

  /** Écrit dans un fichier temporaire puis renomme : une coupure de courant ne laisse jamais un fichier tronqué. */
  write(rel: string, data: unknown): void {
    const p = this.full(rel);
    fs.mkdirSync(path.dirname(p), { recursive: true });
    const tmp = `${p}.tmp`;
    fs.writeFileSync(tmp, JSON.stringify(data, null, 2), 'utf8');
    fs.renameSync(tmp, p);
  }

  list(rel: string, ext = '.json'): string[] {
    try {
      return fs.readdirSync(this.full(rel)).filter((f) => f.endsWith(ext)).sort();
    } catch {
      return [];
    }
  }

  remove(rel: string): void {
    fs.rmSync(this.full(rel), { force: true });
  }

  // --- Sauvegarde et restauration -------------------------------------------------

  private get backupsDir(): string {
    return path.join(this.dir, 'sauvegardes');
  }

  private static stamp(): string {
    const d = new Date();
    const p = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}${p(d.getMonth() + 1)}${p(d.getDate())}-${p(d.getHours())}${p(d.getMinutes())}${p(d.getSeconds())}`;
  }

  /** Copie tout le dossier de données (sauf les sauvegardes) dans sauvegardes/<horodatage>. */
  backup(etiquette = 'sauvegarde'): string {
    let nom = `${etiquette}-${JsonStore.stamp()}`;
    let n = 1;
    while (fs.existsSync(path.join(this.backupsDir, nom))) nom = `${etiquette}-${JsonStore.stamp()}-${n++}`;
    const dest = path.join(this.backupsDir, nom);
    fs.mkdirSync(dest, { recursive: true });
    for (const f of fs.readdirSync(this.dir)) {
      if (f === 'sauvegardes' || f.endsWith('.tmp')) continue;
      fs.cpSync(path.join(this.dir, f), path.join(dest, f), { recursive: true });
    }
    return nom;
  }

  listBackups(): string[] {
    try {
      return fs.readdirSync(this.backupsDir).sort().reverse();
    } catch {
      return [];
    }
  }

  /** Restaure une sauvegarde ; l'état actuel est d'abord sauvegardé sous « avant-restauration ». */
  restore(nom: string): void {
    const src = path.join(this.backupsDir, path.basename(nom));
    if (!fs.existsSync(src)) throw new Error(`Sauvegarde introuvable : ${nom}`);
    this.backup('avant-restauration');
    for (const f of fs.readdirSync(this.dir)) {
      if (f === 'sauvegardes') continue;
      fs.rmSync(path.join(this.dir, f), { recursive: true, force: true });
    }
    for (const f of fs.readdirSync(src)) fs.cpSync(path.join(src, f), path.join(this.dir, f), { recursive: true });
  }
}
