using System;
using System.Linq;
using CoreGraphics;
using Foundation;
using Puissance4Game.GameLogic;
using UIKit;

namespace Puissance4IOS;

public class BoardView : UIView
{
    private readonly GameViewModel _viewModel;

    public event Action<int>? ColumnTapped;

    public BoardView(GameViewModel viewModel)
    {
        _viewModel = viewModel;
        BackgroundColor = UIColor.Clear;
    }

    public override void Draw(CGRect rect)
    {
        base.Draw(rect);

        using var context = UIGraphics.GetCurrentContext();
        if (context is null)
        {
            return;
        }

        var cellSize = Math.Min(rect.Width / GameBoard.Columns, rect.Height / GameBoard.Rows);
        var boardWidth = cellSize * GameBoard.Columns;
        var boardHeight = cellSize * GameBoard.Rows;
        var offsetX = (rect.Width - boardWidth) / 2.0;
        var offsetY = (rect.Height - boardHeight) / 2.0;

        var boardRect = new CGRect(offsetX, offsetY, boardWidth, boardHeight);
        context.SetFillColor(UIColor.SystemBlueColor.CGColor);
        context.FillRect(boardRect);

        var winningCells = _viewModel.WinningSequence.ToHashSet();

        for (var row = 0; row < GameBoard.Rows; row++)
        {
            for (var column = 0; column < GameBoard.Columns; column++)
            {
                var x = offsetX + column * cellSize;
                var y = offsetY + row * cellSize;
                var slotRect = new CGRect(x, y, cellSize, cellSize);

                DrawSlot(context, slotRect, _viewModel.Board[row, column], winningCells.Contains((row, column)));
            }
        }
    }

    public override void TouchesEnded(NSSet touches, UIEvent? evt)
    {
        base.TouchesEnded(touches, evt);

        if (touches.AnyObject is not UITouch touch)
        {
            return;
        }

        var location = touch.LocationInView(this);
        var rect = Bounds;
        var cellSize = Math.Min(rect.Width / GameBoard.Columns, rect.Height / GameBoard.Rows);
        var boardWidth = cellSize * GameBoard.Columns;
        var offsetX = (rect.Width - boardWidth) / 2.0;
        var column = (int)((location.X - offsetX) / cellSize);

        if (column < 0 || column >= GameBoard.Columns)
        {
            return;
        }

        ColumnTapped?.Invoke(column);
    }

    private static void DrawSlot(CGContext context, CGRect slotRect, int value, bool isWinning)
    {
        var inset = slotRect.Inset(slotRect.Width * 0.1, slotRect.Height * 0.1);
        context.SetBlendMode(CGBlendMode.Clear);
        context.FillEllipseInRect(inset);
        context.SetBlendMode(CGBlendMode.Normal);

        UIColor fillColor = value switch
        {
            GameBoard.PlayerOne => UIColor.SystemRedColor,
            GameBoard.PlayerTwo => UIColor.SystemYellowColor,
            _ => UIColor.SystemBackgroundColor
        };

        context.SetFillColor(fillColor.CGColor);
        context.FillEllipseInRect(inset);

        if (isWinning)
        {
            context.SetStrokeColor(UIColor.SystemGreenColor.CGColor);
            context.SetLineWidth(4);
            context.StrokeEllipseInRect(inset);
        }
    }
}
