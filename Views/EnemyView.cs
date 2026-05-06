using System.Drawing;
using System.Drawing.Drawing2D;
using runeforge.Configs;
using runeforge.Models;
using runeforge.Systems;

namespace runeforge.Views;

public sealed class EnemyView : IDisposable
{
    private const float SquareCornerRadius = 7f;
    private const float TriangleCurveTension = 0.22f;
    private const float StarInnerRadiusRatio = 0.46f;
    private const float DiamondInsetRatio = 0.12f;
    private const float MinimumRenderableDiameter = 1.5f;
    private const float IngwazEffectBottomAnchorVerticalOffset = 0f;
    private const float HealthBadgeHorizontalPadding = 4f;
    private const float HealthBadgeVerticalPadding = 2f;
    private const float HealthBadgeMinHeight = 12f;
    private const float HealthBadgeMaxWidthMultiplier = 1.12f;
    private const float HealthBadgeMaxOverflow = 6f;
    private const float HealthBadgeFontSizeCap = 10.5f;
    private const float BossAccentOutlinePadding = 7f;
    private const float BossAccentInnerPadding = 3.5f;
    private readonly Font _font;
    private readonly Dictionary<int, Font> _healthBadgeFonts;
    private readonly Dictionary<HealthBadgeMeasureKey, SizeF> _healthBadgeMeasureCache;
    private readonly StringFormat _textFormat;
    private readonly SolidBrush _highlightBrush;
    private readonly SolidBrush _badgeBrush;
    private readonly SolidBrush _textBrush;
    private readonly Pen _innerPen;
    private readonly Pen _slowedOutlinePen;
    private readonly Pen _laguzSlowedOutlinePen;
    private readonly SolidBrush _shatterShieldBrush;
    private readonly SolidBrush _shatterShieldHighlightBrush;
    private readonly Pen _badgePen;
    private readonly Pen _shatterShieldPen;
    private readonly Pen _bossGlowPen;
    private readonly Pen _bossCorePen;
    private readonly SolidBrush _bossCrownBrush;
    private readonly Pen _bossCrownPen;
    private readonly SolidBrush _auraRangeFillBrush;
    private readonly Pen _auraRangeGlowPen;
    private readonly Pen _auraRangeCorePen;
    private readonly Pen _auraEnemyGlowPen;
    private readonly Pen _auraEnemyCorePen;
    private readonly Pen _segmentedAuraGlowPen;
    private readonly Pen _segmentedAuraCorePen;
    private readonly Bitmap? _ingwazEffectTexture;
    private readonly Dictionary<EnemyType, EnemyPalette> _palettes;
    private readonly Dictionary<EnemyAuraType, EnemyPalette> _auraPalettes;
    private readonly Dictionary<ShapeCacheKey, GraphicsPath> _shapePathCache;
    private readonly StatusAuraColor[] _statusAuraBuffer;
    private readonly PointF[] _bossCrownPoints;
    private readonly PointF[][] _shatterShieldShards;
    private static readonly Color UruzAuraCoreColor = Color.FromArgb(255, 186, 84);
    private static readonly Color UruzAuraGlowColor = Color.FromArgb(255, 196, 96);
    private static readonly Color PerthroAuraCoreColor = PerthroTuning.MarkOutlineColor;
    private static readonly Color PerthroAuraGlowColor = Color.FromArgb(255, 126, 240);
    private static readonly Color MannazAuraCoreColor = MannazTuning.StormAuraCoreColor;
    private static readonly Color MannazAuraGlowColor = MannazTuning.StormAuraGlowColor;
    private static readonly Color FehuAuraCoreColor = FehuTuning.BountyAuraCoreColor;
    private static readonly Color FehuAuraGlowColor = FehuTuning.BountyAuraGlowColor;

