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
                    Margin = new Thickness(4),
                    IsHitTestVisible = false
                };

                //var rim = new Ellipse
                //{
                //    Fill = Brushes.Transparent,
                //    Stroke = new SolidColorBrush(Color.FromArgb(180, 236, 246, 255)),
                //    StrokeThickness = 3,
                //    Effect = new DropShadowEffect
                //    {
                //        Color = Color.FromArgb(180, 2, 18, 29),
                //        BlurRadius = 10,
                //        ShadowDepth = 0,
                //        Opacity = 0.65
                //    }
                //};

                //var glow = new Ellipse
                //{
                //    Margin = new Thickness(6),
                //    Stroke = new SolidColorBrush(Color.FromArgb(150, 12, 63, 126)),
                //    StrokeThickness = 1.8,
                //    Fill = new RadialGradientBrush
                //    {
                //        GradientOrigin = new Point(0.35, 0.35),
                //        Center = new Point(0.5, 0.5),
                //        RadiusX = 0.6,
                //        RadiusY = 0.6,
                //        GradientStops =
                //        {
                //            new GradientStop(Color.FromArgb(40, 255, 255, 255), 0.0),
                //            new GradientStop(Color.FromArgb(10, 255, 255, 255), 0.6),
                //            new GradientStop(Color.FromArgb(0, 255, 255, 255), 1.0)
                //        }
                //    }
                //};

                var rim = CreateRectWithCircularHole(
                            outerW: 50, outerH: 50,
                            holeDiameter: 40,
                            cornerRadius: 0,
                            fill: Brushes.CornflowerBlue,
                            stroke: Brushes.DimGray,
                            strokeThickness: 0);

                cell.Children.Add(rim);
                //cell.Children.Add(glow);
                Grid.SetRow(cell, row);
                Grid.SetColumn(cell, column);
                SlotGrid.Children.Add(cell);
            }
        }
    }

    /// <summary>
    /// Crée un Path WPF représentant un rectangle avec un trou circulaire.
    /// </summary>
    /// <param name="outerW">Largeur du rectangle extérieur.</param>
    /// <param name="outerH">Hauteur du rectangle extérieur.</param>
    /// <param name="holeDiameter">Diamètre du trou (cercle).</param>
    /// <param name="cornerRadius">Rayon d’arrondi des coins du rectangle extérieur (0 = coins vifs).</param>
    /// <param name="holeCenter">Centre du trou (si null, centré automatiquement).</param>
    /// <param name="fill">Brosse de remplissage (par défaut SteelBlue).</param>
    /// <param name="stroke">Brosse de contour (par défaut Transparent).</param>
    /// <param name="strokeThickness">Épaisseur du contour.</param>
    /// <returns>Un Path prêt à être ajouté au visuel.</returns>
    public static Path CreateRectWithCircularHole(
        double outerW, double outerH,
        double holeDiameter,
        double cornerRadius = 0,
        Point? holeCenter = null,
        Brush? fill = null,
        Brush? stroke = null,
        double strokeThickness = 0)
    {
        // Géométrie extérieure (rectangle)
        var outer = new RectangleGeometry(new Rect(0, 0, outerW, outerH), cornerRadius, cornerRadius);

        // Centre du trou (par défaut : centre du rectangle)
        var center = holeCenter ?? new Point(outerW / 2.0, outerH / 2.0);

        // Géométrie intérieure (cercle = trou)
        double r = holeDiameter / 2.0;
        var inner = new EllipseGeometry(center, r, r);

        // Soustraction : extérieur - intérieur
        var ring = new CombinedGeometry(GeometryCombineMode.Exclude, outer, inner);

        // Construction du Path
        var rim = new Path
        {
            Data = ring,
            Fill = fill ?? Brushes.SteelBlue,
            Stroke = stroke ?? Brushes.Transparent,
            StrokeThickness = strokeThickness,
            SnapsToDevicePixels = true
        };

        // Pour un rendu net sur pixels entiers si StrokeThickness > 0
        RenderOptions.SetEdgeMode(rim, EdgeMode.Aliased);

        return rim;
    }


    private void RefreshLayout()
    {
        UpdateIndicatorGeometry();
        UpdateBoardMask();
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

    private void UpdateBoardMask()
    {
        if (!IsLoaded)
        {
            return;
        }

        var width = BoardCanvas.ActualWidth;
        var height = BoardCanvas.ActualHeight;
        if (width <= 0 || height <= 0)
        {
            return;
        }

        var baseGeometry = new RectangleGeometry(new Rect(0, 0, width, height), 22, 22);
        var cellSize = width / GameBoard.Columns;
        var holeRadius = cellSize * 0.36;

        Geometry boardGeometry = baseGeometry;
        for (var row = 0; row < GameBoard.Rows; row++)
        {
            for (var column = 0; column < GameBoard.Columns; column++)
            {
                var center = new Point(column * cellSize + cellSize / 2, row * cellSize + cellSize / 2);
                var hole = new EllipseGeometry(center, holeRadius, holeRadius);
                boardGeometry = Geometry.Combine(boardGeometry, hole, GeometryCombineMode.Exclude, Transform.Identity);
            }
        }

        BoardFrontPath.Data = boardGeometry;
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

    private void SelectionCanvas_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        var pointer = e.GetPosition(element);
        UpdateSelectionFromPointer(pointer, element.ActualWidth);
    }

    private void SelectionCanvas_OnMouseLeave(object sender, MouseEventArgs e)
    {
        this.IndicatorCanvas.Visibility = Visibility.Collapsed;
        // No specific action required on leave for now, but keeping the handler allows
        // future visual feedback (such as hiding the indicator) without altering logic.
    }

    private async void SelectionCanvas_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        var pointer = e.GetPosition(element);
        UpdateSelectionFromPointer(pointer, element.ActualWidth);
        await HandlePlayerMoveAsync();
    }

    private void UpdateSelectionFromPointer(Point pointer, double surfaceWidth)
    {
        if (_isBusy || !_isPlayerTurn)
        {
            return;
        }

        if (surfaceWidth <= 0)
        {
            return;
        }

        var columnWidth = surfaceWidth / GameBoard.Columns;
        if (columnWidth <= 0)
        {
            return;
        }

        var column = (int)Math.Floor(pointer.X / columnWidth);
        column = Math.Clamp(column, 0, GameBoard.Columns - 1);

        if (column != _selectedColumn)
        {
            _selectedColumn = column;
            UpdateIndicatorGeometry();
        }
    }

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

    private void SelectionCanvas_OnMouseEnter(object sender, MouseEventArgs e)
    {
        this.IndicatorCanvas.Visibility = Visibility.Visible;
    }
}
