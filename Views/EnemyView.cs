using System.Drawing;
using runeforge.Models;

namespace runeforge.Views;

public sealed class EnemyView : IDisposable
{
    private readonly Font _font;
    private readonly SolidBrush _enemyBrush;
    private readonly StringFormat _textFormat;

    public EnemyView()
    {
        _font = new Font("Segoe UI", 15f, FontStyle.Bold, GraphicsUnit.Pixel);
        _enemyBrush = new SolidBrush(Color.IndianRed);
        _textFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
    }

    public void Draw(Graphics graphics, Enemy enemy)
    {
        var diameter = enemy.Radius * 2f;
        var drawX = enemy.Position.X - enemy.Radius;
        var drawY = enemy.Position.Y - enemy.Radius;

        graphics.FillEllipse(_enemyBrush, drawX, drawY, diameter, diameter);

        var textBounds = new RectangleF(drawX, drawY, diameter, diameter);
        var hpText = ((int)MathF.Ceiling(enemy.Health)).ToString();
        graphics.DrawString(hpText, _font, Brushes.White, textBounds, _textFormat);
    }

    public void Dispose()
    {
        _font.Dispose();
        _enemyBrush.Dispose();
        _textFormat.Dispose();
    }
}
