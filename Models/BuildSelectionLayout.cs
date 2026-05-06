using runeforge.Configs;

namespace runeforge.Models;

public readonly record struct RuneOptionLayout(RuneType RuneType, Rectangle CardBounds, Rectangle IconBounds, Rectangle LabelBounds);

public static class BuildSelectionLayout
{
    private const int Columns = 6;
    private const int CardWidth = 88;
    private const int CardHeight = 116;
    private const int SlotSize = 62;
    private const int SlotGap = 16;
    private const int SelectedSlotSize = 70;
    private const int SelectedSlotGap = 18;

    public static Rectangle GetOverlayPanel(Rectangle viewport)
    {
        const int panelWidth = 800;
        const int panelHeight = 800;

        return new Rectangle(
            viewport.Left + ((viewport.Width - panelWidth) / 2),
            viewport.Top + ((viewport.Height - panelHeight) / 2),
            panelWidth,
            panelHeight);
    }

    public static IReadOnlyList<RuneOptionLayout> CreateOptionLayouts(Rectangle viewport)
    {
        var panel = GetOverlayPanel(viewport);
        var gridWidth = (Columns * CardWidth) + ((Columns - 1) * SlotGap);
        var startX = panel.Left + ((panel.Width - gridWidth) / 2);
        var startY = panel.Top + 184;
        var layouts = new List<RuneOptionLayout>(RuneDatabase.AllTypes.Count);

        for (var i = 0; i < RuneDatabase.AllTypes.Count; i++)
        {
            var row = i / Columns;
            var column = i % Columns;
            var cardBounds = new Rectangle(
                startX + (column * (CardWidth + SlotGap)),
                startY + (row * (CardHeight + SlotGap)),
                CardWidth,
                CardHeight);
            var iconBounds = new Rectangle(
                cardBounds.Left + ((cardBounds.Width - SlotSize) / 2),
                cardBounds.Top + 10,
                SlotSize,
                SlotSize);
            var labelBounds = new Rectangle(
                cardBounds.Left + 6,
                cardBounds.Bottom - 30,
                cardBounds.Width - 12,
                18);

            layouts.Add(new RuneOptionLayout(RuneDatabase.AllTypes[i], cardBounds, iconBounds, labelBounds));
        }

        return layouts;
    }

    public static Rectangle GetStartButtonBounds(Rectangle viewport)
    {
        var panel = GetOverlayPanel(viewport);
        const int buttonWidth = 130;
        const int buttonHeight = 70;
        return new Rectangle(
            panel.Left + ((panel.Width - buttonWidth) / 2),
            panel.Bottom - 88,
            buttonWidth,
            buttonHeight);
    }

    public static IReadOnlyList<Rectangle> CreateSelectedBuildSlots(Rectangle viewport)
    {
        var panel = GetOverlayPanel(viewport);
        var totalWidth = (BuildSelectionState.BuildSize * SelectedSlotSize) + ((BuildSelectionState.BuildSize - 1) * SelectedSlotGap);
        return CreateSelectedBuildSlots(
            panel.Left + ((panel.Width - totalWidth) / 2),
            panel.Top + 57,
            SelectedSlotSize,
            SelectedSlotGap);
    }

    public static Rectangle GetSelectedBuildFrameBounds(Rectangle viewport)
    {
        var panel = GetOverlayPanel(viewport);
        const int frameWidth = 494;
        const int frameHeight = 146;
        return new Rectangle(
            panel.Left + ((panel.Width - frameWidth) / 2),
            panel.Top + 23,
            frameWidth,
            frameHeight);
    }

    private static IReadOnlyList<Rectangle> CreateSelectedBuildSlots(int startX, int startY, int size, int gap)
    {
        var slots = new List<Rectangle>(BuildSelectionState.BuildSize);

        for (var i = 0; i < BuildSelectionState.BuildSize; i++)
        {
            slots.Add(new Rectangle(startX + (i * (size + gap)), startY, size, size));
        }

        return slots;
    }
}
