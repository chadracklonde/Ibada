# Règle de travail (Chadrack et Claude)

Le cahier des charges fait foi : `docs/cahier-des-charges.md`.

1. Avant de continuer, relire le cahier.
2. Repérer la version en cours (section 3) et ses exigences (section 4).
   Ne développer que ce qui y figure.
3. Si une idée n'y figure pas, l'ajouter d'abord au cahier avec un identifiant,
   une version cible et une source, puis la développer.
4. Après chaque décision ou changement de périmètre, mettre à jour le cahier
   et les décisions ouvertes (section 9), dans le même commit que le code.
5. Quand le code et le cahier divergent, le dire et corriger l'un ou l'autre
   explicitement, sans laisser l'écart.
6. Le fichier du dépôt fait foi.

## Version en cours

V1 — culte de base (lots L1 à L6). Technologie Windows : .NET WPF (C#).
V0 acceptée le 18 novembre 2026 sur décision de Chadrack, sans test sur le matériel de l'église : la recette V0 réelle (projecteur, une heure de projection, analyse des fichiers VideoPsalm) reste à faire.

## Interdits

- Ne jamais committer la bibliothèque VideoPsalm, des Bibles, des chants
  protégés, des vidéos ni des polices sous licence (section 11).
- Ne pas coder une version avant que la précédente soit acceptée par Chadrack (section 3 et section 12).

## Commits

Les commits se font au nom de Chadrack Londe, sans mention de Claude :
pas de `Co-Authored-By`, pas de ligne de session.
