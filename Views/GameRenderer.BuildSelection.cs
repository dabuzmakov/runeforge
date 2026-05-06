using System.Globalization;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Views;

public sealed partial class GameRenderer
{
    private void DrawBuildSelection(Graphics graphics, BuildSelectionState buildSelection)
    {
        var selectionSelectedColor = Color.FromArgb(145, 55, 209);
        using var selectedBorderPen = new Pen(Color.FromArgb(216, selectionSelectedColor), 1.9f);
        using var runeNameBrush = new SolidBrush(Color.FromArgb(232, 186, 148, 94));
        using var buildCounterBrush = new SolidBrush(Color.FromArgb(238, 182, 139, 78));
        using var buildCounterShadowBrush = new SolidBrush(Color.FromArgb(118, 4, 3, 2));
        var activeAnimation = buildSelection.ActiveAnimation;

        var buildFrameBounds = BuildSelectionLayout.GetSelectedBuildFrameBounds(_board.ViewportBounds);
        var buildFrameTexture = GetScaledTexture("build-frame", _buildFrameTexture, buildFrameBounds.Size);
        graphics.DrawImageUnscaled(buildFrameTexture, buildFrameBounds.Location);

        var selectedSlots = BuildSelectionLayout.CreateSelectedBuildSlots(_board.ViewportBounds);
        var cellImageBounds = Inflate(selectedSlots[0], 6, 6);
        var cellTexture = GetScaledTexture("selection-cell", _selectionCellTexture, cellImageBounds.Size);
        for (var i = 0; i < selectedSlots.Count; i++)
        {
            var slotBounds = selectedSlots[i];
            graphics.DrawImageUnscaled(cellTexture, Inflate(slotBounds, 6, 6).Location);

            if (i >= buildSelection.SelectedRunes.Count)
            {
                continue;
            }

            if (activeAnimation?.Kind == BuildSelectionAnimationKind.Remove && activeAnimation.SlotIndex == i)
            {
                continue;
            }

            _runeView.DrawIcon(graphics, buildSelection.SelectedRunes[i], Inflate(slotBounds, -10, -10));
        }

        var countBounds = new Rectangle(
            buildFrameBounds.Left,
            buildFrameBounds.Top + 118,
            buildFrameBounds.Width,
            28);
        var countShadowBounds = new Rectangle(countBounds.X, countBounds.Y + 1, countBounds.Width, countBounds.Height);
        AdaptiveTextRenderer.DrawCentered(
            graphics,
            $"{buildSelection.SelectedRunes.Count}/{BuildSelectionState.BuildSize}",
            _buildValueFont,
            buildCounterShadowBrush,
            countShadowBounds,
            _centerStringFormat);
        AdaptiveTextRenderer.DrawCentered(
            graphics,
            $"{buildSelection.SelectedRunes.Count}/{BuildSelectionState.BuildSize}",
            _buildValueFont,
            buildCounterBrush,
            countBounds,
            _centerStringFormat);

        foreach (var option in BuildSelectionLayout.CreateOptionLayouts(_board.ViewportBounds))
        {
            var hoverAmount = buildSelection.OptionHoverAmounts[option.RuneType];
            var hoverScale = 1f + (hoverAmount * 0.06f);
            var cardBounds = ScaleRectangle(option.CardBounds, hoverScale);
            var iconBounds = ScaleRectangle(option.IconBounds, hoverScale);
            var labelBounds = ScaleRectangle(option.LabelBounds, hoverScale);
            var isSelected = buildSelection.SelectedRunes.Contains(option.RuneType);
            var cardImageBounds = Inflate(cardBounds, 5, 5);
            var cardTexture = GetScaledTexture("selection-vertical-cell", _verticalSelectionCellTexture, cardImageBounds.Size);
            graphics.DrawImageUnscaled(cardTexture, cardImageBounds.Location);
            if (isSelected)
            {
                using var cardPath = CreateRoundedRectanglePath(cardImageBounds, 16);
                graphics.DrawPath(selectedBorderPen, cardPath);
            }

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
                runeNameBrush,
                labelBounds,
                _centerStringFormat);
        }

