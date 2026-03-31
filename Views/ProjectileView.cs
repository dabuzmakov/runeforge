using System.Drawing;
using runeforge.Models;

namespace runeforge.Views;

public sealed class ProjectileView : IDisposable
{
    private readonly Dictionary<int, SolidBrush> _brushCache = new();

    public void Draw(Graphics graphics, Projectile projectile)
    {
        var brush = GetBrush(projectile.Color);
        var diameter = projectile.Radius * 2f;

        graphics.FillEllipse(
            brush,
            projectile.Position.X - projectile.Radius,
            projectile.Position.Y - projectile.Radius,
            diameter,
            diameter);
    }

    public void Dispose()
    {
        foreach (var brush in _brushCache.Values)
        {
            brush.Dispose();
        }
    }

    private SolidBrush GetBrush(Color color)
    {
        var key = color.ToArgb();
        if (_brushCache.TryGetValue(key, out var brush))
        {
            return brush;
        }

        brush = new SolidBrush(color);
        _brushCache.Add(key, brush);
        return brush;
    }
}
