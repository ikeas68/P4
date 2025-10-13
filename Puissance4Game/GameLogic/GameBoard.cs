using System;
using System.Collections.Generic;

namespace Puissance4Game.GameLogic
{
    public class GameBoard
    {
        public const int Rows = 6;
        public const int Columns = 7;
        public const int PlayerOne = 1;
        public const int PlayerTwo = 2;

        private readonly int[,] _cells = new int[Rows, Columns];

        public void Reset()
        {
            Array.Clear(_cells, 0, _cells.Length);
        }

        public int this[int row, int column]
        {
            get { return _cells[row, column]; }
        }

        public bool CanDrop(int column)
        {
            return column >= 0 && column < Columns && _cells[0, column] == 0;
        }

        public IEnumerable<int> GetValidMoves()
        {
            for (var col = 0; col < Columns; col++)
            {
                if (CanDrop(col))
                {
                    yield return col;
                }
            }
        }

        public int DropPiece(int column, int player)
        {
            if (!CanDrop(column))
            {
                return -1;
            }

            for (var row = Rows - 1; row >= 0; row--)
            {
                if (_cells[row, column] == 0)
                {
                    _cells[row, column] = player;
                    return row;
                }
            }

            return -1;
        }

        public void RemovePiece(int column, int row)
        {
            if (column < 0 || column >= Columns || row < 0 || row >= Rows)
            {
                return;
            }

            if (_cells[row, column] != 0)
            {
                _cells[row, column] = 0;
            }
        }

        public bool IsFull()
        {
            for (var column = 0; column < Columns; column++)
            {
                if (_cells[0, column] == 0)
                {
                    return false;
                }
            }

            return true;
        }

        public bool HasConnectedFour(int player)
        {
            for (var row = 0; row < Rows; row++)
            {
                for (var column = 0; column < Columns; column++)
                {
                    if (_cells[row, column] == player && HasConnectedFourFrom(row, column, player))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool HasConnectedFourFrom(int row, int column, int player)
        {
            foreach (var direction in Directions)
            {
                var line = CollectAlignedPositions(row, column, direction.DeltaRow, direction.DeltaColumn, player);
                if (line.Count >= 4)
                {
                    return true;
                }
            }

            return false;
        }

        public IReadOnlyList<(int Row, int Column)> GetWinningSequence(int row, int column, int player)
        {
            if (player == 0)
            {
                return Array.Empty<(int, int)>();
            }

            foreach (var direction in Directions)
            {
                var line = CollectAlignedPositions(row, column, direction.DeltaRow, direction.DeltaColumn, player);
                if (line.Count >= 4)
                {
                    return line;
                }
            }

            return Array.Empty<(int, int)>();
        }

        private static readonly (int DeltaRow, int DeltaColumn)[] Directions =
        {
            (0, 1),
            (1, 0),
            (1, 1),
            (1, -1)
        };

        private List<(int Row, int Column)> CollectAlignedPositions(int row, int column, int dr, int dc, int player)
        {
            var startRow = row;
            var startColumn = column;

            while (IsInside(startRow - dr, startColumn - dc) && _cells[startRow - dr, startColumn - dc] == player)
            {
                startRow -= dr;
                startColumn -= dc;
            }

            var positions = new List<(int Row, int Column)>();
            var currentRow = startRow;
            var currentColumn = startColumn;

            while (IsInside(currentRow, currentColumn) && _cells[currentRow, currentColumn] == player)
            {
                positions.Add((currentRow, currentColumn));
                currentRow += dr;
                currentColumn += dc;
            }

            return positions;
        }

        private static bool IsInside(int row, int column)
        {
            return row >= 0 && row < Rows && column >= 0 && column < Columns;
        }
    }
}
