using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;
using runeforge.Configs;
using runeforge.Effects;
using runeforge.Models;
using runeforge.Runes;

namespace runeforge.Views;

public sealed partial class GameRenderer
{
    private void DrawPath(Graphics graphics)
    {
        if (_pathPoints.Length < 2)
        {
            return;
        }

        DrawHagalazPathPreview(graphics, _model.State.Ui);
    }

    private void DrawTable(Graphics graphics)
    {
        graphics.FillPath(_tableFillBrush, _tableOuterPath);
        graphics.FillPath(_tableInnerBrush, _tableInnerPath);

        var cellImageBounds = _board.Grid.Cells.Count > 0
            ? _board.Grid.Cells[0].Bounds
            : Rectangle.Empty;
        var cellTexture = GetScaledTexture("table-cell", _selectionCellTexture, cellImageBounds.Size);

        foreach (var cell in _board.Grid.Cells)
        {
            graphics.DrawImageUnscaled(cellTexture, cell.Bounds.Location);
        }
    }

    private void DrawTableFrame(Graphics graphics)
    {
        var bounds = Rectangle.Round(GetTableFrameBounds());
        var tableFrameTexture = GetScaledTexture("table-frame", _tableFrameTexture, bounds.Size);
        graphics.DrawImageUnscaled(tableFrameTexture, bounds.Location);
    }

    private RectangleF GetTableFrameBounds()
    {
        const float sourceTableLeft = 126f;
        const float sourceTableTop = 124f;
        const float sourceTableWidth = 792f;
        const float sourceTableHeight = 805f;

        var tableFrameBounds = _tableOuterPath.GetBounds();
        var width = tableFrameBounds.Width * (_tableFrameTexture.Width / sourceTableWidth);
        var height = tableFrameBounds.Height * (_tableFrameTexture.Height / sourceTableHeight);
        return new RectangleF(
            tableFrameBounds.Left - (sourceTableLeft * width / _tableFrameTexture.Width),
            tableFrameBounds.Top - (sourceTableTop * height / _tableFrameTexture.Height),
            width,
            height);
    }

    private void DrawBag(Graphics graphics, bool useOpenBagSprite, bool useActiveBagSprite, float bagScale)
    {
        var texture = useOpenBagSprite
            ? _bagOpenTexture
            : useActiveBagSprite
                ? _bagActiveTexture
                : _bagTexture;
        var cacheKey = useOpenBagSprite
            ? "bag-open"
            : useActiveBagSprite
                ? "bag-active"
                : "bag";
        DrawBottomControl(graphics, cacheKey, texture, _board.BagBounds, bagScale);
    }

    private void DrawRerollButton(Graphics graphics, bool useActiveTexture, float rerollScale)
    {
        var texture = useActiveTexture ? _rerollButtonActiveTexture : _rerollButtonTexture;
        var cacheKey = useActiveTexture ? "reroll-active" : "reroll";
        DrawBottomControl(graphics, cacheKey, texture, _board.RerollBounds, rerollScale);
    }

    private void DrawBottomControl(Graphics graphics, string cacheKey, Bitmap texture, Rectangle bounds, float scaleMultiplier)
    {
        var scale = Math.Min(
            bounds.Width / (float)texture.Width,
            bounds.Height / (float)texture.Height) * scaleMultiplier;

        var drawWidth = Math.Max(1, (int)MathF.Round(texture.Width * scale));
        var drawHeight = Math.Max(1, (int)MathF.Round(texture.Height * scale));
        var drawX = (int)MathF.Round(bounds.Left + (bounds.Width * 0.5f) - (drawWidth * 0.5f));
        var drawY = (int)MathF.Round(bounds.Top + (bounds.Height * 0.5f) - (drawHeight * 0.5f));
        var scaledTexture = GetScaledTexture(cacheKey, texture, new Size(drawWidth, drawHeight));

        graphics.DrawImageUnscaled(scaledTexture, drawX, drawY);
    }

