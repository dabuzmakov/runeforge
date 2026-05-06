using System.Drawing;

namespace runeforge.Views;

internal static class AdaptiveTextRenderer
{
    public static void DrawCentered(
        Graphics graphics,
        string text,
        Font font,
        Brush brush,
        RectangleF bounds,
        StringFormat format,
        float minimumScale = 0.58f,
        float maximumScale = 1f)
    {
        var measuredSize = graphics.MeasureString(text, font, int.MaxValue, format);
        var widthScale = bounds.Width / Math.Max(1f, measuredSize.Width);
        var heightScale = bounds.Height / Math.Max(1f, measuredSize.Height);
        var maxScale = Math.Clamp(maximumScale, minimumScale, 1f);
        var scale = Math.Clamp(MathF.Min(1f, MathF.Min(widthScale, heightScale)), minimumScale, maxScale);

        if (scale >= 0.999f)
        {
            graphics.DrawString(text, font, brush, bounds, format);
            return;
        }

        var state = graphics.Save();
        try
        {
            var centerX = bounds.X + (bounds.Width * 0.5f);
            var centerY = bounds.Y + (bounds.Height * 0.5f);
            graphics.TranslateTransform(centerX, centerY);
            graphics.ScaleTransform(scale, scale);
            graphics.TranslateTransform(-centerX, -centerY);
            graphics.DrawString(text, font, brush, bounds, format);
        }
        finally
        {
            graphics.Restore(state);
        }
    }
}
