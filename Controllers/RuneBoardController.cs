using System.Numerics;
using runeforge.Configs;
using runeforge.Factories;
using runeforge.Models;
using runeforge.Systems;

namespace runeforge.Controllers;

public sealed class RuneBoardController
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
            DraggedRunePosition = new Vector2(mousePosition.X, mousePosition.Y);
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
        UpdateBagVisualState(deltaTime, mousePosition);
        UpdateRuneInteractionState(mousePosition);
    }

    public void ApplyDefeatState(bool isLeftMouseDown, ref bool wasLeftMouseDown)
    {
        DraggedRune = null;
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

    private void TryStartDragging(Point mousePosition)
    {
        var rune = GetRuneAtPoint(mousePosition, excludedRune: null);
        if (rune == null)
        {
            return;
        }

        DraggedRune = rune;
        DraggedRunePosition = rune.Transform.Position;
    }

    private void HandleDragRelease(Point mousePosition)
    {
        var sourceRune = DraggedRune;
        DraggedRune = null;

        if (sourceRune == null)
        {
            return;
        }

        if (Board.BagBounds.Contains(mousePosition))
        {
            StartBagInsertion(sourceRune);
            return;
        }

        var targetRune = GetRuneAtPoint(mousePosition, sourceRune);
        if (targetRune == null || !CanMerge(sourceRune, targetRune))
        {
            return;
        }

        StartMerge(sourceRune, targetRune);
    }

    private void UpdateBagVisualState(float deltaTime, Point mousePosition)
    {
        _isBagHovered = Board.BagBounds.Contains(mousePosition);
        _isDraggingOverBag = DraggedRune != null && _isBagHovered;

        var targetBlend = _isDraggingOverBag ? 1f : 0f;
        _bagHoverBlend = Approach(_bagHoverBlend, targetBlend, deltaTime * 12f);
        State.Ui.UseOpenBagSprite = _isDraggingOverBag;
        State.Ui.BagScale = 1f + (_bagHoverBlend * 0.1f) + EvaluateBagClickPulse() + EvaluateBagInsertPulse();
    }

    private void UpdateRuneInteractionState(Point mousePosition)
    {
        var hoveredMergeTarget = GetHoveredMergeTarget(mousePosition);

        for (var i = 0; i < State.Runes.Count; i++)
        {
            var rune = State.Runes[i];
            rune.Presentation.SetDragged(ReferenceEquals(rune, DraggedRune));
            rune.Presentation.SetMergeHoverTarget(ReferenceEquals(rune, hoveredMergeTarget));
        }
    }

    private RuneEntity? GetHoveredMergeTarget(Point mousePosition)
    {
        if (DraggedRune == null)
        {
            return null;
        }

        var hoveredRune = GetRuneAtPoint(mousePosition, DraggedRune);
        if (hoveredRune == null || !CanMerge(DraggedRune, hoveredRune))
        {
            return null;
        }

        return hoveredRune;
    }

    private void StartMerge(RuneEntity sourceRune, RuneEntity targetRune)
    {
        var mergedTier = RuneTierTuning.Clamp(Math.Max(sourceRune.Stats.Tier, targetRune.Stats.Tier) + 1);
        var mergedType = GetRandomSelectedRuneType();

        sourceRune.Presentation.BeginMergeInto(DraggedRunePosition, targetRune.Transform.Position);
        targetRune.Presentation.SetReservedForMerge(true);
        _pendingMerges.Add(new PendingMerge
        {
            SourceRune = sourceRune,
            TargetRune = targetRune,
            MergedTier = mergedTier,
            MergedType = mergedType
        });
    }

    private void StartBagInsertion(RuneEntity rune)
    {
        rune.Presentation.BeginBagInsert(DraggedRunePosition, GetBagCenter());
        _pendingBagInsertions.Add(rune);
        _bagInsertPulseElapsed = 0f;
    }

    private RuneEntity? GetRuneAtPoint(Point mousePosition, RuneEntity? excludedRune)
    {
        for (var i = State.Runes.Count - 1; i >= 0; i--)
        {
            var rune = State.Runes[i];
            if (!rune.Presentation.IsInteractable || ReferenceEquals(rune, excludedRune))
            {
                continue;
            }

            var cellBounds = Board.Grid.GetCell(rune.Grid.Row, rune.Grid.Column).Bounds;
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
        if (_freeCellsBuffer.Count == 0 || !State.Economy.TrySpendRuneSpawnCost())
        {
            return;
        }

        var cell = _freeCellsBuffer[_random.Next(_freeCellsBuffer.Count)];
        var rune = _runeFactory.Create(cell, GetRandomSelectedRuneType());
        rune.Presentation.BeginSpawnFromBag(GetBagCenter(), rune.Transform.Position);
        State.Runes.Add(rune);
        _effectAnimations.TrySpawnRuneSpawnAnimation(State, GetBagCenter(), rune.Stats.Color);
        _bagClickPulseElapsed = 0f;
    }

    private void PopulateFreeCellsBuffer()
    {
        _freeCellsBuffer.Clear();

        foreach (var cell in Board.Grid.Cells)
        {
            if (!IsCellOccupied(cell.Row, cell.Column))
            {
                _freeCellsBuffer.Add(cell);
            }
        }
    }

    private bool IsCellOccupied(int row, int column)
    {
        foreach (var rune in State.Runes)
        {
            if (rune.Grid.Row == row && rune.Grid.Column == column)
            {
                return true;
            }
        }

        return false;
    }

    private bool TryGetEmptyCellAtPoint(Point mousePosition, out TableGrid.GridCell cell)
    {
        foreach (var candidateCell in Board.Grid.Cells)
        {
            if (!candidateCell.Bounds.Contains(mousePosition))
            {
                continue;
            }

            if (IsCellOccupied(candidateCell.Row, candidateCell.Column))
            {
                break;
            }

            cell = candidateCell;
            return true;
        }

        cell = default;
        return false;
    }

    private RuneType GetRandomSelectedRuneType()
    {
        var selectedRunes = State.Ui.BuildSelection.SelectedRunes;
        return selectedRunes[_random.Next(selectedRunes.Count)];
    }

    private Vector2 GetBagCenter()
    {
        return new Vector2(
            Board.BagBounds.Left + (Board.BagBounds.Width * 0.5f),
            Board.BagBounds.Top + (Board.BagBounds.Height * 0.5f));
    }

    private static bool CanMerge(RuneEntity sourceRune, RuneEntity targetRune)
    {
        return sourceRune.Stats.Tier < RuneTierTuning.MaxTier &&
            targetRune.Stats.Tier < RuneTierTuning.MaxTier &&
            sourceRune.Stats.Tier == targetRune.Stats.Tier &&
            sourceRune.Stats.Type == targetRune.Stats.Type;
    }

    private static float Approach(float value, float target, float step)
    {
        if (value < target)
        {
            return Math.Min(value + step, target);
        }

        return Math.Max(value - step, target);
    }

    private float EvaluateBagClickPulse()
    {
        if (_bagClickPulseElapsed >= BagClickPulseDuration)
        {
            return 0f;
        }

        var progress = _bagClickPulseElapsed / BagClickPulseDuration;
        if (progress <= BagClickPulseChargeRatio)
        {
            var chargeProgress = progress / BagClickPulseChargeRatio;
            return SmoothStep(chargeProgress) * BagClickPulseScale;
        }

        var releaseProgress = (progress - BagClickPulseChargeRatio) / (1f - BagClickPulseChargeRatio);
        return (1f - releaseProgress) * BagClickPulseScale;
    }

    private float EvaluateBagInsertPulse()
    {
        if (_bagInsertPulseElapsed >= BagInsertPulseDuration)
        {
            return 0f;
        }

        var progress = _bagInsertPulseElapsed / BagInsertPulseDuration;
        if (progress <= BagInsertPulseChargeRatio)
        {
            var chargeProgress = progress / BagInsertPulseChargeRatio;
            return SmoothStep(chargeProgress) * BagInsertPulseScale;
        }

        var releaseProgress = (progress - BagInsertPulseChargeRatio) / (1f - BagInsertPulseChargeRatio);
        return (1f - releaseProgress) * BagInsertPulseScale;
    }

    private static float SmoothStep(float value)
    {
        var clamped = Math.Clamp(value, 0f, 1f);
        return clamped * clamped * (3f - (2f * clamped));
    }
}
