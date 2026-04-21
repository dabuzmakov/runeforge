using System.Drawing;
using runeforge.Models;

namespace runeforge.Views;

public sealed partial class GameRenderer
{
    private void DrawDefeatOverlay(Graphics graphics, GameState gameState)
    {
        if (!gameState.IsDefeated)
        {
            return;
        }

        var viewport = graphics.VisibleClipBounds;
        graphics.FillRectangle(_defeatOverlayBrush, viewport);

        var panelWidth = 360f;
        var panelHeight = 104f;
        var panelRect = new RectangleF(
            viewport.Left + ((viewport.Width - panelWidth) * 0.5f),
            viewport.Top + ((viewport.Height - panelHeight) * 0.5f),
            panelWidth,
            panelHeight);

        using var panelPath = CreateRoundedRectanglePath(Rectangle.Round(panelRect), 22);
        graphics.FillPath(_defeatPanelBrush, panelPath);
        graphics.DrawPath(_defeatPanelBorderPen, panelPath);

        var titleRect = new RectangleF(panelRect.X, panelRect.Y, panelRect.Width, panelRect.Height);
        graphics.DrawString("Defeat", _defeatTitleFont, _defeatTextBrush, titleRect, _centerStringFormat);
    }

    private void DrawBuildSelection(Graphics graphics, BuildSelectionState buildSelection)
    {
        graphics.Clear(BackgroundColor);

        using var overlayBrush = new SolidBrush(Color.FromArgb(224, 10, 11, 15));
        using var panelBrush = new SolidBrush(Color.FromArgb(44, 40, 52));
        using var panelInnerBrush = new SolidBrush(Color.FromArgb(30, 30, 38));
        using var panelBorderPen = new Pen(Color.FromArgb(92, 86, 104), 2f);
        using var cardBrush = new SolidBrush(Color.FromArgb(30, 30, 38));
        using var cardBorderPen = new Pen(Color.FromArgb(58, 58, 70), 1f);
        using var selectedCardBrush = new SolidBrush(Color.FromArgb(54, 96, 88, 86));
        using var selectedBorderPen = new Pen(Color.FromArgb(140, 128, 118), 1.8f);
        using var accentBrush = new SolidBrush(Color.FromArgb(140, 128, 118));
        using var textBrush = new SolidBrush(Color.FromArgb(236, 230, 220));
        using var subtleTextBrush = new SolidBrush(Color.FromArgb(176, 182, 176, 188));
        using var emptySlotBrush = new SolidBrush(Color.FromArgb(30, 30, 38));
        using var emptySlotBorderPen = new Pen(Color.FromArgb(58, 58, 70), 1f);
        using var startButtonBrush = new SolidBrush(buildSelection.CanStart
            ? Color.FromArgb(140, 128, 118)
            : Color.FromArgb(70, 70, 78));
        using var startButtonBorderPen = new Pen(buildSelection.CanStart
            ? Color.FromArgb(170, 148, 90, 82)
            : Color.FromArgb(96, 96, 108), 1.2f);

        graphics.FillRectangle(overlayBrush, _board.ViewportBounds);

        var panelBounds = BuildSelectionLayout.GetOverlayPanel(_board.ViewportBounds);
        using var panelPath = CreateRoundedRectanglePath(panelBounds, 28);
        graphics.FillPath(panelBrush, panelPath);
        graphics.DrawPath(panelBorderPen, panelPath);
        using var panelInnerPath = CreateRoundedRectanglePath(Inflate(panelBounds, -14, -14), 22);
        graphics.FillPath(panelInnerBrush, panelInnerPath);
        graphics.DrawString("Runeforge", _buildTitleFont, textBrush, new Rectangle(panelBounds.Left, panelBounds.Top - 70, panelBounds.Width, 40), _centerStringFormat);

        var activeAnimation = buildSelection.ActiveAnimation;

        var selectedSlots = BuildSelectionLayout.CreateSelectedBuildSlots(_board.ViewportBounds);
        for (var i = 0; i < selectedSlots.Count; i++)
        {
            var slotBounds = selectedSlots[i];
            using var slotPath = CreateRoundedRectanglePath(slotBounds, 18);
            graphics.FillPath(emptySlotBrush, slotPath);
            graphics.DrawPath(emptySlotBorderPen, slotPath);

            if (i >= buildSelection.SelectedRunes.Count)
            {
                continue;
            }

            if (activeAnimation?.Kind == BuildSelectionAnimationKind.Remove && activeAnimation.SlotIndex == i)
            {
                continue;
            }

            graphics.FillPath(selectedCardBrush, slotPath);
            graphics.DrawPath(selectedBorderPen, slotPath);
            _runeView.DrawIcon(graphics, buildSelection.SelectedRunes[i], Inflate(slotBounds, -6, -6));
        }

        var countBounds = new Rectangle(
            panelBounds.Left,
            selectedSlots[0].Bottom + 8,
            panelBounds.Width,
            28);
        graphics.DrawString($"{buildSelection.SelectedRunes.Count}/{BuildSelectionState.BuildSize}", _buildTextFont, accentBrush, countBounds, _centerStringFormat);

        foreach (var option in BuildSelectionLayout.CreateOptionLayouts(_board.ViewportBounds))
        {
            var hoverAmount = buildSelection.OptionHoverAmounts[option.RuneType];
            var hoverScale = 1f + (hoverAmount * 0.06f);
            var cardBounds = ScaleRectangle(option.CardBounds, hoverScale);
            var iconBounds = ScaleRectangle(option.IconBounds, hoverScale);
            var labelBounds = ScaleRectangle(option.LabelBounds, hoverScale);
            var isSelected = buildSelection.SelectedRunes.Contains(option.RuneType);
            using var cardPath = CreateRoundedRectanglePath(cardBounds, 18);
            graphics.FillPath(isSelected ? selectedCardBrush : cardBrush, cardPath);
            graphics.DrawPath(isSelected ? selectedBorderPen : cardBorderPen, cardPath);

            var shouldHideIcon = activeAnimation != null &&
                ((activeAnimation.Kind == BuildSelectionAnimationKind.Add && activeAnimation.RuneType == option.RuneType) ||
                (activeAnimation.Kind == BuildSelectionAnimationKind.Remove && activeAnimation.RuneType == option.RuneType));

            if (!shouldHideIcon)
            {
                _runeView.DrawIcon(graphics, option.RuneType, iconBounds, isSelected ? 1f : 0.92f);
            }

            graphics.DrawString(
                option.RuneType.ToString(),
                _buildLabelFont,
                isSelected ? textBrush : subtleTextBrush,
                labelBounds,
                _centerStringFormat);
        }

        var startButtonBounds = BuildSelectionLayout.GetStartButtonBounds(_board.ViewportBounds);
        using var startPath = CreateRoundedRectanglePath(startButtonBounds, 14);
        using var startTextBrush = new SolidBrush(Color.FromArgb(26, 28, 30));
        graphics.FillPath(startButtonBrush, startPath);
        graphics.DrawPath(startButtonBorderPen, startPath);
        graphics.DrawString("Start", _buildTextFont, startTextBrush, startButtonBounds, _centerStringFormat);

        if (activeAnimation != null)
        {
            var baseSize = activeAnimation.Kind == BuildSelectionAnimationKind.Add ? 62f : 70f;
            var pulseScale = 1f + (0.05f * MathF.Sin(activeAnimation.Progress * MathF.PI));
            var iconBounds = CreateCenteredSquareF(activeAnimation.CurrentPosition, baseSize * pulseScale);
            _runeView.DrawIcon(graphics, activeAnimation.RuneType, iconBounds);
        }
    }

