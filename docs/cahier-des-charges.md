# Cahier des charges — Ibada, logiciel de projection pour église (Windows)

Nov 17, 2026 · Chadrack Londe

## 1. Contexte et objectifs

Ce cahier des charges décrit un logiciel Windows de projection pour le culte, d'abord destiné à l'église locale, qui reprend à terme les fonctions de VideoPsalm, OpenLP, ProPresenter et FreeWorship Pro, livrées version après version. Pour toute fonction qui n'existe que dans un logiciel ou un service payant, Ibada développe sa propre alternative, gratuite et libre, ou une fonction proche (section 4.11).

**Objectifs**

- Permettre à un opérateur seul de projeter chants, versets, annonces et médias pendant un culte, sans connexion Internet.
- Livrer une version 1 utilisable en culte réel avec un ordinateur et un projecteur.
- Couvrir progressivement l'ensemble des fonctions recensées, sans en oublier : la section 10 relie chaque fonction des quatre logiciels à une exigence et à une version.
- Offrir une interface en français, puis en swahili, lingala et anglais.
- Ne jamais dépendre d'un abonnement, d'une licence payante ni d'un service en ligne pour utiliser une fonction : chaque fonction d'un logiciel payant est remplacée par une alternative libre ou une fonction proche conçue pour Ibada (section 4.11).

**Périmètre**

