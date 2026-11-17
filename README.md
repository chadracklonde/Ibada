# Ibada

Ibada (swahili : « culte ») est un logiciel Windows de projection pour le culte : paroles de chants, versets bibliques, annonces, images et vidéos. Il est pensé pour une église locale, fonctionne sans Internet et lit directement la bibliothèque VideoPsalm déjà utilisée par l'église.

**Ibada Remote** est l'application mobile qui sert de télécommande.

## État du projet

Phase de cadrage. Le cahier des charges est dans [`docs/cahier-des-charges.md`](docs/cahier-des-charges.md). Le développement avance par étapes : prototype V0, puis versions V1 à V5 (voir la section 3 du cahier des charges).

## Organisation du dépôt

| Dossier | Contenu |
| --- | --- |
| `docs/` | Cahier des charges, description du format de la bibliothèque VideoPsalm, journal des décisions |
| `windows/` | Logiciel Ibada pour Windows |
| `remote/` | Application mobile Ibada Remote |
| `tests/` | Jeu d'essai (chants du domaine public ou propres à l'église uniquement) |

## Contenu exclu du dépôt

Ne jamais ajouter au dépôt la bibliothèque VideoPsalm, des Bibles, des chants protégés, des vidéos ni des polices sous licence. Ces données restent dans le dossier de données de l'application ; le `.gitignore` en écarte les emplacements habituels.

## Licence

À choisir avant toute ouverture du code (voir les décisions ouvertes, section 9 du cahier des charges).