    private void DrawInGameBuildPanel(Graphics graphics, BuildSelectionState buildSelection)
    {
        var panelBounds = BuildSelectionLayout.GetInGameBuildPanel(_board.ViewportBounds);

        using var panelPath = CreateRoundedRectanglePath(panelBounds, 18);
        using var panelBrush = new SolidBrush(Color.FromArgb(172, 28, 24, 34));
        using var panelBorderPen = new Pen(Color.FromArgb(124, 98, 88, 104), 1.5f);
        using var panelInnerPath = CreateRoundedRectanglePath(Inflate(panelBounds, -8, -8), 14);
        using var panelInnerBrush = new SolidBrush(Color.FromArgb(104, 24, 22, 30));
        using var emptySlotBrush = new SolidBrush(Color.FromArgb(34, 30, 30, 38));
        using var emptySlotBorderPen = new Pen(Color.FromArgb(58, 58, 70), 1f);
        using var selectedSlotBrush = new SolidBrush(Color.FromArgb(54, 96, 88, 86));
        using var selectedSlotBorderPen = new Pen(Color.FromArgb(140, 128, 118), 1.4f);

        graphics.FillPath(panelBrush, panelPath);
        graphics.DrawPath(panelBorderPen, panelPath);
        graphics.FillPath(panelInnerBrush, panelInnerPath);

        var slots = BuildSelectionLayout.CreateInGameBuildSlots(_board.ViewportBounds);
        for (var i = 0; i < slots.Count; i++)
        {
            var slotBounds = slots[i];
            using var slotPath = CreateRoundedRectanglePath(slotBounds, 12);
            var hasRune = i < buildSelection.SelectedRunes.Count;
            graphics.FillPath(hasRune ? selectedSlotBrush : emptySlotBrush, slotPath);
            graphics.DrawPath(hasRune ? selectedSlotBorderPen : emptySlotBorderPen, slotPath);

            if (!hasRune)
            {
                continue;
            }

            _runeView.DrawIcon(graphics, buildSelection.SelectedRunes[i], Inflate(slotBounds, -6, -6));
        }
    }

