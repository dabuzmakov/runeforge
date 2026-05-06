namespace runeforge.Models;

public static class TopButtonLayout
{
    private const int ButtonSize = 46;
    private const int ButtonMargin = 22;
    private const int ButtonSpacing = 10;

    public static Rectangle GetExitButtonBounds(Rectangle viewport)
    {
        return new Rectangle(
            viewport.Right - ButtonMargin - ButtonSize,
            viewport.Top + ButtonMargin,
            ButtonSize,
            ButtonSize);
    }

    public static Rectangle GetPauseButtonBounds(Rectangle viewport)
    {
        return GetLeftOfExitButtonBounds(viewport);
    }

    public static Rectangle GetHomeButtonBounds(Rectangle viewport)
    {
        return GetLeftOfExitButtonBounds(viewport);
    }

    private static Rectangle GetLeftOfExitButtonBounds(Rectangle viewport)
    {
        var exitBounds = GetExitButtonBounds(viewport);
        return new Rectangle(
            exitBounds.Left - ButtonSpacing - ButtonSize,
            exitBounds.Top,
            ButtonSize,
            ButtonSize);
    }
}
