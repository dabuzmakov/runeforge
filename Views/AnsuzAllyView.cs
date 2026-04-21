using System.Drawing;
using System.Drawing.Drawing2D;
using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Views;

public sealed class AnsuzAllyView : IDisposable
{
    private const float SquareCornerRadius = 7f;
    private const float MinimumRenderableDiameter = 1.5f;
    private readonly Font _font;
    private readonly StringFormat _textFormat;
    private readonly SolidBrush _outerBrush;
    private readonly SolidBrush _coreBrush;
    private readonly SolidBrush _highlightBrush;
    private readonly SolidBrush _badgeBrush;
    private readonly SolidBrush _textBrush;
    private readonly Pen _borderPen;
    private readonly Pen _innerPen;
    private readonly Pen _badgePen;

    public AnsuzAllyView()
    {
        _font = FontLibrary.Create(12f, FontStyle.Bold);
        _textFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        _outerBrush = new SolidBrush(Color.FromArgb(180, 36, 120, 72));
        _coreBrush = new SolidBrush(Color.FromArgb(228, 98, 208, 128));
        _highlightBrush = new SolidBrush(Color.FromArgb(82, 242, 255, 232));
        _badgeBrush = new SolidBrush(Color.FromArgb(176, 18, 30, 20));
        _textBrush = new SolidBrush(Color.White);
        _borderPen = new Pen(Color.FromArgb(220, 206, 250, 196), 1.6f);
        _innerPen = new Pen(Color.FromArgb(132, 240, 255, 224), 1f);
        _badgePen = new Pen(Color.FromArgb(140, 204, 244, 200), 1f);
    }

    public void Draw(Graphics graphics, AnsuzAllyEntity ally)
    {
        var scale = ally.SpawnScale;
        if (scale <= 0f)
        {
            return;
        }

        var scaledRadius = ally.Radius * scale;
        var diameter = scaledRadius * 2f;
        if (diameter < MinimumRenderableDiameter)
        {
            return;
        }

        var bodyBounds = new RectangleF(
            ally.Transform.Position.X - scaledRadius,
            ally.Transform.Position.Y - scaledRadius,
            diameter,
            diameter);

        FillShape(graphics, _outerBrush, bodyBounds, ally.Shape);
        var innerBounds = Inflate(bodyBounds, -4f, -4f);
        FillShape(graphics, _coreBrush, innerBounds, ally.Shape);
        DrawShape(graphics, _borderPen, bodyBounds, ally.Shape);
        DrawShape(graphics, _innerPen, Inflate(bodyBounds, -2.5f, -2.5f), ally.Shape);
        FillShape(
            graphics,
            _highlightBrush,
            bodyBounds.X + (bodyBounds.Width * 0.18f),
            bodyBounds.Y + (bodyBounds.Height * 0.12f),
            bodyBounds.Width * 0.28f,
            bodyBounds.Height * 0.18f,
            ally.Shape);
        DrawHealthBadge(graphics, ally, bodyBounds);
    }

    public void Dispose()
    {
        _font.Dispose();
        _textFormat.Dispose();
        _outerBrush.Dispose();
        _coreBrush.Dispose();
        _highlightBrush.Dispose();
        _badgeBrush.Dispose();
        _textBrush.Dispose();
        _borderPen.Dispose();
        _innerPen.Dispose();
        _badgePen.Dispose();
    }

    private void DrawHealthBadge(Graphics graphics, AnsuzAllyEntity ally, RectangleF bodyBounds)
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
        graphics.DrawString(((int)MathF.Ceiling(ally.Health)).ToString(), _font, _textBrush, badgeBounds, _textFormat);
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
            return new GraphicsPath();
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
}
