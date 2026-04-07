using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Views;

public sealed class RuneView : IDisposable
{
    private const float MaxDrawSize = 80f;
    private const float CellSize = 100f;
    private const float TierPadding = 5f;

    private readonly IReadOnlyDictionary<string, Bitmap> _runeTextures;
    private readonly ImageAttributes _imageAttributes;
    private readonly Font _tierFont;
    private readonly SolidBrush _tierBrush;

    public RuneView(IReadOnlyDictionary<string, Bitmap> runeTextures)
    {
        _runeTextures = runeTextures;
        _imageAttributes = new ImageAttributes();
        _tierFont = FontLibrary.Create(12f, FontStyle.Bold);
        _tierBrush = new SolidBrush(Color.Gold);
    }

    public void Draw(Graphics graphics, RuneEntity rune)
    {
        Draw(graphics, rune, rune.Presentation.VisualPosition, rune.Presentation.VisualScale, rune.Presentation.VisualAlpha);
    }

    public void Draw(Graphics graphics, RuneEntity rune, Vector2 drawCenter)
    {
        Draw(graphics, rune, drawCenter, rune.Presentation.VisualScale, rune.Presentation.VisualAlpha);
    }

    public void Draw(Graphics graphics, RuneEntity rune, Vector2 drawCenter, float scaleMultiplier, float alphaMultiplier)
    {
        if (!_runeTextures.TryGetValue(rune.Data.TextureKey, out var texture))
        {
            return;
        }

        var textureScale = Math.Min(
            MaxDrawSize / texture.Width,
            MaxDrawSize / texture.Height) * scaleMultiplier;

        var drawWidth = texture.Width * textureScale;
        var drawHeight = texture.Height * textureScale;
        var drawX = drawCenter.X - (drawWidth * 0.5f);
        var drawY = drawCenter.Y - (drawHeight * 0.5f);

        DrawTexture(graphics, texture, drawX, drawY, drawWidth, drawHeight, alphaMultiplier);
        DrawTier(graphics, rune, drawCenter, scaleMultiplier, alphaMultiplier);
    }

    public void DrawIcon(Graphics graphics, RuneType runeType, Rectangle bounds, float alphaMultiplier = 1f)
    {
        DrawIcon(graphics, runeType, new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height), alphaMultiplier);
    }

    public void DrawIcon(Graphics graphics, RuneType runeType, RectangleF bounds, float alphaMultiplier = 1f)
    {
        var textureKey = RuneCatalog.Get(runeType).TextureKey;
        if (!_runeTextures.TryGetValue(textureKey, out var texture))
        {
            return;
        }

        var textureScale = Math.Min(
            bounds.Width / texture.Width,
            bounds.Height / texture.Height);

        var drawWidth = texture.Width * textureScale;
        var drawHeight = texture.Height * textureScale;
        var drawX = bounds.Left + ((bounds.Width - drawWidth) * 0.5f);
        var drawY = bounds.Top + ((bounds.Height - drawHeight) * 0.5f);

        DrawTexture(graphics, texture, drawX, drawY, drawWidth, drawHeight, alphaMultiplier);
    }

    public void Dispose()
    {
        _imageAttributes.Dispose();
        _tierFont.Dispose();
        _tierBrush.Dispose();
    }

    private void DrawTexture(Graphics graphics, Bitmap texture, float drawX, float drawY, float drawWidth, float drawHeight, float alphaMultiplier)
    {
        if (alphaMultiplier >= 0.999f)
        {
            graphics.DrawImage(texture, drawX, drawY, drawWidth, drawHeight);
            return;
        }

        var colorMatrix = new ColorMatrix
        {
            Matrix33 = Math.Clamp(alphaMultiplier, 0f, 1f)
        };

        _imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        graphics.DrawImage(
            texture,
            Rectangle.Round(new RectangleF(drawX, drawY, drawWidth, drawHeight)),
            0f,
            0f,
            texture.Width,
            texture.Height,
            GraphicsUnit.Pixel,
            _imageAttributes);
    }

    private void DrawTier(Graphics graphics, RuneEntity rune, Vector2 drawCenter, float scaleMultiplier, float alphaMultiplier)
    {
        var tierText = ToRoman(rune.Data.Tier);
        var textSize = graphics.MeasureString(tierText, _tierFont);
        var halfSize = CellSize * 0.5f * scaleMultiplier;
        _tierBrush.Color = Color.FromArgb((int)(Math.Clamp(alphaMultiplier, 0f, 1f) * 255f), Color.Gold);
        var textPosition = new PointF(
            drawCenter.X + halfSize - textSize.Width - (TierPadding * scaleMultiplier),
            drawCenter.Y - halfSize + (TierPadding * scaleMultiplier));

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