        var startButtonBounds = BuildSelectionLayout.GetStartButtonBounds(_board.ViewportBounds);
        var startButtonScale = 1f + (buildSelection.StartButtonHoverAmount * 0.05f);
        var scaledStartButtonBounds = ScaleRectangle(startButtonBounds, startButtonScale);
        var startButtonImageBounds = CreateCenteredAspectRectangle(scaledStartButtonBounds, _buttonTexture, 54, 8);
        var buttonTexture = GetScaledTexture("start-button", _buttonTexture, startButtonImageBounds.Size);
        graphics.DrawImageUnscaled(buttonTexture, startButtonImageBounds.Location);

        using var startTextBrush = new SolidBrush(Color.FromArgb(238, 186, 148, 94));
        using var startTextShadowBrush = new SolidBrush(Color.FromArgb(116, 8, 5, 4));
        var startTextShadowBounds = new Rectangle(scaledStartButtonBounds.X, scaledStartButtonBounds.Y + 1, scaledStartButtonBounds.Width, scaledStartButtonBounds.Height);
        graphics.DrawString("Старт", _buildTextFont, startTextShadowBrush, startTextShadowBounds, _centerStringFormat);
        graphics.DrawString("Старт", _buildTextFont, startTextBrush, scaledStartButtonBounds, _centerStringFormat);

        if (activeAnimation != null)
        {
            var baseSize = activeAnimation.Kind == BuildSelectionAnimationKind.Add ? 62f : 70f;
            var pulseScale = 1f + (0.05f * MathF.Sin(activeAnimation.Progress * MathF.PI));
            var iconBounds = CreateCenteredSquareF(activeAnimation.CurrentPosition, baseSize * pulseScale);
            _runeView.DrawIcon(graphics, activeAnimation.RuneType, iconBounds);
        }

