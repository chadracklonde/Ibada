# Journal des décisions

Une ligne par décision, la plus récente en haut. Les décisions ouvertes restent dans la section 9 du cahier des charges.

| Date | Décision | Raison |
| --- | --- | --- |
| 18 nov. 2026 | Divergence signalée : le noyau TypeScript de `windows/prototype-electron/src/core` (chants, Bible, agenda, session, thèmes) relève de la V1 et a été écrit avant la recette V0. Il est gelé : on n'y ajoute rien avant la recette V0 ; seul le périmètre L0 avance (fenêtre de projection, vidéo en boucle, analyseur VideoPsalm) | Règle de `CLAUDE.md` : pas de V1 avant la recette V0 |
| 18 nov. 2026 | Ajout d'une option C (Electron + TypeScript) au prototype L0, dans `windows/prototype-electron/`, à côté de Flutter (A) et WPF (B) | Seule option testable et empaquetable pour Windows depuis Linux ; choix provisoire, la technologie définitive reste à trancher après la recette V0 |
| 18 nov. 2026 | Alternatives libres aux fonctions payantes (cahier, section 4.11) | Ne dépendre d'aucun abonnement ni logiciel payant |
| 18 nov. 2026 | V1 stockée en fichiers JSON plutôt qu'en base SQLite | Plus simple à sauvegarder et à tester ; à revoir si le volume l'exige |
| 17 nov. 2026 | Nom du logiciel : Ibada, avec Ibada Remote pour la télécommande | Choix de Chadrack |
