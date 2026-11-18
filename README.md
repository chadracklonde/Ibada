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
| `windows/` | Logiciel Windows (prototypes V0 : Flutter, WPF et Electron) |
| `remote/` | Application mobile Ibada Remote (V3) |
| `tests/` | Jeu d'essai et analyseur VideoPsalm |

## Prototype V0

Voir `docs/journal-decisions.md` pour l'avancement.
Trois prototypes sont en lice : `windows/prototype-flutter/`, `windows/prototype-wpf/` et `windows/prototype-electron/` (ce dernier est le seul commencé).
La technologie sera tranchée après la recette V0 (section 8 du cahier).

## Licence

À choisir avant toute ouverture du code (section 9 du cahier).
