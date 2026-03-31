using System.Drawing;
using System.Numerics;
using runeforge.Models;

namespace runeforge.Views;

public sealed class RuneView : IDisposable
{
    private const float MaxDrawSize = 80f;
    private const float CellSize = 100f;
    private const float TierPadding = 5f;

    private readonly IReadOnlyDictionary<string, Bitmap> _runeTextures;
    private readonly Font _tierFont;
    private readonly SolidBrush _tierBrush;

    public RuneView(IReadOnlyDictionary<string, Bitmap> runeTextures)
    {
        _runeTextures = runeTextures;
        _tierFont = new Font("Segoe UI", 12f, FontStyle.Bold, GraphicsUnit.Pixel);
        _tierBrush = new SolidBrush(Color.Gold);
    }

    public void Draw(Graphics graphics, Rune rune)
    {
        Draw(graphics, rune, rune.Position);
    }

    public void Draw(Graphics graphics, Rune rune, Vector2 drawCenter)
    {
        if (!_runeTextures.TryGetValue(rune.TextureKey, out var texture))
        {
            return;
        }

        var scale = Math.Min(
            MaxDrawSize / texture.Width,
            MaxDrawSize / texture.Height);

        var drawWidth = texture.Width * scale;
        var drawHeight = texture.Height * scale;
        var drawX = drawCenter.X - (drawWidth * 0.5f);
        var drawY = drawCenter.Y - (drawHeight * 0.5f);

        graphics.DrawImage(texture, drawX, drawY, drawWidth, drawHeight);
        DrawTier(graphics, rune, drawCenter);
    }

    public void Dispose()
    {
        _tierFont.Dispose();
        _tierBrush.Dispose();
    }

    private void DrawTier(Graphics graphics, Rune rune, Vector2 drawCenter)
    {
        var tierText = ToRoman(rune.Tier);
        var textSize = graphics.MeasureString(tierText, _tierFont);
        var textPosition = new PointF(
            drawCenter.X + (CellSize * 0.5f) - textSize.Width - TierPadding,
            drawCenter.Y - (CellSize * 0.5f) + TierPadding);

        graphics.DrawString(tierText, _tierFont, _tierBrush, textPosition);
    }

    private static string ToRoman(int number)
    {
        return number switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "IV",
            5 => "V",
            6 => "VI",
            7 => "VII",
            8 => "VIII",
            9 => "IX",
            10 => "X",
            _ => number.ToString()
        };
    }
}
