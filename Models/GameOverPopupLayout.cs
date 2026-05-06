namespace runeforge.Models;

public enum GameOverPopupButtonKind
{
    Restart,
    Home
}

public static class GameOverPopupLayout
{
    private const float PopupViewportScale = 0.83f;
    private const int PopupSourceWidth = 947;
    private const int PopupSourceHeight = 1083;
    private const float TitleLeftRatio = 0.22f;
    private const float TitleTopRatio = 0.295f;
    private const float TitleWidthRatio = 0.56f;
    private const float TitleHeightRatio = 0.05f;
    private const float StatsLeftRatio = 0.12f;
    private const float StatsTopRatio = 0.425f;
    private const float StatsWidthRatio = 0.76f;
    private const float StatsHeightRatio = 0.165f;
    private const float RuneSectionTopRatio = 0.602f;
    private const float RuneSectionHeightRatio = 0.042f;
    private const float RuneCellsTopRatio = 0.672f;
    private const float RuneCellHeightRatio = 0.088f;
    private const float RuneCellGapRatio = 0.019f;
    private const float ButtonTopRatio = 0.824f;
    private const float ButtonWidthRatio = 0.255f;
    private const float ButtonGapRatio = 0.10f;
    private const int HorizontalMargin = 18;
    private const int VerticalMargin = 18;

    public static Rectangle GetPopupBounds(Rectangle viewport)
    {
        var height = Math.Max(1, (int)MathF.Round((viewport.Height - VerticalMargin) * PopupViewportScale));
        var width = Math.Max(1, (int)MathF.Round(height * (PopupSourceWidth / (float)PopupSourceHeight)));
        if (width > viewport.Width - HorizontalMargin)
        {
            width = Math.Max(1, viewport.Width - HorizontalMargin);
            height = Math.Max(1, (int)MathF.Round(width * (PopupSourceHeight / (float)PopupSourceWidth)));
        }

        return new Rectangle(
            viewport.Left + ((viewport.Width - width) / 2),
            viewport.Top,
            width,
            height);
    }

    public static Rectangle GetAnimatedPopupBounds(Rectangle viewport, float progress)
    {
        var finalBounds = GetPopupBounds(viewport);
        var clampedProgress = Math.Clamp(progress, 0f, 1f);
        var easedProgress = EaseOutCubic(clampedProgress);
        var startY = viewport.Top - finalBounds.Height - Math.Max(24, finalBounds.Height / 18);
        var shakeAmplitude = Math.Min(18f, finalBounds.Height * 0.018f);
        var shakeOffset = MathF.Sin(clampedProgress * 15f) * (1f - clampedProgress) * shakeAmplitude;
        var currentY = Lerp(startY, finalBounds.Top, easedProgress) + shakeOffset;

        return new Rectangle(
            finalBounds.X,
            (int)MathF.Round(currentY),
            finalBounds.Width,
            finalBounds.Height);
    }

    public static Rectangle GetTitleBounds(Rectangle popupBounds)
    {
        return GetRelativeBounds(popupBounds, TitleLeftRatio, TitleTopRatio, TitleWidthRatio, TitleHeightRatio);
    }

    public static Rectangle GetStatsBounds(Rectangle popupBounds)
    {
        return GetRelativeBounds(popupBounds, StatsLeftRatio, StatsTopRatio, StatsWidthRatio, StatsHeightRatio);
    }

    public static Rectangle GetStatsColumnBounds(Rectangle popupBounds, int columnIndex)
    {
        var statsBounds = GetStatsBounds(popupBounds);
        var columnWidth = statsBounds.Width / 3f;
        return Rectangle.Round(new RectangleF(
            statsBounds.Left + (columnIndex * columnWidth),
            statsBounds.Top,
            columnWidth,
            statsBounds.Height));
    }

    public static Rectangle GetRuneSectionLabelBounds(Rectangle popupBounds)
    {
        return GetRelativeBounds(popupBounds, 0.255f, RuneSectionTopRatio, 0.49f, RuneSectionHeightRatio);
    }

    public static IReadOnlyList<Rectangle> CreateRuneCellBounds(Rectangle popupBounds)
    {
        var cellHeight = Math.Max(1, (int)MathF.Round(popupBounds.Height * RuneCellHeightRatio));
        var cellWidth = Math.Max(1, (int)MathF.Round(cellHeight * (816f / 894f)));
        var gap = Math.Max(8, (int)MathF.Round(popupBounds.Width * RuneCellGapRatio));
        var totalWidth = (BuildSelectionState.BuildSize * cellWidth) + ((BuildSelectionState.BuildSize - 1) * gap);
        var startX = popupBounds.Left + ((popupBounds.Width - totalWidth) / 2);
        var top = popupBounds.Top + (int)MathF.Round(popupBounds.Height * RuneCellsTopRatio);
        var cells = new List<Rectangle>(BuildSelectionState.BuildSize);

        for (var i = 0; i < BuildSelectionState.BuildSize; i++)
        {
            cells.Add(new Rectangle(
                startX + (i * (cellWidth + gap)),
                top,
                cellWidth,
                cellHeight));
        }

        return cells;
    }

    public static Rectangle GetButtonBounds(Rectangle popupBounds, GameOverPopupButtonKind kind)
    {
        var buttonWidth = Math.Max(1, (int)MathF.Round(popupBounds.Width * ButtonWidthRatio));
        var buttonHeight = Math.Max(1, (int)MathF.Round(buttonWidth * (359f / 797f)));
        var gap = Math.Max(12, (int)MathF.Round(popupBounds.Width * ButtonGapRatio));
        var totalWidth = (buttonWidth * 2) + gap;
        var startX = popupBounds.Left + ((popupBounds.Width - totalWidth) / 2);
        var top = popupBounds.Top + (int)MathF.Round(popupBounds.Height * ButtonTopRatio);
        var index = kind == GameOverPopupButtonKind.Restart ? 0 : 1;

        return new Rectangle(
            startX + (index * (buttonWidth + gap)),
            top,
            buttonWidth,
            buttonHeight);
    }

    private static Rectangle GetRelativeBounds(Rectangle popupBounds, float leftRatio, float topRatio, float widthRatio, float heightRatio)
    {
        return Rectangle.Round(new RectangleF(
            popupBounds.Left + (popupBounds.Width * leftRatio),
            popupBounds.Top + (popupBounds.Height * topRatio),
            popupBounds.Width * widthRatio,
            popupBounds.Height * heightRatio));
    }

    private static float EaseOutCubic(float value)
    {
        var inverse = 1f - value;
        return 1f - (inverse * inverse * inverse);
    }

    private static float Lerp(float start, float end, float amount)
    {
        return start + ((end - start) * amount);
    }
}
