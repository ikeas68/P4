using System.Collections.Generic;

namespace Puissance4Game.Models;

public record MoveResult(int Row, int Column, bool IsWinningMove, bool IsDraw, IReadOnlyList<(int Row, int Column)> WinningPositions);
