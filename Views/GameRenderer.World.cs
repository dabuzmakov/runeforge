using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;
using runeforge.Configs;
using runeforge.Effects;
using runeforge.Models;

namespace runeforge.Views;

public sealed partial class GameRenderer
{
    private void DrawPath(Graphics graphics)
    {
        if (_pathPoints.Length < 2)
        {
            return;
        }

        graphics.DrawLines(_pathShadowPen, _pathPoints);
        graphics.DrawLines(_pathPen, _pathPoints);
        graphics.DrawLines(_pathCorePen, _pathPoints);
        DrawHagalazPathPreview(graphics, _model.State.Ui);
        DrawPathMarker(graphics, _board.Path[0]);
        DrawPathMarker(graphics, _board.Path[^1]);
    }

    private void DrawTable(Graphics graphics)
    {
        graphics.FillPath(_tableFillBrush, _tableOuterPath);
        graphics.FillPath(_tableInnerBrush, _tableInnerPath);
        graphics.DrawPath(_tableBorderPen, _tableOuterPath);

        foreach (var cellPath in _cellPaths)
        {
            graphics.DrawPath(_cellBorderPen, cellPath);
        }
    }

    private void DrawBag(Graphics graphics, bool useOpenBagSprite, float bagScale)
    {
        var texture = useOpenBagSprite ? _bagOpenTexture : _bagTexture;
        var scale = Math.Min(
            _board.BagBounds.Width / (float)texture.Width,
            _board.BagBounds.Height / (float)texture.Height) * bagScale;

        var drawWidth = texture.Width * scale;
        var drawHeight = texture.Height * scale;
        var drawX = _board.BagBounds.Left + (_board.BagBounds.Width * 0.5f) - (drawWidth * 0.5f);
        var drawY = _board.BagBounds.Top + (_board.BagBounds.Height * 0.5f) - (drawHeight * 0.5f);

        graphics.DrawImage(texture, drawX, drawY, drawWidth, drawHeight);
    }

