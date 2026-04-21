using System.Numerics;
using runeforge.Configs;
using runeforge.Factories;
using runeforge.Models;
using runeforge.Systems;

namespace runeforge.Controllers;

public sealed partial class RuneBoardController
{
    private const float BagClickPulseDuration = 0.14f;
    private const float BagClickPulseScale = 0.08f;
    private const float BagClickPulseChargeRatio = 0.4f;
    private const float BagInsertPulseDuration = 0.3f;
    private const float BagInsertPulseScale = 0.12f;
    private const float BagInsertPulseChargeRatio = 0.32f;

    private sealed class PendingMerge
    {
        public required RuneEntity SourceRune { get; init; }

        public required RuneEntity TargetRune { get; init; }

        public required RuneType MergedType { get; init; }

        public required int MergedTier { get; init; }
    }

    private readonly GameModel _model;
    private readonly RuneFactory _runeFactory;
    private readonly EffectAnimationSystem _effectAnimations;
    private readonly RuneBoardHagalazController _hagalazController;
    private readonly Random _random;
    private readonly List<TableGrid.GridCell> _freeCellsBuffer;
    private readonly List<PendingMerge> _pendingMerges;
    private readonly List<RuneEntity> _pendingBagInsertions;

    private float _bagHoverBlend;
    private float _bagClickPulseElapsed = BagClickPulseDuration;
    private float _bagInsertPulseElapsed = BagInsertPulseDuration;
    private bool _isBagHovered;
    private bool _isDraggingOverBag;

    public RuneBoardController(GameModel model, RuneFactory runeFactory, EffectAnimationSystem effectAnimations)
    {
        _model = model;
        _runeFactory = runeFactory;
        _effectAnimations = effectAnimations;
        _hagalazController = new RuneBoardHagalazController(model.State, model.Board, effectAnimations);
        _random = new Random();
        _freeCellsBuffer = new List<TableGrid.GridCell>(16);
        _pendingMerges = new List<PendingMerge>(8);
        _pendingBagInsertions = new List<RuneEntity>(8);
    }

    private GameBoard Board => _model.Board;

    private GameState State => _model.State;

    private RuneEntity? DraggedRune
    {
        get => State.Ui.DraggedRune;
        set => State.Ui.DraggedRune = value;
    }

    private Vector2 DraggedRunePosition
    {
        get => State.Ui.DraggedRunePosition;
        set => State.Ui.DraggedRunePosition = value;
    }

    private Vector2 DraggedRuneGrabOffset
    {
        get => State.Ui.DraggedRuneGrabOffset;
        set => State.Ui.DraggedRuneGrabOffset = value;
    }

    public bool CanMergeDraggedRuneAt(Point mousePosition)
    {
        if (State.Ui.BuildSelection.IsOpen)
        {
            return false;
        }

        return DraggedRune != null && GetHoveredMergeTarget(mousePosition) != null;
    }

    public bool CanOpenDevSpawnMenuAt(Point mousePosition)
    {
        return !State.Ui.BuildSelection.IsOpen &&
            DraggedRune == null &&
            TryGetEmptyCellAtPoint(mousePosition, out _);
    }

    public bool TrySpawnDevRuneAt(Point mousePosition, RuneType runeType, int tier)
    {
        if (!TryGetEmptyCellAtPoint(mousePosition, out var cell))
        {
            return false;
        }

        State.Runes.Add(_runeFactory.Create(cell, runeType, RuneTierTuning.Clamp(tier)));
        return true;
    }

    public void HandleInput(Point mousePosition, bool isLeftMouseDown, ref bool wasLeftMouseDown)
    {
        var leftPressed = isLeftMouseDown && !wasLeftMouseDown;
        var leftReleased = !isLeftMouseDown && wasLeftMouseDown;

        if (leftPressed)
        {
            TryStartDragging(mousePosition);
        }

        if (DraggedRune != null && isLeftMouseDown)
        {
            DraggedRunePosition = new Vector2(mousePosition.X, mousePosition.Y) + DraggedRuneGrabOffset;
        }

        if (DraggedRune != null && leftReleased)
        {
            HandleDragRelease(mousePosition);
        }
        else if (DraggedRune == null && leftPressed && Board.BagBounds.Contains(mousePosition))
        {
            SpawnRandomRune();
        }

        wasLeftMouseDown = isLeftMouseDown;
    }

    public void UpdateViewState(float deltaTime, Point mousePosition)
    {
        _bagClickPulseElapsed = Math.Min(BagClickPulseDuration, _bagClickPulseElapsed + deltaTime);
        _bagInsertPulseElapsed = Math.Min(BagInsertPulseDuration, _bagInsertPulseElapsed + deltaTime);
        _hagalazController.Update(deltaTime);
        UpdateBagVisualState(deltaTime, mousePosition);
        UpdateRuneInteractionState(mousePosition);
    }

    public void ApplyDefeatState(bool isLeftMouseDown, ref bool wasLeftMouseDown)
    {
        DraggedRune = null;
        DraggedRuneGrabOffset = Vector2.Zero;
        _hagalazController.Reset();
        _bagHoverBlend = 0f;
        _isDraggingOverBag = false;
        _isBagHovered = false;
        State.Ui.UseOpenBagSprite = false;
        State.Ui.BagScale = 1f;
        wasLeftMouseDown = isLeftMouseDown;
        UpdateRuneInteractionState(Point.Empty);
    }

    public void ResolveCompletedAnimations()
    {
        for (var i = _pendingMerges.Count - 1; i >= 0; i--)
        {
            var pendingMerge = _pendingMerges[i];
            if (!pendingMerge.SourceRune.Presentation.IsTransientAnimationComplete)
            {
                continue;
            }

            State.Runes.Remove(pendingMerge.SourceRune);
            State.Runes.Remove(pendingMerge.TargetRune);

            var targetCell = Board.Grid.GetCell(pendingMerge.TargetRune.Grid.Row, pendingMerge.TargetRune.Grid.Column);
            var mergedRune = _runeFactory.Create(targetCell, pendingMerge.MergedType, pendingMerge.MergedTier);
            mergedRune.Presentation.TriggerMergePop();
            State.Runes.Add(mergedRune);

            _effectAnimations.TrySpawnMergeAnimation(State, mergedRune.Transform.Position, mergedRune.Stats.Color);

            _pendingMerges.RemoveAt(i);
        }

        for (var i = _pendingBagInsertions.Count - 1; i >= 0; i--)
        {
            var rune = _pendingBagInsertions[i];
            if (!rune.Presentation.IsTransientAnimationComplete)
            {
                continue;
            }

            _effectAnimations.TrySpawnRuneRemoveAnimation(State, GetBagCenter(), rune.Stats.Color);
            State.Runes.Remove(rune);
            _pendingBagInsertions.RemoveAt(i);
        }
    }
}
