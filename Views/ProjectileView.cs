using System.Drawing;
using runeforge.Models;

namespace runeforge.Views;

public sealed class ProjectileView : IDisposable
{
    private const int BurstFragmentCount = 4;
    private const float BurstRadiusMultiplier = 3.1f;

    private readonly Dictionary<int, SolidBrush> _brushCache = new();
    private readonly SolidBrush _fragmentBrush = new(Color.White);

    public void Draw(Graphics graphics, ProjectileEntity projectile)
    {
        if (projectile.Flight.IsBursting)
        {
            DrawBurst(graphics, projectile);
            return;
        }

        var brush = GetBrush(projectile.Impact.Color);
        var diameter = projectile.Flight.Radius * 2f;

        graphics.FillEllipse(
            brush,
            projectile.Transform.Position.X - projectile.Flight.Radius,
            projectile.Transform.Position.Y - projectile.Flight.Radius,
            diameter,
            diameter);
    }

    public void Dispose()
    {
        _fragmentBrush.Dispose();

        foreach (var brush in _brushCache.Values)
        {
            brush.Dispose();
        }
    }

    private void DrawBurst(Graphics graphics, ProjectileEntity projectile)
    {
        var progress = projectile.Flight.BurstProgress;
        var alpha = (int)(255f * (1f - progress));
        if (alpha <= 0)
        {
            return;
        }

        _fragmentBrush.Color = Color.FromArgb(alpha, projectile.Impact.Color);
        var radius = projectile.Flight.Radius * (1.15f - (progress * 0.25f));
        var fragmentTravel = projectile.Flight.Radius * BurstRadiusMultiplier * progress;
        var angleOffset = (((int)projectile.Impact.SourceRuneType * 37) % 360) * (MathF.PI / 180f);

        for (var i = 0; i < BurstFragmentCount; i++)
        {
            var angle = angleOffset + ((MathF.PI * 2f * i) / BurstFragmentCount);
            var travelScale = 0.72f + (0.18f * i);
            var fragmentRadius = Math.Max(2.6f, radius * (0.8f - (0.08f * i)));
            var offset = new PointF(
                MathF.Cos(angle) * fragmentTravel * travelScale,
                MathF.Sin(angle) * fragmentTravel * travelScale);

            graphics.FillEllipse(
                _fragmentBrush,
                projectile.Transform.Position.X + offset.X - fragmentRadius,
                projectile.Transform.Position.Y + offset.Y - fragmentRadius,
                fragmentRadius * 2f,
                fragmentRadius * 2f);
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
