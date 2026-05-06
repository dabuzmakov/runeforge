using runeforge.Controllers;
using runeforge.Models;
using runeforge.Systems;
using runeforge.Views;

namespace runeforge;

public partial class GameForm : Form
{
    private const int WindowDragHeight = 72;

    private readonly GameLoop _gameLoop;
    private readonly GameModel _model;
    private readonly GameController _controller;
    private readonly GameRenderer _renderer;
    private readonly GameCursorRenderer _cursorRenderer;
    private readonly Icon _appIcon;

    private Point _mousePosition;
    private Point _windowDragOffset;
    private bool _isLeftMouseDown;
    private bool _isWindowDragging;

    public GameForm()
    {
        InitializeComponent();
        _appIcon = LoadAppIcon();
        Icon = _appIcon;

        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.Opaque,
            true);

        UpdateStyles();

        _model = new GameModel(new GameBoard(ClientSize.Width, ClientSize.Height));
        _controller = new GameController(_model);
        _renderer = new GameRenderer(_model);
        _cursorRenderer = new GameCursorRenderer();
        _gameLoop = new GameLoop(UpdateFrame, RenderFrame);

        _gameLoop.Start();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        _renderer.Draw(e.Graphics);
        DrawCursor(e.Graphics);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        Cursor.Show();
        _controller.SaveProfile();
        _gameLoop.Dispose();
        _renderer.Dispose();
        _cursorRenderer.Dispose();
        _appIcon.Dispose();
        base.OnFormClosed(e);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        Cursor.Hide();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        Cursor.Show();
        base.OnMouseLeave(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            Close();
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        _mousePosition = e.Location;

        if (_isWindowDragging)
        {
            Location = new Point(
                Cursor.Position.X - _windowDragOffset.X,
                Cursor.Position.Y - _windowDragOffset.Y);
            return;
        }

        base.OnMouseMove(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        _mousePosition = e.Location;

        if (e.Button == MouseButtons.Left)
        {
            if (HandleLeftMouseDown(e.Location))
            {
                return;
            }
        }
        else if (e.Button == MouseButtons.Right)
        {
            _controller.TryToggleTiwazModeAt(e.Location);
        }

        base.OnMouseDown(e);
    }

    private bool HandleLeftMouseDown(Point location)
    {
        if (HandleStartScreenClick(location) ||
            HandleGameOverPopupClick(location) ||
            HandlePausePopupClick(location))
        {
            _isLeftMouseDown = false;
            return true;
        }

        if (HandleTopButtonClick(location))
        {
            _isLeftMouseDown = false;
            return true;
        }

        if (IsWindowDragArea(location))
        {
            StartWindowDrag(location);
            return true;
        }

        _isLeftMouseDown = true;
        Capture = true;
        return false;
    }

    private bool HandleTopButtonClick(Point location)
    {
        if (TopButtonLayout.GetExitButtonBounds(_model.Board.ViewportBounds).Contains(location))
        {
            Close();
            return true;
        }

        if (CanUseHomeButton() &&
            TopButtonLayout.GetHomeButtonBounds(_model.Board.ViewportBounds).Contains(location))
        {
            _controller.ReturnToStartScreen();
            return true;
        }

        if (CanUsePauseButton() &&
            TopButtonLayout.GetPauseButtonBounds(_model.Board.ViewportBounds).Contains(location))
        {
            _controller.TogglePause();
            return true;
        }

        return false;
    }

    private void StartWindowDrag(Point location)
    {
        _isWindowDragging = true;
        _isLeftMouseDown = false;
        _windowDragOffset = location;
        Capture = true;
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _mousePosition = e.Location;

        if (e.Button == MouseButtons.Left)
        {
            _isWindowDragging = false;
            _isLeftMouseDown = false;
            Capture = false;
        }

        base.OnMouseUp(e);
    }

    private void UpdateFrame(float deltaTime)
    {
        _controller.Update(deltaTime, _mousePosition, _isLeftMouseDown);
    }

    private void RenderFrame()
    {
        Invalidate();
        Update();
    }

    private static bool IsWindowDragArea(Point location)
    {
        return location.Y >= 0 && location.Y <= WindowDragHeight;
    }

    private bool CanUseHomeButton()
    {
        return !_controller.State.Ui.IsStartScreenOpen &&
            _controller.State.Ui.BuildSelection.IsOpen;
    }

    private bool CanUsePauseButton()
    {
        return !_controller.State.Ui.IsStartScreenOpen &&
            !_controller.State.Ui.BuildSelection.IsOpen &&
            !_controller.State.IsDefeated;
    }

    private bool HandleStartScreenClick(Point location)
    {
        if (!_controller.State.Ui.IsStartScreenOpen)
        {
            return false;
        }

        if (StartScreenLayout.GetPlayButtonBounds(_model.Board.ViewportBounds).Contains(location))
        {
            _controller.OpenBuildSelectionFromStartScreen();
            return true;
        }

        return false;
    }

    private bool HandlePausePopupClick(Point location)
    {
        if (!_controller.State.IsPaused ||
            _controller.State.Ui.BuildSelection.IsOpen ||
            _controller.State.IsDefeated)
        {
            return false;
        }

        var popupBounds = PausePopupLayout.GetPopupBounds(_model.Board.ViewportBounds);
        if (PausePopupLayout.GetButtonBounds(popupBounds, PausePopupButtonKind.Resume).Contains(location))
        {
            _controller.ResumeGame();
            return true;
        }

        if (PausePopupLayout.GetButtonBounds(popupBounds, PausePopupButtonKind.Restart).Contains(location))
        {
            _controller.RestartGame();
            return true;
        }

        if (PausePopupLayout.GetButtonBounds(popupBounds, PausePopupButtonKind.Home).Contains(location))
        {
            _controller.ReturnToStartScreen();
            return true;
        }

        return popupBounds.Contains(location);
    }

    private bool HandleGameOverPopupClick(Point location)
    {
        if (!_controller.State.IsDefeated)
        {
            return false;
        }

        var popupBounds = GameOverPopupLayout.GetAnimatedPopupBounds(
            _model.Board.ViewportBounds,
            _controller.State.Ui.GameOverPopupVisibility);

        if (GameOverPopupLayout.GetButtonBounds(popupBounds, GameOverPopupButtonKind.Restart).Contains(location))
        {
            _controller.RestartGame();
            return true;
        }

        if (GameOverPopupLayout.GetButtonBounds(popupBounds, GameOverPopupButtonKind.Home).Contains(location))
        {
            _controller.ReturnToStartScreen();
            return true;
        }

        return popupBounds.Contains(location);
    }

    private void DrawCursor(Graphics graphics)
    {
        _cursorRenderer.Draw(graphics, _mousePosition, ResolveCursorKind());
    }

    private GameCursorKind ResolveCursorKind()
    {
        if (ShouldUseMoveUpCursor())
        {
            return GameCursorKind.MoveUp;
        }

        if (ShouldUseSubtractCursor())
        {
            return GameCursorKind.Subtract;
        }

        if (ShouldUseCannotUseCursor())
        {
            return GameCursorKind.CannotUse;
        }

        if (ShouldUseBuildGreenCursor())
        {
            return GameCursorKind.BuildGreen;
        }

        return ShouldUseAddCursor()
            ? GameCursorKind.Add
            : GameCursorKind.Default;
    }

    private bool ShouldUseMoveUpCursor()
    {
        return _controller.CanMergeDraggedRuneAt(_mousePosition);
    }

    private bool ShouldUseSubtractCursor()
    {
        return !_controller.State.Ui.BuildSelection.IsOpen &&
            !_controller.State.IsPaused &&
            !_controller.State.IsDefeated &&
            _controller.State.Ui.DraggedRune != null &&
            _model.Board.BagBounds.Contains(_mousePosition);
    }

    private bool ShouldUseAddCursor()
    {
        return CanUseRunePointAction() &&
            _model.Board.BagBounds.Contains(_mousePosition);
    }

    private bool ShouldUseBuildGreenCursor()
    {
        return CanUseRunePointAction() &&
            _model.Board.RerollBounds.Contains(_mousePosition);
    }

    private bool ShouldUseCannotUseCursor()
    {
        if (_controller.State.Ui.IsStartScreenOpen)
        {
            return false;
        }

        if (_controller.State.Ui.BuildSelection.IsOpen)
        {
            return !_controller.State.Ui.BuildSelection.CanStart &&
                BuildSelectionLayout.GetStartButtonBounds(_model.Board.ViewportBounds).Contains(_mousePosition);
        }

        if (_controller.State.Ui.DraggedRune == null &&
            !_controller.State.IsPaused &&
            !_controller.State.IsDefeated &&
            _model.Board.RerollBounds.Contains(_mousePosition) &&
            _controller.State.Runes.Count == 0)
        {
            return true;
        }

        return _controller.State.Ui.DraggedRune == null &&
            !_controller.State.IsPaused &&
            !_controller.State.IsDefeated &&
            !_controller.State.Economy.CanAffordCurrentRuneSpawn &&
            IsMouseOverRunePointAction();
    }

    private bool CanUseRunePointAction()
    {
        return !_controller.State.Ui.IsStartScreenOpen &&
            !_controller.State.Ui.BuildSelection.IsOpen &&
            !_controller.State.IsPaused &&
            !_controller.State.IsDefeated &&
            _controller.State.Ui.DraggedRune == null &&
            _controller.State.Economy.CanAffordCurrentRuneSpawn;
    }

    private bool IsMouseOverRunePointAction()
    {
        return _model.Board.BagBounds.Contains(_mousePosition) ||
            _model.Board.RerollBounds.Contains(_mousePosition);
    }

    private static Icon LoadAppIcon()
    {
        return new Icon(ResolveAppIconPath());
    }

    private static string ResolveAppIconPath()
    {
        return AssetResolver.ResolveFile("App", "runeforge-icon.ico");
    }

}
