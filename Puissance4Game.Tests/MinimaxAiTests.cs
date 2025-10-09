using Puissance4Game.GameLogic;
using Xunit;

namespace Puissance4Game.Tests;

public class MinimaxAiTests
{
    private const int AiPlayer = GameBoard.PlayerTwo;
    private const int HumanPlayer = GameBoard.PlayerOne;

    [Fact]
    public void ChooseColumn_ReturnsWinningMoveWhenAvailable()
    {
        var board = new GameBoard();
        var ai = new MinimaxAi(difficulty: 4);

        DropSequence(
            board,
            (0, AiPlayer),
            (1, AiPlayer),
            (2, AiPlayer));

        var column = ai.ChooseColumn(board, AiPlayer, HumanPlayer);

        Assert.Equal(3, column);
    }

    [Fact]
    public void ChooseColumn_BlocksOpponentsImmediateWin()
    {
        var board = new GameBoard();
        var ai = new MinimaxAi(difficulty: 4);

        DropSequence(
            board,
            (0, HumanPlayer),
            (1, HumanPlayer),
            (2, HumanPlayer));

        var column = ai.ChooseColumn(board, AiPlayer, HumanPlayer);

        Assert.Equal(3, column);
    }

    [Fact]
    public void ChooseColumn_PrefersCenterOnEmptyBoard()
    {
        var board = new GameBoard();
        var ai = new MinimaxAi(difficulty: 2);

        var column = ai.ChooseColumn(board, AiPlayer, HumanPlayer);

        Assert.Equal(GameBoard.Columns / 2, column);
    }

    [Fact]
    public void ChooseColumn_ReturnsNegativeOneWhenNoValidMoves()
    {
        var board = new GameBoard();
        var ai = new MinimaxAi(difficulty: 3);

        for (var column = 0; column < GameBoard.Columns; column++)
        {
            for (var row = 0; row < GameBoard.Rows; row++)
            {
                var player = (row + column) % 2 == 0 ? HumanPlayer : AiPlayer;
                board.DropPiece(column, player);
            }
        }

        var columnChoice = ai.ChooseColumn(board, AiPlayer, HumanPlayer);

        Assert.Equal(-1, columnChoice);
    }

    private static void DropSequence(GameBoard board, params (int column, int player)[] moves)
    {
        foreach (var (column, player) in moves)
        {
            board.DropPiece(column, player);
        }
    }
}
