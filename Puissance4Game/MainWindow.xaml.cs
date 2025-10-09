using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Puissance4Game.GameLogic;
using Puissance4Game.Models;

namespace Puissance4Game;

public partial class MainWindow : Window
{
    private readonly GameBoard _board = new();
    private MinimaxAi _ai = new(2);
    private readonly Dictionary<(int Row, int Column), Ellipse> _tokenVisuals = new();
    private readonly List<Storyboard> _activeWinAnimations = new();

    private int _selectedColumn = GameBoard.Columns / 2;
    private bool _isPlayerTurn = true;
    private bool _isBusy;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        BuildSlotGrid();
        DifficultyBox.SelectedIndex = 1;
        BoardCanvas.SizeChanged += (_, _) => RefreshLayout();
        IndicatorCanvas.SizeChanged += (_, _) => UpdateIndicatorGeometry();
        StartNewGame();
        Keyboard.Focus(this);
    }

    private void BuildSlotGrid()
    {
        SlotGrid.Children.Clear();
        for (var row = 0; row < GameBoard.Rows; row++)
        {
            for (var column = 0; column < GameBoard.Columns; column++)
            {
                var cell = new Grid
                {
                    Margin = new Thickness(6),
                    IsHitTestVisible = false
                };

                var borderEllipse = new Ellipse
                {
                    Fill = new SolidColorBrush(Color.FromArgb(255, 11, 79, 108)),
                    Stroke = (Brush)FindResource("BoardHighlightBrush"),
                    StrokeThickness = 2,
                    Effect = new DropShadowEffect
                    {
                        Color = Color.FromArgb(200, 2, 18, 29),
                        BlurRadius = 18,
                        ShadowDepth = 0,
                        Opacity = 0.6
                    }
                };

                var maskEllipse = new Ellipse
                {
                    Margin = new Thickness(6),
                    Fill = new SolidColorBrush(Color.FromArgb(65, 255, 255, 255)),
                    Stroke = new SolidColorBrush(Color.FromArgb(160, 24, 148, 196)),
                    StrokeThickness = 1.5
                };

                cell.Children.Add(borderEllipse);
                cell.Children.Add(maskEllipse);
                Grid.SetRow(cell, row);
                Grid.SetColumn(cell, column);
                SlotGrid.Children.Add(cell);
            }
        }
    }

    private void RefreshLayout()
    {
        UpdateIndicatorGeometry();
        foreach (var (position, ellipse) in _tokenVisuals)
        {
            PositionToken(ellipse, position.Row, position.Column);
        }
    }

    private void UpdateIndicatorGeometry()
    {
        if (!IsLoaded)
        {
            return;
        }

        var boardWidth = BoardCanvas.ActualWidth;
        if (boardWidth <= 0)
        {
            return;
        }

        IndicatorCanvas.Width = boardWidth;
        var cellSize = boardWidth / GameBoard.Columns;
        var indicatorWidth = cellSize * 0.6;
        var indicatorHeight = IndicatorCanvas.ActualHeight > 0 ? IndicatorCanvas.ActualHeight * 0.7 : 26;

        ColumnIndicator.Points = new PointCollection
        {
            new(0, 0),
            new(indicatorWidth, 0),
            new(indicatorWidth / 2, indicatorHeight)
        };

        var x = _selectedColumn * cellSize + (cellSize - indicatorWidth) / 2;
        var y = (IndicatorCanvas.ActualHeight - indicatorHeight) / 2;
        Canvas.SetLeft(ColumnIndicator, x);
        Canvas.SetTop(ColumnIndicator, y);
    }

    private void MoveIndicator(int direction)
    {
        if (_isBusy || !_isPlayerTurn)
        {
            return;
        }

        var newColumn = Math.Clamp(_selectedColumn + direction, 0, GameBoard.Columns - 1);
        if (newColumn == _selectedColumn)
        {
            return;
        }

        _selectedColumn = newColumn;
        UpdateIndicatorGeometry();
    }

    private async void DropButton_OnClick(object sender, RoutedEventArgs e) => await HandlePlayerMoveAsync();

    private async Task HandlePlayerMoveAsync()
    {
        if (_isBusy || !_isPlayerTurn)
        {
            return;
        }

        if (!_board.CanDrop(_selectedColumn))
        {
            StatusText.Text = "Cette colonne est pleine. Choisissez-en une autre.";
            return;
        }

        _isBusy = true;
        StopWinningAnimations();

        var move = await PlaceTokenAsync(_selectedColumn, GameBoard.PlayerOne, (Brush)FindResource("PlayerOneBrush"));
        if (move.Row < 0)
        {
            _isBusy = false;
            return;
        }

        if (move.IsWinningMove)
        {
            HighlightWinningTokens(move.WinningPositions);
            StatusText.Text = "Bravo ! Vous avez gagné !";
            _isPlayerTurn = false;
            _isBusy = false;
            return;
        }

        if (move.IsDraw)
        {
            StatusText.Text = "Match nul !";
            _isPlayerTurn = false;
            _isBusy = false;
            return;
        }

        _isPlayerTurn = false;
        StatusText.Text = "L'ordinateur réfléchit...";
        _isBusy = false;
        await PlayAiMoveAsync();
    }

    private async Task PlayAiMoveAsync()
    {
        if (_isPlayerTurn)
        {
            return;
        }

        _isBusy = true;
        await Task.Delay(350);

        var aiColumn = _ai.ChooseColumn(_board, GameBoard.PlayerTwo, GameBoard.PlayerOne);
        if (aiColumn < 0)
        {
            _isBusy = false;
            return;
        }

        _selectedColumn = aiColumn;
        UpdateIndicatorGeometry();

        var move = await PlaceTokenAsync(aiColumn, GameBoard.PlayerTwo, (Brush)FindResource("PlayerTwoBrush"));
        if (move.Row < 0)
        {
            _isBusy = false;
            return;
        }

        if (move.IsWinningMove)
        {
            HighlightWinningTokens(move.WinningPositions);
            StatusText.Text = "L'ordinateur gagne cette manche.";
            _isPlayerTurn = false;
            _isBusy = false;
            return;
        }

        if (move.IsDraw)
        {
            StatusText.Text = "Match nul !";
            _isPlayerTurn = false;
            _isBusy = false;
            return;
        }

        _isPlayerTurn = true;
        StatusText.Text = "À vous de jouer.";
        _isBusy = false;
        UpdateIndicatorGeometry();
    }

    private async Task<MoveResult> PlaceTokenAsync(int column, int player, Brush brush)
    {
        var row = _board.DropPiece(column, player);
        if (row < 0)
        {
            return new MoveResult(-1, column, false, false, Array.Empty<(int, int)>());
        }

        var ellipse = CreateTokenEllipse(brush);
        _tokenVisuals[(row, column)] = ellipse;
        BoardCanvas.Children.Add(ellipse);

        PositionToken(ellipse, row, column);
        await AnimateTokenDropAsync(ellipse, row, column);

        var winningSequence = _board.GetWinningSequence(row, column, player);
        var isWinning = winningSequence.Count >= 4;
        var isDraw = _board.IsFull();

        return new MoveResult(row, column, isWinning, isDraw, winningSequence);
    }

    private Ellipse CreateTokenEllipse(Brush brush)
    {
        return new Ellipse
        {
            Fill = brush,
            Stroke = Brushes.White,
            StrokeThickness = 2,
            Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 18,
                ShadowDepth = 0,
                Opacity = 0.45
            }
        };
    }

    private void PositionToken(Ellipse ellipse, int row, int column)
    {
        var cellSize = BoardCanvas.ActualWidth / GameBoard.Columns;
        if (cellSize <= 0)
        {
            return;
        }

        var tokenSize = cellSize * 0.72;
        ellipse.Width = tokenSize;
        ellipse.Height = tokenSize;

        var left = column * cellSize + (cellSize - tokenSize) / 2;
        var top = row * cellSize + (cellSize - tokenSize) / 2;

        Canvas.SetLeft(ellipse, left);
        Canvas.SetTop(ellipse, top);
    }

    private Task AnimateTokenDropAsync(Ellipse ellipse, int row, int column)
    {
        var cellSize = BoardCanvas.ActualWidth / GameBoard.Columns;
        var targetTop = row * cellSize + (cellSize - ellipse.Height) / 2;

        var translate = new TranslateTransform
        {
            Y = -(targetTop + ellipse.Height + 30)
        };
        ellipse.RenderTransform = translate;

        var animation = new DoubleAnimation
        {
            To = 0,
            Duration = TimeSpan.FromMilliseconds(420),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        var completionSource = new TaskCompletionSource<bool>();
        animation.Completed += (_, _) =>
        {
            translate.BeginAnimation(TranslateTransform.YProperty, null);
            ellipse.RenderTransform = null;
            completionSource.TrySetResult(true);
        };

        translate.BeginAnimation(TranslateTransform.YProperty, animation);

        return completionSource.Task;
    }

    private void HighlightWinningTokens(IReadOnlyList<(int Row, int Column)> positions)
    {
        StopWinningAnimations();

        foreach (var position in positions)
        {
            if (_tokenVisuals.TryGetValue(position, out var ellipse))
            {
                var animation = new DoubleAnimation
                {
                    From = 1,
                    To = 0.2,
                    Duration = TimeSpan.FromMilliseconds(320),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };

                var storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, ellipse);
                Storyboard.SetTargetProperty(animation, new PropertyPath(UIElement.OpacityProperty));
                storyboard.Begin();
                _activeWinAnimations.Add(storyboard);
            }
        }
    }

    private void StopWinningAnimations()
    {
        foreach (var storyboard in _activeWinAnimations)
        {
            storyboard.Stop();
        }

        foreach (var ellipse in _tokenVisuals.Values)
        {
            ellipse.Opacity = 1;
        }

        _activeWinAnimations.Clear();
    }

    private void LeftButton_OnClick(object sender, RoutedEventArgs e) => MoveIndicator(-1);

    private void RightButton_OnClick(object sender, RoutedEventArgs e) => MoveIndicator(1);

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Left)
        {
            MoveIndicator(-1);
            e.Handled = true;
        }
        else if (e.Key == Key.Right)
        {
            MoveIndicator(1);
            e.Handled = true;
        }
        else if (e.Key is Key.Space or Key.Down)
        {
            _ = HandlePlayerMoveAsync();
            e.Handled = true;
        }
    }

    private void RestartButton_OnClick(object sender, RoutedEventArgs e) => StartNewGame();

    private void StartNewGame()
    {
        _board.Reset();
        StopWinningAnimations();
        BoardCanvas.Children.Clear();
        _tokenVisuals.Clear();
        _selectedColumn = GameBoard.Columns / 2;
        _isPlayerTurn = true;
        _isBusy = false;
        StatusText.Text = "Choisissez une colonne pour commencer.";
        UpdateIndicatorGeometry();
    }

    private void DifficultyBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DifficultyBox.SelectedItem is ComboBoxItem { Tag: string tagValue } && int.TryParse(tagValue, out var parsed))
        {
            _ai = new MinimaxAi(parsed);
        }
        else if (DifficultyBox.SelectedItem is ComboBoxItem { Tag: int tagInt })
        {
            _ai = new MinimaxAi(tagInt);
        }
    }
}