    private void DrawBuffedRuneCells(Graphics graphics, IReadOnlyList<RuneEntity> runes)
    {
        var animationTime = (float)(Environment.TickCount64 * 0.0045);
        var pulse = 0.72f + (0.28f * ((MathF.Sin(animationTime) + 1f) * 0.5f));
        var outerAlpha = (int)(150f * pulse);
        var innerAlpha = (int)(245f * pulse);

        for (var i = 0; i < runes.Count; i++)
        {
            var rune = runes[i];
            if (!rune.Buffs.HasAttackSpeedBuff && !rune.Buffs.HasCriticalHitBuff && !rune.Buffs.HasMultiShotBuff)
            {
                continue;
            }

            var cellBounds = Inflate(_board.Grid.GetCell(rune.Grid.Row, rune.Grid.Column).Bounds, -5, -5);
            if (rune.Buffs.HasAttackSpeedBuff)
            {
                using var outerPen = new Pen(Color.FromArgb(outerAlpha, GeboBuffAccentColor), 2.2f)
                {
                    LineJoin = LineJoin.Round
                };
                using var innerPen = new Pen(Color.FromArgb(innerAlpha, GeboBuffAccentColor), 1.2f)
                {
                    LineJoin = LineJoin.Round
                };
                using var outerPath = CreateRoundedRectanglePath(cellBounds, 12);
                using var innerPath = CreateRoundedRectanglePath(Inflate(cellBounds, -3, -3), 10);
                graphics.DrawPath(outerPen, outerPath);
                graphics.DrawPath(innerPen, innerPath);
            }

            if (rune.Buffs.HasCriticalHitBuff)
            {
                var outerBounds = Inflate(cellBounds, 3, 3);
                var innerOuterBounds = Inflate(outerBounds, -3, -3);
                using var outerPen = new Pen(Color.FromArgb(outerAlpha, WunjoBuffAccentColor), 2.2f)
                {
                    LineJoin = LineJoin.Round
                };
                using var innerPen = new Pen(Color.FromArgb(innerAlpha, WunjoBuffAccentColor), 1.2f)
                {
                    LineJoin = LineJoin.Round
                };
                using var outerPath = CreateRoundedRectanglePath(outerBounds, 14);
                using var innerPath = CreateRoundedRectanglePath(innerOuterBounds, 12);
                graphics.DrawPath(outerPen, outerPath);
                graphics.DrawPath(innerPen, innerPath);
            }

            if (rune.Buffs.HasMultiShotBuff)
            {
                var outerBounds = Inflate(cellBounds, 6, 6);
                var innerOuterBounds = Inflate(outerBounds, -3, -3);
                using var outerPen = new Pen(Color.FromArgb(outerAlpha, DagazBuffAccentColor), 2.2f)
                {
                    LineJoin = LineJoin.Round
                };
                using var innerPen = new Pen(Color.FromArgb(innerAlpha, DagazBuffAccentColor), 1.2f)
                {
                    LineJoin = LineJoin.Round
                };
                using var outerPath = CreateRoundedRectanglePath(outerBounds, 16);
                using var innerPath = CreateRoundedRectanglePath(innerOuterBounds, 14);
                graphics.DrawPath(outerPen, outerPath);
                graphics.DrawPath(innerPen, innerPath);
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
        using var beamGlowPen = new Pen(Color.FromArgb(84, 25, 141, 247), 6f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        using var beamCorePen = new Pen(Color.FromArgb(255, 25, 141, 247), 2.1f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        using var arcGlowPen = new Pen(Color.FromArgb(92, 25, 141, 247), 9f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        using var arcCorePen = new Pen(Color.FromArgb(255, 25, 141, 247), 2.8f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        using var arcFillBrush = new SolidBrush(Color.FromArgb(34, 25, 141, 247));
        using var muzzleGlowBrush = new SolidBrush(Color.FromArgb(96, 25, 141, 247));
        using var muzzleCoreBrush = new SolidBrush(Color.FromArgb(255, 212, 240, 255));

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
            using var fillPath = new GraphicsPath();
            if (visibleArcSpanDegrees > 0.5f)
            {
                fillPath.AddArc(arcOuterRect, visibleArcStartDegrees, visibleArcSpanDegrees);
                fillPath.AddArc(arcInnerRect, visibleArcStartDegrees + visibleArcSpanDegrees, -visibleArcSpanDegrees);
                fillPath.CloseFigure();
                graphics.FillPath(arcFillBrush, fillPath);
                graphics.DrawArc(arcGlowPen, arcOuterRect, visibleArcStartDegrees, visibleArcSpanDegrees);
                graphics.DrawArc(arcCorePen, arcOuterRect, visibleArcStartDegrees, visibleArcSpanDegrees);
            }

            var leftEdgeDirection = Rotate(direction, -(EiwazAimArcSpanDegrees * 0.5f) * (MathF.PI / 180f));
            var rightEdgeDirection = Rotate(direction, (EiwazAimArcSpanDegrees * 0.5f) * (MathF.PI / 180f));
            if (appearProgress >= 0.85f)
            {
                graphics.DrawLine(
                    arcCorePen,
                    start.X + (leftEdgeDirection.X * EiwazAimArcInnerRadius),
                    start.Y + (leftEdgeDirection.Y * EiwazAimArcInnerRadius),
                    start.X + (leftEdgeDirection.X * EiwazAimArcRadius),
                    start.Y + (leftEdgeDirection.Y * EiwazAimArcRadius));
                graphics.DrawLine(
                    arcCorePen,
                    start.X + (rightEdgeDirection.X * EiwazAimArcInnerRadius),
                    start.Y + (rightEdgeDirection.Y * EiwazAimArcInnerRadius),
                    start.X + (rightEdgeDirection.X * EiwazAimArcRadius),
                    start.Y + (rightEdgeDirection.Y * EiwazAimArcRadius));
            }

            graphics.DrawLine(beamGlowPen, beamStart.X, beamStart.Y, visibleBeamEnd.X, visibleBeamEnd.Y);
            graphics.DrawLine(beamCorePen, beamStart.X, beamStart.Y, visibleBeamEnd.X, visibleBeamEnd.Y);
            graphics.FillEllipse(muzzleGlowBrush, beamStart.X - 6f, beamStart.Y - 6f, 12f, 12f);
            graphics.FillEllipse(muzzleCoreBrush, beamStart.X - 2.8f, beamStart.Y - 2.8f, 5.6f, 5.6f);
        }
    }

    private void DrawEnemies(Graphics graphics, IReadOnlyList<EnemyEntity> enemies)
    {
        foreach (var enemy in enemies)
        {
            _enemyView.Draw(graphics, enemy, enemy.Data.IsUruzMarked);
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
            var previewPoints = new PointF[uiState.HagalazPathPreviewPoints.Length];
            for (var i = 0; i < uiState.HagalazPathPreviewPoints.Length; i++)
            {
                var point = uiState.HagalazPathPreviewPoints[i];
                previewPoints[i] = new PointF(point.X, point.Y);
            }

            graphics.DrawLines(_hagalazPreviewPen, previewPoints);
            graphics.DrawLines(_hagalazPreviewCorePen, previewPoints);
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
