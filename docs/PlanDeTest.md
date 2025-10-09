# Plan de test

## Objectifs
Vérifier que la logique métier et l'interface utilisateur répondent aux exigences fonctionnelles du jeu Puissance 4.

## Prérequis
- .NET 6 SDK
- Windows avec support WPF

## Cas de test fonctionnels
| ID | Scénario | Étapes | Résultat attendu |
|----|----------|--------|------------------|
| TF-01 | Lancement de partie | Ouvrir l'application | La fenêtre principale s'affiche avec la grille vide et la difficulté sur *Intermédiaire*. |
| TF-02 | Sélection de colonne à la souris | Survoler plusieurs colonnes | `ColumnIndicator` suit la souris sans retard visuel. |
| TF-03 | Déplacement clavier | Utiliser `◀` et `▶` ou les flèches | L'indicateur se déplace d'une colonne sans sortir de la grille. |
| TF-04 | Chute valide | Cliquer sur *Laisser tomber* avec une colonne libre | Le jeton rouge tombe et se cale dans la cellule la plus basse. |
| TF-05 | Colonne pleine | Remplir une colonne puis retenter | Un message informe que la colonne est indisponible, aucun jeton ne se crée. |
| TF-06 | Victoire joueur | Jouer une séquence gagnante | Animation de victoire, mise en évidence des quatre positions, message "Vous avez gagné !". |
| TF-07 | Match nul | Remplir la grille sans alignement | Message de match nul et blocage de nouvelles actions. |

## Cas de test IA
| ID | Scénario | Étapes | Résultat attendu |
|----|----------|--------|------------------|
| IA-01 | Réponse IA simple | Jouer un coup central en niveau *Débutant* | L'IA réplique dans une colonne proche du centre. |
| IA-02 | Blocage de menace | Créer trois jetons alignés avec une case vide | L'IA place un pion pour bloquer (score négatif dans `EvaluateWindow`). |
| IA-03 | Victoire IA | Offrir une possibilité gagnante à l'IA | L'IA choisit la colonne gagnante et annonce la victoire. |
| IA-04 | Variation difficulté | Changer la difficulté et observer les coups | Les coups deviennent plus compétitifs en niveaux supérieurs (profondeur > 4). |

## Tests de robustesse
- Redimensionner la fenêtre : vérifier que `UpdateBoardMask` recalcule correctement le masque et repositionne les jetons.
- Basculer rapidement la difficulté pendant une partie : l'IA doit continuer à jouer sans geler l'interface.
- Saisir rapidement plusieurs coups : `_isBusy` empêche les doubles entrées.

## Automatisation possible
- Extraire `GameBoard` et `MinimaxAi` dans un projet de tests unitaires pour valider `HasConnectedFour`, `GetWinningSequence` et `ChooseColumn`.
- Utiliser UI Automation (WinAppDriver) pour valider le déplacement de l'indicateur et les mises à jour de `StatusText`.
