using System;
using System.Collections.Generic;
using Puissance4Game.GameLogic;

namespace Puissance4IOS;

public class GameViewModel
{
    private readonly MinimaxAi _ai = new(difficulty: 3);

    public GameBoard Board { get; } = new();
    public bool PlayAgainstAi { get; private set; }
    public bool PlayerOneTurn { get; private set; } = true;
    public int CurrentPlayer => PlayerOneTurn ? GameBoard.PlayerOne : GameBoard.PlayerTwo;
    public int? Winner { get; private set; }
    public bool IsDraw { get; private set; }
    public IReadOnlyList<(int Row, int Column)> WinningSequence { get; private set; } = Array.Empty<(int, int)>();

    public bool IsHumanTurn => !PlayAgainstAi || PlayerOneTurn;
    public bool ShouldTriggerAiMove => PlayAgainstAi && !IsGameOver && !IsHumanTurn;
    public bool IsGameOver => Winner.HasValue || IsDraw;

    public void ResetGame()
    {
        Board.Reset();
        Winner = null;
        IsDraw = false;
        WinningSequence = Array.Empty<(int, int)>();
        PlayerOneTurn = true;
    }

    public void SetMode(bool playAgainstAi)
    {
        PlayAgainstAi = playAgainstAi;
        ResetGame();
    }

    public bool TryHumanMove(int column)
    {
        if (!IsHumanTurn || IsGameOver)
        {
            return false;
        }

        return TryMakeMove(column, CurrentPlayer);
    }

    public void PlayAiTurn()
    {
        if (!PlayAgainstAi || IsGameOver || PlayerOneTurn)
        {
            return;
        }

        var column = _ai.ChooseColumn(Board, GameBoard.PlayerTwo, GameBoard.PlayerOne);
        if (column < 0)
        {
            IsDraw = true;
            return;
        }

        TryMakeMove(column, GameBoard.PlayerTwo);
    }

    private bool TryMakeMove(int column, int player)
    {
        var row = Board.DropPiece(column, player);
        if (row < 0)
        {
            return false;
        }

        if (Board.HasConnectedFour(player))
        {
            Winner = player;
            WinningSequence = Board.GetWinningSequence(row, column, player);
        }
        else if (Board.IsFull())
        {
            IsDraw = true;
        }
        else
        {
            PlayerOneTurn = !PlayerOneTurn;
        }

        return true;
    }
}