    private void DrawBuffedRuneCells(Graphics graphics, IReadOnlyList<RuneEntity> runes)
    {
        var animationTime = (float)(Environment.TickCount64 * 0.0045);
        var pulse = 0.72f + (0.28f * ((MathF.Sin(animationTime) + 1f) * 0.5f));
        var outerAlpha = (int)(150f * pulse);
        var innerAlpha = (int)(245f * pulse);
        _geboBuffOuterPen.Color = Color.FromArgb(outerAlpha, GeboBuffAccentColor);
        _geboBuffInnerPen.Color = Color.FromArgb(innerAlpha, GeboBuffAccentColor);
        _wunjoBuffOuterPen.Color = Color.FromArgb(outerAlpha, WunjoBuffAccentColor);
        _wunjoBuffInnerPen.Color = Color.FromArgb(innerAlpha, WunjoBuffAccentColor);
        _dagazBuffOuterPen.Color = Color.FromArgb(outerAlpha, DagazBuffAccentColor);
        _dagazBuffInnerPen.Color = Color.FromArgb(innerAlpha, DagazBuffAccentColor);

        for (var i = 0; i < runes.Count; i++)
        {
            var rune = runes[i];
            if (!rune.Buffs.HasAttackSpeedBuff && !rune.Buffs.HasCriticalHitBuff && !rune.Buffs.HasMultiShotBuff)
            {
                continue;
            }

            var cellIndex = (rune.Grid.Row * TableGrid.Size) + rune.Grid.Column;
            if (rune.Buffs.HasAttackSpeedBuff)
            {
                graphics.DrawPath(_geboBuffOuterPen, _attackSpeedOuterCellPaths[cellIndex]);
                graphics.DrawPath(_geboBuffInnerPen, _attackSpeedInnerCellPaths[cellIndex]);
            }

            if (rune.Buffs.HasCriticalHitBuff)
            {
                graphics.DrawPath(_wunjoBuffOuterPen, _criticalOuterCellPaths[cellIndex]);
                graphics.DrawPath(_wunjoBuffInnerPen, _criticalInnerCellPaths[cellIndex]);
            }

            if (rune.Buffs.HasMultiShotBuff)
            {
                graphics.DrawPath(_dagazBuffOuterPen, _multiShotOuterCellPaths[cellIndex]);
                graphics.DrawPath(_dagazBuffInnerPen, _multiShotInnerCellPaths[cellIndex]);
            }
        }
    }

