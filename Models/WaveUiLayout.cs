namespace runeforge.Models;

public static class WaveUiLayout
{
    private const float TopLeftPanelY = 24f;
    private const float TopLeftPanelSpacing = 10f;
    private const float TopLeftPanelWidth = 136f;
    private const float TopLeftPanelHeight = 66f;

    public static RectangleF GetWavePanelBounds(Rectangle viewport)
    {
        return GetPanelBounds(viewport, panelIndex: 0);
    }

    public static RectangleF GetHeartPanelBounds(Rectangle viewport)
    {
        return GetPanelBounds(viewport, panelIndex: 1);
    }

    public static RectangleF GetRunePointsPanelBounds(Rectangle viewport)
    {
        return GetPanelBounds(viewport, panelIndex: 2);
    }

    public static RectangleF GetRecordPanelBounds(Rectangle viewport)
    {
        return GetPanelBounds(viewport, panelIndex: 3);
    }

    private static RectangleF GetPanelBounds(Rectangle viewport, int panelIndex)
    {
        const int panelCount = 4;
        var totalWidth = (TopLeftPanelWidth * panelCount) + (TopLeftPanelSpacing * (panelCount - 1));
        var groupLeft = viewport.Left + ((viewport.Width - totalWidth) * 0.5f);

        return new RectangleF(
            groupLeft + (panelIndex * (TopLeftPanelWidth + TopLeftPanelSpacing)),
            TopLeftPanelY,
            TopLeftPanelWidth,
            TopLeftPanelHeight);
    }
}

internal static class GameRendererHeartMetrics
{
    public const float IconSize = 30f;
    public const float IconSpacing = 10f;
    public const float PanelPadding = 10f;
}
