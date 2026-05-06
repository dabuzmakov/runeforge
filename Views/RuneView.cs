using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Numerics;
using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Views;

public sealed class RuneView : IDisposable
{
    private const float MaxDrawSize = 76f;
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
    private const float JeraStackPadding = 6f;
    private const float JeraBadgeMinWidth = 34f;
    private const float JeraBadgeHeight = 15f;
    private const float JeraBadgeHorizontalPadding = 5f;
    private const float JeraBadgeCornerRadius = 5f;
    private const float TiwazIndicatorPadding = 6f;
    private const float TiwazIndicatorWidth = 34f;
    private const float TiwazIndicatorHeight = 10f;
    private const float TiwazIndicatorCornerRadius = 5f;
    private const float TiwazIndicatorInnerInset = 1.5f;

    private readonly IReadOnlyDictionary<string, Bitmap> _runeTextures;
    private readonly IReadOnlyList<Bitmap> _thurisazFrames;
    private readonly ImageAttributes _imageAttributes;
    private readonly Font _tierFont;
    private readonly Font _jeraStackFont;
    private readonly StringFormat _centerTextFormat;
    private readonly SolidBrush _tierBrush;
    private readonly SolidBrush _jeraStackBrush;
    private readonly SolidBrush _jeraStackShadowBrush;
    private readonly SolidBrush _jeraBadgeBrush;
    private readonly Pen _jeraBadgeBorderPen;
    private readonly SolidBrush _buffArrowBrush;
    private readonly Pen _buffArrowDetailPen;
    private readonly Pen _buffArrowOutlinePen;
    private readonly SolidBrush _hagalazIndicatorPanelBrush;
    private readonly Pen _hagalazIndicatorPanelPen;
    private readonly SolidBrush _hagalazIndicatorCellFillBrush;
    private readonly SolidBrush _hagalazIndicatorCellEmptyBrush;
    private readonly SolidBrush _tiwazIndicatorBackgroundBrush;
    private readonly SolidBrush _tiwazIndicatorBrush;
    private readonly Pen _tiwazIndicatorBorderPen;
    private readonly SolidBrush _tiwazIndicatorHighlightBrush;
    private readonly Dictionary<int, string> _tierTextCache;
    private readonly Dictionary<int, SizeF> _tierTextSizeCache;
    private readonly Dictionary<int, SizeF> _jeraStackTextSizeCache;
    private readonly GraphicsPath _scratchPath;
    private readonly PointF[] _buffArrowPoints;
    private readonly PointF[] _buffChevronPoints;
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
    private static readonly Color JeraStackColor = Color.FromArgb(92, 205, 245);
    private static readonly Color TiwazIndicatorColor = Color.FromArgb(245, 198, 16);
    private static readonly Color TiwazIndicatorBorderColor = Color.FromArgb(56, 41, 10);

