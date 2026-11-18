# Analyse de la bibliothèque VideoPsalm — lot L0

Script en lecture seule (VPS-04). Il parcourt un dossier VideoPsalm,
détecte le type réel de chaque fichier (SQLite, ZIP, JSON, XML, binaire),
et produit un rapport Markdown dans `docs/format-videopsalm.md`.

## Usage

```bash
python analyse.py /chemin/vers/bibliotheque-videopsalm \
  --out ../../docs/format-videopsalm.md
```

## Avant de committer le rapport (ajout de Claude)

Pour les fichiers texte, le rapport inclut les 400 premiers caractères de chaque fichier. Ces extraits peuvent contenir des paroles de chants protégées ou du texte biblique sous licence. Le dépôt est public : relire le rapport et supprimer ces extraits avant tout commit (cahier, section 11).
