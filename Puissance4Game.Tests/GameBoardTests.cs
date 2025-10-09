using System.Linq;
using Puissance4Game.GameLogic;
using Xunit;

namespace Puissance4Game.Tests;

public class GameBoardTests
{
    [Fact]
    public void Reset_ClearsBoard()
    {
        var board = new GameBoard();

        for (var column = 0; column < GameBoard.Columns; column++)
        {
            board.DropPiece(column, GameBoard.PlayerOne);
        }

        board.Reset();

        for (var row = 0; row < GameBoard.Rows; row++)
        {
            for (var column = 0; column < GameBoard.Columns; column++)
            {
                Assert.Equal(0, board[row, column]);
            }
        }
    }

    [Fact]
    public void DropPiece_ReturnsRowIndexOfPlacedPiece()
    {
        var board = new GameBoard();

        var firstRow = board.DropPiece(0, GameBoard.PlayerOne);
        var secondRow = board.DropPiece(0, GameBoard.PlayerTwo);

        Assert.Equal(GameBoard.Rows - 1, firstRow);
        Assert.Equal(GameBoard.Rows - 2, secondRow);
    }

    [Fact]
    public void DropPiece_ReturnsNegativeOneWhenColumnFull()
    {
        var board = new GameBoard();

        for (var i = 0; i < GameBoard.Rows; i++)
        {
            board.DropPiece(0, GameBoard.PlayerOne);
        }

        var result = board.DropPiece(0, GameBoard.PlayerOne);

        Assert.Equal(-1, result);
    }

    [Fact]
    public void CanDrop_ReturnsFalseForFullColumn()
    {
        var board = new GameBoard();
        for (var i = 0; i < GameBoard.Rows; i++)
        {
            board.DropPiece(0, GameBoard.PlayerOne);
        }

        Assert.False(board.CanDrop(0));
    }

    [Fact]
    public void CanDrop_ReturnsFalseForOutOfRangeColumns()
    {
        var board = new GameBoard();

        Assert.False(board.CanDrop(-1));
        Assert.False(board.CanDrop(GameBoard.Columns));
    }

    [Fact]
    public void GetValidMoves_ReturnsColumnsWithSpace()
    {
        var board = new GameBoard();
        board.DropPiece(0, GameBoard.PlayerOne);
        board.DropPiece(0, GameBoard.PlayerTwo);
        board.DropPiece(1, GameBoard.PlayerOne);

        for (var i = 0; i < GameBoard.Rows; i++)
        {
            board.DropPiece(2, GameBoard.PlayerOne);
        }

        var validMoves = board.GetValidMoves().ToList();

        Assert.DoesNotContain(2, validMoves);
        Assert.Contains(0, validMoves);
        Assert.Contains(1, validMoves);
        Assert.Equal(GameBoard.Columns - 1, validMoves.Count);
    }

    [Fact]
    public void RemovePiece_MakesCellAvailableAgain()
    {
        var board = new GameBoard();
        var row = board.DropPiece(0, GameBoard.PlayerOne);

        board.RemovePiece(0, row);

        Assert.True(board.CanDrop(0));
        Assert.Equal(0, board[row, 0]);
    }

    [Fact]
    public void IsFull_ReturnsTrueWhenBoardFull()
    {
        var board = new GameBoard();

        for (var column = 0; column < GameBoard.Columns; column++)
        {
            for (var row = 0; row < GameBoard.Rows; row++)
            {
                board.DropPiece(column, row % 2 == 0 ? GameBoard.PlayerOne : GameBoard.PlayerTwo);
            }
        }

        Assert.True(board.IsFull());
    }

    [Fact]
    public void HasConnectedFour_DetectsHorizontalWin()
    {
        var board = new GameBoard();

        for (var column = 0; column < 4; column++)
        {
            board.DropPiece(column, GameBoard.PlayerOne);
        }

        Assert.True(board.HasConnectedFour(GameBoard.PlayerOne));
    }

    [Fact]
    public void HasConnectedFour_DetectsVerticalWin()
    {
        var board = new GameBoard();

        for (var i = 0; i < 4; i++)
        {
            board.DropPiece(0, GameBoard.PlayerTwo);
        }

        Assert.True(board.HasConnectedFour(GameBoard.PlayerTwo));
    }

    [Fact]
    public void HasConnectedFour_DetectsPositiveDiagonalWin()
    {
        var board = new GameBoard();

        DropSequence(
            board,
            (0, GameBoard.PlayerOne),
            (1, GameBoard.PlayerTwo),
            (1, GameBoard.PlayerOne),
            (2, GameBoard.PlayerTwo),
            (2, GameBoard.PlayerTwo),
            (2, GameBoard.PlayerOne),
            (3, GameBoard.PlayerTwo),
            (3, GameBoard.PlayerTwo),
            (3, GameBoard.PlayerTwo),
            (3, GameBoard.PlayerOne));

        Assert.True(board.HasConnectedFour(GameBoard.PlayerOne));
    }

    [Fact]
    public void HasConnectedFour_DetectsNegativeDiagonalWin()
    {
        var board = new GameBoard();

        DropSequence(
            board,
            (3, GameBoard.PlayerOne),
            (2, GameBoard.PlayerTwo),
            (2, GameBoard.PlayerOne),
            (1, GameBoard.PlayerTwo),
            (1, GameBoard.PlayerTwo),
            (1, GameBoard.PlayerOne),
            (0, GameBoard.PlayerTwo),
            (0, GameBoard.PlayerTwo),
            (0, GameBoard.PlayerTwo),
            (0, GameBoard.PlayerOne));

        Assert.True(board.HasConnectedFour(GameBoard.PlayerOne));
    }

    [Fact]
    public void GetWinningSequence_ReturnsWinningCoordinates()
    {
        var board = new GameBoard();

        var row0 = board.DropPiece(0, GameBoard.PlayerOne);
        var row1 = board.DropPiece(1, GameBoard.PlayerOne);
        var row2 = board.DropPiece(2, GameBoard.PlayerOne);
        var row3 = board.DropPiece(3, GameBoard.PlayerOne);

        var sequence = board.GetWinningSequence(row3, 3, GameBoard.PlayerOne);

        Assert.Equal(4, sequence.Count);
        Assert.Contains((row0, 0), sequence);
        Assert.Contains((row1, 1), sequence);
        Assert.Contains((row2, 2), sequence);
        Assert.Contains((row3, 3), sequence);
    }

    private static void DropSequence(GameBoard board, params (int column, int player)[] moves)
    {
        foreach (var (column, player) in moves)
        {
            board.DropPiece(column, player);
        }
    }
}
