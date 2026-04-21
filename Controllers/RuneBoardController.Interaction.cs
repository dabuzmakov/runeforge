using System.Numerics;
using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Controllers;

public sealed partial class RuneBoardController
{
    private void TryStartDragging(Point mousePosition)
    {
        var rune = GetRuneAtPoint(mousePosition, excludedRune: null);
        if (rune == null)
        {
            return;
        }

        DraggedRune = rune;
        DraggedRunePosition = rune.Transform.Position;
        DraggedRuneGrabOffset = rune.Transform.Position - new Vector2(mousePosition.X, mousePosition.Y);
    }

    private void HandleDragRelease(Point mousePosition)
    {
        var sourceRune = DraggedRune;
        DraggedRune = null;
        DraggedRuneGrabOffset = Vector2.Zero;
        _hagalazController.ClearPreview();

        if (sourceRune == null)
        {
            return;
        }

        if (Board.BagBounds.Contains(mousePosition))
        {
            StartBagInsertion(sourceRune);
            return;
        }

        if (_hagalazController.TryTriggerExplosion(sourceRune, mousePosition))
        {
            return;
        }

        var targetRune = GetRuneAtPoint(mousePosition, sourceRune);
        if (targetRune == null || !RuneMergeRules.CanMerge(sourceRune, targetRune))
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
        _hagalazController.UpdatePreview(DraggedRune, mousePosition);

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
        if (hoveredRune == null || !RuneMergeRules.CanMerge(DraggedRune, hoveredRune))
        {
            return null;
        }

        return hoveredRune;
    }

    private void StartMerge(RuneEntity sourceRune, RuneEntity targetRune)
    {
        var mergedTier = RuneTierTuning.Clamp(targetRune.Stats.Tier + 1);
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
