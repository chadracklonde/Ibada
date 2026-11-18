#!/usr/bin/env python3
"""
Analyseur de bibliothèque VideoPsalm — lot L0 (V0).

Objectif (cahier, section 4.0) : analyser les fichiers réels de l'église
AVANT d'écrire le moindre code de lecture en V1. Produit un rapport dans
docs/format-videopsalm.md.

Usage :
    python analyse.py /chemin/vers/dossier/VideoPsalm

Le script ne modifie AUCUN fichier (lecture seule, VPS-04).
"""

from __future__ import annotations
import argparse
import hashlib
import json
import os
import sqlite3
import sys
import zipfile
from collections import Counter
from pathlib import Path
from typing import Any

# Extensions connues ou probables de VideoPsalm (à ajuster après observation)
SUSPECT_EXTENSIONS = {
    ".vpc", ".vpb", ".vpj", ".vpx", ".vp",
    ".db", ".sqlite", ".sqlite3", ".json", ".xml", ".zip",
}


def human_size(n: int) -> str:
    for unit in ("o", "Ko", "Mo", "Go"):
        if n < 1024:
            return f"{n:.1f} {unit}"
        n /= 1024
    return f"{n:.1f} To"


def sha256(path: Path, chunk: int = 1 << 20) -> str:
    h = hashlib.sha256()
    with path.open("rb") as f:
        while block := f.read(chunk):
            h.update(block)
    return h.hexdigest()


def is_sqlite(path: Path) -> bool:
    try:
        with path.open("rb") as f:
            return f.read(16) == b"SQLite format 3\x00"
    except OSError:
        return False


def is_zip(path: Path) -> bool:
    return zipfile.is_zipfile(path)


def inspect_sqlite(path: Path) -> dict[str, Any]:
    info: dict[str, Any] = {"type": "sqlite"}
    try:
        con = sqlite3.connect(f"file:{path}?mode=ro", uri=True)
        cur = con.cursor()
        tables = [r[0] for r in cur.execute(
            "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name"
        ).fetchall()]
        info["tables"] = {}
        for t in tables:
            try:
                cols = cur.execute(f'PRAGMA table_info("{t}")').fetchall()
                count = cur.execute(f'SELECT COUNT(*) FROM "{t}"').fetchone()[0]
                info["tables"][t] = {
                    "columns": [c[1] for c in cols],
                    "row_count": count,
                }
            except sqlite3.Error as e:
                info["tables"][t] = {"error": str(e)}
        con.close()
    except sqlite3.Error as e:
        info["error"] = str(e)
    return info


def inspect_zip(path: Path) -> dict[str, Any]:
    info: dict[str, Any] = {"type": "zip"}
    try:
        with zipfile.ZipFile(path) as z:
            names = z.namelist()
            info["entries_count"] = len(names)
            info["sample_entries"] = names[:20]
            info["extensions"] = dict(Counter(
                Path(n).suffix.lower() for n in names if Path(n).suffix
            ))
    except zipfile.BadZipFile as e:
        info["error"] = str(e)
    return info


def inspect_text_header(path: Path, n: int = 512) -> dict[str, Any]:
    try:
        with path.open("rb") as f:
            head = f.read(n)
        try:
            text = head.decode("utf-8", errors="replace")
        except Exception:
            text = repr(head)
        return {"type": "text-or-binary", "header_preview": text[:400]}
    except OSError as e:
        return {"error": str(e)}


def analyse(root: Path) -> dict[str, Any]:
    report: dict[str, Any] = {
        "root": str(root),
        "files": [],
        "extensions": {},
        "total_size": 0,
    }
    ext_counter: Counter[str] = Counter()
    for dirpath, _dirs, files in os.walk(root):
        for name in files:
            p = Path(dirpath) / name
            if not p.is_file():
                continue
            size = p.stat().st_size
            ext = p.suffix.lower()
            ext_counter[ext] += 1
            report["total_size"] += size
            if ext not in SUSPECT_EXTENSIONS and size < 4096:
                continue
            entry: dict[str, Any] = {
                "path": str(p.relative_to(root)),
                "size": size,
                "size_human": human_size(size),
                "sha256": sha256(p),
            }
            if is_sqlite(p):
                entry.update(inspect_sqlite(p))
            elif is_zip(p):
                entry.update(inspect_zip(p))
            elif ext in {".json", ".xml", ".vp", ".vpj", ".vpc", ".vpb", ".vpx"}:
                entry.update(inspect_text_header(p))
            else:
                entry["type"] = "unknown"
            report["files"].append(entry)
    report["extensions"] = dict(ext_counter.most_common())
    return report


def write_markdown(report: dict[str, Any], out: Path) -> None:
    lines = [
        "# Format de la bibliothèque VideoPsalm — analyse V0",
        "",
        "> Généré automatiquement par `tests/analyse-videopsalm/analyse.py`.",
        "> Ne pas committer les fichiers analysés (section 11 du cahier).",
        "",
        f"Dossier analysé : `{report['root']}`",
        f"Taille totale : {human_size(report['total_size'])}",
        "",
        "## Extensions rencontrées",
        "",
        "| Extension | Nombre |",
        "|---|---|",
    ]
    for ext, n in report["extensions"].items():
        lines.append(f"| `{ext or '(aucune)'}` | {n} |")
    lines += ["", "## Fichiers analysés", ""]
    for f in report["files"]:
        lines.append(f"### `{f['path']}`")
        lines.append("")
        lines.append(f"- Taille : {f['size_human']}")
        lines.append(f"- SHA-256 : `{f['sha256']}`")
        lines.append(f"- Type détecté : **{f.get('type', 'inconnu')}**")
        if "tables" in f:
            lines.append("- Tables SQLite :")
            for t, meta in f["tables"].items():
                cols = ", ".join(meta.get("columns", []))
                lines.append(f"  - `{t}` ({meta.get('row_count', '?')} lignes) : {cols}")
        if "entries_count" in f:
            lines.append(f"- Entrées ZIP : {f['entries_count']}")
            lines.append(f"- Extensions internes : {f['extensions']}")
        if "header_preview" in f:
            lines.append("- Aperçu (400 premiers caractères) :")
            lines.append("  ```")
            lines.append("  " + f["header_preview"].replace("\n", "\n  "))
            lines.append("  ```")
        lines.append("")
    out.write_text("\n".join(lines), encoding="utf-8")


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("dossier", type=Path, help="Dossier de la bibliothèque VideoPsalm")
    ap.add_argument("--out", type=Path, default=Path("docs/format-videopsalm.md"))
    args = ap.parse_args()

    if not args.dossier.is_dir():
        print(f"Erreur : {args.dossier} n'est pas un dossier.", file=sys.stderr)
        return 1

    print(f"Analyse de {args.dossier}…")
    report = analyse(args.dossier)
    args.out.parent.mkdir(parents=True, exist_ok=True)
    write_markdown(report, args.out)
    print(f"Rapport écrit dans {args.out}")
    print(f"{len(report['files'])} fichier(s) analysé(s), "
          f"taille totale {human_size(report['total_size'])}.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
