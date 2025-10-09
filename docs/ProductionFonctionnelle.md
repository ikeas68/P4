# Dossier de production fonctionnelle

## Objectif du produit
L'application **Puissance 4** propose une expérience de jeu locale opposant un joueur humain à une IA.
Elle est construite sur WPF (.NET 6) et vise une interface fluide adaptée aux écrans desktop.

## Parcours utilisateur
1. À l'ouverture, la fenêtre principale centre la scène de jeu et applique le thème bleu nuit défini dans `MainWindow.xaml`.
2. Le joueur choisit la difficulté via la `ComboBox` `DifficultyBox`, quatre niveaux allant de débutant à expert étant proposés.
3. La zone supérieure affiche un indicateur triangulaire jaune qui suit la colonne survolée (`ColumnIndicator`).
4. Le joueur peut déplacer l'indicateur avec la souris ou les flèches du clavier et déclencher une chute par clic, touche `Entrée` ou bouton *Laisser tomber*.
5. Les boutons latéraux `◀` et `▶` complètent la navigation clavier pour l'accessibilité.
6. Un message contextuel (`StatusText`) indique l'état de la partie : tour du joueur, victoire, défaite ou match nul.

## Principales interactions
- **Sélection de colonne** : la souris ou les flèches appellent `UpdateSelectionFromPointer` et `MoveIndicator`, recalculant la géométrie du triangle.
- **Chute d'un jeton** : `HandlePlayerMoveAsync` vérifie la disponibilité de la colonne, anime la chute et déclenche l'IA.
- **État de la partie** : `FinalizeMove` met à jour les textes et déclenche l'animation de victoire.
- **Nouvelle partie** : `StartNewGame` réinitialise la grille, la couleur de fond et le dictionnaire `_tokenVisuals`.

## Rendu visuel
- Plateau masqué par `BoardFrontPath` avec géométrie découpée, créant l'effet de trous.
- Jetons rendus via des `Ellipse` colorées rouge (joueur) et ambre (IA) avec animation de chute.
- Effets visuels (`DropShadowEffect`, gradients radiaux) pour moderniser l'esthétique.

## Accessibilité et feedback
- Focus clavier initial sur la fenêtre (`Keyboard.Focus(this)` dans `OnLoaded`).
- Animation d'indicateur pour informer l'utilisateur du survol de colonne.
- Messages textuels pour chaque état de jeu : victoire, défaite, nul et erreurs d'entrée.

## Gestion de la difficulté
Le menu déroulant ajuste la profondeur de recherche de l'IA (voir dossier technique). Les étiquettes aident les joueurs à choisir un niveau adapté.
