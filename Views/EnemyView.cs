using System.Drawing;
using System.Drawing.Drawing2D;
using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Views;

public sealed class EnemyView : IDisposable
{
    private const float SquareCornerRadius = 7f;
    private const float MinimumRenderableDiameter = 1.5f;
    private readonly Font _font;
    private readonly StringFormat _textFormat;
    private readonly SolidBrush _highlightBrush;
    private readonly SolidBrush _badgeBrush;
    private readonly SolidBrush _textBrush;
    private readonly Pen _innerPen;
    private readonly Pen _slowedOutlinePen;
    private readonly Pen _laguzSlowedOutlinePen;
    private readonly SolidBrush _uruzMarkGlowBrush;
    private readonly Pen _uruzMarkOutlinePen;
    private readonly SolidBrush _shatterShieldBrush;
    private readonly SolidBrush _shatterShieldHighlightBrush;
    private readonly Pen _badgePen;
    private readonly Pen _shatterShieldPen;
    private readonly Dictionary<EnemyType, EnemyPalette> _palettes;

    public EnemyView()
    {
        _font = FontLibrary.Create(12f, FontStyle.Bold);
        _textFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        _highlightBrush = new SolidBrush(Color.FromArgb(64, 255, 240, 225));
        _badgeBrush = new SolidBrush(Color.FromArgb(176, 26, 16, 18));
        _textBrush = new SolidBrush(Color.White);
        _innerPen = new Pen(Color.FromArgb(120, 255, 224, 196), 1f);
        _badgePen = new Pen(Color.FromArgb(140, 244, 213, 165), 1f);
        _slowedOutlinePen = new Pen(Color.FromArgb(220, 88, 196, 255), 2.4f);
        _laguzSlowedOutlinePen = new Pen(Color.FromArgb(228, LaguzTuning.OrbCoreColor), 2.4f);
        _uruzMarkGlowBrush = new SolidBrush(Color.FromArgb(58, 255, 196, 96));
        _uruzMarkOutlinePen = new Pen(Color.FromArgb(214, 255, 186, 84), 1.8f);
        _shatterShieldBrush = new SolidBrush(Color.FromArgb(220, 239, 187, 18));
        _shatterShieldHighlightBrush = new SolidBrush(Color.FromArgb(110, 255, 231, 123));
        _shatterShieldPen = new Pen(Color.FromArgb(236, 166, 120, 10), 1.4f)
        {
            LineJoin = LineJoin.Round
        };
        _palettes = new Dictionary<EnemyType, EnemyPalette>
        {
            { EnemyType.Normal, new EnemyPalette(new SolidBrush(Color.FromArgb(188, 116, 72)), new SolidBrush(Color.FromArgb(222, 150, 52, 60)), new Pen(Color.FromArgb(210, 242, 198, 142), 1.6f)) },
            { EnemyType.Fast, new EnemyPalette(new SolidBrush(Color.FromArgb(194, 114, 140, 72)), new SolidBrush(Color.FromArgb(230, 198, 142, 58)), new Pen(Color.FromArgb(220, 248, 216, 146), 1.6f)) },
            { EnemyType.Slow, new EnemyPalette(new SolidBrush(Color.FromArgb(184, 64, 88, 130)), new SolidBrush(Color.FromArgb(226, 86, 122, 176)), new Pen(Color.FromArgb(214, 180, 224, 255), 1.6f)) }
        };
    }

    public void Draw(Graphics graphics, EnemyEntity enemy, bool isUruzMarked = false)
    {
        var scale = enemy.SpawnScale;
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
        _uruzMarkGlowBrush.Dispose();
        _uruzMarkOutlinePen.Dispose();
        _shatterShieldBrush.Dispose();
        _shatterShieldHighlightBrush.Dispose();
        _shatterShieldPen.Dispose();
        foreach (var palette in _palettes.Values)
        {
            palette.Dispose();
        }
    }

