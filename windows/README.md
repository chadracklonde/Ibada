# Ibada pour Windows

Le code du logiciel Windows sera ajouté ici à partir du prototype V0.

## Bibliothèque `Ibada.Core` et tests (V1, lot L1)

`Ibada.Core` contient la logique sans interface (stockage, sauvegarde, textes, réglages, raccourcis). `Ibada.Core.Tests` est un programme console qui la teste :

```bash
dotnet run --project Ibada.Core.Tests
```

Code de sortie 0 si tous les tests passent. Les prototypes `prototype-wpf/`, `prototype-flutter/` et `prototype-electron/` sont indépendants de ces deux projets.
