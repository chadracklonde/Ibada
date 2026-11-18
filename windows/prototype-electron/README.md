# Prototype L0, option C : Electron + TypeScript

Option ajoutée le 18 novembre 2026 (voir `docs/journal-decisions.md`), car c'est la seule des trois qui se construit et se teste depuis Linux, avec une archive Windows produite sans poste Windows. Le noyau (`src/core`) est du TypeScript pur, indépendant d'Electron, pour rester réutilisable quelle que soit l'option retenue.

Commandes : `npm install`, `npm test`, `npm start`.