    private void DrawOthalaSymbiosis(Graphics graphics, IReadOnlyList<RuneEntity> runes)
    {
        var animationTime = (float)(Environment.TickCount64 * 0.0022);

        for (var i = 0; i < runes.Count; i++)
        {
            var rune = runes[i];
            if (rune.Stats.Type != RuneType.Othala || rune.State.OthalaClusterSize <= 1 || !rune.Presentation.IsCombatActive)
            {
                continue;
            }

            var center = rune.Presentation.VisualPosition;
            var pulse = 0.84f + (((MathF.Sin(animationTime + (i * 0.53f)) + 1f) * 0.5f) * 0.2f);
            var outerRadius = (26f + (rune.State.OthalaClusterSize * 2.6f)) * pulse;
            var innerRadius = outerRadius * 0.62f;
            graphics.FillEllipse(
                _othalaAuraOuterBrush,
                center.X - outerRadius,
                center.Y - outerRadius,
                outerRadius * 2f,
                outerRadius * 2f);
            graphics.FillEllipse(
                _othalaAuraInnerBrush,
                center.X - innerRadius,
                center.Y - innerRadius,
                innerRadius * 2f,
                innerRadius * 2f);
        }

        for (var i = 0; i < runes.Count; i++)
        {
            var leftRune = runes[i];
            if (leftRune.Stats.Type != RuneType.Othala || leftRune.State.OthalaClusterSize <= 1 || !leftRune.Presentation.IsCombatActive)
            {
                continue;
            }

            for (var j = i + 1; j < runes.Count; j++)
            {
                var rightRune = runes[j];
                if (rightRune.Stats.Type != RuneType.Othala ||
                    rightRune.State.OthalaClusterSize != leftRune.State.OthalaClusterSize ||
                    !rightRune.Presentation.IsCombatActive ||
                    !OthalaClusterUtility.AreAdjacent(leftRune, rightRune))
                {
                    continue;
                }

                var start = leftRune.Presentation.VisualPosition;
                var end = rightRune.Presentation.VisualPosition;
                var direction = end - start;
                if (direction.LengthSquared() <= 0.001f)
                {
                    continue;
                }

                direction = Vector2.Normalize(direction);
                var normal = new Vector2(-direction.Y, direction.X);
                var curveOffset = normal * (MathF.Sin(animationTime + (i * 0.29f) + (j * 0.21f)) * 5.5f);
                var midpoint = (start + end) * 0.5f;
                var control = midpoint + curveOffset;

                const int segmentCount = 9;
                for (var segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
                {
                    var t = segmentCount <= 1
                        ? 0.5f
                        : segmentIndex / (float)(segmentCount - 1);
                    var pulseWave = 0.76f + (((MathF.Sin((animationTime * 2.4f) - (t * 4.8f) + (i * 0.37f) + (j * 0.19f)) + 1f) * 0.5f) * 0.34f);
                    var segmentPosition = SampleQuadratic(start, control, end, t);
                    var tangent = SampleQuadraticTangent(start, control, end, t);
                    if (tangent.LengthSquared() <= 0.001f)
                    {
                        tangent = direction;
                    }
                    else
                    {
                        tangent = Vector2.Normalize(tangent);
                    }

                    var segmentNormal = new Vector2(-tangent.Y, tangent.X);
                    var drift = MathF.Sin((animationTime * 3.1f) + (t * 6.4f) + (i * 0.6f) + (j * 0.35f)) * 1.8f;
                    segmentPosition += segmentNormal * drift;

                    var outerLength = (10.5f - (MathF.Abs(t - 0.5f) * 3.2f)) * pulseWave;
                    var outerWidth = (6.2f - (MathF.Abs(t - 0.5f) * 1.2f)) * pulseWave;
                    DrawSoftSegment(graphics, _othalaBridgeOuterPen, segmentPosition, tangent, outerLength, outerWidth);
                    DrawSoftSegment(graphics, _othalaBridgeInnerPen, segmentPosition, tangent, outerLength * 0.58f, outerWidth * 0.54f);
                }
            }
        }
    }

    private static void DrawSoftSegment(Graphics graphics, Pen pen, Vector2 center, Vector2 tangent, float halfLength, float halfWidth)
    {
        var drawWidth = Math.Max(1f, halfWidth * 2f);
        var start = center - (tangent * halfLength);
        var end = center + (tangent * halfLength);
        pen.Width = drawWidth;
        graphics.DrawLine(pen, start.X, start.Y, end.X, end.Y);
    }

    private static Vector2 SampleQuadratic(Vector2 start, Vector2 control, Vector2 end, float t)
    {
        var clampedT = Math.Clamp(t, 0f, 1f);
        var oneMinusT = 1f - clampedT;
        return (oneMinusT * oneMinusT * start) +
            (2f * oneMinusT * clampedT * control) +
            (clampedT * clampedT * end);
    }

    private static Vector2 SampleQuadraticTangent(Vector2 start, Vector2 control, Vector2 end, float t)
    {
        var clampedT = Math.Clamp(t, 0f, 1f);
        return (2f * (1f - clampedT) * (control - start)) +
            (2f * clampedT * (end - control));
    }

    private void DrawTiwazChargingGlow(Graphics graphics, GameState gameState)
    {
        if (!gameState.Tiwaz.IsCharging)
        {
            return;
        }

        var animationTime = gameState.PresentationTimeSeconds * 1.65f;

        for (var i = 0; i < gameState.Runes.Count; i++)
        {
            var tiwazRune = gameState.Runes[i];
            if (tiwazRune.Stats.Type != RuneType.Tiwaz)
            {
                continue;
            }

            var tiwazCenter = tiwazRune.Presentation.VisualPosition;
            var activeSourceCount = 0;
            for (var j = 0; j < gameState.Runes.Count; j++)
            {
                var sourceRune = gameState.Runes[j];
                if (sourceRune.Stats.Type == RuneType.Tiwaz)
                {
                    continue;
                }

                var rowDistance = Math.Abs(sourceRune.Grid.Row - tiwazRune.Grid.Row);
                var columnDistance = Math.Abs(sourceRune.Grid.Column - tiwazRune.Grid.Column);
                if ((rowDistance + columnDistance) != 1)
                {
                    continue;
                }

                activeSourceCount++;
            }

            if (activeSourceCount <= 0)
            {
                continue;
            }

            var glowPulse = 0.86f + (((MathF.Sin(animationTime + (i * 0.41f)) + 1f) * 0.5f) * 0.28f);
            var outerRadius = (38f + (activeSourceCount * 3.2f)) * glowPulse;
            var innerRadius = outerRadius * 0.78f;
            var haloRadius = outerRadius - 6f;
            graphics.FillEllipse(
                _tiwazOuterGlowBrush,
                tiwazCenter.X - outerRadius,
                tiwazCenter.Y - outerRadius,
                outerRadius * 2f,
                outerRadius * 2f);
            graphics.DrawEllipse(
                _tiwazHaloPen,
                tiwazCenter.X - haloRadius,
                tiwazCenter.Y - haloRadius,
                haloRadius * 2f,
                haloRadius * 2f);
            graphics.FillEllipse(
                _tiwazInnerGlowBrush,
                tiwazCenter.X - innerRadius,
                tiwazCenter.Y - innerRadius,
                innerRadius * 2f,
                innerRadius * 2f);
        }
    }

    private void DrawTiwazChargingParticles(Graphics graphics, GameState gameState)
    {
        if (!gameState.Tiwaz.IsCharging)
        {
            return;
        }

        var animationTime = gameState.PresentationTimeSeconds * 1.45f;

        for (var i = 0; i < gameState.Runes.Count; i++)
        {
            var tiwazRune = gameState.Runes[i];
            if (tiwazRune.Stats.Type != RuneType.Tiwaz)
            {
                continue;
            }

            var tiwazCenter = tiwazRune.Presentation.VisualPosition;
            for (var j = 0; j < gameState.Runes.Count; j++)
            {
                var sourceRune = gameState.Runes[j];
                if (sourceRune.Stats.Type == RuneType.Tiwaz)
                {
                    continue;
                }

                var rowDistance = Math.Abs(sourceRune.Grid.Row - tiwazRune.Grid.Row);
                var columnDistance = Math.Abs(sourceRune.Grid.Column - tiwazRune.Grid.Column);
                if ((rowDistance + columnDistance) != 1)
                {
                    continue;
                }

                var sourceCenter = sourceRune.Presentation.VisualPosition;
                var direction = tiwazCenter - sourceCenter;
                if (direction.LengthSquared() <= 0.001f)
                {
                    continue;
                }

                direction = Vector2.Normalize(direction);
                var normal = new Vector2(-direction.Y, direction.X);
                var approachDistance = 44f + (7f * MathF.Sin(animationTime + (i * 0.41f) + (j * 0.29f)));
                for (var pulseIndex = 0; pulseIndex < 2; pulseIndex++)
                {
                    var phase = ((animationTime * 0.58f) + (pulseIndex * 0.37f) + (i * 0.17f) + (j * 0.13f)) % 1f;
                    var easedPhase = SmoothStep(phase);
                    var distanceFromCenter = 22f + (approachDistance * (1f - easedPhase));
                    var swayMagnitude = (1f - easedPhase) * (6.5f + (pulseIndex * 1.4f));
                    var swayOffset = MathF.Sin((animationTime * 3.2f) + (phase * 6.4f) + (i * 0.9f) + (j * 1.3f)) * swayMagnitude;
                    var pulsePosition = tiwazCenter - (direction * distanceFromCenter) + (normal * swayOffset);
                    var pulseRadius = (5.2f - (pulseIndex * 1.05f)) * (0.92f + ((1f - easedPhase) * 0.18f));
                    var phaseFade = SmoothStep(Math.Min(phase / 0.18f, 1f)) * SmoothStep(Math.Min((1f - phase) / 0.24f, 1f));
                    if (phaseFade <= 0.02f)
                    {
                        continue;
                    }

                    _tiwazPulseBrush.Color = Color.FromArgb((int)(176f * phaseFade), 255, 236, 150);
                    graphics.FillEllipse(
                        _tiwazPulseBrush,
                        pulsePosition.X - pulseRadius,
                        pulsePosition.Y - pulseRadius,
                        pulseRadius * 2f,
                        pulseRadius * 2f);

                    var coreRadius = MathF.Max(1.2f, pulseRadius * 0.34f);
                    _tiwazCoreBrush.Color = Color.FromArgb((int)(228f * phaseFade), 255, 248, 214);
                    graphics.FillEllipse(
                        _tiwazCoreBrush,
                        pulsePosition.X - coreRadius,
                        pulsePosition.Y - coreRadius,
                        coreRadius * 2f,
                        coreRadius * 2f);
                }
            }
        }
    }

    private void DrawRunes(Graphics graphics, IReadOnlyList<RuneEntity> runes, RuneEntity? draggedRune)
    {
        foreach (var rune in runes)
        {
            if (ReferenceEquals(rune, draggedRune) || rune.Presentation.ShouldRenderAboveBag)
            {
                continue;
            }

            DrawRuneWithDragContext(graphics, rune, draggedRune);
        }
    }

    private void DrawTopLayerRunes(Graphics graphics, IReadOnlyList<RuneEntity> runes, RuneEntity? draggedRune)
    {
        foreach (var rune in runes)
        {
            if (ReferenceEquals(rune, draggedRune) || !rune.Presentation.ShouldRenderAboveBag)
            {
                continue;
            }

            DrawRuneWithDragContext(graphics, rune, draggedRune);
        }
    }

    private void DrawRuneWithDragContext(Graphics graphics, RuneEntity rune, RuneEntity? draggedRune)
    {
        var alphaMultiplier = draggedRune != null && ShouldDimForDraggedMerge(draggedRune, rune)
            ? 0.3f
            : 1f;

        _runeView.Draw(
            graphics,
            rune,
            rune.Presentation.VisualPosition,
            rune.Presentation.VisualScale,
            rune.Presentation.VisualAlpha * alphaMultiplier);
    }

    private void DrawDraggedRune(Graphics graphics, RuneEntity? draggedRune, Vector2 draggedRunePosition)
    {
        if (draggedRune == null)
        {
            return;
        }

        _runeView.Draw(graphics, draggedRune, draggedRunePosition);
    }

    private void DrawDraggedRuneHoldEffect(Graphics graphics, RuneEntity? draggedRune, Vector2 draggedRunePosition)
    {
        if (draggedRune == null)
        {
            return;
        }

        if (!EffectRegistry.TryGetEffectColorRowIndex(draggedRune.Stats.Color, out var rowIndex))
        {
            return;
        }

        var definition = EffectRegistry.GetRuneHoldEffect();
        var elapsedSeconds = (float)(Environment.TickCount64 / 1000.0);
        var frameIndex = definition.FrameCount <= 1
            ? 0
            : (int)(elapsedSeconds / definition.FrameDuration) % definition.FrameCount;

        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        _effectView.Draw(
            graphics,
            definition,
            rowIndex,
            draggedRunePosition,
            definition.DefaultScale,
            frameIndex);
    }

    private void DrawEffects(Graphics graphics, IReadOnlyList<AnimatedEffect> effects)
    {
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

        foreach (var effect in effects)
        {
            _effectView.Draw(graphics, effect);
        }
    }

    private void DrawSowiloBeams(Graphics graphics, IReadOnlyList<SowiloBeamInstance> beams)
    {
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

        foreach (var beam in beams)
        {
            _sowiloBeamView.Draw(graphics, beam);
        }
    }

    private void DrawProjectiles(Graphics graphics, IReadOnlyList<ProjectileEntity> projectiles)
    {
        foreach (var projectile in projectiles)
        {
            _projectileView.Draw(graphics, projectile);
        }
    }

    private void DrawPerthroBoomerangs(Graphics graphics, IReadOnlyList<PerthroBoomerangEntity> boomerangs)
    {
        foreach (var boomerang in boomerangs)
        {
            _projectileView.DrawPerthroBoomerang(graphics, boomerang);
        }
    }

    private void DrawLaguzOrbs(Graphics graphics, IReadOnlyList<LaguzOrbEntity> laguzOrbs)
    {
        foreach (var laguzOrb in laguzOrbs)
        {
            _laguzOrbView.Draw(graphics, laguzOrb);
        }
    }

    private void DrawLaguzBlackHoles(Graphics graphics, IReadOnlyList<LaguzBlackHoleEntity> laguzBlackHoles)
    {
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

        foreach (var laguzBlackHole in laguzBlackHoles)
        {
            _laguzBlackHoleView.Draw(graphics, laguzBlackHole);
        }
    }

    private void DrawEhwazChainLinks(Graphics graphics, IReadOnlyList<EhwazChainLinkInstance> chainLinks)
    {
        foreach (var chainLink in chainLinks)
        {
            _ehwazChainLinkView.Draw(graphics, chainLink);
        }
    }

    private void DrawAnsuzAllies(Graphics graphics, IReadOnlyList<AnsuzAllyEntity> allies)
    {
        foreach (var ally in allies)
        {
            _ansuzAllyView.Draw(graphics, ally);
        }
    }

    private void DrawEiwazAimLines(Graphics graphics, IReadOnlyList<RuneEntity> runes, IReadOnlyList<EnemyEntity> enemies)
    {
        for (var i = 0; i < runes.Count; i++)
        {
            var rune = runes[i];
            if (rune.Stats.Type != RuneType.Eiwaz || !rune.State.IsEiwazAiming || !rune.Presentation.IsCombatActive)
            {
                continue;
            }

            var target = EnemyQuery.FindById(enemies, rune.State.EiwazTargetEnemyId);
            if (!EnemyQuery.IsTargetable(target))
            {
                continue;
            }

            var targetEnemy = target!;
            var appearProgress = EiwazAimAppearDurationSeconds <= 0f
                ? 1f
                : Math.Clamp(rune.State.EiwazAimElapsedSeconds / EiwazAimAppearDurationSeconds, 0f, 1f);
            appearProgress = SmoothStep(appearProgress);
            var start = rune.Transform.Position;
            var end = targetEnemy.Transform.Position;
            var direction = end - start;
            if (direction.LengthSquared() <= 0.001f)
            {
                continue;
            }

            direction = Vector2.Normalize(direction);
            var angleDegrees = MathF.Atan2(direction.Y, direction.X) * (180f / MathF.PI);
            var beamStart = start + (direction * EiwazAimArcInnerRadius);
            var visibleBeamEnd = Vector2.Lerp(beamStart, end, appearProgress);
            var visibleArcSpanDegrees = EiwazAimArcSpanDegrees * appearProgress;
            var visibleArcStartDegrees = angleDegrees - (visibleArcSpanDegrees * 0.5f);

            var arcOuterRect = CreateCenteredSquareF(start, EiwazAimArcRadius * 2f);
            var arcInnerRect = CreateCenteredSquareF(start, EiwazAimArcInnerRadius * 2f);
            if (visibleArcSpanDegrees > 0.5f)
            {
                _eiwazAimFillPath.Reset();
                _eiwazAimFillPath.AddArc(arcOuterRect, visibleArcStartDegrees, visibleArcSpanDegrees);
                _eiwazAimFillPath.AddArc(arcInnerRect, visibleArcStartDegrees + visibleArcSpanDegrees, -visibleArcSpanDegrees);
                _eiwazAimFillPath.CloseFigure();
                graphics.FillPath(_eiwazArcFillBrush, _eiwazAimFillPath);
                graphics.DrawArc(_eiwazArcGlowPen, arcOuterRect, visibleArcStartDegrees, visibleArcSpanDegrees);
                graphics.DrawArc(_eiwazArcCorePen, arcOuterRect, visibleArcStartDegrees, visibleArcSpanDegrees);
            }

            var leftEdgeDirection = Rotate(direction, -(EiwazAimArcSpanDegrees * 0.5f) * (MathF.PI / 180f));
            var rightEdgeDirection = Rotate(direction, (EiwazAimArcSpanDegrees * 0.5f) * (MathF.PI / 180f));
            if (appearProgress >= 0.85f)
            {
                graphics.DrawLine(
                    _eiwazArcCorePen,
                    start.X + (leftEdgeDirection.X * EiwazAimArcInnerRadius),
                    start.Y + (leftEdgeDirection.Y * EiwazAimArcInnerRadius),
                    start.X + (leftEdgeDirection.X * EiwazAimArcRadius),
                    start.Y + (leftEdgeDirection.Y * EiwazAimArcRadius));
                graphics.DrawLine(
                    _eiwazArcCorePen,
                    start.X + (rightEdgeDirection.X * EiwazAimArcInnerRadius),
                    start.Y + (rightEdgeDirection.Y * EiwazAimArcInnerRadius),
                    start.X + (rightEdgeDirection.X * EiwazAimArcRadius),
                    start.Y + (rightEdgeDirection.Y * EiwazAimArcRadius));
            }

            graphics.DrawLine(_eiwazBeamGlowPen, beamStart.X, beamStart.Y, visibleBeamEnd.X, visibleBeamEnd.Y);
            graphics.DrawLine(_eiwazBeamCorePen, beamStart.X, beamStart.Y, visibleBeamEnd.X, visibleBeamEnd.Y);
            graphics.FillEllipse(_eiwazMuzzleGlowBrush, beamStart.X - 6f, beamStart.Y - 6f, 12f, 12f);
            graphics.FillEllipse(_eiwazMuzzleCoreBrush, beamStart.X - 2.8f, beamStart.Y - 2.8f, 5.6f, 5.6f);
        }
    }

    private void DrawEnemies(Graphics graphics, IReadOnlyList<EnemyEntity> enemies)
    {
        foreach (var enemy in enemies)
        {
            _enemyView.DrawBodyLayer(graphics, enemy, enemy.Data.IsUruzMarked);
            _enemyView.DrawHealthBadgeLayer(graphics, enemy);
        }

        foreach (var enemy in enemies)
        {
            _enemyView.DrawBurnOverlayLayer(graphics, enemy);
        }
    }

    private void DrawUruzTornadoes(Graphics graphics, IReadOnlyList<UruzTornadoEntity> tornadoes)
    {
        foreach (var tornado in tornadoes)
        {
            _uruzTornadoView.Draw(graphics, tornado);
        }
    }

    private void DrawDamagePopups(Graphics graphics, IReadOnlyList<DamagePopupInstance> popups)
    {
        foreach (var popup in popups)
        {
            _damagePopupView.Draw(graphics, popup);
        }
    }

    private void DrawPathMarker(Graphics graphics, Vector2 center)
    {
        graphics.FillEllipse(
            _pathMarkerOuterBrush,
            center.X - PathMarkerOuterRadius,
            center.Y - PathMarkerOuterRadius,
            PathMarkerOuterRadius * 2f,
            PathMarkerOuterRadius * 2f);

        graphics.DrawEllipse(
            _pathMarkerBorderPen,
            center.X - PathMarkerOuterRadius,
            center.Y - PathMarkerOuterRadius,
            PathMarkerOuterRadius * 2f,
            PathMarkerOuterRadius * 2f);

        graphics.FillEllipse(
            _pathMarkerInnerBrush,
            center.X - PathMarkerInnerRadius,
            center.Y - PathMarkerInnerRadius,
            PathMarkerInnerRadius * 2f,
            PathMarkerInnerRadius * 2f);
    }

    private void DrawHagalazPathPreview(Graphics graphics, GameUiState uiState)
    {
        if (!uiState.IsHagalazPathPreviewVisible)
        {
            return;
        }

        if (uiState.HagalazPathPreviewPoints.Length >= 2)
        {
            if (_hagalazPreviewPointsBuffer.Length != uiState.HagalazPathPreviewPoints.Length)
            {
                _hagalazPreviewPointsBuffer = new PointF[uiState.HagalazPathPreviewPoints.Length];
            }

            for (var i = 0; i < uiState.HagalazPathPreviewPoints.Length; i++)
            {
                var point = uiState.HagalazPathPreviewPoints[i];
                _hagalazPreviewPointsBuffer[i] = new PointF(point.X, point.Y);
            }

            graphics.DrawLines(_hagalazPreviewPen, _hagalazPreviewPointsBuffer);
            graphics.DrawLines(_hagalazPreviewCorePen, _hagalazPreviewPointsBuffer);
        }

        var explosionDiameter = HagalazTuning.ExplosionDiameter;
        graphics.FillEllipse(
            _hagalazPreviewAreaBrush,
            uiState.HagalazPathPreviewCenter.X - HagalazTuning.ExplosionRadius,
            uiState.HagalazPathPreviewCenter.Y - HagalazTuning.ExplosionRadius,
            explosionDiameter,
            explosionDiameter);
        graphics.DrawEllipse(
            _hagalazPreviewAreaPen,
            uiState.HagalazPathPreviewCenter.X - HagalazTuning.ExplosionRadius,
            uiState.HagalazPathPreviewCenter.Y - HagalazTuning.ExplosionRadius,
            explosionDiameter,
            explosionDiameter);
        graphics.FillEllipse(
            _hagalazPreviewMarkerBrush,
            uiState.HagalazPathPreviewCenter.X - HagalazPreviewMarkerRadius,
            uiState.HagalazPathPreviewCenter.Y - HagalazPreviewMarkerRadius,
            HagalazPreviewMarkerRadius * 2f,
            HagalazPreviewMarkerRadius * 2f);
    }
}
