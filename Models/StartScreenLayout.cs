namespace runeforge.Models;

public readonly record struct StartScreenStatLayout(Rectangle Bounds, Rectangle LabelBounds, Rectangle ValueBounds);

public static class StartScreenLayout
{
    private const float PlayButtonAspectRatio = 1749f / 771f;
    private const float StatBadgeAspectRatio = 479f / 784f;
    private const float PlayButtonWidthRatio = 0.28f;
    private const float PlayButtonTopRatio = 0.57f;
    private const float StatsHeightRatio = 0.23f;
    private const int PlayButtonTopOffset = 100;
    private const int StatsGap = 40;
    private const int StatsBottomGap = 40;

    public static Rectangle GetPlayButtonBounds(Rectangle viewport)
    {
        var buttonWidth = Math.Max(180, (int)MathF.Round(viewport.Width * PlayButtonWidthRatio));
        buttonWidth = Math.Min(buttonWidth, viewport.Width - 96);
        var buttonHeight = Math.Max(1, (int)MathF.Round(buttonWidth / PlayButtonAspectRatio));
        var top = viewport.Top + (int)MathF.Round(viewport.Height * PlayButtonTopRatio) + PlayButtonTopOffset;

        return new Rectangle(
            viewport.Left + ((viewport.Width - buttonWidth) / 2),
            top,
            buttonWidth,
            buttonHeight);
    }

    public static IReadOnlyList<StartScreenStatLayout> CreateStatLayouts(Rectangle viewport)
    {
        var playButtonBounds = GetPlayButtonBounds(viewport);
        var height = Math.Clamp(
            (int)MathF.Round(viewport.Height * StatsHeightRatio),
            150,
            254);
        var statWidth = Math.Max(1, (int)MathF.Round(height * StatBadgeAspectRatio));
        var totalWidth = (statWidth * 3) + (StatsGap * 2);
        if (totalWidth > viewport.Width - 96)
        {
            statWidth = Math.Max(1, (viewport.Width - 96 - (StatsGap * 2)) / 3);
            height = Math.Max(1, (int)MathF.Round(statWidth / StatBadgeAspectRatio));
            totalWidth = (statWidth * 3) + (StatsGap * 2);
        }

        var startX = viewport.Left + ((viewport.Width - totalWidth) / 2);
        var top = Math.Max(
            viewport.Top + 260,
            playButtonBounds.Top - StatsBottomGap - height);
        var layouts = new List<StartScreenStatLayout>(3);

        for (var i = 0; i < 3; i++)
        {
            var bounds = new Rectangle(
                startX + (i * (statWidth + StatsGap)),
                top,
                statWidth,
                height);
            var labelBounds = new Rectangle(
                bounds.Left + 10,
                bounds.Top + (int)MathF.Round(bounds.Height * 0.10f),
                bounds.Width - 20,
                Math.Max(1, (int)MathF.Round(bounds.Height * 0.20f)));
            var valueBounds = new Rectangle(
                bounds.Left + 10,
                bounds.Top + (int)MathF.Round(bounds.Height * 0.70f),
                bounds.Width - 20,
                Math.Max(1, (int)MathF.Round(bounds.Height * 0.18f)));

            layouts.Add(new StartScreenStatLayout(bounds, labelBounds, valueBounds));
        }

        return layouts;
    }
}
