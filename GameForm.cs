using runeforge.Controllers;
using runeforge.Models;
using runeforge.Views;

namespace runeforge;

public partial class GameForm : Form
{
    private readonly GameLoop _gameLoop;
    private readonly GameController _controller;
    private readonly GameRenderer _renderer;

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

        var board = new GameBoard(ClientSize.Width, ClientSize.Height);
        _controller = new GameController(board);
        _renderer = new GameRenderer(board);
        _gameLoop = new GameLoop(UpdateFrame, Invalidate);

        _gameLoop.Start();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(GameRenderer.BackgroundColor);
        _renderer.Draw(e.Graphics, _controller.State, _controller.DraggedRune, _controller.DraggedRunePosition);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _gameLoop.Dispose();
        _renderer.Dispose();
        base.OnFormClosed(e);
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
}
