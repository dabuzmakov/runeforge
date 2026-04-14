using System.Numerics;
using runeforge.Models;

namespace runeforge.Controllers;

public sealed class BuildSelectionController
{
    private readonly GameBoard _board;

    public BuildSelectionController(GameBoard board)
    {
        _board = board;
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

        var selectedSlots = BuildSelectionLayout.CreateSelectedBuildSlots(_board.ViewportBounds);
        for (var i = 0; i < buildSelection.SelectedRunes.Count && i < selectedSlots.Count; i++)
        {
            if (!selectedSlots[i].Contains(mousePosition))
            {
                continue;
            }

            StartRemove(buildSelection, buildSelection.SelectedRunes[i], i);
            wasLeftMouseDown = isLeftMouseDown;
            return;
        }

        foreach (var option in BuildSelectionLayout.CreateOptionLayouts(_board.ViewportBounds))
        {
            if (!option.CardBounds.Contains(mousePosition))
            {
                continue;
            }

            StartAdd(buildSelection, option.RuneType);
            wasLeftMouseDown = isLeftMouseDown;
            return;
        }

        if (buildSelection.CanStart && BuildSelectionLayout.GetStartButtonBounds(_board.ViewportBounds).Contains(mousePosition))
        {
            buildSelection.IsOpen = false;
        }

        wasLeftMouseDown = isLeftMouseDown;
    }

    private void StartAdd(BuildSelectionState buildSelection, RuneType runeType)
    {
        if (buildSelection.SelectedRunes.Contains(runeType) || buildSelection.SelectedRunes.Count >= BuildSelectionState.BuildSize)
        {
            return;
        }

        RuneOptionLayout? sourceOption = null;
        foreach (var option in BuildSelectionLayout.CreateOptionLayouts(_board.ViewportBounds))
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

    private void StartRemove(BuildSelectionState buildSelection, RuneType runeType, int slotIndex)
    {
        RuneOptionLayout? targetOption = null;
        foreach (var option in BuildSelectionLayout.CreateOptionLayouts(_board.ViewportBounds))
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
        foreach (var option in BuildSelectionLayout.CreateOptionLayouts(_board.ViewportBounds))
        {
            var current = buildSelection.OptionHoverAmounts[option.RuneType];
            var target = option.CardBounds.Contains(mousePosition) ? 1f : 0f;
            buildSelection.OptionHoverAmounts[option.RuneType] = Approach(current, target, deltaTime * 10f);
        }
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
}
