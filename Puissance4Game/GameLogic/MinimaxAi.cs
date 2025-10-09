using System;
using System.Collections.Generic;
using System.Linq;

namespace Puissance4Game.GameLogic;

public class MinimaxAi
{
    private readonly int _maxDepth;

    public MinimaxAi(int difficulty)
    {
        _maxDepth = difficulty switch
        {
            <= 1 => 2,
            2 => 4,
            3 => 5,
            >= 4 => 6
        };
    }

    public int ChooseColumn(GameBoard board, int aiPlayer, int humanPlayer)
    {
        var validMoves = board.GetValidMoves().ToList();
        if (!validMoves.Any())
        {
            return -1;
        }

        var orderedMoves = validMoves
            .OrderBy(column => Math.Abs(column - GameBoard.Columns / 2.0))
            .ToList();

        var bestScore = int.MinValue;
        var bestColumn = orderedMoves.First();

        foreach (var column in orderedMoves)
        {
            var row = board.DropPiece(column, aiPlayer);
            var score = Minimax(board, _maxDepth - 1, false, aiPlayer, humanPlayer, int.MinValue, int.MaxValue);
            board.RemovePiece(column, row);

            if (score > bestScore)
            {
                bestScore = score;
                bestColumn = column;
            }
        }

        return bestColumn;
    }

    private int Minimax(GameBoard board, int depth, bool maximizingPlayer, int aiPlayer, int humanPlayer, int alpha, int beta)
    {
        if (depth == 0 || board.IsFull() || board.HasConnectedFour(aiPlayer) || board.HasConnectedFour(humanPlayer))
        {
            return Evaluate(board, aiPlayer, humanPlayer);
        }

        if (maximizingPlayer)
        {
            var value = int.MinValue;
            foreach (var column in board.GetValidMoves())
            {
                var row = board.DropPiece(column, aiPlayer);
                value = Math.Max(value, Minimax(board, depth - 1, false, aiPlayer, humanPlayer, alpha, beta));
                board.RemovePiece(column, row);
                alpha = Math.Max(alpha, value);
                if (alpha >= beta)
                {
                    break;
                }
            }

            return value;
        }
        else
        {
            var value = int.MaxValue;
            foreach (var column in board.GetValidMoves())
            {
                var row = board.DropPiece(column, humanPlayer);
                value = Math.Min(value, Minimax(board, depth - 1, true, aiPlayer, humanPlayer, alpha, beta));
                board.RemovePiece(column, row);
                beta = Math.Min(beta, value);
                if (alpha >= beta)
                {
                    break;
                }
            }

            return value;
        }
    }

    private int Evaluate(GameBoard board, int aiPlayer, int humanPlayer)
    {
        if (board.HasConnectedFour(aiPlayer))
        {
            return 1_000_000;
        }

        if (board.HasConnectedFour(humanPlayer))
        {
            return -1_000_000;
        }

        var score = 0;
        var centerColumn = GameBoard.Columns / 2;
        var centerCount = 0;
        for (var row = 0; row < GameBoard.Rows; row++)
        {
            if (board[row, centerColumn] == aiPlayer)
            {
                centerCount++;
            }
        }

        score += centerCount * 6;

        score += EvaluateLines(board, aiPlayer, humanPlayer);

        return score;
    }

    private int EvaluateLines(GameBoard board, int aiPlayer, int humanPlayer)
    {
        var score = 0;

        // Horizontal
        for (var row = 0; row < GameBoard.Rows; row++)
        {
            var rowValues = new int[GameBoard.Columns];
            for (var column = 0; column < GameBoard.Columns; column++)
            {
                rowValues[column] = board[row, column];
            }

            for (var column = 0; column <= GameBoard.Columns - 4; column++)
            {
                var window = rowValues.Skip(column).Take(4).ToArray();
                score += EvaluateWindow(window, aiPlayer, humanPlayer);
            }
        }

        // Vertical
        for (var column = 0; column < GameBoard.Columns; column++)
        {
            var columnValues = new int[GameBoard.Rows];
            for (var row = 0; row < GameBoard.Rows; row++)
            {
                columnValues[row] = board[row, column];
            }

            for (var row = 0; row <= GameBoard.Rows - 4; row++)
            {
                var window = columnValues.Skip(row).Take(4).ToArray();
                score += EvaluateWindow(window, aiPlayer, humanPlayer);
            }
        }

        // Positive slope diagonals
        for (var row = 0; row <= GameBoard.Rows - 4; row++)
        {
            for (var column = 0; column <= GameBoard.Columns - 4; column++)
            {
                var window = new[]
                {
                    board[row, column],
                    board[row + 1, column + 1],
                    board[row + 2, column + 2],
                    board[row + 3, column + 3]
                };
                score += EvaluateWindow(window, aiPlayer, humanPlayer);
            }
        }

        // Negative slope diagonals
        for (var row = 3; row < GameBoard.Rows; row++)
        {
            for (var column = 0; column <= GameBoard.Columns - 4; column++)
            {
                var window = new[]
                {
                    board[row, column],
                    board[row - 1, column + 1],
                    board[row - 2, column + 2],
                    board[row - 3, column + 3]
                };
                score += EvaluateWindow(window, aiPlayer, humanPlayer);
            }
        }

        return score;
    }

    private int EvaluateWindow(IReadOnlyList<int> window, int aiPlayer, int humanPlayer)
    {
        var aiCount = window.Count(value => value == aiPlayer);
        var humanCount = window.Count(value => value == humanPlayer);
        var emptyCount = window.Count(value => value == 0);

        var score = 0;

        if (aiCount == 4)
        {
            score += 5_000;
        }
        else if (aiCount == 3 && emptyCount == 1)
        {
            score += 150;
        }
        else if (aiCount == 2 && emptyCount == 2)
        {
            score += 20;
        }

        if (humanCount == 4)
        {
            score -= 5_000;
        }
        else if (humanCount == 3 && emptyCount == 1)
        {
            score -= 180;
        }
        else if (humanCount == 2 && emptyCount == 2)
        {
            score -= 18;
        }

        return score;
    }
}
