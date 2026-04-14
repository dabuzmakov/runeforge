using System.Drawing;
using System.Drawing.Drawing2D;
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
    private const float BuffArrowPadding = 9f;
    private const float BuffArrowStemWidth = 4f;
    private const float BuffArrowStemHeight = 7f;
    private const float BuffArrowHeadWidth = 11f;
    private const float BuffArrowHeadHeight = 7f;
    private const float BuffArrowBobAmplitude = 1.5f;
    private const float BuffArrowInnerChevronInset = 2.4f;

    private readonly IReadOnlyDictionary<string, Bitmap> _runeTextures;
    private readonly ImageAttributes _imageAttributes;
    private readonly Font _tierFont;
    private readonly SolidBrush _tierBrush;
    private readonly SolidBrush _buffArrowBrush;
    private readonly Pen _buffArrowDetailPen;
    private readonly Pen _buffArrowOutlinePen;

    public RuneView(IReadOnlyDictionary<string, Bitmap> runeTextures)
    {
        _runeTextures = runeTextures;
        _imageAttributes = new ImageAttributes();
        _tierFont = FontLibrary.Create(12f, FontStyle.Bold);
        _tierBrush = new SolidBrush(Color.Gold);
        _buffArrowBrush = new SolidBrush(Color.FromArgb(236, 217, 68, 211));
        _buffArrowDetailPen = new Pen(Color.FromArgb(210, 244, 176, 242), 1.2f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        _buffArrowOutlinePen = new Pen(Color.FromArgb(220, 245, 184, 244), 1.3f)
        {
            LineJoin = LineJoin.Round
        };
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
        if (!_runeTextures.TryGetValue(rune.Stats.TextureKey, out var texture))
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
        DrawAttackSpeedBuffIndicator(graphics, rune, drawCenter, scaleMultiplier, alphaMultiplier);
    }

    public void DrawIcon(Graphics graphics, RuneType runeType, Rectangle bounds, float alphaMultiplier = 1f)
    {
        DrawIcon(graphics, runeType, new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height), alphaMultiplier);
    }

    public void DrawIcon(Graphics graphics, RuneType runeType, RectangleF bounds, float alphaMultiplier = 1f)
    {
        var textureKey = RuneDatabase.Get(runeType).TextureKey;
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
        _buffArrowBrush.Dispose();
        _buffArrowDetailPen.Dispose();
        _buffArrowOutlinePen.Dispose();
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
        var tierText = ToRoman(rune.Stats.Tier);
        var textSize = graphics.MeasureString(tierText, _tierFont);
        var halfSize = CellSize * 0.5f * scaleMultiplier;
        _tierBrush.Color = Color.FromArgb((int)(Math.Clamp(alphaMultiplier, 0f, 1f) * 255f), Color.Gold);
        var textPosition = new PointF(
            drawCenter.X + halfSize - textSize.Width - (TierPadding * scaleMultiplier),
            drawCenter.Y - halfSize + (TierPadding * scaleMultiplier));

        graphics.DrawString(tierText, _tierFont, _tierBrush, textPosition);
    }

    private void DrawAttackSpeedBuffIndicator(Graphics graphics, RuneEntity rune, Vector2 drawCenter, float scaleMultiplier, float alphaMultiplier)
    {
        if (!rune.Buffs.HasAttackSpeedBuff)
        {
            return;
        }

        var halfSize = CellSize * 0.5f * scaleMultiplier;
        var animationTime = (float)(Environment.TickCount64 * 0.0035);
        var bobOffset = MathF.Sin(animationTime) * BuffArrowBobAmplitude * scaleMultiplier;
        var pulse = 0.72f + (0.28f * ((MathF.Sin(animationTime + 0.8f) + 1f) * 0.5f));
        var centerX = drawCenter.X + halfSize - (BuffArrowPadding * scaleMultiplier) - ((BuffArrowHeadWidth * 0.5f) * scaleMultiplier);
        var topY = drawCenter.Y + halfSize - (BuffArrowPadding * scaleMultiplier) - ((BuffArrowStemHeight + BuffArrowHeadHeight) * scaleMultiplier) - bobOffset;
        var stemWidth = BuffArrowStemWidth * scaleMultiplier;
        var stemHeight = BuffArrowStemHeight * scaleMultiplier;
        var headWidth = BuffArrowHeadWidth * scaleMultiplier;
        var headHeight = BuffArrowHeadHeight * scaleMultiplier;
        var alpha = (int)(Math.Clamp(alphaMultiplier * pulse, 0f, 1f) * 255f);
        var detailAlpha = (int)(Math.Clamp(alphaMultiplier * (0.55f + (0.25f * pulse)), 0f, 1f) * 255f);

        _buffArrowBrush.Color = Color.FromArgb(alpha, 217, 68, 211);
        _buffArrowOutlinePen.Color = Color.FromArgb(alpha, 245, 184, 244);
        _buffArrowDetailPen.Color = Color.FromArgb(detailAlpha, 244, 176, 242);

        using var arrowPath = new GraphicsPath();
        arrowPath.AddPolygon(
        [
            new PointF(centerX, topY),
            new PointF(centerX + (headWidth * 0.5f), topY + (headHeight * 0.95f)),
            new PointF(centerX + (stemWidth * 0.78f), topY + headHeight),
            new PointF(centerX + (stemWidth * 0.5f), topY + headHeight + stemHeight),
            new PointF(centerX - (stemWidth * 0.5f), topY + headHeight + stemHeight),
            new PointF(centerX - (stemWidth * 0.78f), topY + headHeight),
            new PointF(centerX - (headWidth * 0.5f), topY + (headHeight * 0.95f))
        ]);

        graphics.FillPath(_buffArrowBrush, arrowPath);
        graphics.DrawPath(_buffArrowOutlinePen, arrowPath);

        var chevronInset = BuffArrowInnerChevronInset * scaleMultiplier;
        graphics.DrawLines(
            _buffArrowDetailPen,
        [
            new PointF(centerX - ((headWidth * 0.5f) - chevronInset), topY + (headHeight * 0.72f)),
            new PointF(centerX, topY + chevronInset),
            new PointF(centerX + ((headWidth * 0.5f) - chevronInset), topY + (headHeight * 0.72f))
        ]);
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
