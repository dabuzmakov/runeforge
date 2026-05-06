using System.Numerics;
using runeforge.Models;

namespace runeforge.Controllers;

public sealed class BuildSelectionController
{
    private readonly IReadOnlyList<RuneOptionLayout> _optionLayouts;
    private readonly IReadOnlyList<Rectangle> _selectedSlots;
    private readonly Rectangle _startButtonBounds;

    public BuildSelectionController(GameBoard board)
    {
        _optionLayouts = BuildSelectionLayout.CreateOptionLayouts(board.ViewportBounds);
        _selectedSlots = BuildSelectionLayout.CreateSelectedBuildSlots(board.ViewportBounds);
        _startButtonBounds = BuildSelectionLayout.GetStartButtonBounds(board.ViewportBounds);
    }

    public void Update(GameState gameState, float deltaTime, Point mousePosition, bool isLeftMouseDown, ref bool wasLeftMouseDown)
    {
        UpdateAnimation(gameState.Ui.BuildSelection, deltaTime);
        UpdateHover(gameState.Ui.BuildSelection, mousePosition, deltaTime);
        HandleInput(gameState.Ui.BuildSelection, mousePosition, isLeftMouseDown, ref wasLeftMouseDown);
    }

    private void HandleInput(BuildSelectionState buildSelection, Point mousePosition, bool isLeftMouseDown, ref bool wasLeftMouseDown)
    {
        var leftPressed = isLeftMouseDown && !wasLeftMouseDown;
        if (!leftPressed)
        {
            wasLeftMouseDown = isLeftMouseDown;
            return;
        }

        if (buildSelection.ActiveAnimation != null)
        {
            wasLeftMouseDown = isLeftMouseDown;
            return;
        }

        for (var i = 0; i < buildSelection.SelectedRunes.Count && i < _selectedSlots.Count; i++)
        {
            if (!_selectedSlots[i].Contains(mousePosition))
            {
                continue;
            }

            StartRemove(buildSelection, buildSelection.SelectedRunes[i], i);
            wasLeftMouseDown = isLeftMouseDown;
            return;
        }

        foreach (var option in _optionLayouts)
        {
            if (!option.CardBounds.Contains(mousePosition))
            {
                continue;
            }

            ToggleOption(buildSelection, option.RuneType);
            wasLeftMouseDown = isLeftMouseDown;
            return;
        }

        if (buildSelection.CanStart && _startButtonBounds.Contains(mousePosition))
        {
            buildSelection.IsOpen = false;
        }

        wasLeftMouseDown = isLeftMouseDown;
    }

    private void ToggleOption(BuildSelectionState buildSelection, RuneType runeType)
    {
        var selectedIndex = buildSelection.SelectedRunes.IndexOf(runeType);
        if (selectedIndex >= 0)
        {
            StartRemove(buildSelection, runeType, selectedIndex);
            return;
        }

        StartAdd(buildSelection, runeType);
    }

    private void StartAdd(BuildSelectionState buildSelection, RuneType runeType)
    {
        if (buildSelection.SelectedRunes.Contains(runeType) || buildSelection.SelectedRunes.Count >= BuildSelectionState.BuildSize)
        {
            return;
        }

        var sourceOption = FindOption(runeType);

        if (sourceOption == null)
        {
            return;
        }

        var targetSlotIndex = buildSelection.SelectedRunes.Count;
        var targetSlot = _selectedSlots[targetSlotIndex];
        buildSelection.ActiveAnimation = new BuildSelectionAnimation(
            BuildSelectionAnimationKind.Add,
            runeType,
            targetSlotIndex,
            GetCenter(sourceOption.Value.IconBounds),
            GetCenter(targetSlot));
    }

    private void StartRemove(BuildSelectionState buildSelection, RuneType runeType, int slotIndex)
    {
        var targetOption = FindOption(runeType);

        if (targetOption == null)
        {
            return;
        }

        var sourceSlot = _selectedSlots[slotIndex];
        buildSelection.ActiveAnimation = new BuildSelectionAnimation(
            BuildSelectionAnimationKind.Remove,
            runeType,
            slotIndex,
            GetCenter(sourceSlot),
            GetCenter(targetOption.Value.IconBounds));
    }

    private static void UpdateAnimation(BuildSelectionState buildSelection, float deltaTime)
    {
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

    private void UpdateHover(BuildSelectionState buildSelection, Point mousePosition, float deltaTime)
    {
        RuneOptionLayout? hoveredOption = null;

        var isStartButtonHovered = buildSelection.CanStart &&
            _startButtonBounds.Contains(mousePosition);
        buildSelection.StartButtonHoverAmount = Approach(
            buildSelection.StartButtonHoverAmount,
            isStartButtonHovered ? 1f : 0f,
            deltaTime * 10f);

        foreach (var option in _optionLayouts)
        {
            var isHovered = option.CardBounds.Contains(mousePosition);
            if (isHovered)
            {
                hoveredOption = option;
            }

            var current = buildSelection.OptionHoverAmounts[option.RuneType];
            var target = isHovered ? 1f : 0f;
            buildSelection.OptionHoverAmounts[option.RuneType] = Approach(current, target, deltaTime * 10f);
        }

        if (hoveredOption.HasValue)
        {
            var hoveredRuneType = hoveredOption.Value.RuneType;
            if (buildSelection.PendingTooltipRuneType != hoveredRuneType)
            {
                buildSelection.PendingTooltipRuneType = hoveredRuneType;
                buildSelection.PendingTooltipHoverSeconds = 0f;
                buildSelection.HoveredRuneType = null;
            }
            else
            {
                buildSelection.PendingTooltipHoverSeconds += Math.Max(0f, deltaTime);
                if (buildSelection.PendingTooltipHoverSeconds >= BuildSelectionState.TooltipShowDelaySeconds)
                {
                    buildSelection.HoveredRuneType = hoveredRuneType;
                    buildSelection.HoveredCardBounds = hoveredOption.Value.CardBounds;
                    buildSelection.TooltipAnchor = new Point(
                        hoveredOption.Value.CardBounds.Right + 16,
                        hoveredOption.Value.CardBounds.Top + 8);
                }
            }
        }
        else
        {
            buildSelection.PendingTooltipRuneType = null;
            buildSelection.PendingTooltipHoverSeconds = 0f;
            buildSelection.HoveredRuneType = null;
        }

        var tooltipTarget = buildSelection.HoveredRuneType.HasValue ? 1f : 0f;
        buildSelection.TooltipOpacity = Approach(buildSelection.TooltipOpacity, tooltipTarget, deltaTime * 12f);
    }

    private static float Approach(float value, float target, float step)
    {
        if (value < target)
        {
            return Math.Min(value + step, target);
        }

        return Math.Max(value - step, target);
    }

    private static Vector2 GetCenter(Rectangle bounds)
    {
        return new Vector2(bounds.Left + (bounds.Width * 0.5f), bounds.Top + (bounds.Height * 0.5f));
    }

    private RuneOptionLayout? FindOption(RuneType runeType)
    {
        foreach (var option in _optionLayouts)
        {
            if (option.RuneType == runeType)
            {
                return option;
            }
        }

        return null;
    }
}