    public RuneView(IReadOnlyDictionary<string, Bitmap> runeTextures, IReadOnlyList<Bitmap> thurisazFrames)
    {
        _runeTextures = runeTextures;
        _thurisazFrames = thurisazFrames;
        _imageAttributes = new ImageAttributes();
        _tierFont = FontLibrary.Create(12f, FontStyle.Bold);
        _jeraStackFont = FontLibrary.CreateNumeric(12f, FontStyle.Bold);
        _centerTextFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.NoWrap
        };
        _tierBrush = new SolidBrush(Color.Gold);
        _jeraStackBrush = new SolidBrush(JeraStackColor);
        _jeraStackShadowBrush = new SolidBrush(Color.FromArgb(180, 14, 18, 24));
        _jeraBadgeBrush = new SolidBrush(Color.FromArgb(172, 15, 24, 34));
        _jeraBadgeBorderPen = new Pen(Color.FromArgb(224, 135, 228, 255), 1f)
        {
            LineJoin = LineJoin.Round
        };
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
        _tiwazIndicatorBackgroundBrush = new SolidBrush(Color.FromArgb(210, 24, 20, 12));
        _tiwazIndicatorBrush = new SolidBrush(TiwazIndicatorColor);
        _tiwazIndicatorBorderPen = new Pen(TiwazIndicatorBorderColor, 1.2f)
        {
            LineJoin = LineJoin.Round
        };
        _tiwazIndicatorHighlightBrush = new SolidBrush(Color.FromArgb(255, 255, 232, 142));
        _tierTextCache = new Dictionary<int, string>();
        _tierTextSizeCache = new Dictionary<int, SizeF>();
        _jeraStackTextSizeCache = new Dictionary<int, SizeF>();
        _scratchPath = new GraphicsPath();
        _buffArrowPoints = new PointF[7];
        _buffChevronPoints = new PointF[3];
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
        DrawJeraStacks(graphics, rune, drawCenter, scaleMultiplier, alphaMultiplier);
        DrawTiwazDischargeIndicator(graphics, rune, drawCenter, scaleMultiplier, alphaMultiplier);
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
        _jeraStackFont.Dispose();
        _centerTextFormat.Dispose();
        _tierBrush.Dispose();
        _jeraStackBrush.Dispose();
        _jeraStackShadowBrush.Dispose();
        _jeraBadgeBrush.Dispose();
        _jeraBadgeBorderPen.Dispose();
        _buffArrowBrush.Dispose();
        _buffArrowDetailPen.Dispose();
        _buffArrowOutlinePen.Dispose();
        _hagalazIndicatorPanelBrush.Dispose();
        _hagalazIndicatorPanelPen.Dispose();
        _hagalazIndicatorCellFillBrush.Dispose();
        _hagalazIndicatorCellEmptyBrush.Dispose();
        _tiwazIndicatorBackgroundBrush.Dispose();
        _tiwazIndicatorBrush.Dispose();
        _tiwazIndicatorBorderPen.Dispose();
        _tiwazIndicatorHighlightBrush.Dispose();
        _scratchPath.Dispose();
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
        var tierText = GetTierText(rune.Stats.Tier);
        var textSize = GetTierTextSize(graphics, rune.Stats.Tier, tierText);
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