        DrawBuildSelectionTooltip(graphics, buildSelection);
    }

    private void DrawBuildSelectionTooltip(Graphics graphics, BuildSelectionState buildSelection)
    {
        if (!buildSelection.HoveredRuneType.HasValue || buildSelection.TooltipOpacity <= 0.01f)
        {
            return;
        }

        var tooltip = RuneTooltipCatalog.Get(buildSelection.HoveredRuneType.Value);
        var tooltipBounds = GetBuildSelectionTooltipBounds(buildSelection.TooltipAnchor);
        var opacity = Math.Clamp(buildSelection.TooltipOpacity, 0f, 1f);
        var effectContentBounds = new RectangleF(tooltipBounds.X + 16f, tooltipBounds.Y + 110f, tooltipBounds.Width - 32f, 1000f);
        var effectLines = LayoutTooltipSegments(graphics, tooltip.EffectSegments, effectContentBounds.Width);
        const float effectLineHeight = 18f;
        var effectHeight = Math.Max(effectLineHeight, effectLines.Count * effectLineHeight);
        tooltipBounds = new RectangleF(tooltipBounds.X, tooltipBounds.Y, tooltipBounds.Width, 128f + effectHeight);

        using var shadowBrush = new SolidBrush(Color.FromArgb((int)(opacity * 76f), 4, 4, 8));
        using var panelBrush = new SolidBrush(Color.FromArgb((int)(opacity * 216f), 18, 18, 24));
        using var innerBrush = new SolidBrush(Color.FromArgb((int)(opacity * 174f), 28, 28, 36));
        using var borderPen = new Pen(Color.FromArgb((int)(opacity * 212f), 96, 92, 116), 1.4f);
        using var titleBrush = new SolidBrush(Color.FromArgb((int)(opacity * 255f), 246, 242, 255));
        using var statLabelBrush = new SolidBrush(Color.FromArgb((int)(opacity * 218f), 194, 188, 204));
        using var statValueBrush = new SolidBrush(Color.FromArgb((int)(opacity * 255f), 224, 214, 236));
        using var bodyBrush = new SolidBrush(Color.FromArgb((int)(opacity * 240f), 232, 228, 238));
        using var damageBrush = new SolidBrush(Color.FromArgb((int)(opacity * 255f), 246, 198, 88));
        using var percentBrush = new SolidBrush(Color.FromArgb((int)(opacity * 255f), 244, 108, 126));
        using var cooldownBrush = new SolidBrush(Color.FromArgb((int)(opacity * 255f), 122, 222, 170));
        using var buffBrush = new SolidBrush(Color.FromArgb((int)(opacity * 255f), 174, 122, 224));
        using var debuffBrush = new SolidBrush(Color.FromArgb((int)(opacity * 255f), 255, 150, 96));

        var shadowBounds = Rectangle.Round(new RectangleF(
            tooltipBounds.X + 5f,
            tooltipBounds.Y + 6f,
            tooltipBounds.Width,
            tooltipBounds.Height));
        using var shadowPath = CreateRoundedRectanglePath(shadowBounds, 18);
        using var panelPath = CreateRoundedRectanglePath(Rectangle.Round(tooltipBounds), 18);
        using var innerPath = CreateRoundedRectanglePath(Rectangle.Round(new RectangleF(
            tooltipBounds.X + 8f,
            tooltipBounds.Y + 8f,
            tooltipBounds.Width - 16f,
            tooltipBounds.Height - 16f)), 14);

        graphics.FillPath(shadowBrush, shadowPath);
        graphics.FillPath(panelBrush, panelPath);
        graphics.DrawPath(borderPen, panelPath);
        graphics.FillPath(innerBrush, innerPath);

        var titleRect = new RectangleF(tooltipBounds.X + 16f, tooltipBounds.Y + 14f, tooltipBounds.Width - 32f, 22f);
        graphics.DrawString(buildSelection.HoveredRuneType.Value.ToString(), _buildTooltipTitleFont, titleBrush, titleRect);

        var statsTop = tooltipBounds.Y + 42f;
        DrawTooltipStat(graphics, "Баз. атака", tooltip.BaseAttackText, statsTop, tooltipBounds, statLabelBrush, statValueBrush);
        DrawTooltipStat(graphics, "Баз. скорость", tooltip.BaseAttackSpeedText, statsTop + 22f, tooltipBounds, statLabelBrush, statValueBrush);

        var effectLabelRect = new RectangleF(tooltipBounds.X + 16f, tooltipBounds.Y + 92f, tooltipBounds.Width - 32f, 18f);
        graphics.DrawString("Эффект", _buildTooltipStatFont, statLabelBrush, effectLabelRect);

        var effectRect = new RectangleF(tooltipBounds.X + 16f, tooltipBounds.Y + 112f, tooltipBounds.Width - 32f, tooltipBounds.Height - 126f);
        DrawTooltipSegmentLines(
            graphics,
            effectLines,
            effectRect.Location,
            effectLineHeight,
            bodyBrush,
            damageBrush,
            percentBrush,
            cooldownBrush,
            buffBrush,
            debuffBrush);
    }

    private void DrawTooltipStat(
        Graphics graphics,
        string label,
        string value,
        float top,
        RectangleF tooltipBounds,
        Brush labelBrush,
        Brush valueBrush)
    {
        var labelRect = new RectangleF(tooltipBounds.X + 16f, top, 122f, 18f);
        var valueRect = new RectangleF(tooltipBounds.X + 140f, top, tooltipBounds.Width - 156f, 18f);
        graphics.DrawString(label, _buildTooltipStatFont, labelBrush, labelRect);

        AdaptiveTextRenderer.DrawCentered(
            graphics,
            value,
            _buildTooltipStatFont,
            valueBrush,
            valueRect,
            _farStringFormat,
            minimumScale: 0.5f);
    }

    private RectangleF GetBuildSelectionTooltipBounds(Point anchor)
    {
        const float tooltipWidth = 344f;
        const float tooltipHeight = 230f;
        const float viewportMargin = 18f;

        var x = (float)anchor.X;
        var y = (float)anchor.Y;
        if ((x + tooltipWidth) > (_board.ViewportBounds.Right - viewportMargin))
        {
            x = anchor.X - tooltipWidth - 118f;
        }

        if ((y + tooltipHeight) > (_board.ViewportBounds.Bottom - viewportMargin))
        {
            y = _board.ViewportBounds.Bottom - tooltipHeight - viewportMargin;
        }

        var startBounds = BuildSelectionLayout.GetStartButtonBounds(_board.ViewportBounds);
        var proposedRect = new RectangleF(x, y, tooltipWidth, tooltipHeight);
        if (proposedRect.IntersectsWith(startBounds))
        {
            y = startBounds.Top - tooltipHeight - 12f;
        }

        y = Math.Max(viewportMargin, y);
        x = Math.Max(viewportMargin, x);

        return new RectangleF(x, y, tooltipWidth, tooltipHeight);
    }

    private List<List<RuneTooltipSegment>> LayoutTooltipSegments(
        Graphics graphics,
        IReadOnlyList<RuneTooltipSegment> segments,
        float maxWidth)
    {
        var lines = new List<List<RuneTooltipSegment>>();
        var currentLine = new List<RuneTooltipSegment>();
        var currentWidth = 0f;
        var spaceWidth = GetTooltipSpaceWidth(graphics);

        foreach (var token in CreateTooltipTokens(segments))
        {
            var tokenWidth = MeasureTooltipText(graphics, token.Text);
            var candidateWidth = currentLine.Count == 0
                ? tokenWidth
                : currentWidth + spaceWidth + tokenWidth;

            if (currentLine.Count > 0 && candidateWidth > maxWidth)
            {
                lines.Add(currentLine);
                currentLine = [];
                currentWidth = 0f;
            }

            currentLine.Add(token);
            currentWidth = currentLine.Count == 1
                ? tokenWidth
                : currentWidth + spaceWidth + tokenWidth;
        }

        if (currentLine.Count > 0)
        {
            lines.Add(currentLine);
        }

        return lines;
    }

    private static List<RuneTooltipSegment> CreateTooltipTokens(IReadOnlyList<RuneTooltipSegment> segments)
    {
        var tokens = new List<RuneTooltipSegment>();

        foreach (var segment in segments)
        {
            var words = segment.Text
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var word in words)
            {
                if (IsStandalonePunctuation(word) && tokens.Count > 0)
                {
                    var previous = tokens[^1];
                    tokens[^1] = previous with { Text = previous.Text + word };
                    continue;
                }

                tokens.Add(new RuneTooltipSegment(word, segment.Tone));
            }
        }

        return tokens;
    }

    private static bool IsStandalonePunctuation(string value)
    {
        return value.Length <= 2 && value.All(static character =>
            character is '.' or ',' or ':' or ';' or '!' or '?' or ')' or '(');
    }

    private void DrawTooltipSegmentLines(
        Graphics graphics,
        IReadOnlyList<List<RuneTooltipSegment>> lines,
        PointF origin,
        float lineHeight,
        Brush normalBrush,
        Brush damageBrush,
        Brush percentBrush,
        Brush cooldownBrush,
        Brush buffBrush,
        Brush debuffBrush)
    {
        var spaceWidth = GetTooltipSpaceWidth(graphics);

        for (var lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            var x = origin.X;
            var y = origin.Y + (lineIndex * lineHeight);
            var line = lines[lineIndex];

            for (var segmentIndex = 0; segmentIndex < line.Count; segmentIndex++)
            {
                var segment = line[segmentIndex];
                var brush = segment.Tone switch
                {
                    RuneTooltipTone.Damage => damageBrush,
                    RuneTooltipTone.Percent => percentBrush,
                    RuneTooltipTone.Cooldown => cooldownBrush,
                    RuneTooltipTone.Buff => buffBrush,
                    RuneTooltipTone.Debuff => debuffBrush,
                    _ => normalBrush
                };

                graphics.DrawString(segment.Text, _buildTooltipBodyFont, brush, new PointF(x, y), _tooltipTextFormat);
                x += MeasureTooltipText(graphics, segment.Text);
                if (segmentIndex < line.Count - 1)
                {
                    x += spaceWidth;
                }
            }
        }
    }

    private float MeasureTooltipText(Graphics graphics, string text)
    {
        if (_tooltipTextWidthCache.TryGetValue(text, out var cachedWidth))
        {
            return cachedWidth;
        }

        var width = graphics.MeasureString(text, _buildTooltipBodyFont, PointF.Empty, _tooltipTextFormat).Width;
        _tooltipTextWidthCache.Add(text, width);
        return width;
    }

    private float GetTooltipSpaceWidth(Graphics graphics)
    {
        if (_tooltipSpaceWidth.HasValue)
        {
            return _tooltipSpaceWidth.Value;
        }

        _tooltipSpaceWidth = MathF.Max(_buildTooltipBodyFont.Size * 0.36f, MeasureTooltipText(graphics, "n n") - MeasureTooltipText(graphics, "nn"));
        return _tooltipSpaceWidth.Value;
    }
}
