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
    private const float HagalazIndicatorWidth = 31f;
    private const float HagalazIndicatorHeight = 8f;
    private const float HagalazIndicatorPadding = 9f;
    private const float HagalazIndicatorGap = 2f;
    private const float HagalazIndicatorCornerRadius = 4f;
    private const float HagalazIndicatorCellCornerRadius = 2f;

    private readonly IReadOnlyDictionary<string, Bitmap> _runeTextures;
    private readonly IReadOnlyList<Bitmap> _thurisazFrames;
    private readonly ImageAttributes _imageAttributes;
    private readonly Font _tierFont;
    private readonly SolidBrush _tierBrush;
    private readonly SolidBrush _buffArrowBrush;
    private readonly Pen _buffArrowDetailPen;
    private readonly Pen _buffArrowOutlinePen;
    private readonly SolidBrush _hagalazIndicatorPanelBrush;
    private readonly Pen _hagalazIndicatorPanelPen;
    private readonly SolidBrush _hagalazIndicatorCellFillBrush;
    private readonly SolidBrush _hagalazIndicatorCellEmptyBrush;
    private static readonly Color GeboArrowColor = Color.FromArgb(217, 68, 211);
    private static readonly Color GeboArrowOutlineColor = Color.FromArgb(245, 184, 244);
    private static readonly Color GeboArrowDetailColor = Color.FromArgb(244, 176, 242);
    private static readonly Color WunjoArrowColor = Color.FromArgb(213, 49, 56);
    private static readonly Color WunjoArrowOutlineColor = Color.FromArgb(248, 154, 158);
    private static readonly Color WunjoArrowDetailColor = Color.FromArgb(255, 205, 208);
    private static readonly Color DagazArrowColor = Color.FromArgb(246, 135, 1);
    private static readonly Color DagazArrowOutlineColor = Color.FromArgb(255, 214, 142);
    private static readonly Color DagazArrowDetailColor = Color.FromArgb(255, 238, 198);
    private static readonly Color HagalazIndicatorFillColor = Color.FromArgb(238, 150, 56);
    private static readonly Color HagalazIndicatorHighlightColor = Color.FromArgb(255, 221, 164);

    public RuneView(IReadOnlyDictionary<string, Bitmap> runeTextures, IReadOnlyList<Bitmap> thurisazFrames)
    {
        _runeTextures = runeTextures;
        _thurisazFrames = thurisazFrames;
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
        _hagalazIndicatorPanelBrush = new SolidBrush(Color.FromArgb(108, 42, 28, 18));
        _hagalazIndicatorPanelPen = new Pen(Color.FromArgb(176, 255, 221, 164), 1f)
        {
            LineJoin = LineJoin.Round
        };
        _hagalazIndicatorCellFillBrush = new SolidBrush(Color.FromArgb(255, HagalazIndicatorFillColor));
        _hagalazIndicatorCellEmptyBrush = new SolidBrush(Color.FromArgb(76, 255, 221, 164));
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
        DrawThurisazCharge(graphics, rune, drawCenter, drawWidth, drawHeight, alphaMultiplier);
        DrawTier(graphics, rune, drawCenter, scaleMultiplier, alphaMultiplier);
        DrawHagalazIndicator(graphics, rune, drawCenter, scaleMultiplier, alphaMultiplier);
        DrawBuffIndicators(graphics, rune, drawCenter, scaleMultiplier, alphaMultiplier);
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
        _hagalazIndicatorPanelBrush.Dispose();
        _hagalazIndicatorPanelPen.Dispose();
        _hagalazIndicatorCellFillBrush.Dispose();
        _hagalazIndicatorCellEmptyBrush.Dispose();
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

    private void DrawThurisazCharge(
        Graphics graphics,
        RuneEntity rune,
        Vector2 drawCenter,
        float runeDrawWidth,
        float runeDrawHeight,
        float alphaMultiplier)
    {
        if (rune.Stats.Type != RuneType.Thurisaz || _thurisazFrames.Count == 0)
        {
            return;
        }

        var progress = rune.State.ThurisazChargeProgress;
        if (progress <= 0.001f)
        {
            return;
        }

        var scale = SmoothStep(progress);
        var size = MathF.Min(runeDrawWidth, runeDrawHeight) * 0.5f * ThurisazTuning.VisualScaleMultiplier * scale;
        if (size <= 0.5f)
        {
            return;
        }

        var frameIndex = GetAnimationFrameIndex(_thurisazFrames.Count);
        var frame = _thurisazFrames[frameIndex];
        DrawRotatedTexture(
            graphics,
            frame,
            drawCenter,
            rune.State.ThurisazAimAngleRadians,
            size,
            size,
            alphaMultiplier * (0.45f + (0.55f * progress)));
    }

    private void DrawRotatedTexture(
        Graphics graphics,
        Bitmap texture,
        Vector2 center,
        float rotationRadians,
        float drawWidth,
        float drawHeight,
        float alphaMultiplier)
    {
        var graphicsState = graphics.Save();
        graphics.TranslateTransform(center.X, center.Y);
        graphics.RotateTransform(rotationRadians * (180f / MathF.PI));
        DrawTexture(
            graphics,
            texture,
            -(drawWidth * 0.5f),
            -(drawHeight * 0.5f),
            drawWidth,
            drawHeight,
            alphaMultiplier);
        graphics.Restore(graphicsState);
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

    private void DrawBuffIndicators(Graphics graphics, RuneEntity rune, Vector2 drawCenter, float scaleMultiplier, float alphaMultiplier)
    {
        if (!rune.Buffs.HasAttackSpeedBuff && !rune.Buffs.HasCriticalHitBuff && !rune.Buffs.HasMultiShotBuff)
        {
            return;
        }

        var arrowSlot = 0;

        if (rune.Buffs.HasCriticalHitBuff)
        {
            DrawBuffArrow(
                graphics,
                drawCenter,
                scaleMultiplier,
                alphaMultiplier,
                WunjoArrowColor,
                WunjoArrowOutlineColor,
                WunjoArrowDetailColor,
                arrowSlot * (BuffArrowHeadWidth + 4f) * scaleMultiplier);
            arrowSlot++;
        }

        if (rune.Buffs.HasAttackSpeedBuff)
        {
            DrawBuffArrow(
                graphics,
                drawCenter,
                scaleMultiplier,
                alphaMultiplier,
                GeboArrowColor,
                GeboArrowOutlineColor,
                GeboArrowDetailColor,
                arrowSlot * (BuffArrowHeadWidth + 4f) * scaleMultiplier);
            arrowSlot++;
        }

        if (rune.Buffs.HasMultiShotBuff)
        {
            DrawBuffArrow(
                graphics,
                drawCenter,
                scaleMultiplier,
                alphaMultiplier,
                DagazArrowColor,
                DagazArrowOutlineColor,
                DagazArrowDetailColor,
                arrowSlot * (BuffArrowHeadWidth + 4f) * scaleMultiplier);
        }
    }

    private void DrawHagalazIndicator(Graphics graphics, RuneEntity rune, Vector2 drawCenter, float scaleMultiplier, float alphaMultiplier)
    {
        if (rune.Stats.Type != RuneType.Hagalaz)
        {
            return;
        }

        var halfSize = CellSize * 0.5f * scaleMultiplier;
        var panelWidth = HagalazIndicatorWidth * scaleMultiplier;
        var panelHeight = HagalazIndicatorHeight * scaleMultiplier;
        var panelX = drawCenter.X - halfSize + (HagalazIndicatorPadding * scaleMultiplier);
        var panelY = drawCenter.Y + halfSize - panelHeight - (HagalazIndicatorPadding * scaleMultiplier);
        var panelBounds = new RectangleF(panelX, panelY, panelWidth, panelHeight);
        var panelAlpha = (int)(Math.Clamp(alphaMultiplier, 0f, 1f) * 255f);
        var cellFillAlpha = (int)(Math.Clamp(alphaMultiplier, 0f, 1f) * 255f);
        var cellEmptyAlpha = (int)(Math.Clamp(alphaMultiplier * 0.5f, 0f, 1f) * 255f);

        _hagalazIndicatorPanelBrush.Color = Color.FromArgb((int)(panelAlpha * 0.72f), 42, 28, 18);
        _hagalazIndicatorPanelPen.Color = Color.FromArgb((int)(panelAlpha * 0.9f), HagalazIndicatorHighlightColor);
        _hagalazIndicatorCellFillBrush.Color = Color.FromArgb(cellFillAlpha, HagalazIndicatorFillColor);
        _hagalazIndicatorCellEmptyBrush.Color = Color.FromArgb(cellEmptyAlpha, HagalazIndicatorHighlightColor);

        using var panelPath = CreateRoundedRectanglePath(panelBounds, HagalazIndicatorCornerRadius * scaleMultiplier);
        graphics.FillPath(_hagalazIndicatorPanelBrush, panelPath);
        graphics.DrawPath(_hagalazIndicatorPanelPen, panelPath);

        var innerPadding = 1.4f * scaleMultiplier;
        var totalGapWidth = HagalazIndicatorGap * scaleMultiplier * (HagalazTuning.ChargeSegmentCount - 1);
        var availableCellWidth = panelWidth - (innerPadding * 2f) - totalGapWidth;
        var cellWidth = availableCellWidth / HagalazTuning.ChargeSegmentCount;
        var cellHeight = panelHeight - (innerPadding * 2f);
        for (var i = 0; i < HagalazTuning.ChargeSegmentCount; i++)
        {
            var cellX = panelX + innerPadding + (i * (cellWidth + (HagalazIndicatorGap * scaleMultiplier)));
            var cellBounds = new RectangleF(cellX, panelY + innerPadding, cellWidth, cellHeight);
            using var cellPath = CreateRoundedRectanglePath(cellBounds, HagalazIndicatorCellCornerRadius * scaleMultiplier);
            graphics.FillPath(
                i < rune.State.HagalazChargeSegments ? _hagalazIndicatorCellFillBrush : _hagalazIndicatorCellEmptyBrush,
                cellPath);
        }
    }

    private void DrawBuffArrow(
        Graphics graphics,
        Vector2 drawCenter,
        float scaleMultiplier,
        float alphaMultiplier,
        Color fillColor,
        Color outlineColor,
        Color detailColor,
        float xOffset)
    {
        var halfSize = CellSize * 0.5f * scaleMultiplier;
        var animationTime = (float)(Environment.TickCount64 * 0.0035);
        var bobOffset = MathF.Sin(animationTime) * BuffArrowBobAmplitude * scaleMultiplier;
        var pulse = 0.72f + (0.28f * ((MathF.Sin(animationTime + 0.8f) + 1f) * 0.5f));
        var centerX = drawCenter.X + halfSize - (BuffArrowPadding * scaleMultiplier) - ((BuffArrowHeadWidth * 0.5f) * scaleMultiplier) - xOffset;
        var topY = drawCenter.Y + halfSize - (BuffArrowPadding * scaleMultiplier) - ((BuffArrowStemHeight + BuffArrowHeadHeight) * scaleMultiplier) - bobOffset;
        var stemWidth = BuffArrowStemWidth * scaleMultiplier;
        var stemHeight = BuffArrowStemHeight * scaleMultiplier;
        var headWidth = BuffArrowHeadWidth * scaleMultiplier;
        var headHeight = BuffArrowHeadHeight * scaleMultiplier;
        var alpha = (int)(Math.Clamp(alphaMultiplier * pulse, 0f, 1f) * 255f);
        var detailAlpha = (int)(Math.Clamp(alphaMultiplier * (0.55f + (0.25f * pulse)), 0f, 1f) * 255f);

        _buffArrowBrush.Color = Color.FromArgb(alpha, fillColor);
        _buffArrowOutlinePen.Color = Color.FromArgb(alpha, outlineColor);
        _buffArrowDetailPen.Color = Color.FromArgb(detailAlpha, detailColor);

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

    private static GraphicsPath CreateRoundedRectanglePath(RectangleF bounds, float radius)
    {
        var path = new GraphicsPath();
        if (bounds.Width <= 0f || bounds.Height <= 0f)
        {
            return path;
        }

        var clampedRadius = MathF.Max(0.1f, MathF.Min(radius, MathF.Min(bounds.Width, bounds.Height) * 0.5f));
        var diameter = clampedRadius * 2f;
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    private static int GetAnimationFrameIndex(int frameCount)
    {
        if (frameCount <= 1)
        {
            return 0;
        }

        var totalElapsedSeconds = Environment.TickCount64 / 1000d;
        return (int)(totalElapsedSeconds / ThurisazTuning.AnimationFrameDurationSeconds) % frameCount;
    }

    private static float SmoothStep(float value)
    {
        var clamped = Math.Clamp(value, 0f, 1f);
        return clamped * clamped * (3f - (2f * clamped));
    }
}
