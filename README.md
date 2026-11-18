# Ibada

Ibada (swahili : « culte ») est un logiciel Windows de projection pour le culte :
paroles de chants, versets bibliques, annonces, images et vidéos. Il fonctionne
sans Internet et lit directement la bibliothèque VideoPsalm de l'église.

**Ibada Remote** est l'application mobile de télécommande (V3).

## État

Phase de cadrage — V0 (prototype technique) en cours.
Le cahier des charges fait foi : `docs/cahier-des-charges.md`.

## Organisation

| Dossier | Contenu |
|---|---|
| `docs/` | Cahier des charges, format VideoPsalm, journal des décisions |
| `windows/` | Logiciel Windows : .NET WPF retenu (`prototype-wpf/`) ; Flutter et Electron gardés en référence |
| `remote/` | Application mobile Ibada Remote (V3) |
| `tests/` | Jeu d'essai et analyseur VideoPsalm |

## Prototype V0

Voir `docs/journal-decisions.md` pour l'avancement.
La technologie du logiciel Windows est **.NET WPF** (décision du 18 novembre 2026), à confirmer par la recette V0 (section 8 du cahier) sur le matériel de l'église. `windows/prototype-flutter/` reste en référence ; `windows/prototype-electron/` est gelé.

## Licence

À choisir avant toute ouverture du code (section 9 du cahier).