    private void DrawBody(Graphics graphics, EnemyEntity enemy, RectangleF bodyBounds, bool isUruzMarked)
    {
        var palette = _palettes[enemy.Data.Type];
        if (isUruzMarked)
        {
            var glowBounds = Inflate(bodyBounds, 7f, 7f);
            FillShape(graphics, _uruzMarkGlowBrush, glowBounds, enemy.Data.Config.Shape);
        }

        FillShape(graphics, palette.OuterBrush, bodyBounds, enemy.Data.Config.Shape);

        var innerBounds = Inflate(bodyBounds, -4f, -4f);
        FillShape(graphics, palette.CoreBrush, innerBounds, enemy.Data.Config.Shape);
        DrawShape(graphics, palette.BorderPen, bodyBounds, enemy.Data.Config.Shape);

        var ringBounds = Inflate(bodyBounds, -2.5f, -2.5f);
        DrawShape(graphics, _innerPen, ringBounds, enemy.Data.Config.Shape);

        FillShape(
            graphics,
            _highlightBrush,
            bodyBounds.X + (bodyBounds.Width * 0.18f),
            bodyBounds.Y + (bodyBounds.Height * 0.12f),
            bodyBounds.Width * 0.28f,
            bodyBounds.Height * 0.18f,
            EnemyShape.Circle);

        if (isUruzMarked)
        {
            DrawShape(graphics, _uruzMarkOutlinePen, Inflate(bodyBounds, 3f, 3f), enemy.Data.Config.Shape);
        }

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

        DrawShatterShield(graphics, enemy, bodyBounds);
    }

    private void DrawHealthBadge(Graphics graphics, EnemyEntity enemy, RectangleF bodyBounds)
    {
        if (bodyBounds.Width <= 8f || bodyBounds.Height <= 6f)
        {
            return;
        }

        var badgeBounds = new RectangleF(
            bodyBounds.X + 4f,
            bodyBounds.Y + (bodyBounds.Height * 0.37f),
            bodyBounds.Width - 8f,
            16f);

        graphics.FillRectangle(_badgeBrush, badgeBounds.X, badgeBounds.Y, badgeBounds.Width, badgeBounds.Height);
        graphics.DrawRectangle(_badgePen, badgeBounds.X, badgeBounds.Y, badgeBounds.Width, badgeBounds.Height);

        var hpText = ((int)MathF.Ceiling(enemy.Data.Health)).ToString();
        graphics.DrawString(hpText, _font, _textBrush, badgeBounds, _textFormat);
    }

    private static RectangleF Inflate(RectangleF rectangle, float amountX, float amountY)
    {
        return new RectangleF(
            rectangle.X - amountX,
            rectangle.Y - amountY,
            rectangle.Width + (amountX * 2f),
            rectangle.Height + (amountY * 2f));
    }

    private static void FillShape(Graphics graphics, Brush brush, RectangleF bounds, EnemyShape shape)
    {
        if (bounds.Width <= 0.01f || bounds.Height <= 0.01f)
        {
            return;
        }

        if (shape == EnemyShape.Square)
        {
            using var path = CreateRoundedRectanglePath(bounds, SquareCornerRadius);
            graphics.FillPath(brush, path);
            return;
        }

        graphics.FillEllipse(brush, bounds);
    }

    private static void FillShape(Graphics graphics, Brush brush, float x, float y, float width, float height, EnemyShape shape)
    {
        FillShape(graphics, brush, new RectangleF(x, y, width, height), shape);
    }

    private static void DrawShape(Graphics graphics, Pen pen, RectangleF bounds, EnemyShape shape)
    {
        if (bounds.Width <= 0.01f || bounds.Height <= 0.01f)
        {
            return;
        }

        if (shape == EnemyShape.Square)
        {
            using var path = CreateRoundedRectanglePath(bounds, SquareCornerRadius);
            graphics.DrawPath(pen, path);
            return;
        }

        graphics.DrawEllipse(pen, bounds);
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

        PointF[][] shards =
        [
            [
                new PointF(left + (shieldWidth * 0.09f), upperMidY),
                new PointF(centerX - (shieldWidth * 0.12f), top + (shieldHeight * 0.02f)),
                new PointF(centerX - (shieldWidth * 0.03f), lowerMidY),
                new PointF(left + (shieldWidth * 0.18f), bottom)
            ],
            [
                new PointF(centerX - (shieldWidth * 0.09f), top + (shieldHeight * 0.06f)),
                new PointF(centerX + (shieldWidth * 0.11f), top + (shieldHeight * 0.14f)),
                new PointF(centerX + (shieldWidth * 0.04f), lowerMidY),
                new PointF(centerX - (shieldWidth * 0.05f), bottomTipY)
            ],
            [
                new PointF(centerX + (shieldWidth * 0.16f), top + (shieldHeight * 0.08f)),
                new PointF(right - (shieldWidth * 0.07f), upperMidY + (shieldHeight * 0.04f)),
                new PointF(right - (shieldWidth * 0.14f), bottom),
                new PointF(centerX + (shieldWidth * 0.05f), lowerMidY)
            ]
        ];

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
        var minX = points.Min(static point => point.X);
        var minY = points.Min(static point => point.Y);
        var maxX = points.Max(static point => point.X);
        var maxY = points.Max(static point => point.Y);
        return new RectangleF(minX, minY, maxX - minX, maxY - minY);
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
}
