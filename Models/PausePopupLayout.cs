namespace runeforge.Models;

public enum PausePopupButtonKind
{
    Restart,
    Home,
    Resume
}

public static class PausePopupLayout
{
    private const int PopupWidth = 576;
    private const int PopupHeight = 474;
    private const int ButtonWidth = 110;
    private const int ButtonHeight = 94;
    private const int ButtonGap = 22;

    public static Rectangle GetPopupBounds(Rectangle viewport)
    {
        var width = Math.Min(PopupWidth, Math.Max(1, viewport.Width - 96));
        var height = Math.Max(1, (int)MathF.Round(width * (PopupHeight / (float)PopupWidth)));
        if (height > viewport.Height - 72)
        {
            height = Math.Max(1, viewport.Height - 72);
            width = Math.Max(1, (int)MathF.Round(height * (PopupWidth / (float)PopupHeight)));
        }

        return new Rectangle(
            viewport.Left + ((viewport.Width - width) / 2),
            viewport.Top + ((viewport.Height - height) / 2),
            width,
            height);
    }

    public static Rectangle GetTitleBounds(Rectangle popupBounds)
    {
        return new Rectangle(
            popupBounds.Left + (int)MathF.Round(popupBounds.Width * 0.09f),
            popupBounds.Top + (int)MathF.Round(popupBounds.Height * 0.29f),
            (int)MathF.Round(popupBounds.Width * 0.82f),
            (int)MathF.Round(popupBounds.Height * 0.21f));
    }

    public static Rectangle GetButtonBounds(Rectangle popupBounds, PausePopupButtonKind kind)
    {
        var scale = popupBounds.Width / (float)PopupWidth;
        var buttonWidth = Math.Max(1, (int)MathF.Round(ButtonWidth * scale));
        var buttonHeight = Math.Max(1, (int)MathF.Round(ButtonHeight * scale));
        var buttonGap = Math.Max(1, (int)MathF.Round(ButtonGap * scale));
        var totalWidth = (buttonWidth * 3) + (buttonGap * 2);
        var startX = popupBounds.Left + ((popupBounds.Width - totalWidth) / 2);
        var y = popupBounds.Top + (int)MathF.Round(popupBounds.Height * 0.615f);
        var index = kind switch
        {
            PausePopupButtonKind.Restart => 0,
            PausePopupButtonKind.Home => 1,
            PausePopupButtonKind.Resume => 2,
            _ => 0
        };

        return new Rectangle(
            startX + (index * (buttonWidth + buttonGap)),
            y,
            buttonWidth,
            buttonHeight);
    }
}