    private void DrawHeartsUi(Graphics graphics, GameState gameState)
    {
        var totalWidth = (GameState.MaxHearts * HeartIconSize) + ((GameState.MaxHearts - 1) * HeartIconSpacing);
        var panelWidth = totalWidth + (HeartPanelPadding * 2f);
        var panelHeight = HeartIconSize + (HeartPanelPadding * 2f);
        var panelRect = new RectangleF(22f, 18f, panelWidth, panelHeight);

        using var panelPath = CreateRoundedRectanglePath(Rectangle.Round(panelRect), 18);
        using var panelBrush = new SolidBrush(Color.FromArgb(168, 24, 20, 28));
        using var panelBorderPen = new Pen(Color.FromArgb(124, 98, 88, 104), 1.5f);

        graphics.FillPath(panelBrush, panelPath);
        graphics.DrawPath(panelBorderPen, panelPath);

        var drawY = panelRect.Y + HeartPanelPadding;
        var drawX = panelRect.X + HeartPanelPadding;

        for (var i = 0; i < GameState.MaxHearts; i++)
        {
            var texture = i < gameState.RemainingHearts ? _heartTexture : _brokenHeartTexture;
            graphics.DrawImage(texture, drawX, drawY, HeartIconSize, HeartIconSize);
            drawX += HeartIconSize + HeartIconSpacing;
        }
    }

    private void DrawWaveUi(Graphics graphics, GameState gameState)
    {
        var heartsPanelWidth = (GameState.MaxHearts * HeartIconSize) + ((GameState.MaxHearts - 1) * HeartIconSpacing) + (HeartPanelPadding * 2f);
        var panelRect = new RectangleF(
            22f,
            18f + HeartIconSize + (HeartPanelPadding * 2f) + 10f,
            heartsPanelWidth,
            HeartIconSize + (HeartPanelPadding * 2f));

        using var panelPath = CreateRoundedRectanglePath(Rectangle.Round(panelRect), 18);
        using var panelBrush = new SolidBrush(Color.FromArgb(168, 24, 20, 28));
        using var panelBorderPen = new Pen(Color.FromArgb(124, 98, 88, 104), 1.5f);
        using var titleBrush = new SolidBrush(Color.FromArgb(236, 230, 220));

        graphics.FillPath(panelBrush, panelPath);
        graphics.DrawPath(panelBorderPen, panelPath);

        graphics.DrawString($"Wave {gameState.Waves.CurrentWaveNumber}", _waveTitleFont, titleBrush, panelRect, _centerStringFormat);
    }

