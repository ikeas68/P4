using Puissance4Game.GameLogic;
using UIKit;

namespace Puissance4IOS;

public class GameViewController : UIViewController
{
    private readonly GameViewModel _viewModel = new();
    private BoardView? _boardView;
    private UILabel? _statusLabel;
    private UIButton? _resetButton;
    private UISegmentedControl? _modeSelector;
    private UIActivityIndicatorView? _activityIndicator;

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        View.BackgroundColor = UIColor.SystemBackgroundColor;

        ConfigureSubviews();
        LayoutSubviews();

        _viewModel.ResetGame();
        UpdateStatusLabel();
        _boardView?.SetNeedsDisplay();
    }

    private void ConfigureSubviews()
    {
        _statusLabel = new UILabel
        {
            TextAlignment = UITextAlignment.Center,
            Lines = 2,
            Font = UIFont.PreferredHeadline,
            TranslatesAutoresizingMaskIntoConstraints = false
        };

        _modeSelector = new UISegmentedControl(new[] { "2 Joueurs", "IA" })
        {
            SelectedSegment = 0,
            TranslatesAutoresizingMaskIntoConstraints = false
        };
        _modeSelector.ValueChanged += (_, _) =>
        {
            var playAgainstAi = _modeSelector.SelectedSegment == 1;
            _viewModel.SetMode(playAgainstAi);
            UpdateStatusLabel();
            _boardView?.SetNeedsDisplay();
        };

        _resetButton = new UIButton(UIButtonType.System);
        _resetButton.SetTitle("Nouvelle partie", UIControlState.Normal);
        _resetButton.TranslatesAutoresizingMaskIntoConstraints = false;
        _resetButton.TouchUpInside += (_, _) =>
        {
            ToggleLoading(false);
            _viewModel.ResetGame();
            UpdateStatusLabel();
            _boardView?.SetNeedsDisplay();
        };

        _boardView = new BoardView(_viewModel)
        {
            TranslatesAutoresizingMaskIntoConstraints = false
        };
        _boardView.ColumnTapped += HandleColumnTapped;

        _activityIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Large)
        {
            HidesWhenStopped = true,
            TranslatesAutoresizingMaskIntoConstraints = false
        };

        View.AddSubviews(_statusLabel, _modeSelector, _resetButton, _boardView, _activityIndicator);
    }

    private void LayoutSubviews()
    {
        if (_statusLabel is null || _modeSelector is null || _resetButton is null || _boardView is null || _activityIndicator is null)
        {
            return;
        }

        var guide = View.SafeAreaLayoutGuide;

        NSLayoutConstraint.ActivateConstraints(new[]
        {
            _statusLabel.TopAnchor.ConstraintEqualTo(guide.TopAnchor, 16),
            _statusLabel.LeadingAnchor.ConstraintEqualTo(guide.LeadingAnchor, 16),
            _statusLabel.TrailingAnchor.ConstraintEqualTo(guide.TrailingAnchor, -16),

            _modeSelector.TopAnchor.ConstraintEqualTo(_statusLabel.BottomAnchor, 16),
            _modeSelector.CenterXAnchor.ConstraintEqualTo(guide.CenterXAnchor),

            _resetButton.TopAnchor.ConstraintEqualTo(_modeSelector.BottomAnchor, 16),
            _resetButton.CenterXAnchor.ConstraintEqualTo(guide.CenterXAnchor),

            _boardView.TopAnchor.ConstraintEqualTo(_resetButton.BottomAnchor, 24),
            _boardView.LeadingAnchor.ConstraintEqualTo(guide.LeadingAnchor, 16),
            _boardView.TrailingAnchor.ConstraintEqualTo(guide.TrailingAnchor, -16),
            _boardView.BottomAnchor.ConstraintEqualTo(guide.BottomAnchor, -24),

            _activityIndicator.CenterXAnchor.ConstraintEqualTo(_boardView.CenterXAnchor),
            _activityIndicator.CenterYAnchor.ConstraintEqualTo(_boardView.CenterYAnchor)
        });
    }

    private void HandleColumnTapped(int column)
    {
        if (!_viewModel.TryHumanMove(column))
        {
            return;
        }

        UpdateStatusLabel();
        _boardView?.SetNeedsDisplay();

        if (!_viewModel.ShouldTriggerAiMove)
        {
            return;
        }

        ToggleLoading(true);

        System.Threading.Tasks.Task.Run(() =>
        {
            _viewModel.PlayAiTurn();
            InvokeOnMainThread(() =>
            {
                ToggleLoading(false);
                UpdateStatusLabel();
                _boardView?.SetNeedsDisplay();
            });
        });
    }

    private void ToggleLoading(bool isLoading)
    {
        if (_activityIndicator is null)
        {
            return;
        }

        if (_boardView is not null)
        {
            _boardView.UserInteractionEnabled = !isLoading;
        }

        if (isLoading)
        {
            _activityIndicator.StartAnimating();
        }
        else
        {
            _activityIndicator.StopAnimating();
        }
    }

    private void UpdateStatusLabel()
    {
        if (_statusLabel is null)
        {
            return;
        }

        if (_viewModel.Winner.HasValue)
        {
            var winnerText = _viewModel.Winner.Value == GameBoard.PlayerOne ? "Joueur 1" : "Joueur 2";
            _statusLabel.Text = $"{winnerText} a gagné !";
            return;
        }

        if (_viewModel.IsDraw)
        {
            _statusLabel.Text = "Égalité";
            return;
        }

        if (_viewModel.PlayAgainstAi && !_viewModel.IsHumanTurn)
        {
            _statusLabel.Text = "Tour de l'IA";
            return;
        }

        var currentPlayer = _viewModel.CurrentPlayer == GameBoard.PlayerOne ? "Joueur 1" : "Joueur 2";
        _statusLabel.Text = $"Tour de {currentPlayer}";
    }
}
