# Dossier de production technique

## Architecture générale
- **App.xaml** configure WPF et définit les ressources partagées.
- **MainWindow.xaml / .cs** gèrent l'interface et les interactions utilisateur.
- **GameLogic** contient la logique métier : `GameBoard` pour l'état du plateau et `MinimaxAi` pour l'intelligence artificielle.
- **Models** expose `MoveResult`, un record résumant les effets d'un coup.

## GameBoard
La classe `GameBoard` encapsule la grille 6x7 et fournit :
- `CanDrop` pour vérifier la disponibilité d'une colonne.
- `DropPiece` / `RemovePiece` pour simuler un coup.
- `HasConnectedFour` et `GetWinningSequence` pour détecter les alignements gagnants.
- `GetValidMoves` pour itérer sur les colonnes jouables.
Ces méthodes sont utilisées aussi bien par l'interface que par l'IA.

## IA Minimax
`MinimaxAi` adapte la profondeur selon la difficulté (`<=1 → profondeur 2`, `4 → profondeur 6`).
L'algorithme :
1. Trie les coups proches du centre pour améliorer la qualité de recherche.
2. Applique Minimax avec élagage alpha-bêta.
3. Évalue chaque position via `Evaluate` :
   - Victoire/défaite immédiate (±1 000 000).
   - Contrôle de la colonne centrale (bonus de 6 par pion).
   - Analyse de toutes les fenêtres de 4 cases (horizontales, verticales, diagonales) par `EvaluateLines`.
4. `EvaluateWindow` attribue des scores selon le nombre de jetons IA / humain dans chaque fenêtre (5000 pour un alignement, 150 pour trois avec case vide, etc.).

## Interface et animations
- `BuildSlotGrid` génère dynamiquement les cellules avec effets d'ombre et glow.
- `_tokenVisuals` mémorise les ellipses pour repositionner/redessiner lors des redimensionnements.
- Les animations sont basées sur `Storyboard` pour les chutes et l'animation de victoire.
- `UpdateBoardMask` découpe dynamiquement le masque du plateau pour suivre la taille réelle de la fenêtre.

## Flux de partie
1. `StartNewGame` réinitialise le plateau (`GameBoard.Reset`) et l'état UI.
2. `HandlePlayerMoveAsync` gère l'entrée joueur puis appelle `PlayAiTurnAsync`.
3. `PlayAiTurnAsync` consulte `MinimaxAi.ChooseColumn` et simule la réponse.
4. `FinalizeMove` actualise le statut, identifie les séquences gagnantes et lance l'animation appropriée.

## Points d'extension
- Ajuster les profondeurs de recherche pour équilibrer la difficulté.
- Ajouter un second mode humain en branchant `HandlePlayerMoveAsync` sur un autre joueur.
- Introduire un système de sauvegarde en sérialisant `_cells` du `GameBoard`.
