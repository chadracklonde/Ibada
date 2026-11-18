# Journal des décisions

## 2026-11-17 — Création du dépôt, cahier des charges initial
## 2026-11-18 — Section 4.11 : alternatives libres aux fonctions payantes
## 2026-11-18 — Démarrage V0 : deux prototypes (Flutter et WPF) + analyseur VideoPsalm
## 2026-11-18 — Technologie du logiciel Windows : .NET WPF (C#) retenu

### Décision : .NET WPF (C#) pour le logiciel Windows

Motifs (cahier, section 6) :
- Gestion mature des écrans multiples et du plein écran sur second écran
  (System.Windows.Forms.Screen.AllScreens + WindowStartupLocation=Manual).
- Lecture vidéo native via MediaElement (Windows Media Foundation), sans
  plugin tiers, stable sur le matériel de l'église.
- Un seul langage pour le logiciel Windows ; l'application Ibada Remote
  (V3) sera tranchée séparément.
- Courbe d'apprentissage acceptable pour un développeur solo.

Conséquences :
- Le prototype Flutter reste dans le dépôt comme référence de comparaison
  mais n'est plus la cible de développement.
- Les lots L1 à L27 sont écrits en C# / XAML.
- La décision est actée dans la section 9 du cahier (décisions ouvertes).

### Décisions encore ouvertes
- Visibilité du dépôt (public aujourd'hui vs privé section 11) — à trancher
- Licence du code — avant toute ouverture
- Format VideoPsalm : lecture seule ou écriture (VPS-06) — après analyse L0
- Application Ibada Remote : Android d'abord, technologie à décider avant V3
- Ordre des langues après le français (swahili, lingala, anglais)

## 2026-11-18 — Prototype Electron : gelé, non retenu (ajout de Claude)
Un troisième prototype (`windows/prototype-electron/`, Electron + TypeScript) a été commencé avant la décision WPF, avec un noyau de V1 (chants, Bible, agenda, session, thèmes) écrit avant la recette V0, contrairement à `CLAUDE.md`. Avec le choix de WPF, ce code n'est plus la cible : il est gelé, rien n'y est ajouté. À supprimer du dépôt ou à garder comme référence, au choix de Chadrack.

## 2026-11-18 — Prototype WPF : corrections de Claude, à valider sur Windows
Placement de la projection en pixels physiques (SetWindowPos) pour éviter l'erreur due à la mise à l'échelle Windows ; bouton « Diapositive » pour revenir de l'écran noir ou du logo ; mesure du délai jusqu'au premier affichage dans `startup-log.txt`. Non compilé ni testé (pas de SDK .NET ni de Windows côté Claude) : le premier `dotnet build` chez Chadrack fait foi.

## 2026-11-18 — V0 acceptée, démarrage de la V1 (décision de Chadrack)
Chadrack décide d'avancer à l'étape suivante après vérification de la cohérence des fichiers du prototype WPF (XAML bien formés, gestionnaires et noms présents), sans test sur le matériel. La recette V0 réelle reste à faire. `CLAUDE.md` est mis à jour : version en cours V1, interdiction de coder une version avant que la précédente soit acceptée.
Choix techniques de la V1 : logique dans une bibliothèque C# pure `Ibada.Core` (testable sans Windows), stockage en fichiers JSON avec écriture atomique (SQLite reporté : le paquet NuGet n'est pas disponible dans l'environnement de test de Claude).

## 2026-11-18 — Application WPF de la V1 écrite, non compilée
`windows/Ibada.App` (net8.0-windows) : fenêtre de l'opérateur construite en code (pas de XAML à tenir en accord), projection, dessin des diapositives (`SlideView`), restauration de sauvegarde. Contrôles faits ici : syntaxe C# analysée avec Roslyn, XML bien formé, 64 tests de `Ibada.Core`. Aucune compilation WPF possible côté Claude : le premier `dotnet build windows/Ibada.App` chez Chadrack fait foi, et ses erreurs de compilation sont à renvoyer. Le journal d'erreurs (`journal/erreurs.log`) est exclu des sauvegardes et conservé lors d'une restauration. Chadrack fournira ensuite le rapport de l'analyseur VideoPsalm (à relire avant tout commit : il peut contenir des extraits de chants protégés).