    private void DrawJeraStacks(Graphics graphics, RuneEntity rune, Vector2 drawCenter, float scaleMultiplier, float alphaMultiplier)
    {
        if (rune.Stats.Type != RuneType.Jera)
        {
            return;
        }

        var stackCount = rune.State.JeraSharedStacks;
        var stackText = stackCount.ToString();
        var textSize = GetJeraStackTextSize(graphics, stackCount, stackText);
        var halfSize = CellSize * 0.5f * scaleMultiplier;
        var alpha = (int)(Math.Clamp(alphaMultiplier, 0f, 1f) * 255f);
        _jeraStackBrush.Color = Color.FromArgb(alpha, JeraStackColor);
        _jeraStackShadowBrush.Color = Color.FromArgb((int)(alpha * 0.8f), 14, 18, 24);
        _jeraBadgeBrush.Color = Color.FromArgb((int)(alpha * 0.82f), 15, 24, 34);
        _jeraBadgeBorderPen.Color = Color.FromArgb((int)(alpha * 0.92f), 135, 228, 255);

        var badgeWidth = MathF.Max(
            JeraBadgeMinWidth * scaleMultiplier,
            textSize.Width + (JeraBadgeHorizontalPadding * 2f * scaleMultiplier));
        var badgeHeight = JeraBadgeHeight * scaleMultiplier;
        var badgeBounds = new RectangleF(
            drawCenter.X - halfSize + (JeraStackPadding * scaleMultiplier),
            drawCenter.Y + halfSize - badgeHeight - (JeraStackPadding * scaleMultiplier),
            badgeWidth,
            badgeHeight);

        var badgePath = RebuildRoundedRectanglePath(badgeBounds, JeraBadgeCornerRadius * scaleMultiplier);
        graphics.FillPath(_jeraBadgeBrush, badgePath);
        graphics.DrawPath(_jeraBadgeBorderPen, badgePath);

        var textBounds = new RectangleF(
            badgeBounds.X,
            badgeBounds.Y + (0.6f * scaleMultiplier),
            badgeBounds.Width,
            badgeBounds.Height);
        var shadowBounds = new RectangleF(
            textBounds.X + scaleMultiplier,
            textBounds.Y + scaleMultiplier,
            textBounds.Width,
            textBounds.Height);

        AdaptiveTextRenderer.DrawCentered(
            graphics,
            stackText,
            _jeraStackFont,
            _jeraStackShadowBrush,
            shadowBounds,
            _centerTextFormat);
        AdaptiveTextRenderer.DrawCentered(
            graphics,
            stackText,
            _jeraStackFont,
            _jeraStackBrush,
            textBounds,
            _centerTextFormat);
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

        var panelPath = RebuildRoundedRectanglePath(panelBounds, HagalazIndicatorCornerRadius * scaleMultiplier);
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
            var cellPath = RebuildRoundedRectanglePath(cellBounds, HagalazIndicatorCellCornerRadius * scaleMultiplier);
            graphics.FillPath(
                i < rune.State.HagalazChargeSegments ? _hagalazIndicatorCellFillBrush : _hagalazIndicatorCellEmptyBrush,
                cellPath);
        }
    }

    private void DrawTiwazDischargeIndicator(Graphics graphics, RuneEntity rune, Vector2 drawCenter, float scaleMultiplier, float alphaMultiplier)
    {
        if (rune.Stats.Type != RuneType.Tiwaz || !rune.State.IsTiwazDischargeIndicatorActive)
        {
            return;
        }

        var halfSize = CellSize * 0.5f * scaleMultiplier;
        var indicatorWidth = TiwazIndicatorWidth * scaleMultiplier;
        var indicatorHeight = TiwazIndicatorHeight * scaleMultiplier;
        var indicatorX = drawCenter.X - halfSize + (TiwazIndicatorPadding * scaleMultiplier);
        var indicatorY = drawCenter.Y + halfSize - indicatorHeight - (TiwazIndicatorPadding * scaleMultiplier);
        var indicatorBounds = new RectangleF(indicatorX, indicatorY, indicatorWidth, indicatorHeight);
        var alpha = (int)(Math.Clamp(alphaMultiplier, 0f, 1f) * 255f);

        _tiwazIndicatorBackgroundBrush.Color = Color.FromArgb((int)(alpha * 0.9f), 24, 20, 12);
        _tiwazIndicatorBrush.Color = Color.FromArgb(alpha, TiwazIndicatorColor);
        _tiwazIndicatorBorderPen.Color = Color.FromArgb(alpha, TiwazIndicatorBorderColor);
        _tiwazIndicatorHighlightBrush.Color = Color.FromArgb((int)(alpha * 0.88f), 255, 232, 142);

        var indicatorPath = RebuildRoundedRectanglePath(indicatorBounds, TiwazIndicatorCornerRadius * scaleMultiplier);
        graphics.FillPath(_tiwazIndicatorBackgroundBrush, indicatorPath);
        graphics.DrawPath(_tiwazIndicatorBorderPen, indicatorPath);

        var progress = Math.Clamp(rune.State.TiwazDischargeProgress, 0f, 1f);
        if (progress <= 0.001f)
        {
            return;
        }

        var innerInset = TiwazIndicatorInnerInset * scaleMultiplier;
        var fillBounds = new RectangleF(
            indicatorX + innerInset,
            indicatorY + innerInset,
            MathF.Max(0f, (indicatorWidth - (innerInset * 2f)) * progress),
            indicatorHeight - (innerInset * 2f));
        var fillPath = RebuildRoundedRectanglePath(fillBounds, MathF.Min(fillBounds.Width, fillBounds.Height) * 0.45f);
        graphics.FillPath(_tiwazIndicatorBrush, fillPath);

        var gleamWidth = MathF.Max(0f, fillBounds.Width - (2f * scaleMultiplier));
        if (gleamWidth > 1f)
        {
            var gleamBounds = new RectangleF(
                fillBounds.X + scaleMultiplier,
                fillBounds.Y + scaleMultiplier,
                gleamWidth,
                MathF.Max(1f, (fillBounds.Height * 0.42f) - scaleMultiplier));
            var gleamPath = RebuildRoundedRectanglePath(gleamBounds, MathF.Min(gleamBounds.Width, gleamBounds.Height) * 0.45f);
            graphics.FillPath(_tiwazIndicatorHighlightBrush, gleamPath);
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

        _buffArrowPoints[0] = new PointF(centerX, topY);
        _buffArrowPoints[1] = new PointF(centerX + (headWidth * 0.5f), topY + (headHeight * 0.95f));
        _buffArrowPoints[2] = new PointF(centerX + (stemWidth * 0.78f), topY + headHeight);
        _buffArrowPoints[3] = new PointF(centerX + (stemWidth * 0.5f), topY + headHeight + stemHeight);
        _buffArrowPoints[4] = new PointF(centerX - (stemWidth * 0.5f), topY + headHeight + stemHeight);
        _buffArrowPoints[5] = new PointF(centerX - (stemWidth * 0.78f), topY + headHeight);
        _buffArrowPoints[6] = new PointF(centerX - (headWidth * 0.5f), topY + (headHeight * 0.95f));
        _scratchPath.Reset();
        _scratchPath.AddPolygon(_buffArrowPoints);

        graphics.FillPath(_buffArrowBrush, _scratchPath);
        graphics.DrawPath(_buffArrowOutlinePen, _scratchPath);

        var chevronInset = BuffArrowInnerChevronInset * scaleMultiplier;
        _buffChevronPoints[0] = new PointF(centerX - ((headWidth * 0.5f) - chevronInset), topY + (headHeight * 0.72f));
        _buffChevronPoints[1] = new PointF(centerX, topY + chevronInset);
        _buffChevronPoints[2] = new PointF(centerX + ((headWidth * 0.5f) - chevronInset), topY + (headHeight * 0.72f));
        graphics.DrawLines(_buffArrowDetailPen, _buffChevronPoints);
    }

    private string GetTierText(int tier)
    {
        if (_tierTextCache.TryGetValue(tier, out var text))
        {
            return text;
        }

        text = ToRoman(tier);
        _tierTextCache.Add(tier, text);
        return text;
    }

    private SizeF GetTierTextSize(Graphics graphics, int tier, string text)
    {
        if (_tierTextSizeCache.TryGetValue(tier, out var size))
        {
            return size;
        }

        size = graphics.MeasureString(text, _tierFont);
        _tierTextSizeCache.Add(tier, size);
        return size;
    }

    private SizeF GetJeraStackTextSize(Graphics graphics, int stackCount, string text)
    {
        if (_jeraStackTextSizeCache.TryGetValue(stackCount, out var size))
        {
            return size;
        }

        if (_jeraStackTextSizeCache.Count > 512)
        {
            _jeraStackTextSizeCache.Clear();
        }

        size = graphics.MeasureString(text, _jeraStackFont);
        _jeraStackTextSizeCache.Add(stackCount, size);
        return size;
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

    private GraphicsPath RebuildRoundedRectanglePath(RectangleF bounds, float radius)
    {
        _scratchPath.Reset();
        AddRoundedRectangle(_scratchPath, bounds, radius);
        return _scratchPath;
    }

    private static void AddRoundedRectangle(GraphicsPath path, RectangleF bounds, float radius)
    {
        if (bounds.Width <= 0f || bounds.Height <= 0f)
        {
            return;
        }

        var clampedRadius = MathF.Max(0.1f, MathF.Min(radius, MathF.Min(bounds.Width, bounds.Height) * 0.5f));
        var diameter = clampedRadius * 2f;
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
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
