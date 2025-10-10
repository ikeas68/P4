using System.Collections.Generic;

namespace Puissance4Game.Models
{
    public class MoveResult
    {
        public MoveResult(int row, int column, bool isWinningMove, bool isDraw, IReadOnlyList<(int Row, int Column)> winningPositions)
        {
            Row = row;
            Column = column;
            IsWinningMove = isWinningMove;
            IsDraw = isDraw;
            WinningPositions = winningPositions;
        }

        public int Row { get; }

        public int Column { get; }

        public bool IsWinningMove { get; }

        public bool IsDraw { get; }

        public IReadOnlyList<(int Row, int Column)> WinningPositions { get; }
    }
}