    public EnemyView()
    {
        _font = FontLibrary.Create(12f, FontStyle.Bold);
        _healthBadgeFonts = new Dictionary<int, Font>();
        _healthBadgeMeasureCache = new Dictionary<HealthBadgeMeasureKey, SizeF>();
        _shapePathCache = new Dictionary<ShapeCacheKey, GraphicsPath>();
        _statusAuraBuffer = new StatusAuraColor[4];
        _bossCrownPoints = new PointF[9];
        _shatterShieldShards =
        [
            new PointF[4],
            new PointF[4],
            new PointF[4]
        ];
        _textFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            FormatFlags = StringFormatFlags.NoWrap
        };
        _highlightBrush = new SolidBrush(Color.FromArgb(64, 255, 240, 225));
        _badgeBrush = new SolidBrush(Color.FromArgb(176, 26, 16, 18));
        _textBrush = new SolidBrush(Color.White);
        _innerPen = new Pen(Color.FromArgb(120, 255, 224, 196), 1f);
        _badgePen = new Pen(Color.FromArgb(140, 244, 213, 165), 1f);
        _slowedOutlinePen = new Pen(Color.FromArgb(220, 88, 196, 255), 2.4f);
        _laguzSlowedOutlinePen = new Pen(Color.FromArgb(228, LaguzTuning.OrbCoreColor), 2.4f);
        _shatterShieldBrush = new SolidBrush(Color.FromArgb(220, 239, 187, 18));
        _shatterShieldHighlightBrush = new SolidBrush(Color.FromArgb(110, 255, 231, 123));
        _shatterShieldPen = new Pen(Color.FromArgb(236, 166, 120, 10), 1.4f)
        {
            LineJoin = LineJoin.Round
        };
        _bossGlowPen = new Pen(Color.FromArgb(110, 255, 214, 84), 4.6f)
        {
            LineJoin = LineJoin.Round
        };
        _bossCorePen = new Pen(Color.FromArgb(236, 255, 233, 156), 2.2f)
        {
            LineJoin = LineJoin.Round
        };
        _bossCrownBrush = new SolidBrush(Color.FromArgb(224, 255, 211, 72));
        _bossCrownPen = new Pen(Color.FromArgb(246, 255, 242, 196), 1.2f)
        {
            LineJoin = LineJoin.Round
        };
        _auraRangeFillBrush = new SolidBrush(Color.Transparent);
        _auraRangeGlowPen = new Pen(Color.Transparent, 6f);
        _auraRangeCorePen = new Pen(Color.Transparent, 1.8f);
        _auraEnemyGlowPen = new Pen(Color.Transparent, 5.2f)
        {
            LineJoin = LineJoin.Round
        };
        _auraEnemyCorePen = new Pen(Color.Transparent, 2.4f)
        {
            LineJoin = LineJoin.Round
        };
        _segmentedAuraGlowPen = new Pen(Color.Transparent, 7.2f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        _segmentedAuraCorePen = new Pen(Color.Transparent, 2.4f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        _ingwazEffectTexture = TryLoadIngwazEffectTexture();
        _palettes = new Dictionary<EnemyType, EnemyPalette>
        {
            { EnemyType.Normal, new EnemyPalette(new SolidBrush(Color.FromArgb(188, 116, 72)), new SolidBrush(Color.FromArgb(222, 150, 52, 60)), new Pen(Color.FromArgb(210, 242, 198, 142), 1.6f)) },
            { EnemyType.Fast, new EnemyPalette(new SolidBrush(Color.FromArgb(194, 114, 140, 72)), new SolidBrush(Color.FromArgb(230, 198, 142, 58)), new Pen(Color.FromArgb(220, 248, 216, 146), 1.6f)) },
            { EnemyType.Slow, new EnemyPalette(new SolidBrush(Color.FromArgb(184, 64, 88, 130)), new SolidBrush(Color.FromArgb(226, 86, 122, 176)), new Pen(Color.FromArgb(214, 180, 224, 255), 1.6f)) },
            { EnemyType.Regenerator, new EnemyPalette(new SolidBrush(Color.FromArgb(198, 150, 76, 144)), new SolidBrush(Color.FromArgb(234, 236, 112, 202)), new Pen(Color.FromArgb(220, 255, 220, 246), 1.6f)) },
            { EnemyType.Teleporter, new EnemyPalette(new SolidBrush(Color.FromArgb(190, 72, 60, 152)), new SolidBrush(Color.FromArgb(234, 138, 112, 232)), new Pen(Color.FromArgb(220, 226, 212, 255), 1.6f)) },
            { EnemyType.Aura, new EnemyPalette(new SolidBrush(Color.FromArgb(182, 86, 106, 126)), new SolidBrush(Color.FromArgb(226, 144, 166, 188)), new Pen(Color.FromArgb(214, 218, 234, 246), 1.6f)) },
            { EnemyType.Cluster, new EnemyPalette(new SolidBrush(Color.FromArgb(186, 92, 96, 104)), new SolidBrush(Color.FromArgb(224, 126, 132, 142)), new Pen(Color.FromArgb(210, 220, 226, 232), 1.6f)) },
            { EnemyType.ClusterShard, new EnemyPalette(new SolidBrush(Color.FromArgb(188, 116, 72)), new SolidBrush(Color.FromArgb(222, 150, 52, 60)), new Pen(Color.FromArgb(210, 242, 198, 142), 1.6f)) }
        };
        _auraPalettes = new Dictionary<EnemyAuraType, EnemyPalette>
        {
            { EnemyAuraType.Regeneration, new EnemyPalette(new SolidBrush(Color.FromArgb(186, 52, 138, 76)), new SolidBrush(Color.FromArgb(230, 102, 214, 126)), new Pen(Color.FromArgb(220, 208, 255, 214), 1.6f)) },
            { EnemyAuraType.Speed, new EnemyPalette(new SolidBrush(Color.FromArgb(190, 146, 118, 34)), new SolidBrush(Color.FromArgb(232, 232, 204, 86)), new Pen(Color.FromArgb(220, 255, 240, 196), 1.6f)) },
            { EnemyAuraType.Immunity, new EnemyPalette(new SolidBrush(Color.FromArgb(188, 58, 122, 150)), new SolidBrush(Color.FromArgb(230, 108, 202, 244)), new Pen(Color.FromArgb(220, 214, 248, 255), 1.6f)) }
        };
    }

    public void Draw(Graphics graphics, EnemyEntity enemy, bool isUruzMarked = false)
    {
        DrawBodyLayer(graphics, enemy, isUruzMarked);
        DrawBurnOverlayLayer(graphics, enemy);
        DrawHealthBadgeLayer(graphics, enemy);
    }

    public void DrawBodyLayer(Graphics graphics, EnemyEntity enemy, bool isUruzMarked = false)
    {
        var scale = enemy.PresentationScale;
        if (scale <= 0f)
        {
            return;
        }

        var scaledRadius = enemy.Data.Radius * scale;
        var diameter = scaledRadius * 2f;
        if (diameter < MinimumRenderableDiameter)
        {
            return;
        }

        var drawX = enemy.Transform.Position.X - scaledRadius;
        var drawY = enemy.Transform.Position.Y - scaledRadius;
        var bodyBounds = new RectangleF(drawX, drawY, diameter, diameter);

        DrawBody(graphics, enemy, bodyBounds, isUruzMarked);
    }

    public void DrawBurnOverlayLayer(Graphics graphics, EnemyEntity enemy)
    {
        if (!TryGetBodyBounds(enemy, out _))
        {
            return;
        }

        DrawIngwazBurningEffect(graphics, enemy);
    }

    public void DrawHealthBadgeLayer(Graphics graphics, EnemyEntity enemy)
    {
        if (!TryGetBodyBounds(enemy, out var bodyBounds))
        {
            return;
        }

        DrawHealthBadge(graphics, enemy, bodyBounds);
    }

    public void Dispose()
    {
        _font.Dispose();
        _textFormat.Dispose();
        _highlightBrush.Dispose();
        _badgeBrush.Dispose();
        _textBrush.Dispose();
        _innerPen.Dispose();
        _badgePen.Dispose();
        _slowedOutlinePen.Dispose();
        _laguzSlowedOutlinePen.Dispose();
        _shatterShieldBrush.Dispose();
        _shatterShieldHighlightBrush.Dispose();
        _shatterShieldPen.Dispose();
        _bossGlowPen.Dispose();
        _bossCorePen.Dispose();
        _bossCrownBrush.Dispose();
        _bossCrownPen.Dispose();
        _auraRangeFillBrush.Dispose();
        _auraRangeGlowPen.Dispose();
        _auraRangeCorePen.Dispose();
        _auraEnemyGlowPen.Dispose();
        _auraEnemyCorePen.Dispose();
        _segmentedAuraGlowPen.Dispose();
        _segmentedAuraCorePen.Dispose();
        _ingwazEffectTexture?.Dispose();
        foreach (var badgeFont in _healthBadgeFonts.Values)
        {
            badgeFont.Dispose();
        }

        foreach (var shapePath in _shapePathCache.Values)
        {
            shapePath.Dispose();
        }

        foreach (var palette in _palettes.Values)
        {
            palette.Dispose();
        }

        foreach (var palette in _auraPalettes.Values)
        {
            palette.Dispose();
        }
    }

    private void DrawBody(Graphics graphics, EnemyEntity enemy, RectangleF bodyBounds, bool isUruzMarked)
    {
        DrawAuraRange(graphics, enemy);

        var palette = GetPalette(enemy);
        FillShape(graphics, palette.OuterBrush, bodyBounds, enemy.Data.Config.Shape);

        var innerBounds = Inflate(bodyBounds, -4f, -4f);
        FillShape(graphics, palette.CoreBrush, innerBounds, enemy.Data.Config.Shape);
        DrawShape(graphics, palette.BorderPen, bodyBounds, enemy.Data.Config.Shape);

        var ringBounds = Inflate(bodyBounds, -2.5f, -2.5f);
        DrawShape(graphics, _innerPen, ringBounds, enemy.Data.Config.Shape);

        var highlightBounds = GetHighlightBounds(bodyBounds, enemy.Data.Config.Shape);
        FillShape(
            graphics,
            _highlightBrush,
            highlightBounds,
            EnemyShape.Circle);

        DrawAuraEnemyGlow(graphics, enemy, bodyBounds);
        DrawClusterCore(graphics, enemy, bodyBounds);
        DrawBossAccents(graphics, enemy, bodyBounds);

        if (enemy.StatusEffects.IsIsaSlowed && enemy.StatusEffects.IsLaguzSlowed)
        {
            DrawShape(graphics, _laguzSlowedOutlinePen, Inflate(bodyBounds, 6f, 6f), enemy.Data.Config.Shape);
            DrawShape(graphics, _slowedOutlinePen, Inflate(bodyBounds, 3f, 3f), enemy.Data.Config.Shape);
        }
        else if (enemy.StatusEffects.IsLaguzSlowed)
        {
            var auraBounds = Inflate(bodyBounds, 4f, 4f);
            DrawShape(graphics, _laguzSlowedOutlinePen, auraBounds, enemy.Data.Config.Shape);
        }
        else if (enemy.StatusEffects.IsIsaSlowed)
        {
            var auraBounds = Inflate(bodyBounds, 4f, 4f);
            DrawShape(graphics, _slowedOutlinePen, auraBounds, enemy.Data.Config.Shape);
        }

        DrawSegmentedStatusAuras(graphics, enemy, bodyBounds, isUruzMarked);

        DrawShatterShield(graphics, enemy, bodyBounds);
    }

    private void DrawHealthBadge(Graphics graphics, EnemyEntity enemy, RectangleF bodyBounds)
    {
        if (bodyBounds.Width <= 8f || bodyBounds.Height <= 6f)
        {
            return;
        }

        var hpText = ((int)MathF.Ceiling(enemy.Data.Health)).ToString();
        var maxBadgeWidth = GetMaxHealthBadgeWidth(enemy, bodyBounds);
        var maxTextWidth = Math.Max(1f, maxBadgeWidth - (HealthBadgeHorizontalPadding * 2f));
        var maxFontSize = Math.Clamp(bodyBounds.Height * 0.3f, 7f, HealthBadgeFontSizeCap);
        var minFontSize = Math.Min(maxFontSize, Math.Clamp(bodyBounds.Height * 0.2f, 5.5f, 9.5f));
        var fontSize = ResolveHealthBadgeFontSize(graphics, hpText, maxFontSize, minFontSize, maxTextWidth);
        var badgeFontKey = QuantizeHealthBadgeFontSize(fontSize);
        var badgeFont = GetHealthBadgeFontByKey(badgeFontKey);
        var textSize = MeasureHealthBadgeText(graphics, hpText, badgeFont, badgeFontKey);
        var badgeWidth = Math.Min(maxBadgeWidth, textSize.Width + (HealthBadgeHorizontalPadding * 2f));
        var badgeHeight = Math.Max(HealthBadgeMinHeight, textSize.Height + (HealthBadgeVerticalPadding * 2f));
        var badgeBounds = new RectangleF(
            bodyBounds.X + ((bodyBounds.Width - badgeWidth) * 0.5f),
            bodyBounds.Y + ((bodyBounds.Height - badgeHeight) * 0.5f),
            badgeWidth,
            badgeHeight);

        graphics.FillRectangle(_badgeBrush, badgeBounds.X, badgeBounds.Y, badgeBounds.Width, badgeBounds.Height);
        graphics.DrawRectangle(_badgePen, badgeBounds.X, badgeBounds.Y, badgeBounds.Width, badgeBounds.Height);
        graphics.DrawString(hpText, badgeFont, _textBrush, badgeBounds, _textFormat);
    }

    private static float GetMaxHealthBadgeWidth(EnemyEntity enemy, RectangleF bodyBounds)
    {
        if (enemy.Data.Type == EnemyType.Teleporter)
        {
            return Math.Max(bodyBounds.Width * 0.82f, bodyBounds.Width - 6f);
        }

        if (enemy.Data.Config.Shape == EnemyShape.Triangle)
        {
            return Math.Max(bodyBounds.Width * 0.84f, bodyBounds.Width - 5f);
        }

        return Math.Max(
            bodyBounds.Width * HealthBadgeMaxWidthMultiplier,
            bodyBounds.Width + HealthBadgeMaxOverflow);
    }

    private static bool TryGetBodyBounds(EnemyEntity enemy, out RectangleF bodyBounds)
    {
        var scale = enemy.PresentationScale;
        if (scale <= 0f)
        {
            bodyBounds = default;
            return false;
        }

        var scaledRadius = enemy.Data.Radius * scale;
        var diameter = scaledRadius * 2f;
        if (diameter < MinimumRenderableDiameter)
        {
            bodyBounds = default;
            return false;
        }

        bodyBounds = new RectangleF(
            enemy.Transform.Position.X - scaledRadius,
            enemy.Transform.Position.Y - scaledRadius,
            diameter,
            diameter);
        return true;
    }

    private static RectangleF Inflate(RectangleF rectangle, float amountX, float amountY)
    {
        return new RectangleF(
            rectangle.X - amountX,
            rectangle.Y - amountY,
            rectangle.Width + (amountX * 2f),
            rectangle.Height + (amountY * 2f));
    }

    private static RectangleF GetHighlightBounds(RectangleF bodyBounds, EnemyShape shape)
    {
        if (shape == EnemyShape.Triangle)
        {
            return new RectangleF(
                bodyBounds.X + (bodyBounds.Width * 0.36f),
                bodyBounds.Y + (bodyBounds.Height * 0.24f),
                bodyBounds.Width * 0.18f,
                bodyBounds.Height * 0.12f);
        }

        if (shape == EnemyShape.Star)
        {
            return new RectangleF(
                bodyBounds.X + (bodyBounds.Width * 0.31f),
                bodyBounds.Y + (bodyBounds.Height * 0.22f),
                bodyBounds.Width * 0.17f,
                bodyBounds.Height * 0.13f);
        }

        if (shape == EnemyShape.Diamond)
        {
            return new RectangleF(
                bodyBounds.X + (bodyBounds.Width * 0.32f),
                bodyBounds.Y + (bodyBounds.Height * 0.22f),
                bodyBounds.Width * 0.2f,
                bodyBounds.Height * 0.13f);
        }

        return new RectangleF(
            bodyBounds.X + (bodyBounds.Width * 0.18f),
            bodyBounds.Y + (bodyBounds.Height * 0.12f),
            bodyBounds.Width * 0.28f,
            bodyBounds.Height * 0.18f);
    }

    private void FillShape(Graphics graphics, Brush brush, RectangleF bounds, EnemyShape shape)
    {
        if (bounds.Width <= 0.01f || bounds.Height <= 0.01f)
        {
            return;
        }

        if (shape == EnemyShape.Circle)
        {
            graphics.FillEllipse(brush, bounds);
            return;
        }

        var path = GetShapePath(shape, bounds.Width, bounds.Height);
        if (path.PointCount == 0)
        {
            return;
        }

        var state = graphics.Save();
        graphics.TranslateTransform(bounds.X + (bounds.Width * 0.5f), bounds.Y + (bounds.Height * 0.5f));
        graphics.FillPath(brush, path);
        graphics.Restore(state);
    }

    private void DrawShape(Graphics graphics, Pen pen, RectangleF bounds, EnemyShape shape)
    {
        if (bounds.Width <= 0.01f || bounds.Height <= 0.01f)
        {
            return;
        }

        if (shape == EnemyShape.Circle)
        {
            graphics.DrawEllipse(pen, bounds);
            return;
        }

        var path = GetShapePath(shape, bounds.Width, bounds.Height);
        if (path.PointCount == 0)
        {
            return;
        }

        var state = graphics.Save();
        graphics.TranslateTransform(bounds.X + (bounds.Width * 0.5f), bounds.Y + (bounds.Height * 0.5f));
        graphics.DrawPath(pen, path);
        graphics.Restore(state);
    }

    private static GraphicsPath CreateRoundedRectanglePath(RectangleF bounds, float radius)
    {
        if (bounds.Width <= 0.01f || bounds.Height <= 0.01f)
        {
            var emptyPath = new GraphicsPath();
            return emptyPath;
        }

        var clampedRadius = MathF.Min(radius, MathF.Min(bounds.Width, bounds.Height) * 0.5f);
        var diameter = clampedRadius * 2f;
        var path = new GraphicsPath();

        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }

    private static GraphicsPath CreateRoundedTrianglePath(RectangleF bounds)
    {
        if (bounds.Width <= 0.01f || bounds.Height <= 0.01f)
        {
            return new GraphicsPath();
        }

        var points = new[]
        {
            new PointF(bounds.X + (bounds.Width * 0.5f), bounds.Y + (bounds.Height * 0.08f)),
            new PointF(bounds.Right - (bounds.Width * 0.12f), bounds.Bottom - (bounds.Height * 0.12f)),
            new PointF(bounds.X + (bounds.Width * 0.12f), bounds.Bottom - (bounds.Height * 0.12f))
        };

        var path = new GraphicsPath();
        path.AddClosedCurve(points, TriangleCurveTension);
        return path;
    }

    private static GraphicsPath CreateStarPath(RectangleF bounds)
    {
        if (bounds.Width <= 0.01f || bounds.Height <= 0.01f)
        {
            return new GraphicsPath();
        }

        var centerX = bounds.X + (bounds.Width * 0.5f);
        var centerY = bounds.Y + (bounds.Height * 0.5f);
        var outerRadius = MathF.Min(bounds.Width, bounds.Height) * 0.5f;
        var innerRadius = outerRadius * StarInnerRadiusRatio;
        var points = new PointF[10];

        for (var i = 0; i < points.Length; i++)
        {
            var angle = (-MathF.PI / 2f) + (i * MathF.PI / 5f);
            var radius = i % 2 == 0 ? outerRadius : innerRadius;
            points[i] = new PointF(
                centerX + (MathF.Cos(angle) * radius),
                centerY + (MathF.Sin(angle) * radius));
        }

        var path = new GraphicsPath();
        path.AddPolygon(points);
        return path;
    }

    private static GraphicsPath CreateDiamondPath(RectangleF bounds)
    {
        if (bounds.Width <= 0.01f || bounds.Height <= 0.01f)
        {
            return new GraphicsPath();
        }

        var points = new[]
        {
            new PointF(bounds.X + (bounds.Width * 0.5f), bounds.Y + (bounds.Height * DiamondInsetRatio)),
            new PointF(bounds.Right - (bounds.Width * DiamondInsetRatio), bounds.Y + (bounds.Height * 0.5f)),
            new PointF(bounds.X + (bounds.Width * 0.5f), bounds.Bottom - (bounds.Height * DiamondInsetRatio)),
            new PointF(bounds.X + (bounds.Width * DiamondInsetRatio), bounds.Y + (bounds.Height * 0.5f))
        };

        var path = new GraphicsPath();
        path.AddPolygon(points);
        return path;
    }

    private float ResolveHealthBadgeFontSize(
        Graphics graphics,
        string hpText,
        float maxFontSize,
        float minFontSize,
        float maxTextWidth)
    {
        var maxFontKey = QuantizeHealthBadgeFontSize(maxFontSize);
        var maxFont = GetHealthBadgeFontByKey(maxFontKey);
        var maxSize = MeasureHealthBadgeText(graphics, hpText, maxFont, maxFontKey);
        if (maxSize.Width <= maxTextWidth)
        {
            return maxFontSize;
        }

        var scaledFontSize = maxFontSize * (maxTextWidth / Math.Max(1f, maxSize.Width));
        return Math.Clamp(scaledFontSize, minFontSize, maxFontSize);
    }

    private Font GetHealthBadgeFont(float size)
    {
        return GetHealthBadgeFontByKey(QuantizeHealthBadgeFontSize(size));
    }

    private Font GetHealthBadgeFontByKey(int quantizedKey)
    {
        if (_healthBadgeFonts.TryGetValue(quantizedKey, out var font))
        {
            return font;
        }

        font = FontLibrary.CreateNumeric(quantizedKey / 2f, FontStyle.Bold);
        _healthBadgeFonts.Add(quantizedKey, font);
        return font;
    }

    private SizeF MeasureHealthBadgeText(Graphics graphics, string text, Font font, int fontKey)
    {
        var key = new HealthBadgeMeasureKey(text, fontKey);
        if (_healthBadgeMeasureCache.TryGetValue(key, out var size))
        {
            return size;
        }

        if (_healthBadgeMeasureCache.Count > 768)
        {
            _healthBadgeMeasureCache.Clear();
        }

        size = graphics.MeasureString(text, font, int.MaxValue, _textFormat);
        _healthBadgeMeasureCache.Add(key, size);
        return size;
    }

    private static int QuantizeHealthBadgeFontSize(float size)
    {
        return Math.Max(1, (int)MathF.Round(size * 2f));
    }

    private GraphicsPath GetShapePath(EnemyShape shape, float width, float height)
    {
        var widthKey = Math.Max(1, (int)MathF.Round(width * 2f));
        var heightKey = Math.Max(1, (int)MathF.Round(height * 2f));
        var cacheKey = new ShapeCacheKey(shape, widthKey, heightKey);
        if (_shapePathCache.TryGetValue(cacheKey, out var cachedPath))
        {
            return cachedPath;
        }

        var quantizedWidth = widthKey / 2f;
        var quantizedHeight = heightKey / 2f;
        var localBounds = new RectangleF(
            -(quantizedWidth * 0.5f),
            -(quantizedHeight * 0.5f),
            quantizedWidth,
            quantizedHeight);

        var path = shape switch
        {
            EnemyShape.Square => CreateRoundedRectanglePath(localBounds, SquareCornerRadius),
            EnemyShape.Triangle => CreateRoundedTrianglePath(localBounds),
            EnemyShape.Star => CreateStarPath(localBounds),
            EnemyShape.Diamond => CreateDiamondPath(localBounds),
            _ => new GraphicsPath()
        };

        _shapePathCache.Add(cacheKey, path);
        return path;
    }

    private EnemyPalette GetPalette(EnemyEntity enemy)
    {
        if (enemy.Data.Type == EnemyType.Aura && _auraPalettes.TryGetValue(enemy.AuraType, out var auraPalette))
        {
            return auraPalette;
        }

        return _palettes[enemy.Data.Type];
    }

    private void DrawAuraEnemyGlow(Graphics graphics, EnemyEntity enemy, RectangleF bodyBounds)
    {
        if (enemy.Data.Type != EnemyType.Aura)
        {
            return;
        }

        var (coreColor, glowColor) = enemy.AuraType switch
        {
            EnemyAuraType.Regeneration => (AuraEnemyTuning.RegenerationCoreColor, AuraEnemyTuning.RegenerationGlowColor),
            EnemyAuraType.Speed => (AuraEnemyTuning.SpeedCoreColor, AuraEnemyTuning.SpeedGlowColor),
            EnemyAuraType.Immunity => (AuraEnemyTuning.ImmunityCoreColor, AuraEnemyTuning.ImmunityGlowColor),
            _ => (Color.Transparent, Color.Transparent)
        };

        _auraEnemyGlowPen.Color = Color.FromArgb(108, glowColor);
        _auraEnemyCorePen.Color = Color.FromArgb(184, coreColor);
        DrawShape(graphics, _auraEnemyGlowPen, Inflate(bodyBounds, 6f, 6f), enemy.Data.Config.Shape);
        DrawShape(graphics, _auraEnemyCorePen, Inflate(bodyBounds, 2.8f, 2.8f), enemy.Data.Config.Shape);
    }

    private void DrawAuraRange(Graphics graphics, EnemyEntity enemy)
    {
        if (enemy.Data.Type != EnemyType.Aura)
        {
            return;
        }

        var (coreColor, glowColor) = enemy.AuraType switch
        {
            EnemyAuraType.Regeneration => (AuraEnemyTuning.RegenerationCoreColor, AuraEnemyTuning.RegenerationGlowColor),
            EnemyAuraType.Speed => (AuraEnemyTuning.SpeedCoreColor, AuraEnemyTuning.SpeedGlowColor),
            EnemyAuraType.Immunity => (AuraEnemyTuning.ImmunityCoreColor, AuraEnemyTuning.ImmunityGlowColor),
            _ => (Color.Transparent, Color.Transparent)
        };

        if (coreColor.A == 0 && glowColor.A == 0)
        {
            return;
        }

        var radius = AuraEnemyTuning.AuraRadius;
        var bounds = new RectangleF(
            enemy.Transform.Position.X - radius,
            enemy.Transform.Position.Y - radius,
            radius * 2f,
            radius * 2f);

        _auraRangeFillBrush.Color = Color.FromArgb(28, coreColor);
        _auraRangeGlowPen.Color = Color.FromArgb(44, glowColor);
        _auraRangeCorePen.Color = Color.FromArgb(116, coreColor);
        graphics.FillEllipse(_auraRangeFillBrush, bounds);
        graphics.DrawEllipse(_auraRangeGlowPen, bounds);
        graphics.DrawEllipse(_auraRangeCorePen, bounds);
    }

    private void DrawClusterCore(Graphics graphics, EnemyEntity enemy, RectangleF bodyBounds)
    {
        if (enemy.Data.Type != EnemyType.Cluster)
        {
            return;
        }

        var shardPalette = _palettes[EnemyType.ClusterShard];
        var coreDiameter = bodyBounds.Width * 0.2665f;
        var gapX = MathF.Max(2f, (bodyBounds.Width - (coreDiameter * 2f)) / 3f);
        var gapY = MathF.Max(2f, (bodyBounds.Height - (coreDiameter * 2f)) / 3f);
        var startX = bodyBounds.X + gapX;
        var startY = bodyBounds.Y + gapY;

        for (var row = 0; row < 2; row++)
        {
            for (var column = 0; column < 2; column++)
            {
                var coreBounds = new RectangleF(
                    startX + (column * (coreDiameter + gapX)),
                    startY + (row * (coreDiameter + gapY)),
                    coreDiameter,
                    coreDiameter);
                graphics.FillEllipse(shardPalette.OuterBrush, coreBounds);
                var innerBounds = Inflate(coreBounds, -1.5f, -1.5f);
                graphics.FillEllipse(shardPalette.CoreBrush, innerBounds);
                graphics.DrawEllipse(shardPalette.BorderPen, coreBounds);
            }
        }
    }

    private void DrawBossAccents(Graphics graphics, EnemyEntity enemy, RectangleF bodyBounds)
    {
        if (!enemy.Data.IsBoss)
        {
            return;
        }

        var glowBounds = Inflate(bodyBounds, BossAccentOutlinePadding, BossAccentOutlinePadding);
        var coreBounds = Inflate(bodyBounds, BossAccentInnerPadding, BossAccentInnerPadding);
        DrawShape(graphics, _bossGlowPen, glowBounds, enemy.Data.Config.Shape);
        DrawShape(graphics, _bossCorePen, coreBounds, enemy.Data.Config.Shape);

        var crownWidth = MathF.Max(14f, bodyBounds.Width * 0.4f);
        var crownHeight = MathF.Max(9f, bodyBounds.Height * 0.19f);
        var crownCenterX = bodyBounds.X + (bodyBounds.Width * 0.5f);
        var crownVerticalOffset = enemy.Data.Type switch
        {
            EnemyType.Regenerator => bodyBounds.Height * 0.055f,
            EnemyType.Teleporter => bodyBounds.Height * 0.05f,
            EnemyType.Aura => bodyBounds.Height * 0.048f,
            _ => enemy.Data.Config.Shape switch
            {
                EnemyShape.Triangle => bodyBounds.Height * 0.035f,
                EnemyShape.Star => bodyBounds.Height * 0.03f,
                EnemyShape.Diamond => bodyBounds.Height * 0.028f,
                _ => 0f
            }
        };
        var crownBaseY = bodyBounds.Y - MathF.Max(4f, bodyBounds.Height * 0.08f) + crownVerticalOffset;
        _bossCrownPoints[0] = new PointF(crownCenterX - (crownWidth * 0.5f), crownBaseY);
        _bossCrownPoints[1] = new PointF(crownCenterX - (crownWidth * 0.32f), crownBaseY - (crownHeight * 0.68f));
        _bossCrownPoints[2] = new PointF(crownCenterX - (crownWidth * 0.1f), crownBaseY);
        _bossCrownPoints[3] = new PointF(crownCenterX, crownBaseY - crownHeight);
        _bossCrownPoints[4] = new PointF(crownCenterX + (crownWidth * 0.1f), crownBaseY);
        _bossCrownPoints[5] = new PointF(crownCenterX + (crownWidth * 0.32f), crownBaseY - (crownHeight * 0.68f));
        _bossCrownPoints[6] = new PointF(crownCenterX + (crownWidth * 0.5f), crownBaseY);
        _bossCrownPoints[7] = new PointF(crownCenterX + (crownWidth * 0.5f), crownBaseY + (crownHeight * 0.34f));
        _bossCrownPoints[8] = new PointF(crownCenterX - (crownWidth * 0.5f), crownBaseY + (crownHeight * 0.34f));

        graphics.FillPolygon(_bossCrownBrush, _bossCrownPoints);
        graphics.DrawPolygon(_bossCrownPen, _bossCrownPoints);
    }

    private void DrawShatterShield(Graphics graphics, EnemyEntity enemy, RectangleF bodyBounds)
    {
        var shatterStackCount = enemy.StatusEffects.ShatterStackCount;
        if (shatterStackCount <= 0)
        {
            return;
        }

        var shieldWidth = bodyBounds.Width * 0.78f;
        var shieldHeight = bodyBounds.Height * 0.72f;
        var centerX = bodyBounds.X + (bodyBounds.Width * 0.5f);
        var centerY = bodyBounds.Y + (bodyBounds.Height * 0.42f);
        var left = centerX - (shieldWidth * 0.5f);
        var top = centerY - (shieldHeight * 0.5f);
        var right = centerX + (shieldWidth * 0.5f);
        var bottom = centerY + (shieldHeight * 0.5f);
        var upperMidY = top + (shieldHeight * 0.28f);
        var lowerMidY = top + (shieldHeight * 0.66f);
        var bottomTipY = bottom + (shieldHeight * 0.12f);

        var shards = _shatterShieldShards;
        shards[0][0] = new PointF(left + (shieldWidth * 0.09f), upperMidY);
        shards[0][1] = new PointF(centerX - (shieldWidth * 0.12f), top + (shieldHeight * 0.02f));
        shards[0][2] = new PointF(centerX - (shieldWidth * 0.03f), lowerMidY);
        shards[0][3] = new PointF(left + (shieldWidth * 0.18f), bottom);
        shards[1][0] = new PointF(centerX - (shieldWidth * 0.09f), top + (shieldHeight * 0.06f));
        shards[1][1] = new PointF(centerX + (shieldWidth * 0.11f), top + (shieldHeight * 0.14f));
        shards[1][2] = new PointF(centerX + (shieldWidth * 0.04f), lowerMidY);
        shards[1][3] = new PointF(centerX - (shieldWidth * 0.05f), bottomTipY);
        shards[2][0] = new PointF(centerX + (shieldWidth * 0.16f), top + (shieldHeight * 0.08f));
        shards[2][1] = new PointF(right - (shieldWidth * 0.07f), upperMidY + (shieldHeight * 0.04f));
        shards[2][2] = new PointF(right - (shieldWidth * 0.14f), bottom);
        shards[2][3] = new PointF(centerX + (shieldWidth * 0.05f), lowerMidY);

        var visibleShardCount = Math.Min(shatterStackCount, shards.Length);
        for (var i = 0; i < visibleShardCount; i++)
        {
            graphics.FillPolygon(_shatterShieldBrush, shards[i]);
            graphics.DrawPolygon(_shatterShieldPen, shards[i]);
        }

        var highlightWidth = shieldWidth * 0.16f;
        var highlightHeight = shieldHeight * 0.08f;
        for (var i = 0; i < visibleShardCount; i++)
        {
            var shardBounds = GetBounds(shards[i]);
            graphics.FillEllipse(
                _shatterShieldHighlightBrush,
                shardBounds.X + (shardBounds.Width * 0.15f),
                shardBounds.Y + (shardBounds.Height * 0.1f),
                highlightWidth,
                highlightHeight);
        }
    }

    private static RectangleF GetBounds(PointF[] points)
    {
        if (points.Length == 0)
        {
            return RectangleF.Empty;
        }

        var minX = points[0].X;
        var minY = points[0].Y;
        var maxX = points[0].X;
        var maxY = points[0].Y;

        for (var i = 1; i < points.Length; i++)
        {
            var point = points[i];
            minX = Math.Min(minX, point.X);
            minY = Math.Min(minY, point.Y);
            maxX = Math.Max(maxX, point.X);
            maxY = Math.Max(maxY, point.Y);
        }

        return new RectangleF(minX, minY, maxX - minX, maxY - minY);
    }

    private void DrawSegmentedStatusAuras(Graphics graphics, EnemyEntity enemy, RectangleF bodyBounds, bool isUruzMarked)
    {
        var auras = _statusAuraBuffer;
        var auraCount = 0;

        if (isUruzMarked)
        {
            auras[auraCount++] = new StatusAuraColor(UruzAuraCoreColor, UruzAuraGlowColor);
        }

        if (enemy.StatusEffects.IsPerthroMarked)
        {
            auras[auraCount++] = new StatusAuraColor(PerthroAuraCoreColor, PerthroAuraGlowColor);
        }

        if (enemy.StatusEffects.IsMannazStormMarked)
        {
            auras[auraCount++] = new StatusAuraColor(MannazAuraCoreColor, MannazAuraGlowColor);
        }

        if (enemy.StatusEffects.IsFehuMarked)
        {
            auras[auraCount++] = new StatusAuraColor(FehuAuraCoreColor, FehuAuraGlowColor);
        }

        if (auraCount <= 0)
        {
            return;
        }

        var auraBounds = Inflate(bodyBounds, 6.5f, 6.5f);
        var sweepPerAura = 360f / auraCount;
        var visibleSweep = MathF.Max(18f, sweepPerAura - 7f);
        var startAngleOffset = -90f;

        for (var i = 0; i < auraCount; i++)
        {
            var startAngle = startAngleOffset + (sweepPerAura * i) + ((sweepPerAura - visibleSweep) * 0.5f);
            _segmentedAuraGlowPen.Color = Color.FromArgb(86, auras[i].Glow);
            _segmentedAuraCorePen.Color = Color.FromArgb(224, auras[i].Core);
            graphics.DrawArc(_segmentedAuraGlowPen, auraBounds, startAngle, visibleSweep);
            graphics.DrawArc(_segmentedAuraCorePen, auraBounds, startAngle, visibleSweep);
        }
    }

    private void DrawIngwazBurningEffect(Graphics graphics, EnemyEntity enemy)
    {
        if (_ingwazEffectTexture == null || !enemy.StatusEffects.IsBurning)
        {
            return;
        }

        var sourceRectangle = new Rectangle(
            GetAnimationFrameIndex(IngwazTuning.EffectFrameCount, 0.06f) * IngwazTuning.EffectFrameSize,
            IngwazTuning.EffectRowIndex * IngwazTuning.EffectFrameSize,
            IngwazTuning.EffectFrameSize,
            IngwazTuning.EffectFrameSize);
        if (sourceRectangle.Right > _ingwazEffectTexture.Width || sourceRectangle.Bottom > _ingwazEffectTexture.Height)
        {
            return;
        }

        var scale = IngwazTuning.GetEffectScaleForStackCount(enemy.StatusEffects.BurnStackCount, enemy.Data.Radius);
        var drawWidth = sourceRectangle.Width * scale;
        var drawHeight = sourceRectangle.Height * scale;
        var destinationRectangle = new RectangleF(
            enemy.Transform.Position.X - (drawWidth * 0.5f),
            enemy.Transform.Position.Y - drawHeight + IngwazEffectBottomAnchorVerticalOffset,
            drawWidth,
            drawHeight);

        graphics.DrawImage(_ingwazEffectTexture, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);
    }

    private static int GetAnimationFrameIndex(int frameCount, float frameDurationSeconds)
    {
        if (frameCount <= 1)
        {
            return 0;
        }

        var totalElapsedSeconds = Environment.TickCount64 / 1000d;
        return (int)(totalElapsedSeconds / frameDurationSeconds) % frameCount;
    }

    private static Bitmap? TryLoadIngwazEffectTexture()
    {
        try
        {
            var texturePath = AssetResolver.ResolveFile("Effects", "SpriteSheets", "ingwaz-burn.png");
            using var stream = File.OpenRead(texturePath);
            using var image = Image.FromStream(stream);
            return new Bitmap(image);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        return null;
    }

    private sealed class EnemyPalette : IDisposable
    {
        public EnemyPalette(SolidBrush outerBrush, SolidBrush coreBrush, Pen borderPen)
        {
            OuterBrush = outerBrush;
            CoreBrush = coreBrush;
            BorderPen = borderPen;
        }

        public SolidBrush OuterBrush { get; }

        public SolidBrush CoreBrush { get; }

        public Pen BorderPen { get; }

        public void Dispose()
        {
            if (!ReferenceEquals(OuterBrush, CoreBrush))
            {
                CoreBrush.Dispose();
            }

            OuterBrush.Dispose();
            BorderPen.Dispose();
        }
    }

    private readonly record struct ShapeCacheKey(EnemyShape Shape, int WidthKey, int HeightKey);

    private readonly record struct HealthBadgeMeasureKey(string Text, int FontKey);

    private readonly record struct StatusAuraColor(Color Core, Color Glow);
}
