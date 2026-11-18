# Journal des décisions

## 2026-11-17 — Création du dépôt, cahier des charges initial
Nom retenu : Ibada ; application mobile : Ibada Remote.

## 2026-11-18 — Section 4.11 : alternatives libres aux fonctions payantes
Pas de copie des fonctions payantes ni de dépendance à un abonnement (ALT-01 à ALT-13).

## 2026-11-18 — Démarrage V0 : deux prototypes (Flutter et WPF) + analyseur VideoPsalm

## 2026-11-18 — Ajout d'un troisième prototype : Electron + TypeScript (ajout de Claude)
Dossier `windows/prototype-electron/`. C'est la seule option que Claude peut construire et tester depuis Linux, avec une archive Windows sans poste Windows. Choix provisoire : la technologie définitive reste à trancher après la recette V0. Si Chadrack préfère deux prototypes seulement, ce dossier se retire.

## 2026-11-18 — Écart signalé : noyau V1 écrit avant la recette V0 (ajout de Claude)
Le noyau TypeScript de `windows/prototype-electron/src/core` (chants, Bible, agenda, session, thèmes) relève de la V1 et a été écrit avant la recette V0, contrairement à `CLAUDE.md`. Il est gelé : rien n'y est ajouté avant la recette V0. Seul le périmètre L0 avance (fenêtre de projection, vidéo en boucle, analyseur VideoPsalm). La V1 est stockée en JSON plutôt qu'en SQLite (à revoir).

### Décisions ouvertes (à trancher)
- Technologie Windows : Flutter / .NET WPF / Tauri / Electron — après V0
- Visibilité du dépôt : public (aujourd'hui) vs privé (section 11)
- Licence du code
- Format VideoPsalm : lecture seule ou écriture (VPS-06)
