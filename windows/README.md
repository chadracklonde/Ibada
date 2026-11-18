# Ibada pour Windows

Le code du logiciel Windows sera ajouté ici à partir du prototype V0.

## Bibliothèque `Ibada.Core` et tests (V1, lot L1)

`Ibada.Core` contient la logique sans interface (stockage, sauvegarde, textes, réglages, raccourcis). `Ibada.Core.Tests` est un programme console qui la teste :

```bash
dotnet run --project Ibada.Core.Tests
```

Code de sortie 0 si tous les tests passent. Les prototypes `prototype-wpf/`, `prototype-flutter/` et `prototype-electron/` sont indépendants de ces deux projets.

## Application `Ibada.App` (V1, lots L1 et L2)

Fenêtre de l'opérateur et de projection en WPF. Jamais compilée par Claude (pas de Windows ni de paquets WPF dans son environnement). Sur Windows avec le SDK .NET 8 :

```
dotnet build Ibada.App
dotnet run --project Ibada.App
```

Données dans `%APPDATA%\Ibada` (ou le dossier de la variable `IBADA_DATA`). En cas d'erreur de compilation, renvoyer le message tel quel. Essayer : choisir l'écran, ouvrir la projection, les touches Espace, flèches, Entrée, B, L, C, Ctrl+S, les thèmes, sauvegarder puis restaurer.
