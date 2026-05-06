using System.Globalization;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Views;

public sealed partial class GameRenderer
{
    private void DrawInGameBuildPanel(Graphics graphics, GameState gameState)
    {
        var buildSelection = gameState.Ui.BuildSelection;
        var panelBounds = GetBottomPanelBounds();
        var panelTexture = GetPreparedBottomPanelTexture(buildSelection, gameState.Runes, panelBounds.Size);
        graphics.DrawImageUnscaled(panelTexture, panelBounds.Location);
    }

    private Bitmap GetPreparedBottomPanelTexture(
        BuildSelectionState buildSelection,
        IReadOnlyList<RuneEntity> tableRunes,
        Size targetSize)
    {
        var tierSums = CalculateSelectedRuneTierSums(buildSelection, tableRunes);
        var buildKey = CreateBottomPanelBuildKey(buildSelection, tierSums);
        var key = $"bottom-panel-build:{targetSize.Width}x{targetSize.Height}:{buildKey}";
        if (_preparedBottomPanelCache.TryGetValue(key, out var cachedTexture))
        {
            return cachedTexture;
        }

        foreach (var texture in _preparedBottomPanelCache.Values)
        {
            texture.Dispose();
        }

        _preparedBottomPanelCache.Clear();

        var basePanelTexture = GetScaledTexture("bottom-panel", _bottomPanelTexture, targetSize);
        var preparedTexture = new Bitmap(targetSize.Width, targetSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        using var preparedGraphics = Graphics.FromImage(preparedTexture);
        preparedGraphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
        preparedGraphics.DrawImageUnscaled(basePanelTexture, Point.Empty);
        preparedGraphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
        preparedGraphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        preparedGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        preparedGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
        preparedGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        preparedGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var slots = CreateBottomPanelSlots(new Rectangle(Point.Empty, targetSize));
        for (var i = 0; i < slots.Count; i++)
        {
            var slotBounds = slots[i];
            var cellImageBounds = Inflate(slotBounds, 6, 6);
            var cellTexture = GetScaledTexture("bottom-panel-cell", _selectionCellTexture, cellImageBounds.Size);
            preparedGraphics.DrawImageUnscaled(cellTexture, cellImageBounds.Location);

            var hasRune = i < buildSelection.SelectedRunes.Count;
            if (!hasRune)
            {
                continue;
            }

            _runeView.DrawIcon(preparedGraphics, buildSelection.SelectedRunes[i], Inflate(slotBounds, -9, -9));
            DrawBottomPanelTierBadge(preparedGraphics, slotBounds, tierSums.GetValueOrDefault(buildSelection.SelectedRunes[i]));
        }

        _preparedBottomPanelCache.Add(key, preparedTexture);
        return preparedTexture;
    }

    private string CreateBottomPanelBuildKey(
        BuildSelectionState buildSelection,
        IReadOnlyDictionary<RuneType, int> tierSums)
    {
        _bottomPanelKeyBuilder.Clear();
        for (var i = 0; i < buildSelection.SelectedRunes.Count; i++)
        {
            if (i > 0)
            {
                _bottomPanelKeyBuilder.Append('-');
            }

            var runeType = buildSelection.SelectedRunes[i];
            _bottomPanelKeyBuilder
                .Append(runeType)
                .Append(':')
                .Append(tierSums.GetValueOrDefault(runeType));
        }

        return _bottomPanelKeyBuilder.ToString();
    }

    private static Dictionary<RuneType, int> CalculateSelectedRuneTierSums(
        BuildSelectionState buildSelection,
        IReadOnlyList<RuneEntity> tableRunes)
    {
        var result = new Dictionary<RuneType, int>();
        foreach (var runeType in buildSelection.SelectedRunes)
        {
            result.TryAdd(runeType, 0);
        }

        foreach (var rune in tableRunes)
        {
            if (result.ContainsKey(rune.Stats.Type))
            {
                result[rune.Stats.Type] += rune.Stats.Tier;
            }
        }

        return result;
    }

    private void DrawBottomPanelTierBadge(Graphics graphics, Rectangle slotBounds, int tierSum)
    {
        var badgeBounds = new Rectangle(
            slotBounds.Right - 25,
            slotBounds.Bottom - 20,
            22,
            16);
        var shadowBounds = new Rectangle(badgeBounds.X, badgeBounds.Y + 1, badgeBounds.Width, badgeBounds.Height);

        using var shadowPath = CreateRoundedRectanglePath(shadowBounds, 8);
        using var badgePath = CreateRoundedRectanglePath(badgeBounds, 8);
        using var shadowBrush = new SolidBrush(Color.FromArgb(104, 0, 0, 0));
        using var badgeBrush = new SolidBrush(Color.FromArgb(210, 8, 7, 7));
        using var badgeBorderPen = new Pen(Color.FromArgb(178, 150, 118, 78), 1f);
        using var textBrush = new SolidBrush(Color.FromArgb(238, 205, 158, 91));
        using var textShadowBrush = new SolidBrush(Color.FromArgb(126, 4, 3, 2));

        graphics.FillPath(shadowBrush, shadowPath);
        graphics.FillPath(badgeBrush, badgePath);
        graphics.DrawPath(badgeBorderPen, badgePath);

        var textBounds = new Rectangle(badgeBounds.X, badgeBounds.Y + 1, badgeBounds.Width, badgeBounds.Height);
        var textShadowBounds = new Rectangle(textBounds.X, textBounds.Y + 1, textBounds.Width, textBounds.Height);
        AdaptiveTextRenderer.DrawCentered(
            graphics,
            tierSum.ToString(),
            _buildTooltipStatFont,
            textShadowBrush,
            textShadowBounds,
            _centerStringFormat,
            minimumScale: 0.55f);
        AdaptiveTextRenderer.DrawCentered(
            graphics,
            tierSum.ToString(),
            _buildTooltipStatFont,
            textBrush,
            textBounds,
            _centerStringFormat,
            minimumScale: 0.55f);
    }

    private Rectangle GetBottomPanelBounds()
    {
        var viewport = _board.ViewportBounds;
        const int targetPanelWidth = 400;
        const int bottomMargin = 22;
        var panelWidth = Math.Min(targetPanelWidth, viewport.Width - 32);
        var panelHeight = Math.Max(1, (int)MathF.Round(panelWidth * (_bottomPanelTexture.Height / (float)_bottomPanelTexture.Width)));
        return new Rectangle(
            viewport.Left + ((viewport.Width - panelWidth) / 2),
            viewport.Bottom - bottomMargin - panelHeight,
            panelWidth,
            panelHeight);
    }

    private static IReadOnlyList<Rectangle> CreateBottomPanelSlots(Rectangle panelBounds)
    {
        const int slotGap = 18;
        var slotHeight = Math.Min(50, Math.Max(1, panelBounds.Height - 28));
        var slotWidth = Math.Max(1, (int)MathF.Round(slotHeight * (816f / 894f)));
        var totalWidth = (BuildSelectionState.BuildSize * slotWidth) + ((BuildSelectionState.BuildSize - 1) * slotGap);
        var startX = panelBounds.Left + ((panelBounds.Width - totalWidth) / 2);
        var startY = panelBounds.Top + ((panelBounds.Height - slotHeight) / 2);
        var slots = new List<Rectangle>(BuildSelectionState.BuildSize);

        for (var i = 0; i < BuildSelectionState.BuildSize; i++)
        {
            slots.Add(new Rectangle(
                startX + (i * (slotWidth + slotGap)),
                startY,
                slotWidth,
                slotHeight));
        }

        return slots;
    }

    private void DrawHeartsUi(Graphics graphics, GameState gameState)
    {
        var basePanelRect = WaveUiLayout.GetHeartPanelBounds(_board.ViewportBounds);
        var panelRect = new RectangleF(
            basePanelRect.X + gameState.Ui.HeartLossPanelShakeOffset,
            basePanelRect.Y,
            basePanelRect.Width,
            basePanelRect.Height);

        DrawTopBadgeBackground(graphics, "top-heart-badge", _heartBadgeTexture, panelRect);

        if (gameState.Ui.HeartLossPanelFlashOpacity > 0f)
        {
            using var panelPath = CreateRoundedRectanglePath(Rectangle.Round(panelRect), 18);
            var flashOpacity = gameState.Ui.HeartLossPanelFlashOpacity;
            using var flashBrush = new SolidBrush(Color.FromArgb((int)(flashOpacity * 92f), 176, 34, 44));
            using var flashBorderPen = new Pen(Color.FromArgb((int)(flashOpacity * 168f), 248, 112, 112), 1.8f);
            graphics.FillPath(flashBrush, panelPath);
            graphics.DrawPath(flashBorderPen, panelPath);
        }

        var drawY = panelRect.Y + ((panelRect.Height - HeartIconSize) * 0.5f);
        var totalWidth = (GameState.MaxHearts * HeartIconSize) + ((GameState.MaxHearts - 1) * HeartIconSpacing);
        var drawX = panelRect.X + ((panelRect.Width - totalWidth) * 0.5f);

        for (var i = 0; i < GameState.MaxHearts; i++)
        {
            var iconBounds = new RectangleF(drawX, drawY, HeartIconSize, HeartIconSize);
            var glowOpacity = gameState.Ui.GetHeartLossGlowOpacity(i);
            if (glowOpacity > 0f)
            {
                using var glowBrush = new SolidBrush(Color.FromArgb((int)(glowOpacity * 118f), 255, 82, 92));
                graphics.FillEllipse(glowBrush, ScaleRectangle(iconBounds, 1.5f));
            }

            var scale = gameState.Ui.GetHeartLossScale(i);
            iconBounds = scale > 1f
                ? ScaleRectangle(iconBounds, scale)
                : iconBounds;

            var texture = i < gameState.RemainingHearts ? _heartTexture : _brokenHeartTexture;
            graphics.DrawImage(texture, iconBounds.X, iconBounds.Y, iconBounds.Width, iconBounds.Height);
            drawX += HeartIconSize + HeartIconSpacing;
        }

        if (gameState.Ui.HeartLossCount > 0 && gameState.Ui.HeartLossTextOpacity > 0f)
        {
            var textRect = new RectangleF(
                panelRect.X,
                panelRect.Y - 8f - gameState.Ui.HeartLossTextRiseOffset,
                panelRect.Width,
                22f);
            var textShadowRect = new RectangleF(textRect.X, textRect.Y + 1f, textRect.Width, textRect.Height);
            var textValue = $"-{gameState.Ui.HeartLossCount}";
            using var textBrush = new SolidBrush(Color.FromArgb((int)(gameState.Ui.HeartLossTextOpacity * 255f), 255, 218, 218));
            using var textShadowBrush = new SolidBrush(Color.FromArgb((int)(gameState.Ui.HeartLossTextOpacity * 124f), 52, 10, 10));
            graphics.DrawString(textValue, _economyValueFont, textShadowBrush, textShadowRect, _centerStringFormat);
            graphics.DrawString(textValue, _economyValueFont, textBrush, textRect, _centerStringFormat);
        }
    }

    private void DrawHeartLossScreenFlash(Graphics graphics, GameState gameState)
    {
        if (gameState.Ui.HeartLossScreenFlashOpacity <= 0f)
        {
            return;
        }

        using var flashBrush = new SolidBrush(Color.FromArgb(
            (int)(gameState.Ui.HeartLossScreenFlashOpacity * 255f),
            108,
            14,
            18));
        graphics.FillRectangle(flashBrush, _board.ViewportBounds);
    }

    private void DrawWaveUi(Graphics graphics, GameState gameState)
    {
        var panelRect = WaveUiLayout.GetWavePanelBounds(_board.ViewportBounds);

        DrawTopBadgeBackground(graphics, "top-badge", _badgeTexture, panelRect);

        DrawTopBadgeText(graphics, panelRect, "Волна", gameState.Waves.CurrentWaveNumber.ToString(), _topBadgeTitleBrush, _topBadgeValueBrush);
    }

    private void DrawRecordUi(Graphics graphics, GameState gameState)
    {
        var panelRect = WaveUiLayout.GetRecordPanelBounds(_board.ViewportBounds);

        DrawTopBadgeBackground(graphics, "top-badge", _badgeTexture, panelRect);

        DrawTopBadgeText(graphics, panelRect, "Рекорд", gameState.BestCompletedWaveRecord.ToString(CultureInfo.InvariantCulture), _topBadgeTitleBrush, _topBadgeValueBrush);
    }

    private void DrawRunePointsUi(Graphics graphics, GameState gameState)
    {
        var panelRect = WaveUiLayout.GetRunePointsPanelBounds(_board.ViewportBounds);

        DrawTopBadgeBackground(graphics, "top-badge", _badgeTexture, panelRect);

        DrawTopBadgeText(graphics, panelRect, "Энергия", $"{gameState.Economy.RunePoints} RP", _topBadgeTitleBrush, _topBadgeValueBrush);
    }

    private void DrawTopBadgeText(Graphics graphics, RectangleF panelRect, string title, string value, Brush titleBrush, Brush valueBrush)
    {
        const float titleHeight = 13f;
        const float valueHeight = 18f;
        const float lineGap = 1f;
        var contentTop = panelRect.Y + ((panelRect.Height - titleHeight - lineGap - valueHeight) * 0.5f);
        var titleRect = new RectangleF(panelRect.X, contentTop, panelRect.Width, titleHeight);
        var valueRect = new RectangleF(panelRect.X, contentTop + titleHeight + lineGap, panelRect.Width, valueHeight);

        graphics.DrawString(title, _economyTitleFont, titleBrush, titleRect, _centerStringFormat);
        AdaptiveTextRenderer.DrawCentered(
            graphics,
            value,
            _economyValueFont,
            valueBrush,
            valueRect,
            _centerStringFormat);
    }

    private void DrawTopBadgeBackground(Graphics graphics, string cacheKey, Bitmap texture, RectangleF bounds)
    {
        var roundedBounds = Rectangle.Round(bounds);
        var badgeTexture = GetScaledTexture(cacheKey, texture, roundedBounds.Size);
        graphics.DrawImageUnscaled(badgeTexture, roundedBounds.Location);
    }

    private void DrawBottomControlCostBadges(Graphics graphics, GameState gameState)
    {
        var canAfford = gameState.Economy.CanAffordCurrentRuneSpawn;
        DrawControlCostBadge(graphics, _board.BagBounds, gameState.Economy.CurrentRuneSpawnCost, canAfford);
        DrawControlCostBadge(graphics, _board.RerollBounds, gameState.Economy.CurrentRuneSpawnCost, canAfford);
    }

    private void DrawControlCostBadge(Graphics graphics, Rectangle controlBounds, int cost, bool canAfford)
    {
        var layout = GetControlCostBadgeLayout(controlBounds);
        var textBrush = canAfford
            ? _controlCostAffordableTextBrush
            : _controlCostUnavailableTextBrush;

        graphics.FillPath(_controlCostShadowBrush, layout.ShadowPath);
        graphics.FillPath(_controlCostBadgeBrush, layout.BadgePath);
        graphics.DrawPath(_controlCostBadgeBorderPen, layout.BadgePath);
        var badgeText = $"{cost} RP";
        AdaptiveTextRenderer.DrawCentered(
            graphics,
            badgeText,
            _bagCostFont,
            _controlCostTextShadowBrush,
            layout.TextShadowBounds,
            _centerStringFormat);
        AdaptiveTextRenderer.DrawCentered(
            graphics,
            badgeText,
            _bagCostFont,
            textBrush,
            layout.TextBounds,
            _centerStringFormat);
    }

    private ControlCostBadgeRenderLayout GetControlCostBadgeLayout(Rectangle controlBounds)
    {
        if (_controlCostBadgeLayoutCache.TryGetValue(controlBounds, out var cachedLayout))
        {
            return cachedLayout;
        }

        const float badgeWidth = 78f;
        const float badgeHeight = 30f;
        var badgeRect = new RectangleF(
            controlBounds.Left + ((controlBounds.Width - badgeWidth) * 0.5f),
            controlBounds.Top + (controlBounds.Height * 0.55f),
            badgeWidth,
            badgeHeight);
        var shadowRect = new RectangleF(badgeRect.X, badgeRect.Y + 2f, badgeRect.Width, badgeRect.Height);
        var textRect = new RectangleF(badgeRect.X, badgeRect.Y + 3f, badgeRect.Width, badgeRect.Height);
        var textShadowRect = new RectangleF(textRect.X, textRect.Y + 1f, textRect.Width, textRect.Height);
        var layout = new ControlCostBadgeRenderLayout(
            CreateRoundedRectanglePath(Rectangle.Round(shadowRect), 13),
            CreateRoundedRectanglePath(Rectangle.Round(badgeRect), 13),
            textRect,
            textShadowRect);

        _controlCostBadgeLayoutCache.Add(controlBounds, layout);
        return layout;
    }

    private sealed class ControlCostBadgeRenderLayout : IDisposable
    {
        public ControlCostBadgeRenderLayout(
            GraphicsPath shadowPath,
            GraphicsPath badgePath,
            RectangleF textBounds,
            RectangleF textShadowBounds)
        {
            ShadowPath = shadowPath;
            BadgePath = badgePath;
            TextBounds = textBounds;
            TextShadowBounds = textShadowBounds;
        }

        public GraphicsPath ShadowPath { get; }

        public GraphicsPath BadgePath { get; }

        public RectangleF TextBounds { get; }

        public RectangleF TextShadowBounds { get; }

        public void Dispose()
        {
            ShadowPath.Dispose();
            BadgePath.Dispose();
        }
    }
}