    private void DrawRunePointsUi(Graphics graphics, GameState gameState)
    {
        var panelWidth = (GameState.MaxHearts * HeartIconSize) + ((GameState.MaxHearts - 1) * HeartIconSpacing) + (HeartPanelPadding * 2f);
        var panelHeight = HeartIconSize + (HeartPanelPadding * 2f);
        var panelRect = new RectangleF(
            _board.ViewportBounds.Left + 160f,
            18f,
            panelWidth,
            panelHeight);

        using var panelPath = CreateRoundedRectanglePath(Rectangle.Round(panelRect), 18);
        using var panelBrush = new SolidBrush(Color.FromArgb(176, 28, 24, 34));
        using var panelBorderPen = new Pen(Color.FromArgb(150, 148, 132, 92), 1.5f);
        using var titleBrush = new SolidBrush(Color.FromArgb(198, 210, 202, 188));
        using var valueBrush = new SolidBrush(Color.FromArgb(236, 230, 220));

        graphics.FillPath(panelBrush, panelPath);
        graphics.DrawPath(panelBorderPen, panelPath);

        var titleRect = new RectangleF(panelRect.X, panelRect.Y + 6f, panelRect.Width, 14f);
        var valueRect = new RectangleF(panelRect.X, panelRect.Y + 20f, panelRect.Width, 24f);
        graphics.DrawString("Rune Points", _economyTitleFont, titleBrush, titleRect, _centerStringFormat);
        graphics.DrawString($"{gameState.Economy.RunePoints} RP", _economyValueFont, valueBrush, valueRect, _centerStringFormat);
    }

    private void DrawBagSpawnCostBadge(Graphics graphics, GameState gameState)
    {
        var canAfford = gameState.Economy.CanAffordCurrentRuneSpawn;
        const float badgeWidth = 78f;
        const float badgeHeight = 30f;
        var badgeRect = new RectangleF(
            _board.BagBounds.Left + ((_board.BagBounds.Width - badgeWidth) * 0.5f),
            _board.BagBounds.Top + 88f,
            badgeWidth,
            badgeHeight);

        var shadowRect = new RectangleF(badgeRect.X, badgeRect.Y + 2f, badgeRect.Width, badgeRect.Height);
        using var shadowPath = CreateRoundedRectanglePath(Rectangle.Round(shadowRect), 13);
        using var badgePath = CreateRoundedRectanglePath(Rectangle.Round(badgeRect), 13);
        using var shadowBrush = new SolidBrush(Color.FromArgb(58, 8, 6, 8));
        using var badgeBrush = new SolidBrush(Color.FromArgb(138, 38, 28, 22));
        using var badgeBorderPen = new Pen(Color.FromArgb(126, 170, 146, 106), 1.1f);
        using var textBrush = new SolidBrush(canAfford
            ? Color.FromArgb(255, 246, 236, 214)
            : Color.FromArgb(255, 232, 128, 128));
        using var textShadowBrush = new SolidBrush(Color.FromArgb(116, 12, 10, 10));

        graphics.FillPath(shadowBrush, shadowPath);
        graphics.FillPath(badgeBrush, badgePath);
        graphics.DrawPath(badgeBorderPen, badgePath);
        var textRect = new RectangleF(badgeRect.X, badgeRect.Y + 3, badgeRect.Width, badgeRect.Height);
        var textShadowRect = new RectangleF(textRect.X, textRect.Y + 1f, textRect.Width, textRect.Height);
        graphics.DrawString($"{gameState.Economy.CurrentRuneSpawnCost} RP", _bagCostFont, textShadowBrush, textShadowRect, _centerStringFormat);
        graphics.DrawString($"{gameState.Economy.CurrentRuneSpawnCost} RP", _bagCostFont, textBrush, textRect, _centerStringFormat);
    }
}
