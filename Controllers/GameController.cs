using System.Numerics;
using runeforge.Configs;
using runeforge.Factories;
using runeforge.Models;
using runeforge.Systems;

namespace runeforge.Controllers;

public sealed class GameController
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

    private readonly GameBoard _board;
    private readonly GameState _gameState;
    private readonly RuneFactory _runeFactory;
    private readonly EnemyFactory _enemyFactory;
    private readonly ProjectileFactory _projectileFactory;
    private readonly EnemySystem _enemySystem;
    private readonly RuneCombatSystem _runeCombatSystem;
    private readonly ProjectileSystem _projectileSystem;
    private readonly RuneEffectSystem _runeEffectSystem;
    private readonly EffectAnimationSystem _effectAnimationSystem;
    private readonly RunePresentationSystem _runePresentationSystem;
    private readonly Random _random;
    private readonly List<TableGrid.GridCell> _freeCellsBuffer;
    private readonly List<PendingMerge> _pendingMerges;
    private readonly List<RuneEntity> _pendingBagInsertions;

    private bool _wasLeftMouseDown;
    private float _bagHoverBlend;
    private float _bagClickPulseElapsed = BagClickPulseDuration;
    private float _bagInsertPulseElapsed = BagInsertPulseDuration;
    private bool _isBagHovered;
    private bool _isDraggingOverBag;

    public GameController(GameBoard board)
    {
        _board = board;
        _gameState = new GameState();
        _runeFactory = new RuneFactory();
        _enemyFactory = new EnemyFactory();
        _projectileFactory = new ProjectileFactory();
        _enemySystem = new EnemySystem(_enemyFactory);
        _runeCombatSystem = new RuneCombatSystem(_projectileFactory);
        _projectileSystem = new ProjectileSystem();
        _runeEffectSystem = new RuneEffectSystem();
        _effectAnimationSystem = new EffectAnimationSystem();
        _runePresentationSystem = new RunePresentationSystem();
        _random = new Random();
        _freeCellsBuffer = new List<TableGrid.GridCell>(16);
        _pendingMerges = new List<PendingMerge>(8);
        _pendingBagInsertions = new List<RuneEntity>(8);
    }

    public GameState State => _gameState;

    public bool CanMergeDraggedRuneAt(Point mousePosition)
    {
        if (_gameState.Ui.BuildSelection.IsOpen)
        {
            return false;
        }

        return DraggedRune != null && GetHoveredMergeTarget(mousePosition) != null;
    }

    private RuneEntity? DraggedRune
    {
        get => _gameState.Ui.DraggedRune;
        set => _gameState.Ui.DraggedRune = value;
    }

    private Vector2 DraggedRunePosition
    {
        get => _gameState.Ui.DraggedRunePosition;
        set => _gameState.Ui.DraggedRunePosition = value;
    }

    public void Update(float deltaTime, Point mousePosition, bool isLeftMouseDown)
    {
        if (_gameState.Ui.BuildSelection.IsOpen)
        {
            UpdateBuildSelectionAnimation(deltaTime);
            UpdateBuildSelectionHover(mousePosition, deltaTime);
            HandleBuildSelectionInput(mousePosition, isLeftMouseDown);
            _wasLeftMouseDown = isLeftMouseDown;
            return;
        }

        if (_gameState.IsDefeated)
        {
            ApplyDefeatState(isLeftMouseDown, deltaTime);
            return;
        }

        HandleInput(mousePosition, isLeftMouseDown);
        _enemySystem.Update(_gameState, _board.Path, deltaTime);

        if (_gameState.IsDefeated)
        {
            ApplyDefeatState(isLeftMouseDown, deltaTime);
            return;
        }

        _runeCombatSystem.Update(_gameState, deltaTime);
        _projectileSystem.Update(_gameState, deltaTime, _runeEffectSystem);
        _bagClickPulseElapsed = Math.Min(BagClickPulseDuration, _bagClickPulseElapsed + deltaTime);
        _bagInsertPulseElapsed = Math.Min(BagInsertPulseDuration, _bagInsertPulseElapsed + deltaTime);
        UpdateBagVisualState(deltaTime, mousePosition);
        UpdateRuneInteractionState(mousePosition);
        _runePresentationSystem.Update(_gameState, deltaTime);
        _effectAnimationSystem.Update(_gameState, deltaTime);
        ResolveCompletedAnimations();
    }

    private void HandleBuildSelectionInput(Point mousePosition, bool isLeftMouseDown)
    {
        var leftPressed = isLeftMouseDown && !_wasLeftMouseDown;
        if (!leftPressed)
        {
            return;
        }

        var buildSelection = _gameState.Ui.BuildSelection;
        if (buildSelection.ActiveAnimation != null)
        {
            return;
        }

        var selectedSlots = BuildSelectionLayout.CreateSelectedBuildSlots(_board.ViewportBounds);
        for (var i = 0; i < buildSelection.SelectedRunes.Count && i < selectedSlots.Count; i++)
        {
            if (!selectedSlots[i].Contains(mousePosition))
            {
                continue;
            }

            StartRemoveFromBuild(buildSelection, buildSelection.SelectedRunes[i], i);
            return;
        }

        foreach (var option in BuildSelectionLayout.CreateOptionLayouts(_board.ViewportBounds))
        {
            if (!option.CardBounds.Contains(mousePosition))
            {
                continue;
            }

            StartAddToBuild(buildSelection, option.RuneType);
            return;
        }

        if (buildSelection.CanStart && BuildSelectionLayout.GetStartButtonBounds(_board.ViewportBounds).Contains(mousePosition))
        {
            buildSelection.IsOpen = false;
        }
    }

    private void StartAddToBuild(BuildSelectionState buildSelection, RuneType runeType)
    {
        if (buildSelection.SelectedRunes.Contains(runeType))
        {
            return;
        }

        if (buildSelection.SelectedRunes.Count >= BuildSelectionState.BuildSize)
        {
            return;
        }

        var options = BuildSelectionLayout.CreateOptionLayouts(_board.ViewportBounds);
        RuneOptionLayout? sourceOption = null;
        foreach (var option in options)
        {
            if (option.RuneType == runeType)
            {
                sourceOption = option;
                break;
            }
        }

        if (sourceOption == null)
        {
            return;
        }

        var targetSlotIndex = buildSelection.SelectedRunes.Count;
        var targetSlot = BuildSelectionLayout.CreateSelectedBuildSlots(_board.ViewportBounds)[targetSlotIndex];
        buildSelection.ActiveAnimation = new BuildSelectionAnimation(
            BuildSelectionAnimationKind.Add,
            runeType,
            targetSlotIndex,
            GetCenter(sourceOption.Value.IconBounds),
            GetCenter(targetSlot));
    }

    private void StartRemoveFromBuild(BuildSelectionState buildSelection, RuneType runeType, int slotIndex)
    {
        var options = BuildSelectionLayout.CreateOptionLayouts(_board.ViewportBounds);
        RuneOptionLayout? targetOption = null;
        foreach (var option in options)
        {
            if (option.RuneType == runeType)
            {
                targetOption = option;
                break;
            }
        }

        if (targetOption == null)
        {
            return;
        }

        var sourceSlot = BuildSelectionLayout.CreateSelectedBuildSlots(_board.ViewportBounds)[slotIndex];
        buildSelection.ActiveAnimation = new BuildSelectionAnimation(
            BuildSelectionAnimationKind.Remove,
            runeType,
            slotIndex,
            GetCenter(sourceSlot),
            GetCenter(targetOption.Value.IconBounds));
    }

    private void UpdateBuildSelectionAnimation(float deltaTime)
    {
        var buildSelection = _gameState.Ui.BuildSelection;
        var animation = buildSelection.ActiveAnimation;
        if (animation == null)
        {
            return;
        }

        animation.Update(deltaTime);
        if (!animation.IsFinished)
        {
            return;
        }

        if (animation.Kind == BuildSelectionAnimationKind.Add)
        {
            buildSelection.SelectedRunes.Add(animation.RuneType);
        }
        else if (animation.SlotIndex >= 0 && animation.SlotIndex < buildSelection.SelectedRunes.Count)
        {
            buildSelection.SelectedRunes.RemoveAt(animation.SlotIndex);
        }

        buildSelection.ActiveAnimation = null;
    }

    private void UpdateBuildSelectionHover(Point mousePosition, float deltaTime)
    {
        var buildSelection = _gameState.Ui.BuildSelection;

        foreach (var option in BuildSelectionLayout.CreateOptionLayouts(_board.ViewportBounds))
        {
            var current = buildSelection.OptionHoverAmounts[option.RuneType];
            var target = option.CardBounds.Contains(mousePosition) ? 1f : 0f;
            buildSelection.OptionHoverAmounts[option.RuneType] = Approach(current, target, deltaTime * 10f);
        }
    }

    private void HandleInput(Point mousePosition, bool isLeftMouseDown)
    {
        var leftPressed = isLeftMouseDown && !_wasLeftMouseDown;
        var leftReleased = !isLeftMouseDown && _wasLeftMouseDown;

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
        else if (DraggedRune == null && leftPressed && _board.BagBounds.Contains(mousePosition))
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

        if (_board.BagBounds.Contains(mousePosition))
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
        _isBagHovered = _board.BagBounds.Contains(mousePosition);
        _isDraggingOverBag = DraggedRune != null && _isBagHovered;

        var targetBlend = _isDraggingOverBag ? 1f : 0f;
        _bagHoverBlend = Approach(_bagHoverBlend, targetBlend, deltaTime * 12f);
        _gameState.Ui.UseOpenBagSprite = _isDraggingOverBag;
        _gameState.Ui.BagScale = 1f + (_bagHoverBlend * 0.1f) + EvaluateBagClickPulse() + EvaluateBagInsertPulse();
    }

    private void UpdateRuneInteractionState(Point mousePosition)
    {
        var hoveredMergeTarget = GetHoveredMergeTarget(mousePosition);

        for (var i = 0; i < _gameState.Runes.Count; i++)
        {
            var rune = _gameState.Runes[i];
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
        sourceRune.Presentation.BeginMergeInto(DraggedRunePosition, targetRune.Transform.Position);
        targetRune.Presentation.SetReservedForMerge(true);
        _pendingMerges.Add(new PendingMerge
        {
            SourceRune = sourceRune,
            TargetRune = targetRune,
            MergedTier = Math.Max(sourceRune.Data.Tier, targetRune.Data.Tier) + 1,
            MergedType = targetRune.Data.Type
        });
    }

    private void StartBagInsertion(RuneEntity rune)
    {
        rune.Presentation.BeginBagInsert(DraggedRunePosition, GetBagCenter());
        _pendingBagInsertions.Add(rune);
        _bagInsertPulseElapsed = 0f;
    }

    private void ResolveCompletedAnimations()
    {
        for (var i = _pendingMerges.Count - 1; i >= 0; i--)
        {
            var pendingMerge = _pendingMerges[i];
            if (!pendingMerge.SourceRune.Presentation.IsTransientAnimationComplete)
            {
                continue;
            }

            _gameState.Runes.Remove(pendingMerge.SourceRune);
            _gameState.Runes.Remove(pendingMerge.TargetRune);

            var targetCell = _board.Grid.GetCell(pendingMerge.TargetRune.Grid.Row, pendingMerge.TargetRune.Grid.Column);
            var mergedRune = _runeFactory.Create(targetCell, pendingMerge.MergedType, pendingMerge.MergedTier);
            mergedRune.Presentation.TriggerMergePop();
            _gameState.Runes.Add(mergedRune);
            _effectAnimationSystem.TrySpawnMergeAnimation(_gameState, mergedRune.Transform.Position, mergedRune.Data.Color);
            _pendingMerges.RemoveAt(i);
        }

        for (var i = _pendingBagInsertions.Count - 1; i >= 0; i--)
        {
            var rune = _pendingBagInsertions[i];
            if (!rune.Presentation.IsTransientAnimationComplete)
            {
                continue;
            }

            _effectAnimationSystem.TrySpawnRuneRemoveAnimation(_gameState, GetBagCenter(), rune.Data.Color);
            _gameState.Runes.Remove(rune);
            _pendingBagInsertions.RemoveAt(i);
        }
    }

    private static bool CanMerge(RuneEntity sourceRune, RuneEntity targetRune)
    {
        return sourceRune.Data.Tier == targetRune.Data.Tier &&
            sourceRune.Data.Type == targetRune.Data.Type;
    }

    private RuneEntity? GetRuneAtPoint(Point mousePosition, RuneEntity? excludedRune)
    {
        for (var i = _gameState.Runes.Count - 1; i >= 0; i--)
        {
            var rune = _gameState.Runes[i];
            if (!rune.Presentation.IsInteractable)
            {
                continue;
            }

            if (ReferenceEquals(rune, excludedRune))
            {
                continue;
            }

            var cellBounds = _board.Grid.GetCell(rune.Grid.Row, rune.Grid.Column).Bounds;
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
        var rune = _runeFactory.Create(cell, GetRandomRuneType());
        rune.Presentation.BeginSpawnFromBag(GetBagCenter(), rune.Transform.Position);
        _gameState.Runes.Add(rune);
        _effectAnimationSystem.TrySpawnRuneSpawnAnimation(_gameState, GetBagCenter(), rune.Data.Color);
        _bagClickPulseElapsed = 0f;
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
            if (rune.Grid.Row == row && rune.Grid.Column == column)
            {
                return true;
            }
        }

        return false;
    }

    private RuneType GetRandomRuneType()
    {
        var selectedRunes = _gameState.Ui.BuildSelection.SelectedRunes;
        return selectedRunes[_random.Next(selectedRunes.Count)];
    }

    private void ApplyDefeatState(bool isLeftMouseDown, float deltaTime)
    {
        DraggedRune = null;
        _bagHoverBlend = 0f;
        _isDraggingOverBag = false;
        _isBagHovered = false;
        _gameState.Ui.UseOpenBagSprite = false;
        _gameState.Ui.BagScale = 1f;
        _wasLeftMouseDown = isLeftMouseDown;
        UpdateRuneInteractionState(Point.Empty);
        _runePresentationSystem.Update(_gameState, deltaTime);
        _effectAnimationSystem.Update(_gameState, deltaTime);
    }

    private Vector2 GetBagCenter()
    {
        return new Vector2(_board.BagBounds.Left + (_board.BagBounds.Width * 0.5f), _board.BagBounds.Top + (_board.BagBounds.Height * 0.5f));
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

    private static Vector2 GetCenter(Rectangle bounds)
    {
        return new Vector2(bounds.Left + (bounds.Width * 0.5f), bounds.Top + (bounds.Height * 0.5f));
    }
}
