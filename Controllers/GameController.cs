using System.Numerics;
using runeforge.Factories;
using runeforge.Models;
using runeforge.Systems;

namespace runeforge.Controllers;

public sealed class GameController
{
    private static readonly RuneType[] RunePool = Enum.GetValues<RuneType>();

    private readonly GameBoard _board;
    private readonly GameState _gameState;
    private readonly RuneFactory _runeFactory;
    private readonly EnemySystem _enemySystem;
    private readonly RuneSystem _runeSystem;
    private readonly ProjectileSystem _projectileSystem;
    private readonly EffectSystem _effectSystem;
    private readonly Random _random;
    private readonly List<TableGrid.GridCell> _freeCellsBuffer;

    private bool _wasLeftMouseDown;
    private Rune? _draggedRune;
    private Vector2 _draggedRunePosition;

    public GameController(GameBoard board)
    {
        _board = board;
        _gameState = new GameState();
        _runeFactory = new RuneFactory();
        _enemySystem = new EnemySystem();
        _runeSystem = new RuneSystem();
        _projectileSystem = new ProjectileSystem();
        _effectSystem = new EffectSystem();
        _random = new Random();
        _freeCellsBuffer = new List<TableGrid.GridCell>(16);
    }

    public GameState State => _gameState;

    public Rune? DraggedRune => _draggedRune;

    public Vector2 DraggedRunePosition => _draggedRunePosition;

    public void Update(float deltaTime, Point mousePosition, bool isLeftMouseDown)
    {
        HandleInput(mousePosition, isLeftMouseDown);
        _enemySystem.Update(_gameState, _board.Path, deltaTime);
        _runeSystem.Update(_gameState.Runes, _gameState.Enemies, _gameState.Projectiles, deltaTime);
        _projectileSystem.Update(_gameState.Projectiles, _gameState.Enemies, deltaTime, _effectSystem);
    }

    private void HandleInput(Point mousePosition, bool isLeftMouseDown)
    {
        var leftPressed = isLeftMouseDown && !_wasLeftMouseDown;
        var leftReleased = !isLeftMouseDown && _wasLeftMouseDown;

        if (leftPressed)
        {
            TryStartDragging(mousePosition);
        }

        if (_draggedRune != null && isLeftMouseDown)
        {
            _draggedRunePosition = new Vector2(mousePosition.X, mousePosition.Y);
        }

        if (_draggedRune != null && leftReleased)
        {
            HandleDragRelease(mousePosition);
        }
        else if (_draggedRune == null && leftPressed && _board.BagBounds.Contains(mousePosition))
        {
            SpawnRandomRune();
        }

        _wasLeftMouseDown = isLeftMouseDown;
    }

    private void TryStartDragging(Point mousePosition)
    {
        var rune = GetRuneAtPoint(mousePosition, excludedRune: null);
        if (rune == null)
        {
            return;
        }

        _draggedRune = rune;
        _draggedRunePosition = rune.Position;
    }

    private void HandleDragRelease(Point mousePosition)
    {
        var sourceRune = _draggedRune;
        _draggedRune = null;

        if (sourceRune == null)
        {
            return;
        }

        if (_board.BagBounds.Contains(mousePosition))
        {
            _gameState.Runes.Remove(sourceRune);
            return;
        }

        var targetRune = GetRuneAtPoint(mousePosition, sourceRune);
        if (targetRune == null || !CanMerge(sourceRune, targetRune))
        {
            return;
        }

        MergeRunes(sourceRune, targetRune);
    }

    private void MergeRunes(Rune sourceRune, Rune targetRune)
    {
        _gameState.Runes.Remove(sourceRune);
        _gameState.Runes.Remove(targetRune);

        var mergedTier = Math.Max(sourceRune.Tier, targetRune.Tier) + 1;
        var mergedType = GetRandomRuneType();
        var targetCell = _board.Grid.GetCell(targetRune.GridRow, targetRune.GridColumn);
        var mergedRune = _runeFactory.Create(targetCell, mergedType, mergedTier);

        _gameState.Runes.Add(mergedRune);
    }

    private static bool CanMerge(Rune sourceRune, Rune targetRune)
    {
        return sourceRune.Tier == targetRune.Tier &&
            sourceRune.Color == targetRune.Color;
    }

    private Rune? GetRuneAtPoint(Point mousePosition, Rune? excludedRune)
    {
        for (var i = _gameState.Runes.Count - 1; i >= 0; i--)
        {
            var rune = _gameState.Runes[i];
            if (ReferenceEquals(rune, excludedRune))
            {
                continue;
            }

            var cellBounds = _board.Grid.GetCell(rune.GridRow, rune.GridColumn).Bounds;
            if (cellBounds.Contains(mousePosition))
            {
                return rune;
            }
        }

        return null;
    }

    private void SpawnRandomRune()
    {
        PopulateFreeCellsBuffer();
        if (_freeCellsBuffer.Count == 0)
        {
            return;
        }

        var cell = _freeCellsBuffer[_random.Next(_freeCellsBuffer.Count)];
        _gameState.Runes.Add(_runeFactory.Create(cell, GetRandomRuneType()));
    }

    private void PopulateFreeCellsBuffer()
    {
        _freeCellsBuffer.Clear();

        foreach (var cell in _board.Grid.Cells)
        {
            if (!IsCellOccupied(cell.Row, cell.Column))
            {
                _freeCellsBuffer.Add(cell);
            }
        }
    }

    private bool IsCellOccupied(int row, int column)
    {
        foreach (var rune in _gameState.Runes)
        {
            if (rune.GridRow == row && rune.GridColumn == column)
            {
                return true;
            }
        }

        return false;
    }

    private RuneType GetRandomRuneType()
    {
        return RunePool[_random.Next(RunePool.Length)];
    }
}