- Plateforme : Windows 10 et 11, 64 bits.
- Hors périmètre au départ : macOS et Linux (l'application mobile de télécommande fait partie du projet, voir 4.9).
- Nom du logiciel : Ibada (swahili : « culte »). L'application mobile de télécommande s'appelle provisoirement Ibada Remote.

**Bibliothèque de contenu** : le logiciel lit directement la bibliothèque VideoPsalm déjà utilisée par l'église (chants et Bibles), voir la section 4.0.

**Sources des fonctions recensées** (pages consultées le 17 novembre 2026) : [VideoPsalm](https://videopsalm.org/features/), [OpenLP](https://openlp.org/), [ProPresenter](https://renewedvision.com/propresenter/all-features), [FreeWorship Pro](https://freeworship.co.uk/features/pro-features/). Dans les tableaux, les sources sont notées **VP**, **OL**, **PP** et **FW**.

## 2. Utilisateurs et scénarios d'usage

Le logiciel sert quatre profils, dont l'opérateur est le plus exigeant : il agit en direct, sous pression, souvent seul.

| Profil | Besoin principal | Versions concernées |
| --- | --- | --- |
| Opérateur (projection) | Enchaîner chants, versets et annonces sans erreur ni écran parasite | V1 à V5 |
| Responsable de louange ou pasteur | Préparer l'agenda à l'avance, demander un verset ou changer un chant à la dernière minute | V1 à V3 |
| Chorale et musiciens | Voir les accords, transposer, lire la suite sur un écran de scène | V2 à V3 |
| Administrateur | Gérer bibliothèques, thèmes, sauvegardes, imports et licences | V1 à V4 |

**Scénarios types**

1. **Préparation (veille du culte)** : créer l'agenda, ajouter chants, lectures, annonces et images, vérifier l'aperçu, enregistrer.
2. **Déroulement du culte** : lancer l'agenda, projeter chaque élément, passer d'un couplet à l'autre au clavier ou à la télécommande, afficher un logo ou un écran noir entre deux éléments.
3. **Demande imprévue** : le pasteur annonce un verset ou un chant hors programme ; l'opérateur le cherche et l'affiche en moins de dix secondes sans interrompre l'élément en cours de préparation.
4. **Correction en direct** : une faute dans les paroles est corrigée à la volée et enregistrée.
5. **Incident** : plantage ou coupure de courant ; au redémarrage, l'agenda et la position dans le culte sont retrouvés.
6. **Événement spécial** : un culte avec vidéos, présentation PowerPoint et compte à rebours.

## 3. Feuille de route par versions

Le développement avance en six étapes : un prototype technique, puis cinq versions. On ne passe à la suivante qu'une fois le critère de sortie atteint. Chaque exigence de la section 4 porte sa version cible.

| Étape | Thème | Contenu principal | Critère de sortie |
| --- | --- | --- | --- |
| V0 | Prototype technique | Deux fenêtres (opérateur et projection plein écran sur l'écran secondaire), texte net avec ombre et contour, vidéo en boucle, démarrage rapide, analyse des fichiers réels de la bibliothèque VideoPsalm, création du dépôt privé (section 11) | Le prototype tient une heure de projection sans saccade sur le matériel de l'église |
| V1 | Culte de base | Lecture de la bibliothèque VideoPsalm (chants et Bibles), bibliothèque de chants, Bible hors ligne, agenda, diapositives personnalisées, images, thèmes de base, aperçu et direct, sauvegarde | Un culte complet est conduit avec la V1 seule, sans autre logiciel de secours |
| V2 | Culte complet | Vidéos et fonds animés, PowerPoint et PDF, accords et transposition, bilingue, alertes, minuteurs, vue scène, imports depuis les autres logiciels, édition à la volée | Trois cultes consécutifs sans incident bloquant ; les chants de l'église sont migrés |
| V3 | Confort et diffusion | Application mobile de télécommande (Ibada Remote) et page web, écran de scène complet, transitions, bibliothèques en ligne, version portable, mode tactile, recherche de médias, impression des livrets, pages web | Un deuxième opérateur formé prend en main le logiciel avec le seul guide fourni |
| V4 | Intégrations et automatisation | Macros, actions de diapositive, API locale, Stream Deck, SongSelect et rapports CCLI, MultiTracks, planification, annonces parallèles, plusieurs sorties, plusieurs télécommandes avec droits | Un culte entièrement piloté par macros et télécommande, avec rapport CCLI généré |
| V5 | Régie professionnelle | NDI, SDI, clé alpha, Edge Blend, timeline, MIDI, DMX, timecode, liaison réseau entre ordinateurs, entrées vidéo, Planning Center | Une diffusion en direct avec deux ordinateurs redondants est testée |

**Règle de priorité** : une fonction qui sert à chaque culte (V1, V2) passe avant une fonction de confort (V3), et avant une fonction de régie professionnelle (V4, V5). Les versions V4 et V5 peuvent être révisées selon les besoins réels de l'église.

## 4. Exigences fonctionnelles

Chaque exigence a un identifiant (module + numéro), une version cible et la ou les sources dont elle vient (VP, OL, PP, FW). Quand une fonction existe sous le même principe dans plusieurs logiciels, elle n'est écrite qu'une fois.

### 4.0 Compatibilité avec la bibliothèque VideoPsalm (VPS)

L'église dispose déjà de la bibliothèque VideoPsalm (recueils de chants, Bibles). Le logiciel doit s'en servir directement, sans conversion manuelle. Le format exact de ces fichiers n'est pas décrit sur la page des fonctionnalités : il sera analysé sur des fichiers réels pendant V0, avant d'écrire le moindre code de lecture.

| ID | Exigence | Version | Source |
| --- | --- | --- | --- |
| VPS-01 | Lecture directe des recueils de chants VideoPsalm : titres, auteurs, numéros, paroles et couplets | V1 | décision du projet |
| VPS-02 | Lecture directe des Bibles VideoPsalm, avec recherche par référence et par mot-clé | V1 | décision du projet |
| VPS-03 | Dossier de bibliothèque configurable : le logiciel pointe vers le dossier VideoPsalm existant ; VideoPsalm et le nouveau logiciel peuvent rester installés ensemble | V1 | décision du projet |
| VPS-04 | Protection des fichiers d'origine : lecture seule par défaut, copie de sauvegarde avant toute écriture, aucun fichier altéré ni perdu | V1 | décision du projet |
| VPS-05 | Contenu non reconnu : ignoré, signalé dans un journal lisible, sans bloquer le chargement du reste de la bibliothèque | V1 | décision du projet |
| VPS-06 | Enregistrement des chants modifiés ou créés : soit au format VideoPsalm (pour rester compatible), soit dans la base du logiciel avec export au format VideoPsalm ; choix à faire après l'analyse des fichiers | V2 | décision du projet |
| VPS-07 | Lecture des autres éléments VideoPsalm utiles : recueils d'annonces et de versets, listes de culte, thèmes, selon ce que l'analyse montre | V2 | décision du projet |
| VPS-08 | Détection au démarrage des recueils ou Bibles ajoutés dans le dossier VideoPsalm depuis la dernière ouverture | V2 | décision du projet |

### 4.1 Chants (CHT)

| ID | Exigence | Version | Source |
| --- | --- | --- | --- |
| CHT-01 | Bibliothèque de chants : titre, auteur, compositeur, numéro CCLI, paroles, types de couplets (couplet, refrain, pont) | V1 | VP, OL |
| CHT-02 | Recherche instantanée dans les titres, paroles, auteurs, compositeurs et numéros CCLI | V1 | VP, OL, PP |
| CHT-03 | Ordre des couplets : plusieurs arrangements dans un même chant, sans doublon dans la bibliothèque | V1 | OL, FW, PP |
| CHT-04 | Affichage d'un chant couplet par couplet, avec retour au chant précédent et reprise du refrain | V1 | VP, OL |
| CHT-05 | Recueils de chants : organisation en recueils, tri par titre ou par numéro, exclusion de recueils précis de la recherche | V2 | VP |
| CHT-06 | Édition à la volée : modifier paroles et couplets pendant le culte sans interrompre l'affichage | V2 | VP, FW, PP |
| CHT-07 | Éditeur de paroles avec annulation et rétablissement illimités, vue de tout le texte et insertion de coupures entre diapositives | V2 | VP, PP |
| CHT-08 | Accords affichés sur les paroles, transposition, capo, notation ChordPro | V2 | VP |
| CHT-09 | Pistes d'accompagnement et fichiers MP3 associés au chant, avec lecture automatique | V2 | VP, OL |
| CHT-10 | Affichage bilingue d'un chant (deux langues superposées ou enchaînées) | V2 | VP |
| CHT-11 | Import de chants depuis d'autres logiciels (OpenSong, OpenLP, ProPresenter) et depuis des fichiers texte ; VideoPsalm est traité en 4.0 | V2 | VP, OL |
| CHT-12 | Mise en évidence de mots individuels pendant la projection | V3 | VP |
| CHT-13 | Bibliothèque de chants en ligne extensible : téléchargement de chants dans plusieurs langues depuis le logiciel | V3 | VP |
| CHT-14 | Impression de livrets de chants, avec ou sans accords | V3 | VP (annoncé) |
| CHT-15 | Import des chants depuis CCLI SongSelect en un clic (abonnement SongSelect requis) | V4 | VP, PP, FW |
| CHT-16 | Rapports CCLI et création d'un recueil CCLI ; déclaration d'usage à CCLI | V4 | VP, PP |
| CHT-17 | Import de paroles, grilles d'accords et repères MIDI depuis un compte MultiTracks | V4 | PP |

### 4.2 Bible (BIB)

| ID | Exigence | Version | Source |
| --- | --- | --- | --- |
| BIB-01 | Bibles hors ligne, import de plusieurs formats de modules bibliques | V1 | VP, OL |
| BIB-02 | Recherche de versets par référence (ex. Luc 12:10-17) | V1 | VP, OL |
| BIB-03 | Recherche par mot-clé et recherche plein texte dans plusieurs traductions | V1 | VP, OL, FW |
| BIB-04 | Affichage d'un verset à la volée, avec passage au verset suivant ou précédent | V1 | VP, OL |
| BIB-05 | Affichage bilingue ou parallèle de deux traductions | V2 | VP |
| BIB-06 | Recueil de versets : liste de versets préparée à l'avance (lectures du jour) | V2 | VP |
| BIB-07 | Organisation des bibliothèques de Bibles, sauvegarde et restauration | V3 | VP |
| BIB-08 | Téléchargement de traductions depuis le logiciel ou depuis un site biblique ; bibliothèque biblique extensible | V3 | VP, OL |

### 4.3 Agenda du culte (AGD)

| ID | Exigence | Version | Source |
| --- | --- | --- | --- |
| AGD-01 | Agenda : liste ordonnée de chants, versets, images, vidéos, présentations, annonces et diapositives personnalisées, réorganisable | V1 | VP, OL, PP |
| AGD-02 | Enregistrement, ouverture et duplication d'un agenda ; modèle d'agenda réutilisable | V1 | VP, PP |
| AGD-03 | Reprise automatique de l'agenda et de la position après un plantage ou une coupure | V1 | nouveau |
| AGD-04 | Glisser-déposer de vidéos et d'images directement dans l'agenda | V2 | FW |
| AGD-05 | Lecture en boucle d'un chant, d'un diaporama ou d'un élément de l'agenda | V2 | VP, OL |
| AGD-06 | Transition prédéfinie par élément de l'agenda et enchaînement automatique (utile au bilingue) | V3 | VP |
| AGD-07 | Agenda portable entre l'ordinateur de la maison et celui de l'église (dossier synchronisé type Dropbox, OneDrive ou pCloud) | V3 | VP |
| AGD-08 | Synchronisation de l'agenda avec l'ordre du service de Planning Center | V5 | PP |
| AGD-09 | Planification : lancement programmé des annonces et des diffusions à une date et une heure données | V4 | PP |

### 4.4 Médias (MED)

| ID | Exigence | Version | Source |
| --- | --- | --- | --- |
| MED-01 | Images : import, organisation en dossiers, galerie avec miniatures, diaporamas à passage automatique | V1 | VP, OL |
| MED-02 | Lecture audio et vidéo des formats courants, avec contrôles de lecture (pause, position, volume) et galerie vidéo à miniatures | V2 | VP, OL |
| MED-03 | Fonds vidéo en boucle derrière les paroles ; changement de fond en un clic, y compris en direct | V2 | VP |
| MED-04 | Présentations PowerPoint (import et contrôle depuis le logiciel) et PDF | V2 | VP, OL |
| MED-05 | Documents Word et LibreOffice Impress | V3 | VP, OL |
| MED-06 | Recherche d'images et de vidéos par mots-clés dans leurs métadonnées | V3 | VP |
| MED-07 | Pages web affichées avec interaction en direct, et vidéos YouTube | V3 | VP, FW |
| MED-08 | Réglage de la vidéo : luminosité et contraste, pour compenser une projection délavée | V3 | FW |
| MED-09 | Bibliothèque de visuels et de fonds libres de droits intégrée (équivalent de ProContent, sans abonnement) | V4 | PP |
| MED-10 | Entrées vidéo externes (caméra, capture) intégrées avec superpositions | V5 | PP |

### 4.5 Annonces, alertes et temps (ANN)

| ID | Exigence | Version | Source |
| --- | --- | --- | --- |
| ANN-01 | Diapositives personnalisées (liturgie, annonces, autres contenus) avec mise en forme et boucle | V1 | OL |
| ANN-02 | Affichage du logo de l'église, écran noir et écran vide en un clic | V1 | VP |
| ANN-03 | Recueil d'annonces et d'informations réutilisables | V2 | VP |
| ANN-04 | Alertes et messages instantanés affichés par-dessus le contenu en cours | V2 | VP |
| ANN-05 | Horloge, compte à rebours jusqu'à une heure, minuteur, chronomètre, temps écoulé | V2 | VP, PP |
| ANN-06 | Texte dynamique : minuteurs et horloges insérés dans le texte d'une diapositive | V3 | PP |
| ANN-07 | Annonces en parallèle : une présentation distincte (annonces, halls d'entrée) diffusée depuis le même ordinateur | V4 | PP |
| ANN-08 | Bandeau de paroles (Lyric Banner), fonction citée par ProPresenter sans description : à préciser avant d'être spécifiée | V4 | PP |

### 4.6 Thèmes et mise en forme (THM)

| ID | Exigence | Version | Source |
| --- | --- | --- | --- |
| THM-01 | Thèmes : fond, police, taille, couleur, alignement (gauche, droite, haut, centre, bas), gras, italique, souligné, ombre portée, contour | V1 | VP, PP |
| THM-02 | Modification d'un thème répercutée en un clic sur toutes les diapositives qui l'utilisent | V1 | PP |
| THM-03 | Rendu indépendant de la résolution : même mise en page de 1280×720 à 3840×2160 | V1 | VP |
| THM-04 | Thèmes fournis (une quarantaine) et couleurs de texte prédéfinies | V2 | VP |
| THM-05 | Modèles d'en-tête et de pied de page configurables (référence du verset, titre du chant, auteur) | V2 | VP |
| THM-06 | Transitions : entre paroles, entre fonds, entre éléments de l'agenda | V3 | VP |
| THM-07 | Styles hiérarchiques (un style hérite d'un autre) et explorateur des attributs de style | V3 | VP |
| THM-08 | Verrouillage du style : le style de l'église ou de la maison ne peut pas être modifié par erreur | V3 | FW |
| THM-09 | Effets de texte et d'arrière-plan, remplissage d'une forme avec une vidéo ou une entrée vidéo | V4 | PP |
| THM-10 | Objets personnalisés (formes, textes, médias), masques, accessoires (props) superposés à tous les calques, et looks (calques affichés par sortie) | V4 | PP |

### 4.7 Projection et écrans (PRJ)

| ID | Exigence | Version | Source |
| --- | --- | --- | --- |
| PRJ-01 | Fenêtre de projection plein écran sur l'écran choisi, sans bordure ni notification système | V1 | VP, OL, PP |
| PRJ-02 | Aperçu avant envoi et aperçu du direct dans la fenêtre de l'opérateur | V1 | VP, PP |
| PRJ-03 | Mise en direct d'un élément au clic ou au clavier ; passage au suivant et au précédent | V1 | VP, OL |
| PRJ-04 | Aperçu de tout format d'écran (16:9, 16:10, 4:3, écrans larges) | V2 | VP |
| PRJ-05 | Vue scène : écran de retour pour musiciens et prédicateur, accessible depuis un autre appareil du réseau local | V2 | VP, OL, PP |
| PRJ-06 | Écran de scène : messages visibles seulement sur la scène, texte actuel et suivant, dispositions modifiables | V3 | PP |
| PRJ-07 | Plusieurs écrans ou sorties alimentés par une même source, avec vues multiples pour les opérateurs | V4 | PP |
| PRJ-08 | Sortie clé et remplissage (Alpha Key) vers un mélangeur vidéo | V5 | PP |
| PRJ-09 | Sortie réseau NDI vers des entrées compatibles | V5 | PP |
| PRJ-10 | Sortie et entrée SDI (cartes Blackmagic) | V5 | PP |
| PRJ-11 | Edge Blend : fusion de plusieurs projecteurs sur une grande surface | V5 | PP |
| PRJ-12 | Diffusion en direct résiliente de type Resi : à étudier ; hors périmètre tant que le besoin n'est pas confirmé (service externe) | V5 | PP |

### 4.8 Ergonomie et poste de travail (ERG)

| ID | Exigence | Version | Source |
| --- | --- | --- | --- |
| ERG-01 | Interface claire en ruban ou barre d'outils, utilisable entièrement au clavier (raccourcis configurables) | V1 | VP |
| ERG-02 | Interface traduisible ; français en V1, puis swahili, lingala et anglais | V1 | VP |
| ERG-03 | Sauvegarde et restauration de toutes les bibliothèques et réglages ; sauvegarde à la demande de l'utilisateur | V1 | VP |
| ERG-04 | Modification rapide d'un texte par clic droit | V2 | PP |
| ERG-05 | Recherche globale (Ctrl+F) dans toutes les bibliothèques : chants, Bible, médias, annonces | V2 | PP |
| ERG-06 | Prise en charge tactile (tablette ou écran tactile) | V3 | VP |
| ERG-07 | Mode spectateur ou débutant : interface réduite aux actions de projection | V3 | VP |
| ERG-08 | Version portable sur clé USB, sans installation | V3 | VP, OL |
| ERG-09 | Protection des bibliothèques de l'église contre la copie ou la suppression accidentelle (ordinateur de la maison et de l'église) | V3 | VP |
| ERG-10 | Prise en main en moins de dix minutes : assistant au premier lancement et guide intégré | V3 | OL |

### 4.9 Télécommande (TEL)

| ID | Exigence | Version | Source |
| --- | --- | --- | --- |
| TEL-01 | Télécommande web accessible depuis le navigateur d'un téléphone ou d'une tablette sur le réseau local : recherche, mise en direct, passage de diapositive | V3 | OL, VP (annoncé) |
| TEL-02 | Accès à la télécommande protégé par un code, activable et désactivable | V3 | nouveau |
| TEL-03 | Application mobile Ibada Remote, Android d'abord (iOS à confirmer) : télécommande, aperçu et recherche | V3 | OL, VP (annoncé), PP |

**Exigences de l'application Ibada Remote**

| ID | Exigence | Version | Source |
| --- | --- | --- | --- |
| TEL-04 | Appairage : l'application trouve l'ordinateur sur le réseau local (découverte automatique, saisie de l'adresse ou lecture d'un QR code) et s'authentifie avec le code de TEL-02 | V3 | décision du projet |
| TEL-05 | Fonctionnement sans Internet, par le Wi-Fi de l'église ou par le point d'accès d'un téléphone ou de l'ordinateur | V3 | décision du projet |
| TEL-06 | Commandes : suivant, précédent, choix d'un couplet, écran noir, logo, mise en direct d'un élément de l'agenda, recherche d'un chant ou d'un verset | V3 | OL, VP (annoncé), PP |
| TEL-07 | Aperçu de la diapositive en direct et vue de l'agenda sur le téléphone | V3 | PP |
| TEL-08 | Reconnexion automatique après une coupure du Wi-Fi ; la projection n'est jamais interrompue et l'ordinateur reste pilotable au clavier | V3 | décision du projet |
| TEL-09 | Plusieurs télécommandes connectées en même temps, avec des droits distincts (opérateur, lecture seule) | V4 | décision du projet |

### 4.10 Automatisation et intégrations (AUT)

| ID | Exigence | Version | Source |
| --- | --- | --- | --- |
| AUT-01 | Macros : enchaîner des actions automatiquement, déclenchées par une diapositive, un appareil ou à la demande | V4 | PP |
| AUT-02 | Actions de diapositive : associer à une diapositive une action (changer d'entrée, vider un média, lancer une macro) | V4 | PP |
| AUT-03 | Marqueurs de lecture placés dans un média pour déclencher macros ou messages | V4 | PP |
| AUT-04 | Prise en charge d'un Elgato Stream Deck (suivant, précédent, messages, macros) | V4 | PP |
| AUT-05 | API locale documentée pour piloter le logiciel depuis d'autres outils | V4 | PP |
| AUT-06 | Timeline : rejouer diapositives, médias, actions et macros selon des temporisations enregistrées | V5 | PP |
| AUT-07 | MIDI et DMX : synchronisation avec consoles audio et lumière | V5 | PP |
| AUT-08 | Timecode : synchronisation avec matériel et logiciels compatibles | V5 | PP |
| AUT-09 | Liaison réseau (Network Link) : pilotage de plusieurs ordinateurs locaux, avec redondance | V5 | PP |
| AUT-10 | Intégration Planning Center : synchronisation des playlists avec l'ordre du service et Services LIVE | V5 | PP |
| AUT-11 | Service Cloud (cité par ProPresenter sans description) : à préciser ; la sauvegarde sur dossier synchronisé (AGD-07) couvre le besoin de base | V5 | PP |

### 4.11 Alternatives libres aux fonctions payantes (ALT)

Décision du 18 novembre 2026 : on ne reproduit pas les fonctions des logiciels et services payants (ProPresenter, FreeWorship Pro, SongSelect, MultiTracks, ProContent, Resi, Planning Center, Stream Deck) telles quelles. Pour chacune, Ibada propose une alternative gratuite, fondée sur des formats et protocoles ouverts, ou une fonction proche conçue pour l'église. Les exigences d'origine de la section 4 restent le but fonctionnel ; la présente section dit comment l'atteindre sans rien payer. Une fonction de ce type ne peut pas être déclarée terminée si elle exige un compte, un abonnement ou un logiciel payant.

| ID | Exigence | Remplace ou complète | Version | Source |
| --- | --- | --- | --- | --- |
| ALT-01 | Règle générale : aucune fonction d'Ibada ne requiert d'abonnement, de licence par poste, de compte en ligne ni de filigrane ; pas de limite d'installations | Toutes | V1 | Décision |
| ALT-02 | Pas de copie des éléments propriétaires : noms, interfaces, visuels et fichiers des logiciels payants ne sont pas reproduits ; les fonctions sont réimplémentées à notre façon | Toutes (source PP, FW) | V1 | Décision |
| ALT-03 | Formats ouverts d'échange : OpenLyrics et OpenSong pour les chants, ChordPro pour les accords, OSIS ou USFM pour les Bibles, ICS et CSV pour les agendas ; export complet à tout moment | CHT-11, AGD-07 | V2 | OL |
| ALT-04 | Journal d'utilisation des chants (date, chant, numéro CCLI s'il est connu) avec export CSV, sans abonnement ni compte ; l'église l'utilise pour ses déclarations | CHT-15, CHT-16 (SongSelect) | V3 | PP |
| ALT-05 | Import de chants depuis fichiers texte, OpenLyrics et ChordPro déposés par l'utilisateur, à la place de la recherche dans le catalogue SongSelect | CHT-15 | V3 | PP |
| ALT-06 | Pistes audio locales : associer plusieurs fichiers audio à un chant (instrument, voix, version) et les lire depuis l'agenda, à la place de MultiTracks | CHT-17, CHT-09 | V4 | PP |
| ALT-07 | Bibliothèque de visuels libres de droits : l'utilisateur importe des paquets de fonds et d'images du domaine public ou sous licence libre (CC0, CC BY), avec leur licence enregistrée ; Ibada fournit un petit lot d'origine propre | MED-09 (ProContent) | V4 | PP |
| ALT-08 | Boutons personnalisables : pages de boutons (suivant, précédent, messages, macros) affichées dans la télécommande web et mobile, plus raccourcis clavier et MIDI, à la place du Stream Deck | AUT-04 | V4 | PP |
| ALT-09 | Modèles de thèmes partageables en fichier : un thème, un fond et ses réglages s'exportent et s'importent entre ordinateurs, à la place des « looks » et accessoires propriétaires | THM-10, THM-02 | V4 | PP |
| ALT-10 | Diffusion en direct sans service payant : sortie plein écran capturable par un logiciel libre de diffusion (par exemple OBS Studio), et sortie NDI quand elle est disponible, à la place de Resi | PRJ-12, PRJ-09 | V5 | PP |
| ALT-11 | Planification sans compte en ligne : import et export de l'ordre du service au format CSV ou ICS, et API locale (AUT-05), à la place de Planning Center | AGD-08, AUT-10 | V5 | PP |
| ALT-12 | Liaison réseau sur protocole ouvert (WebSocket sur le réseau local, sans serveur extérieur) entre plusieurs ordinateurs, à la place du service Cloud et du Network Link propriétaires | AUT-09, AUT-11 | V5 | PP |
| ALT-13 | Sauvegarde et synchronisation par dossier au choix (disque, clé USB, dossier synchronisé par un outil libre), sans service Cloud imposé | AUT-11, AGD-07 | V5 | PP |

## 5. Exigences non fonctionnelles

La fiabilité pendant le culte passe avant toute nouvelle fonction : un logiciel de projection qui plante devant l'assemblée est pire qu'un logiciel plus simple. Les valeurs chiffrées sont des cibles proposées, à valider avec le prototype V0 sur le matériel de l'église.

| Domaine | Exigence |
| --- | --- |
| Hors ligne | Toutes les fonctions du culte (V1 et V2) marchent sans Internet. Internet ne sert qu'aux téléchargements, à SongSelect et aux mises à jour. |
| Rapidité d'affichage | Passage d'une diapositive à l'écran en moins de 100 ms après le clic ; démarrage du logiciel en moins de 5 s. |
| Recherche | Résultat en moins de 200 ms sur une bibliothèque de 5 000 chants ; recherche plein texte dans une Bible en moins de 1 s. |
| Fiabilité | La fenêtre de projection n'affiche jamais de boîte d'erreur, de notification Windows ni de curseur. En cas de problème, elle reste sur le dernier élément ou passe à l'écran noir. |
| Reprise | Sauvegarde automatique de l'agenda toutes les 60 s ; reprise au redémarrage à l'élément en cours (AGD-03). Le logiciel tolère une coupure de courant. |
| Données | Format ouvert : base SQLite et fichiers JSON, export complet possible. Aucune donnée envoyée à l'extérieur sans action de l'utilisateur. |
| Matériel cible | Windows 10 ou 11 64 bits, 8 Go de mémoire recommandés, carte graphique intégrée suffisante, un écran ou projecteur HDMI secondaire, résolutions de 1280×720 à 3840×2160. |
| Langues et texte | Unicode complet : accents français, caractères du lingala et du swahili. Polices libres embarquées. |
| Lisibilité | Taille de texte minimale et contraste configurables pour les salles mal éclairées ou les projecteurs faibles. |
| Sécurité | Télécommande protégée par un code (TEL-02) ; réseau local seulement par défaut. |
| Maintenabilité | Tests automatisés sur le moteur de rendu et l'import ; documentation du format de fichier ; journal d'erreurs lisible par l'administrateur. |

## 6. Architecture technique

Le choix de la technologie dépend d'un seul risque : afficher proprement un second écran plein écran, avec texte net et vidéo fluide. On le tranche par le prototype V0, pas sur le papier.

| Option | Atouts | Points à vérifier au prototype |
| --- | --- | --- |
| Flutter (Windows) | Déjà maîtrisé pour d'autres applications ; un seul langage ; interface soignée | Deux fenêtres dont une plein écran sur le moniteur choisi ; lecture vidéo fluide ; ombre et contour du texte |
| .NET (WPF) | Gestion mature des écrans multiples et de la vidéo sous Windows | Apprentissage de C# et de XAML |
| Tauri ou Electron | Texte, transitions et thèmes faciles en HTML et CSS ; seconde fenêtre simple | Poids de l'application ; fluidité vidéo sur ordinateur modeste |

**Prototypes** : un dossier par option dans `windows/` (`prototype-flutter`, `prototype-wpf`, `prototype-electron`). L'option Electron est la première réalisée, parce qu'elle seule se construit et se teste depuis Linux ; son noyau (`src/core`) est du TypeScript pur, indépendant de l'interface. Ce choix reste provisoire (`docs/journal-decisions.md`).

**Décision à prendre après V0** : la technologie qui réussit les trois vérifications ci-dessus sur le matériel de l'église.

**Modules** (indépendants de la technologie choisie)

- **Bibliothèques** : chants, Bibles, médias, annonces, dans une base SQLite locale (en V1, fichiers JSON avec écriture atomique, voir `docs/journal-decisions.md`).
- **Moteur de diapositives** : transforme un chant, un verset ou une annonce en diapositives selon un thème et une résolution.
- **Moteur de rendu** : dessine la diapositive, ses transitions et ses fonds animés ; il sert l'aperçu, le direct et l'écran de scène.
- **Gestionnaire d'écrans** : détecte les moniteurs, attribue une sortie à chacun.
- **Lecteur média** : audio et vidéo, avec une bibliothèque de décodage éprouvée (par exemple libVLC, utilisée par OpenLP).
- **Agenda** : liste ordonnée, état du culte en cours, sauvegarde automatique.
- **Serveur local** : petit serveur HTTP et WebSocket pour l'application mobile Ibada Remote, la page web de télécommande, la vue scène et l'API, actif seulement sur demande.
- **Import et export** : lecteur de la bibliothèque VideoPsalm (VPS), lecture des formats d'autres logiciels, export complet.
- **Actions et macros** (V4) : moteur d'événements qui relie déclencheurs et actions.

**Modèle de données (tables principales)** : chant, couplet, arrangement, recueil, traduction biblique, verset, média, agenda, élément d'agenda, thème, annonce, réglage, journal d'usage (pour les rapports CCLI).

## 7. Contenu, droits et licences

Le logiciel ne doit livrer que du contenu dont la redistribution est permise ; le reste est importé par l'église elle-même.

| Contenu | Règle proposée |
| --- | --- |
| Bibles | Fournir seulement des traductions du domaine public ou sous licence qui autorise la redistribution. Les autres sont importées par l'église. Vérifier la licence de chaque traduction avant de l'embarquer. |
| Chants | Fournir des chants du domaine public et ceux de l'église. La reproduction de paroles protégées relève de la licence de l'église (par exemple CCLI) ; l'import SongSelect exige un abonnement. |
| Code d'autres projets | OpenLP est présenté comme logiciel libre, mais sa page ne précise pas la licence : la lire sur le dépôt avant de réutiliser une ligne de code ou un format. |
| Formats d'import | Lire un format (OpenSong, OpenLP, VideoPsalm, ProPresenter) est une question d'interopérabilité à vérifier format par format ; ne pas redistribuer leurs bibliothèques de contenu avec le logiciel. La lecture de la bibliothèque VideoPsalm installée par l'église est prévue (VPS) ; vérifier les conditions d'utilisation de VideoPsalm et de chaque recueil ou Bible. |
| Polices, fonds, vidéos | N'embarquer que des ressources libres de droits, avec leur licence notée dans un fichier du dossier du logiciel. |
| Marques et noms | Ne pas reprendre les noms, icônes ni interfaces d'autres logiciels ; choisir un nom propre. |

## 8. Tests et recette

Chaque version est validée par un essai en conditions réelles avant d'être adoptée pour le culte du dimanche. L'ancien logiciel reste installé en secours jusqu'à la fin de la V2.

| Version | Recette |
| --- | --- |
| V0 | Une heure de projection test sur l'écran secondaire et le projecteur de l'église : texte net, vidéo en boucle fluide, aucune fenêtre parasite. La bibliothèque VideoPsalm de l'église est ouverte et ses fichiers sont analysés. |
| V1 | Répétition générale d'un culte complet : 10 chants, 5 passages bibliques, 3 annonces. Un verset inattendu est affiché en moins de 10 s. Les chants et Bibles de la bibliothèque VideoPsalm s'ouvrent sans conversion ni altération des fichiers. Un plantage provoqué est suivi d'une reprise à l'élément en cours. |
| V2 | Trois cultes consécutifs sans incident bloquant. Tous les chants de l'église sont importés sans perte de couplets. Une vidéo, un PowerPoint et un PDF passent sans réglage manuel. |
| V3 | Un second opérateur tient un culte avec le seul guide fourni. Ibada Remote pilote un culte complet depuis un téléphone, sans Internet. |
| V4 | Un culte piloté par macros ; rapport CCLI du mois généré et vérifié ; plusieurs sorties actives. |
| V5 | Essai de diffusion avec NDI ou SDI et deux ordinateurs redondants ; bascule sur l'ordinateur de secours en cours de culte. |

**Pratiques tout au long du projet**

- Tests automatisés du moteur de diapositives (coupure des couplets, ordre, thèmes) et de l'import.
- Jeu d'essai fixe : 50 chants, une Bible, 5 vidéos, 3 présentations, pour comparer d'une version à l'autre.
- Liste de contrôle avant chaque culte : écrans détectés, son, projecteur, sauvegarde de la veille.
- Un registre des incidents, relu après chaque version.

## 9. Risques et décisions ouvertes

Le risque principal est le volume : ces quatre logiciels représentent des années de travail cumulé. Le découpage par versions limite ce risque, à condition de ne pas commencer la V3 avant que la V1 serve réellement au culte.

| Risque | Conséquence | Parade |
| --- | --- | --- |
| Affichage sur second écran instable | Culte perturbé, perte de confiance | Prototype V0 avant tout autre développement ; logiciel de secours conservé |
| Portée trop large | V1 jamais livrée | Respect strict des critères de sortie de la section 3 |
| Un seul développeur | Projet bloqué en cas d'indisponibilité | Code et documentation dans un dépôt, format de données ouvert, export complet |
| Droits sur Bibles et chants | Blocage de la diffusion du logiciel | Règles de la section 7 ; contenus importés par l'église |
| Import de formats tiers incomplet | Chants mal migrés | Jeu d'essai réel de l'église, vérification chant par chant pour les premiers imports |
| Matériel faible ou coupures de courant | Saccades, perte de travail | Cibles de la section 5 testées sur le matériel réel ; sauvegarde automatique |
| Fonctions citées sans description (Lyric Banner, Cloud) | Exigence floue | À préciser avant d'être développées (ANN-08, AUT-11) |
| Alternatives libres moins complètes que les services payants | Écart de confort avec ProPresenter ou SongSelect | Section 4.11 : formats ouverts, import par l'utilisateur ; écarts listés dans les notes de version |

**Décisions à prendre**

- [ ] Nom du logiciel : Ibada, retenu le 18 novembre 2026
- [ ] Technologie : Flutter, .NET ou Tauri, tranchée après le prototype V0 ; technologie de l'application mobile et plateforme (Android d'abord, iOS à confirmer) à décider aussi
- [ ] Langues de l'interface après le français : swahili, lingala, anglais, dans quel ordre
- [ ] Traductions bibliques à inclure, et leur licence
- [ ] Bibliothèque VideoPsalm : lecture seule, ou aussi écriture dans ce format (VPS-06), à trancher après l'analyse des fichiers
- [ ] Matériel de l'église : ordinateur, projecteur, écrans, carte son
- [x] Fonctions des logiciels payants : alternatives libres ou fonctions proches, jamais de copie ni de dépendance payante (18 novembre 2026, section 4.11)
- [ ] Diffusion du logiciel : usage interne ou partage avec d'autres églises ; dépôt privé au départ, licence du code à choisir avant toute ouverture (section 11)

## 10. Matrice de couverture

Toutes les fonctions relevées sur les quatre pages sont rattachées à une exigence de la section 4. Quatre points seulement n'ont pas d'exigence propre ; ils sont listés en fin de section avec la raison.

### VideoPsalm

| Fonction de VideoPsalm | Exigence |
| --- | --- |
| Paroles de chants, couplets | CHT-01, CHT-04 |
| Versets bibliques, recherche par référence, affichage à la volée | BIB-01, BIB-02, BIB-04, PRJ-03 |
| Diaporamas d'images, galerie d'images à miniatures | MED-01 |
| Vidéos autonomes, galerie vidéo, contrôles de lecture | MED-02 |
| PowerPoint, PDF, Word, documents LibreOffice | MED-04, MED-05 |
| Pages web et YouTube | MED-07 |
| Boucles vidéo en fond, fond vidéo en direct, échange du fond image en un clic | MED-03 |
| Accords sur les paroles, transposition, capo, ChordPro | CHT-08 |
| Lecture automatique des MP3 associés aux chants | CHT-09 |
| Alertes et messages instantanés | ANN-04 |
| Horloge, compte à rebours, chronomètre | ANN-05 |
| Milliers de chants dans plusieurs langues, bibliothèque de chants en ligne extensible | CHT-13 |
| Plus de 3 000 traductions bibliques, bibliothèque biblique extensible | BIB-08 |
| Recherche instantanée (chants, versets, CCLI) | CHT-02, BIB-03 |
| Plus de 28 langues d'interface, interface traduisible | ERG-02 |
| Une quarantaine de thèmes, couleurs de texte prédéfinies | THM-04 |
| Prise en charge CCLI, recueil CCLI, rapports CCLI | CHT-15, CHT-16 |
| Mode spectateur pour débutants | ERG-07 |
| Agenda portable, compatibilité pCloud, Dropbox, OneDrive | AGD-07 |
| Interface en ruban, utilisation au clavier | ERG-01 |
| Sauvegarde et restauration des bibliothèques | ERG-03, BIB-07 |
| Mise en évidence de mots individuels | CHT-12 |
| Version portable sur clé USB | ERG-08 |
| Affichage bilingue des chants et des versets | CHT-10, BIB-05 |
| Présentation indépendante de la résolution | THM-03 |
| Mode édition à la volée ; traitement de texte des paroles avec annulation illimitée | CHT-06, CHT-07 |
| Aperçu de tout format d'écran ; mode aperçu pendant le direct | PRJ-04, PRJ-02 |
| Protection des chants de l'église | ERG-09 |
| Recherche d'images et de vidéos par mots-clés | MED-06 |
| Vue scène | PRJ-05 |
| Recueils personnels de chants, d'annonces et de versets ; tri ; exclusion de recueils | CHT-05, ANN-03, BIB-06 |
| Compatibilité OpenSong, ProPresenter | CHT-11 |
| Styles hiérarchiques, explorateur des attributs de style | THM-07 |
| Transitions des paroles, des fonds, des éléments de l'agenda ; présélection de transition | THM-06, AGD-06 |
| Attributs de texte, alignement, ombre, contour | THM-01 |
| Lecture en boucle des chants, diaporamas, etc. | AGD-05 |
| Prise en charge tactile | ERG-06 |
| Modèles d'en-tête et de pied de page | THM-05 |
| Affichage automatique du logo de l'église | ANN-02 |
| Impression de livrets de chants (annoncé) | CHT-14 |
| Télécommande WiFi universelle et application mobile (annoncées) | TEL-01, TEL-03 |

### OpenLP

| Fonction d'OpenLP | Exigence |
| --- | --- |
| Import de chants depuis diverses sources et d'autres logiciels | CHT-11 |
| Étiquetage des types de couplets, ordre des couplets, mise en forme, auteurs, recherche | CHT-01, CHT-02, CHT-03 |
| Pistes d'accompagnement | CHT-09 |
| Lecture de presque tous les formats audio et vidéo (via VLC) | MED-02, section 6 |
| Bibles : import de formats, téléchargement, recherche par référence ou mot-clé | BIB-01, BIB-02, BIB-03, BIB-08 |
| Diapositives personnalisées (liturgie, annonces) avec boucle | ANN-01 |
| Présentations PowerPoint, PowerPoint Viewer, LibreOffice Impress | MED-04, MED-05 |
| Images : import, dossiers, diaporamas automatiques | MED-01 |
| Télécommande par navigateur et applications mobiles | TEL-01, TEL-03 |
| Vue scène sur tout appareil du réseau local | PRJ-05 |
| Mise en service en moins de dix minutes | ERG-10 |
| Version portable Windows | ERG-08 |

### ProPresenter

| Fonction de ProPresenter | Exigence |
| --- | --- |
| Macros ; Slide Actions ; Playback Markers | AUT-01 ; AUT-02 ; AUT-03 |
| Timeline ; MIDI et DMX ; Timecode | AUT-06 ; AUT-07 ; AUT-08 |
| Stream Deck ; API | AUT-04 ; AUT-05 |
| Calendar Scheduling | AGD-09 |
| Network Link | AUT-09 |
| Planning Center | AUT-10, AGD-08 |
| MultiTracks ; SongSelect by CCLI | CHT-17 ; CHT-15, CHT-16 |
| Quick Search | ERG-05 |
| Alpha Key | PRJ-08 |
| Announcements | ANN-07 |
| Arrangements | CHT-03 |
| Custom Objects ; Masks ; Props ; Looks | THM-10 |
| Images et vidéos en premier ou arrière-plan | MED-01, MED-02, MED-03 |
| Playlists (avec modèle réutilisable) | AGD-01, AGD-02 |
| Effets et remplissages ; effets de texte et d'arrière-plan | THM-09 |
| ProContent | MED-09 |
| Quick Edit | ERG-04 |
| Reflow Editor | CHT-07 |
| Themes | THM-02 |
| Timers and Clocks ; Timers et texte dynamique | ANN-05 ; ANN-06 |
| Multiple screens ; Operator Views | PRJ-07 |
| Edge Blend | PRJ-11 |
| Live Screen Previews | PRJ-02 |
| NDI ; SDI | PRJ-09 ; PRJ-10 |
| Resi | PRJ-12 |
| Video Inputs | MED-10 |
| Stage Display : messages, texte actuel et suivant, dispositions flexibles | PRJ-05, PRJ-06 |
| Lyric Banner (cité sans description) | ANN-08 |
| ProPresenter Remote | TEL-03 |
| Cloud (cité sans description) | AUT-11 |

### FreeWorship Pro

| Fonction de FreeWorship Pro | Exigence |
| --- | --- |
| Intégration CCLI SongSelect | CHT-15 |
| QuickEdit+ | CHT-06 |
| Ordre des couplets | CHT-03 |
| Glisser-déposer dans la liste du service | AGD-04 |
| Réglage vidéo (luminosité, contraste) | MED-08 |
| Pages web avec interaction en direct | MED-07 |
| Recherche de passages bibliques, plein texte | BIB-03 |
| Verrouillage du style | THM-08 |

### Alternatives libres (ALT)

Pour les fonctions des logiciels et services payants, la colonne « Exigence » des tableaux ci-dessus donne le but fonctionnel ; la façon de l'atteindre sans rien payer est donnée par les exigences ALT-01 à ALT-13 de la section 4.11.

### Points sans exigence propre

- **Support par e-mail 24 h sur 24 (VideoPsalm)** : service humain, non logiciel. Il est remplacé par un guide intégré (ERG-10) et un journal d'erreurs (section 5).
- **Fonctionnement sans Internet (VideoPsalm)** : traité comme exigence générale, section 5.
- **Syphon (ProPresenter, macOS)** : sans objet, le logiciel étant réservé à Windows ; NDI (PRJ-09) joue le même rôle.
- **Gratuit, open source, installations illimitées (OpenLP) et disponibilité sur macOS, Linux, FreeBSD** : choix de modèle et de plateforme, à trancher dans les décisions ouvertes (section 9) ; hors périmètre au départ (section 1).

## 11. Dépôt de code et gestion du projet

Le code, la documentation et les versions d'Ibada sont tenus dans un seul dépôt GitHub, privé au départ. Le dépôt est créé avec le prototype V0, avant le premier code de V1.

| Élément | Règle |
| --- | --- |
| Dépôt | Un seul dépôt, nom proposé `chadracklonde/ibada`, avec des dossiers séparés pour le logiciel Windows, l'application Ibada Remote, la documentation et les tests. Les deux applications évoluent ensemble. |
| Visibilité | Privé au départ. Ouverture éventuelle seulement après le choix de la licence du code et la vérification des conditions d'utilisation de VideoPsalm (section 7). |
| Contenu exclu du dépôt | Jamais de bibliothèque VideoPsalm, de Bibles, de chants protégés, de vidéos ni de polices sous licence. Ces données restent dans le dossier de données de l'application et sont listées dans `.gitignore`. Le jeu d'essai (section 8) n'utilise que des chants du domaine public ou propres à l'église. |
| Versions | Chaque version porte une étiquette (V0, V1, … V5). La version installée à l'église est une étiquette stable ; le développement se fait sur une branche séparée. |
| Suivi du travail | Chaque exigence de la section 4 (CHT-01, VPS-03, TEL-04…) devient une tâche portant son identifiant et sa version. Les jalons V1 à V5 reprennent les critères de sortie de la section 3. |
| Livraison | L'installateur Windows et l'application Ibada Remote sont publiés comme versions du dépôt. |
| Documentation | Un fichier de présentation, la description du format de la bibliothèque VideoPsalm une fois analysé (VPS), et un journal des décisions techniques. |
| Travail sans Internet | Le dépôt est cloné sur l'ordinateur de développement ; les modifications sont envoyées dès que la connexion le permet. |
| Auteur des commits | Les envois se font au nom de Chadrack Londe, sans mention d'un outil ou d'un assistant. |

## 12. Voies à suivre et règle de travail

Ce cahier est la référence du projet. Cette section recense ce qu'il reste à faire et à décider ; elle est mise à jour au fil du travail.

**Règle de travail (Chadrack et Claude)**

1. Avant de continuer, relire ce cahier : dans le dépôt, `docs/cahier-des-charges.md`, rappelé par le fichier `CLAUDE.md` à la racine.
2. Repérer la version en cours (section 3) et ses exigences (section 4) ; ne développer que ce qui y figure.
3. Si une idée n'y figure pas, l'ajouter d'abord au cahier avec un identifiant, une version cible et une source, puis la développer.
4. Après chaque décision ou changement de périmètre, mettre à jour le cahier et les décisions ouvertes (section 9), dans le même commit que le code concerné.
5. Quand le code et le cahier divergent, le dire et corriger l'un ou l'autre explicitement, sans laisser l'écart.
6. Le fichier du dépôt fait foi ; la copie du document en ligne est mise à jour à la demande.

**Voies à suivre, dans l'ordre**

1. Décider de la visibilité du dépôt (public aujourd'hui, privé prévu en section 11) et corriger la section 11 en conséquence.
2. Fournir un recueil de chants et une Bible de la bibliothèque VideoPsalm, hors du dépôt, puis analyser leur structure et la documenter dans `docs/` (VPS-01 à VPS-05).
3. Réaliser le prototype V0 de la fenêtre de projection sur le matériel de l'église, avec chaque technologie encore en lice.
4. Choisir la technologie du logiciel Windows et celle d'Ibada Remote.
5. Vérifier les conditions d'utilisation de VideoPsalm et les licences des Bibles, puis choisir la licence du code.
6. Passer la recette V0 (une heure de projection), puis commencer la V1.
7. À la fin de chaque version : recette, adoption au culte, mise à jour du cahier, création des tâches de la version suivante.
8. Faire avancer les versions V1 à V5 dans l'ordre ; revoir le contenu de V4 et V5 après la V3, selon les besoins réels de l'église.

**Choix en suspens et voies possibles**

| Décision | Voies possibles | Quand |
| --- | --- | --- |
| Technologie du logiciel Windows | Flutter, .NET (WPF), Tauri ou Electron | Après le prototype V0 |
| Bibliothèque VideoPsalm | Lecture seule ; lecture et écriture au format VideoPsalm ; base propre avec export au format VideoPsalm (VPS-06) | Après l'analyse des fichiers |
| Application Ibada Remote | Android seulement ; Android puis iOS ; même technologie que le logiciel Windows ou application native | Avant la V3 |
| Langues de l'interface | Français, puis swahili, lingala, anglais dans l'ordre à fixer | Avant la V2 |
| Visibilité du dépôt et licence du code | Dépôt privé ou public ; licence libre ou fermée | Avant toute ouverture du code |
| Diffusion du logiciel | Usage interne ; partage avec d'autres églises | Après la V2 |
| Alternatives aux fonctions payantes | Garder les ALT-01 à ALT-13 ; en abandonner certaines si l'église n'en a pas l'usage | Après la V2 |
| Fonctions citées sans description | Préciser ou abandonner Lyric Banner (ANN-08), Cloud (AUT-11) et Resi (PRJ-12) | Avant la V4 |
| Contenu de V4 et V5 | Garder, réduire ou réordonner selon les besoins de l'église | Après la V3 |

## 13. Découpage en lots de travail

Le travail est découpé en 28 lots, rattachés aux versions de la section 3. Un lot est un morceau livrable et testable seul. Chaque exigence de la section 4 appartient à un seul lot ; aucune n'est oubliée.

**Un lot est terminé quand**

1. toutes ses exigences sont réalisées ;
2. ses tests automatisés passent et il est essayé sur le matériel de l'église ;
3. le cahier et les décisions ouvertes (section 9) sont à jour ;
4. ses tâches sont fermées dans le dépôt.

Une version est terminée quand tous ses lots le sont et que sa recette (section 8) est passée. Les lots d'une version se font dans l'ordre du tableau, sauf si la colonne « Dépend de » permet de les mener en parallèle.

### V0 : prototype technique

| Lot | Contenu | Exigences | Dépend de |
| --- | --- | --- | --- |
| L0 | Prototype de la fenêtre de projection (second écran, texte avec ombre et contour, vidéo en boucle), analyse des fichiers VideoPsalm, choix de la technologie, vérification des licences | Aucune (prototype) | Aucun |

### V1 : culte de base

| Lot | Contenu | Exigences | Dépend de |
| --- | --- | --- | --- |
| L1 | Socle du logiciel : base de données, gestion des écrans, interface en français, clavier, sauvegarde et restauration, règles ALT | ERG-01, ERG-02, ERG-03, ALT-01, ALT-02 | L0 |
| L2 | Moteur de diapositives et de rendu, thèmes de base, projection plein écran, aperçu et direct | THM-01, THM-02, THM-03, PRJ-01, PRJ-02, PRJ-03 | L1 |
| L3 | Lecteur de la bibliothèque VideoPsalm (chants et Bibles), protection des fichiers d'origine, journal des contenus non reconnus | VPS-01, VPS-02, VPS-03, VPS-04, VPS-05, BIB-01 | L1 |
| L4 | Chants : bibliothèque, recherche instantanée, ordre des couplets, affichage couplet par couplet | CHT-01, CHT-02, CHT-03, CHT-04 | L2, L3 |
| L5 | Bible : recherche par référence et par mot-clé, affichage d'un verset à la volée | BIB-02, BIB-03, BIB-04 | L2, L3 |
| L6 | Agenda du culte, reprise après plantage, diapositives personnalisées, logo et écran noir, images | AGD-01, AGD-02, AGD-03, ANN-01, ANN-02, MED-01 | L4, L5 |

Recette V1 : section 8.

### V2 : culte complet

| Lot | Contenu | Exigences | Dépend de |
| --- | --- | --- | --- |
| L7 | Vidéo et audio, fonds animés, PowerPoint et PDF, glisser-déposer, lecture en boucle | MED-02, MED-03, MED-04, AGD-04, AGD-05 | L6 |
| L8 | Chants avancés : recueils, édition à la volée, éditeur de paroles, MP3 associés, bilingue, imports d'autres logiciels, formats ouverts | CHT-05, CHT-06, CHT-07, CHT-09, CHT-10, CHT-11, ALT-03 | L4 |
| L9 | Accords : affichage, transposition, capo, ChordPro | CHT-08 | L8 |
| L10 | Bible avancée : affichage bilingue, recueil de versets | BIB-05, BIB-06 | L5 |
| L11 | Annonces, alertes, horloge, compte à rebours et minuteurs | ANN-03, ANN-04, ANN-05 | L6 |
| L12 | Thèmes fournis, en-têtes et pieds de page, aperçu de tout format d'écran, vue scène | THM-04, THM-05, PRJ-04, PRJ-05 | L2 |
| L13 | VideoPsalm avancé (écriture, autres éléments, détection des nouveautés), modification rapide, recherche globale | VPS-06, VPS-07, VPS-08, ERG-04, ERG-05 | L3, L8 |

Recette V2 : section 8.

### V3 : confort et diffusion

| Lot | Contenu | Exigences | Dépend de |
| --- | --- | --- | --- |
| L14 | Serveur local, page web de télécommande, protection par code | TEL-01, TEL-02 | L6 |
| L15 | Application Ibada Remote : appairage, fonctionnement sans Internet, commandes, aperçu, reconnexion | TEL-03, TEL-04, TEL-05, TEL-06, TEL-07, TEL-08 | L14 |
| L16 | Écran de scène, transitions, styles hiérarchiques, verrouillage du style, mise en évidence de mots | PRJ-06, THM-06, THM-07, THM-08, AGD-06, CHT-12 | L12 |
| L17 | Médias avancés : Word et LibreOffice, recherche de médias par mots-clés, pages web et YouTube, réglage vidéo | MED-05, MED-06, MED-07, MED-08 | L7 |
| L18 | Bibliothèques en ligne, téléchargement de Bibles, impression des livrets, agenda portable, journal des chants et import par fichiers | CHT-13, CHT-14, BIB-07, BIB-08, AGD-07, ALT-04, ALT-05 | L8, L10 |
| L19 | Ergonomie et diffusion : tactile, mode débutant, version portable, protection des bibliothèques, prise en main, texte dynamique | ERG-06, ERG-07, ERG-08, ERG-09, ERG-10, ANN-06 | L11 |

Recette V3 : section 8. Les lots L14 et L15 peuvent avancer en parallèle des lots L16 à L19.

### V4 : intégrations et automatisation

| Lot | Contenu | Exigences | Dépend de |
| --- | --- | --- | --- |
| L20 | Automatisation : macros, actions de diapositive, marqueurs, boutons personnalisables (Stream Deck en option), API locale | AUT-01, AUT-02, AUT-03, AUT-04, AUT-05, ALT-08 | L14 |
| L21 | CCLI et services de chants : SongSelect (facultatif), rapports CCLI, pistes audio locales | CHT-15, CHT-16, CHT-17, ALT-06 | L8 |
| L22 | Planification, annonces en parallèle, bandeau de paroles (à préciser) | AGD-09, ANN-07, ANN-08 | L11, L20 |
| L23 | Plusieurs sorties, effets, objets personnalisés, masques, bibliothèque de visuels libres, modèles de thèmes partageables | PRJ-07, THM-09, THM-10, MED-09, ALT-07, ALT-09 | L12 |
| L24 | Plusieurs télécommandes avec droits distincts | TEL-09 | L15 |

Recette V4 : section 8.

### V5 : régie professionnelle

| Lot | Contenu | Exigences | Dépend de |
| --- | --- | --- | --- |
| L25 | Sorties professionnelles : clé alpha, NDI, SDI, Edge Blend, diffusion résiliente (à étudier), diffusion par logiciel libre | PRJ-08, PRJ-09, PRJ-10, PRJ-11, PRJ-12, ALT-10 | L23 |
| L26 | Timeline, MIDI et DMX, timecode | AUT-06, AUT-07, AUT-08 | L20 |
| L27 | Liaison réseau entre ordinateurs, planification sans compte, service Cloud (à préciser), dossier de synchronisation, entrées vidéo | AUT-09, AUT-10, AUT-11, AGD-08, MED-10, ALT-11, ALT-12, ALT-13 | L20, L23 |

Recette V5 : section 8.
