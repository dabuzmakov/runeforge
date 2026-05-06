using System.Globalization;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Views;

public sealed partial class GameRenderer
{
    private void DrawStartScreen(Graphics graphics, GameState gameState)
    {
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

        DrawStartScreenStats(graphics, gameState);
        DrawStartScreenPlayButton(graphics, gameState);
    }

    private void DrawStartScreenPlayButton(Graphics graphics, GameState gameState)
    {
        var hoverAmount = Math.Clamp(gameState.Ui.StartScreenPlayButtonHoverAmount, 0f, 1f);
        var buttonBounds = StartScreenLayout.GetPlayButtonBounds(_board.ViewportBounds);
        var scaledButtonBounds = ScaleRectangle(buttonBounds, 1f + (hoverAmount * 0.045f));
        var buttonTexture = GetScaledTexture("start-play-button", _playButtonTexture, scaledButtonBounds.Size);
        graphics.DrawImageUnscaled(buttonTexture, scaledButtonBounds.Location);

        using var textBrush = new SolidBrush(Color.FromArgb(252, 238, 204, 118));
        using var textShadowBrush = new SolidBrush(Color.FromArgb(170, 48, 20, 6));
        var textBounds = new Rectangle(
            scaledButtonBounds.X,
            scaledButtonBounds.Y - (int)MathF.Round(scaledButtonBounds.Height * 0.05f) + 5,
            scaledButtonBounds.Width,
            scaledButtonBounds.Height);
        var textShadowBounds = new Rectangle(textBounds.X, textBounds.Y + 2, textBounds.Width, textBounds.Height);
        graphics.DrawString("Играть", _gameOverSectionFont, textShadowBrush, textShadowBounds, _centerStringFormat);
        graphics.DrawString("Играть", _gameOverSectionFont, textBrush, textBounds, _centerStringFormat);
    }

