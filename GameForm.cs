using runeforge.Controllers;
using runeforge.Models;
using runeforge.Views;

namespace runeforge;

public partial class GameForm : Form
{
    private const int CursorDrawSize = 36;

    private readonly GameLoop _gameLoop;
    private readonly GameBoard _board;
    private readonly GameController _controller;
    private readonly GameRenderer _renderer;
    private readonly Bitmap _defaultCursorTexture;
    private readonly Bitmap _addCursorTexture;
    private readonly Bitmap _moveUpCursorTexture;
    private readonly Bitmap _subtractCursorTexture;

    private Point _mousePosition;
    private bool _isLeftMouseDown;

    public GameForm()
    {
        InitializeComponent();

        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.Opaque,
            true);

        UpdateStyles();

        _board = new GameBoard(ClientSize.Width, ClientSize.Height);
        _controller = new GameController(_board);
        _renderer = new GameRenderer(_board);
        _defaultCursorTexture = LoadCursorTexture("Cursor Default");
        _addCursorTexture = LoadCursorTexture("Cursor Add");
        _moveUpCursorTexture = LoadCursorTexture("Cursor Move Up");
        _subtractCursorTexture = LoadCursorTexture("Cursor Substract");
        _gameLoop = new GameLoop(UpdateFrame, Invalidate);

        _gameLoop.Start();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(GameRenderer.BackgroundColor);
        _renderer.Draw(e.Graphics, _controller.State);
        DrawCursor(e.Graphics);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        Cursor.Show();
        _gameLoop.Dispose();
        _renderer.Dispose();
        _defaultCursorTexture.Dispose();
        _addCursorTexture.Dispose();
        _moveUpCursorTexture.Dispose();
        _subtractCursorTexture.Dispose();
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

    protected override void OnMouseMove(MouseEventArgs e)
    {
        _mousePosition = e.Location;
        base.OnMouseMove(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        _mousePosition = e.Location;

        if (e.Button == MouseButtons.Left)
        {
            _isLeftMouseDown = true;
            Capture = true;
        }

        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _mousePosition = e.Location;

        if (e.Button == MouseButtons.Left)
        {
            _isLeftMouseDown = false;
            Capture = false;
        }

        base.OnMouseUp(e);
    }

    private void UpdateFrame(float deltaTime)
    {
        _controller.Update(deltaTime, _mousePosition, _isLeftMouseDown);
    }

    private void DrawCursor(Graphics graphics)
    {
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        var cursorTexture = _defaultCursorTexture;
        if (ShouldUseMoveUpCursor())
        {
            cursorTexture = _moveUpCursorTexture;
        }
        else if (ShouldUseSubtractCursor())
        {
            cursorTexture = _subtractCursorTexture;
        }
        else if (ShouldUseAddCursor())
        {
            cursorTexture = _addCursorTexture;
        }

        graphics.DrawImage(cursorTexture, _mousePosition.X, _mousePosition.Y, CursorDrawSize, CursorDrawSize);
    }

    private bool ShouldUseMoveUpCursor()
    {
        return _controller.CanMergeDraggedRuneAt(_mousePosition);
    }

    private bool ShouldUseSubtractCursor()
    {
        return !_controller.State.Ui.BuildSelection.IsOpen &&
            _controller.State.Ui.DraggedRune != null &&
            _board.BagBounds.Contains(_mousePosition);
    }

    private bool ShouldUseAddCursor()
    {
        return !_controller.State.Ui.BuildSelection.IsOpen &&
            _controller.State.Ui.DraggedRune == null &&
            _board.BagBounds.Contains(_mousePosition);
    }

    private static Bitmap LoadCursorTexture(string textureName)
    {
        var texturePath = ResolveCursorTexturePath(textureName);
        return new Bitmap(texturePath);
    }

    private static string ResolveCursorTexturePath(string textureName)
    {
        string[] candidatePaths =
        [
            Path.Combine(AppContext.BaseDirectory, "Assets", "Sprites", textureName + ".png"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Assets", "Sprites", textureName + ".png"))
        ];

        foreach (var path in candidatePaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        throw new FileNotFoundException($"Cursor texture '{textureName}' was not found.");
    }
}
