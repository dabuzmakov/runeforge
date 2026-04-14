using System.Drawing;
using System.Drawing.Drawing2D;
using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Views;

public sealed class EnemyView : IDisposable
{
    private const float SquareCornerRadius = 7f;

    private readonly Font _font;
    private readonly StringFormat _textFormat;
    private readonly SolidBrush _highlightBrush;
    private readonly SolidBrush _badgeBrush;
    private readonly SolidBrush _textBrush;
    private readonly Pen _innerPen;
    private readonly Pen _slowedOutlinePen;
    private readonly Pen _badgePen;
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
        _palettes = new Dictionary<EnemyType, EnemyPalette>
        {
            { EnemyType.Normal, new EnemyPalette(new SolidBrush(Color.FromArgb(188, 116, 72)), new SolidBrush(Color.FromArgb(222, 150, 52, 60)), new Pen(Color.FromArgb(210, 242, 198, 142), 1.6f)) },
            { EnemyType.Fast, new EnemyPalette(new SolidBrush(Color.FromArgb(194, 114, 140, 72)), new SolidBrush(Color.FromArgb(230, 198, 142, 58)), new Pen(Color.FromArgb(220, 248, 216, 146), 1.6f)) },
            { EnemyType.Slow, new EnemyPalette(new SolidBrush(Color.FromArgb(184, 64, 88, 130)), new SolidBrush(Color.FromArgb(226, 86, 122, 176)), new Pen(Color.FromArgb(214, 180, 224, 255), 1.6f)) }
        };
    }

    public void Draw(Graphics graphics, EnemyEntity enemy)
    {
        var diameter = enemy.Data.Radius * 2f;
        var drawX = enemy.Transform.Position.X - enemy.Data.Radius;
        var drawY = enemy.Transform.Position.Y - enemy.Data.Radius;
        var bodyBounds = new RectangleF(drawX, drawY, diameter, diameter);

        DrawBody(graphics, enemy, bodyBounds);
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
        foreach (var palette in _palettes.Values)
        {
            palette.Dispose();
        }
    }

    private void DrawBody(Graphics graphics, EnemyEntity enemy, RectangleF bodyBounds)
    {
        var palette = _palettes[enemy.Data.Type];
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

        if (enemy.StatusEffects.IsSlowed)
        {
            var auraBounds = Inflate(bodyBounds, 4f, 4f);
            DrawShape(graphics, _slowedOutlinePen, auraBounds, enemy.Data.Config.Shape);
        }
    }

    private void DrawHealthBadge(Graphics graphics, EnemyEntity enemy, RectangleF bodyBounds)
    {
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