    private void DrawStartScreenStats(Graphics graphics, GameState gameState)
    {
        var statLayouts = StartScreenLayout.CreateStatLayouts(_board.ViewportBounds);
        var statValues = new[]
        {
            ("Убито врагов", FormatGameOverNumber(gameState.TotalKilledEnemyCount)),
            ("Рекордная волна", gameState.BestCompletedWaveRecord.ToString(CultureInfo.InvariantCulture)),
            ("Время в игре", FormatStartScreenHours(gameState.TotalPlayTimeSeconds))
        };

        var badgeTextures = new[]
        {
            _startFragsBadgeTexture,
            _startWaveBadgeTexture,
            _startGameTimeBadgeTexture
        };

        using var valueBrush = new SolidBrush(Color.FromArgb(248, 238, 198, 112));
        using var valueShadowBrush = new SolidBrush(Color.FromArgb(150, 42, 20, 8));

        for (var i = 0; i < statLayouts.Count; i++)
        {
            var layout = statLayouts[i];
            var badgeTexture = GetScaledTexture($"start-stat-badge:{i}", badgeTextures[i], layout.Bounds.Size);
            graphics.DrawImageUnscaled(badgeTexture, layout.Bounds.Location);

            graphics.DrawString(statValues[i].Item1, _gameOverLabelFont, valueBrush, layout.LabelBounds, _centerStringFormat);
            var valueShadowBounds = new Rectangle(
                layout.ValueBounds.X,
                layout.ValueBounds.Y + 2,
                layout.ValueBounds.Width,
                layout.ValueBounds.Height);
            AdaptiveTextRenderer.DrawCentered(
                graphics,
                statValues[i].Item2,
                _gameOverValueFont,
                valueShadowBrush,
                valueShadowBounds,
                _centerStringFormat,
                minimumScale: 0.28f);
            AdaptiveTextRenderer.DrawCentered(
                graphics,
                statValues[i].Item2,
                _gameOverValueFont,
                valueBrush,
                layout.ValueBounds,
                _centerStringFormat,
                minimumScale: 0.28f);
        }
    }

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
        graphics.DrawString("Поражение", _defeatTitleFont, _defeatTextBrush, titleRect, _centerStringFormat);
    }

    private void DrawTopButtons(Graphics graphics, GameState gameState)
    {
        if (gameState.Ui.IsStartScreenOpen)
        {
            DrawTopButton(
                graphics,
                "top-button:exit",
                _exitButtonTexture,
                TopButtonLayout.GetExitButtonBounds(_board.ViewportBounds),
                gameState.Ui.ExitButtonHoverAmount);
            return;
        }

        if (gameState.Ui.BuildSelection.IsOpen)
        {
            DrawTopButton(
                graphics,
                "top-button:home",
                _homeButtonTexture,
                TopButtonLayout.GetHomeButtonBounds(_board.ViewportBounds),
                gameState.Ui.HomeButtonHoverAmount);
        }
        else if (!gameState.IsDefeated)
        {
            DrawTopButton(
                graphics,
                "top-button:pause",
                _pauseButtonTexture,
                TopButtonLayout.GetPauseButtonBounds(_board.ViewportBounds),
                gameState.Ui.PauseButtonHoverAmount);
        }

        DrawTopButton(
            graphics,
            "top-button:exit",
            _exitButtonTexture,
            TopButtonLayout.GetExitButtonBounds(_board.ViewportBounds),
            gameState.Ui.ExitButtonHoverAmount);
    }

    private void DrawTopButton(Graphics graphics, string cacheKey, Bitmap texture, Rectangle bounds, float hoverAmount)
    {
        var scale = 1f + (Math.Clamp(hoverAmount, 0f, 1f) * 0.06f);
        var scaledBounds = ScaleRectangle(bounds, scale);
        var buttonTexture = GetScaledTexture(cacheKey, texture, scaledBounds.Size);
        graphics.DrawImageUnscaled(buttonTexture, scaledBounds.Location);
    }

    private void DrawPausePopup(Graphics graphics, GameState gameState)
    {
        var visibility = SmoothStep(gameState.Ui.PausePopupVisibility);
        if (visibility <= 0.001f)
        {
            return;
        }

        using var overlayBrush = new SolidBrush(Color.FromArgb((int)MathF.Round(188f * visibility), 4, 3, 8));
        graphics.FillRectangle(overlayBrush, _board.ViewportBounds);

        var popupAppearScale = 0.965f + (0.035f * visibility);
        var popupBounds = ScaleRectangle(
            PausePopupLayout.GetPopupBounds(_board.ViewportBounds),
            popupAppearScale);
        var popupTexture = GetScaledTexture("pause-popup", _pausePopupTexture, popupBounds.Size);
        var popupImageAttributes = GetOpacityImageAttributes(visibility);
        graphics.DrawImage(
            popupTexture,
            popupBounds,
            0,
            0,
            popupTexture.Width,
            popupTexture.Height,
            GraphicsUnit.Pixel,
            popupImageAttributes);

        using var titleBrush = new SolidBrush(Color.FromArgb((int)MathF.Round(248f * visibility), 238, 188, 82));
        using var titleShadowBrush = new SolidBrush(Color.FromArgb((int)MathF.Round(138f * visibility), 18, 10, 4));
        var titleBounds = PausePopupLayout.GetTitleBounds(popupBounds);
        DrawPausePopupTitle(graphics, titleBounds, titleBrush, titleShadowBrush, _noWrapCenterStringFormat);

        DrawPausePopupButton(
            graphics,
            "pause-popup-button:restart",
            _restartButtonTexture,
            PausePopupLayout.GetButtonBounds(popupBounds, PausePopupButtonKind.Restart),
            gameState.Ui.PausePopupRestartHoverAmount,
            visibility);
        DrawPausePopupButton(
            graphics,
            "pause-popup-button:home",
            _popupHomeButtonTexture,
            PausePopupLayout.GetButtonBounds(popupBounds, PausePopupButtonKind.Home),
            gameState.Ui.PausePopupHomeHoverAmount,
            visibility);
        DrawPausePopupButton(
            graphics,
            "pause-popup-button:resume",
            _resumeButtonTexture,
            PausePopupLayout.GetButtonBounds(popupBounds, PausePopupButtonKind.Resume),
            gameState.Ui.PausePopupResumeHoverAmount,
            visibility);
    }

    private void DrawPausePopupButton(Graphics graphics, string cacheKey, Bitmap texture, Rectangle bounds, float hoverAmount, float visibility)
    {
        var scale = (0.985f + (0.015f * visibility)) + (Math.Clamp(hoverAmount, 0f, 1f) * 0.04f);
        var scaledBounds = ScaleRectangle(bounds, scale);
        var buttonTexture = GetScaledTexture(cacheKey, texture, scaledBounds.Size);
        var imageAttributes = GetOpacityImageAttributes(visibility);
        graphics.DrawImage(
            buttonTexture,
            scaledBounds,
            0,
            0,
            buttonTexture.Width,
            buttonTexture.Height,
            GraphicsUnit.Pixel,
            imageAttributes);
    }

    private void DrawPausePopupTitle(
        Graphics graphics,
        Rectangle titleBounds,
        Brush titleBrush,
        Brush titleShadowBrush,
        StringFormat titleFormat)
    {
        var lineGap = Math.Max(4, (int)MathF.Round(titleBounds.Height * 0.12f));
        var lineHeight = Math.Max(1, (titleBounds.Height - lineGap) / 2);
        var topLineBounds = new Rectangle(titleBounds.X, titleBounds.Y, titleBounds.Width, lineHeight);
        var bottomLineBounds = new Rectangle(titleBounds.X, titleBounds.Bottom - lineHeight, titleBounds.Width, lineHeight);
        var topLineShadowBounds = new Rectangle(topLineBounds.X, topLineBounds.Y + 4, topLineBounds.Width, topLineBounds.Height);
        var bottomLineShadowBounds = new Rectangle(bottomLineBounds.X, bottomLineBounds.Y + 4, bottomLineBounds.Width, bottomLineBounds.Height);

        graphics.DrawString("Игра", _pauseTitleFont, titleShadowBrush, topLineShadowBounds, titleFormat);
        graphics.DrawString("Игра", _pauseTitleFont, titleBrush, topLineBounds, titleFormat);
        graphics.DrawString("приостановлена", _pauseTitleFont, titleShadowBrush, bottomLineShadowBounds, titleFormat);
        graphics.DrawString("приостановлена", _pauseTitleFont, titleBrush, bottomLineBounds, titleFormat);
    }

    private void DrawGameOverPopup(Graphics graphics, GameState gameState)
    {
        var progress = Math.Clamp(gameState.Ui.GameOverPopupVisibility, 0f, 1f);
        if (progress <= 0.001f)
        {
            return;
        }

        var opacity = SmoothStep(progress);
        using var overlayBrush = new SolidBrush(Color.FromArgb((int)MathF.Round(198f * opacity), 4, 3, 8));
        graphics.FillRectangle(overlayBrush, _board.ViewportBounds);

        var popupBounds = GameOverPopupLayout.GetAnimatedPopupBounds(_board.ViewportBounds, progress);
        var popupTexture = GetScaledTexture("game-over-popup", _gameOverPopupTexture, popupBounds.Size);
        var imageAttributes = GetOpacityImageAttributes(opacity);
        graphics.DrawImage(
            popupTexture,
            popupBounds,
            0,
            0,
            popupTexture.Width,
            popupTexture.Height,
            GraphicsUnit.Pixel,
            imageAttributes);

        using var titleBrush = new SolidBrush(Color.FromArgb((int)MathF.Round(248f * opacity), 239, 226, 192));
        using var titleShadowBrush = new SolidBrush(Color.FromArgb((int)MathF.Round(146f * opacity), 16, 10, 6));
        using var labelBrush = new SolidBrush(Color.FromArgb((int)MathF.Round(236f * opacity), 216, 171, 96));
        using var valueBrush = new SolidBrush(Color.FromArgb((int)MathF.Round(248f * opacity), 244, 234, 208));
        using var sectionBrush = new SolidBrush(Color.FromArgb((int)MathF.Round(238f * opacity), 220, 179, 104));
        using var valueShadowBrush = new SolidBrush(Color.FromArgb((int)MathF.Round(132f * opacity), 18, 12, 8));
        var titleBounds = GameOverPopupLayout.GetTitleBounds(popupBounds);
        var titleShadowBounds = new Rectangle(titleBounds.X, titleBounds.Y + 2, titleBounds.Width, titleBounds.Height);
        graphics.DrawString("Игра окончена", _gameOverTitleFont, titleShadowBrush, titleShadowBounds, _noWrapCenterStringFormat);
        graphics.DrawString("Игра окончена", _gameOverTitleFont, titleBrush, titleBounds, _noWrapCenterStringFormat);

        DrawGameOverStatColumn(
            graphics,
            GameOverPopupLayout.GetStatsColumnBounds(popupBounds, 0),
            "Волна",
            gameState.Waves.CurrentWaveNumber.ToString(CultureInfo.InvariantCulture),
            _waveIconTexture,
            "game-over-wave-icon",
            labelBrush,
            valueBrush,
            valueShadowBrush,
            _noWrapCenterStringFormat,
            imageAttributes);
        DrawGameOverStatColumn(
            graphics,
            GameOverPopupLayout.GetStatsColumnBounds(popupBounds, 1),
            "Убито врагов",
            FormatGameOverNumber(gameState.KilledEnemyCount),
            _fragsIconTexture,
            "game-over-frags-icon",
            labelBrush,
            valueBrush,
            valueShadowBrush,
            _noWrapCenterStringFormat,
            imageAttributes);
        DrawGameOverStatColumn(
            graphics,
            GameOverPopupLayout.GetStatsColumnBounds(popupBounds, 2),
            "Время игры",
            FormatGameOverDuration(gameState.MatchDurationSeconds),
            _gameDurationIconTexture,
            "game-over-duration-icon",
            labelBrush,
            valueBrush,
            valueShadowBrush,
            _noWrapCenterStringFormat,
            imageAttributes);

        var runeSectionBounds = GameOverPopupLayout.GetRuneSectionLabelBounds(popupBounds);
        var runeSectionShadowBounds = new Rectangle(runeSectionBounds.X, runeSectionBounds.Y + 2, runeSectionBounds.Width, runeSectionBounds.Height);
        graphics.DrawString("Комплект рун", _gameOverSectionFont, valueShadowBrush, runeSectionShadowBounds, _noWrapCenterStringFormat);
        graphics.DrawString("Комплект рун", _gameOverSectionFont, sectionBrush, runeSectionBounds, _noWrapCenterStringFormat);

        var runeCells = GameOverPopupLayout.CreateRuneCellBounds(popupBounds);
        var cellImageBounds = Inflate(runeCells[0], 6, 6);
        var cellTexture = GetScaledTexture("game-over-cell", _selectionCellTexture, cellImageBounds.Size);
        for (var i = 0; i < runeCells.Count; i++)
        {
            var cellBounds = runeCells[i];
            graphics.DrawImageUnscaled(cellTexture, Inflate(cellBounds, 6, 6).Location);

            if (i >= gameState.Ui.BuildSelection.SelectedRunes.Count)
            {
                continue;
            }

            _runeView.DrawIcon(
                graphics,
                gameState.Ui.BuildSelection.SelectedRunes[i],
                Inflate(cellBounds, -15, -15),
                alphaMultiplier: opacity);
        }

        DrawGameOverButton(
            graphics,
            "game-over-button:restart",
            _gameOverRestartButtonTexture,
            GameOverPopupLayout.GetButtonBounds(popupBounds, GameOverPopupButtonKind.Restart),
            gameState.Ui.GameOverPopupRestartHoverAmount,
            imageAttributes);
        DrawGameOverButton(
            graphics,
            "game-over-button:home",
            _gameOverHomeButtonTexture,
            GameOverPopupLayout.GetButtonBounds(popupBounds, GameOverPopupButtonKind.Home),
            gameState.Ui.GameOverPopupHomeHoverAmount,
            imageAttributes);
    }

    private void DrawGameOverStatColumn(
        Graphics graphics,
        Rectangle bounds,
        string label,
        string value,
        Bitmap iconTexture,
        string iconCacheKey,
        Brush labelBrush,
        Brush valueBrush,
        Brush valueShadowBrush,
        StringFormat centerFormat,
        ImageAttributes imageAttributes)
    {
        var labelBounds = new RectangleF(bounds.X, bounds.Y + (bounds.Height * 0.03f), bounds.Width, bounds.Height * 0.27f);
        graphics.DrawString(label, _gameOverLabelFont, labelBrush, labelBounds, centerFormat);

        var iconScale = iconCacheKey.Contains("duration", StringComparison.Ordinal)
            ? 0.86f
            : 1.02f;
        var contentTop = bounds.Top + (int)MathF.Round(bounds.Height * 0.41f);
        var contentHeight = (int)MathF.Round(bounds.Height * 0.30f);
        var columnCenterX = bounds.Left + (bounds.Width * 0.5f);
        var valueHeight = contentHeight;
        var valueMaxWidth = bounds.Width * 0.78f;
        var measuredValueSize = graphics.MeasureString(value, _gameOverValueFont, PointF.Empty, _typographicNoWrapStringFormat);
        var defaultValueScale = CalculateGameOverValueScale(
            graphics,
            "00.00.00",
            valueMaxWidth,
            valueHeight,
            _typographicNoWrapStringFormat);
        var valueScale = Math.Min(
            defaultValueScale,
            Math.Clamp(
                MathF.Min(
                    1f,
                    MathF.Min(
                        valueMaxWidth / Math.Max(1f, measuredValueSize.Width),
                        valueHeight / Math.Max(1f, measuredValueSize.Height))),
                0.40f,
                1f));
        var valueWidth = MathF.Min(valueMaxWidth, MathF.Ceiling(measuredValueSize.Width * valueScale) + 3f);
        var valueBounds = new RectangleF(
            columnCenterX - (valueWidth * 0.5f),
            contentTop,
            valueWidth,
            valueHeight);

        var valueShadowBounds = new RectangleF(valueBounds.X, valueBounds.Y + 2f, valueBounds.Width, valueBounds.Height);
        AdaptiveTextRenderer.DrawCentered(
            graphics,
            value,
            _gameOverValueFont,
            valueShadowBrush,
            valueShadowBounds,
            centerFormat,
            minimumScale: 0.40f,
            maximumScale: defaultValueScale);
        AdaptiveTextRenderer.DrawCentered(
            graphics,
            value,
            _gameOverValueFont,
            valueBrush,
            valueBounds,
            centerFormat,
            minimumScale: 0.40f,
            maximumScale: defaultValueScale);

        const int iconGap = 20;
        const int iconVerticalOffset = -5;
        var iconSlotSize = Math.Max(1, contentHeight);
        var iconSlot = new Rectangle(
            (int)MathF.Round(valueBounds.Left) - iconGap - iconSlotSize,
            contentTop + iconVerticalOffset,
            iconSlotSize,
            iconSlotSize);
        var iconBounds = ScaleRectangle(CreateCenteredAspectRectangle(iconSlot, iconTexture), iconScale);
        iconBounds.X = (int)MathF.Round(valueBounds.Left) - iconGap - iconBounds.Width;
        var iconImage = GetScaledTexture(iconCacheKey, iconTexture, iconBounds.Size);
        graphics.DrawImage(
            iconImage,
            iconBounds,
            0,
            0,
            iconImage.Width,
            iconImage.Height,
            GraphicsUnit.Pixel,
            imageAttributes);
    }

    private float CalculateGameOverValueScale(
        Graphics graphics,
        string sampleValue,
        float valueMaxWidth,
        float valueHeight,
        StringFormat measureFormat)
    {
        var sampleSize = graphics.MeasureString(sampleValue, _gameOverValueFont, PointF.Empty, measureFormat);
        return Math.Clamp(
            MathF.Min(
                1f,
                MathF.Min(
                    valueMaxWidth / Math.Max(1f, sampleSize.Width),
                    valueHeight / Math.Max(1f, sampleSize.Height))),
            0.40f,
            1f);
    }

    private void DrawGameOverButton(
        Graphics graphics,
        string cacheKey,
        Bitmap texture,
        Rectangle bounds,
        float hoverAmount,
        ImageAttributes imageAttributes)
    {
        var scale = 1f + (Math.Clamp(hoverAmount, 0f, 1f) * 0.05f);
        var scaledBounds = ScaleRectangle(bounds, scale);
        var buttonTexture = GetScaledTexture(cacheKey, texture, scaledBounds.Size);
        graphics.DrawImage(
            buttonTexture,
            scaledBounds,
            0,
            0,
            buttonTexture.Width,
            buttonTexture.Height,
            GraphicsUnit.Pixel,
            imageAttributes);
    }

    private static string FormatGameOverNumber(long value)
    {
        return value.ToString("#,0", CultureInfo.InvariantCulture).Replace(",", " ");
    }

    private static string FormatGameOverDuration(float durationSeconds)
    {
        return FormatDuration(durationSeconds);
    }

    private static string FormatDuration(double durationSeconds)
    {
        var totalSeconds = Math.Max(0, (int)Math.Floor(durationSeconds));
        var duration = TimeSpan.FromSeconds(totalSeconds);
        return duration.TotalHours >= 1d
            ? $"{(int)duration.TotalHours:00}:{duration.Minutes:00}:{duration.Seconds:00}"
            : $"{duration.Minutes:00}:{duration.Seconds:00}";
    }

    private static string FormatStartScreenHours(double durationSeconds)
    {
        var hours = (int)Math.Floor(Math.Max(0d, durationSeconds) / 3600d);
        return $"{hours} ч.";
    }
}
