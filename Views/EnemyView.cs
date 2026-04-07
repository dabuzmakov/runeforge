using System.Drawing;
using System.Drawing.Drawing2D;
using runeforge.Models;

namespace runeforge.Views;

public sealed class EnemyView : IDisposable
{
    private readonly Font _font;
    private readonly StringFormat _textFormat;
    private readonly SolidBrush _outerBrush;
    private readonly SolidBrush _coreBrush;
    private readonly SolidBrush _highlightBrush;
    private readonly SolidBrush _badgeBrush;
    private readonly SolidBrush _textBrush;
    private readonly Pen _borderPen;
    private readonly Pen _innerPen;
    private readonly Pen[] _slowPens;
    private readonly Pen _badgePen;

    public EnemyView()
    {
        _font = FontLibrary.Create(12f, FontStyle.Bold);
        _textFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        _outerBrush = new SolidBrush(Color.FromArgb(188, 116, 72));
        _coreBrush = new SolidBrush(Color.FromArgb(222, 150, 52, 60));
        _highlightBrush = new SolidBrush(Color.FromArgb(64, 255, 240, 225));
        _badgeBrush = new SolidBrush(Color.FromArgb(176, 26, 16, 18));
        _textBrush = new SolidBrush(Color.White);
        _borderPen = new Pen(Color.FromArgb(210, 242, 198, 142), 1.6f);
        _innerPen = new Pen(Color.FromArgb(120, 255, 224, 196), 1f);
        _badgePen = new Pen(Color.FromArgb(140, 244, 213, 165), 1f);
        _slowPens =
        [
            new Pen(Color.FromArgb(148, 105, 220, 255), 2f),
            new Pen(Color.FromArgb(176, 105, 220, 255), 2f),
            new Pen(Color.FromArgb(204, 105, 220, 255), 2f)
        ];
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
        _outerBrush.Dispose();
        _coreBrush.Dispose();
        _highlightBrush.Dispose();
        _badgeBrush.Dispose();
        _textBrush.Dispose();
        _borderPen.Dispose();
        _innerPen.Dispose();
        _badgePen.Dispose();
        foreach (var slowPen in _slowPens)
        {
            slowPen.Dispose();
        }
    }

    private void DrawBody(Graphics graphics, EnemyEntity enemy, RectangleF bodyBounds)
    {
        graphics.FillEllipse(_outerBrush, bodyBounds);

        var innerBounds = Inflate(bodyBounds, -4f, -4f);
        graphics.FillEllipse(_coreBrush, innerBounds);
        graphics.DrawEllipse(_borderPen, bodyBounds);

        var ringBounds = Inflate(bodyBounds, -2.5f, -2.5f);
        graphics.DrawEllipse(_innerPen, ringBounds);

        graphics.FillEllipse(
            _highlightBrush,
            bodyBounds.X + (bodyBounds.Width * 0.18f),
            bodyBounds.Y + (bodyBounds.Height * 0.12f),
            bodyBounds.Width * 0.28f,
            bodyBounds.Height * 0.18f);

        if (enemy.Path.SlowStacks > 0)
        {
            var auraBounds = Inflate(bodyBounds, 4f, 4f);
            var slowPen = _slowPens[Math.Min(enemy.Path.SlowStacks, _slowPens.Length) - 1];
            graphics.DrawEllipse(slowPen, auraBounds);
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
}
