using System.Drawing;
using System.Drawing.Drawing2D;
using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Views;

public sealed class ProjectileView : IDisposable
{
    private const int BurstFragmentCount = 4;
    private const float BurstRadiusMultiplier = 3.1f;

    private readonly IReadOnlyList<Bitmap> _thurisazFrames;
    private readonly Bitmap? _eiwazProjectileTexture;
    private readonly Bitmap? _ingwazProjectileTexture;
    private readonly Bitmap? _perthroProjectileTexture;
    private readonly Dictionary<int, SolidBrush> _brushCache = new();
    private readonly SolidBrush _fragmentBrush = new(Color.White);

    public ProjectileView(
        IReadOnlyList<Bitmap> thurisazFrames,
        Bitmap? eiwazProjectileTexture,
        Bitmap? ingwazProjectileTexture,
        Bitmap? perthroProjectileTexture)
    {
        _thurisazFrames = thurisazFrames;
        _eiwazProjectileTexture = eiwazProjectileTexture;
        _ingwazProjectileTexture = ingwazProjectileTexture;
        _perthroProjectileTexture = perthroProjectileTexture;
    }

    public void Draw(Graphics graphics, ProjectileEntity projectile)
    {
        if (projectile.Flight.IsBursting)
        {
            DrawBurst(graphics, projectile);
            return;
        }

        if (projectile.Impact.SourceRuneType == RuneType.Thurisaz)
        {
            DrawThurisazFireball(graphics, projectile);
            return;
        }

        if (projectile.Impact.SourceRuneType == RuneType.Eiwaz)
        {
            DrawEiwazProjectile(graphics, projectile);
            return;
        }

        if (projectile.Impact.SourceRuneType == RuneType.Ingwaz)
        {
            DrawIngwazProjectile(graphics, projectile);
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

    public void DrawPerthroBoomerang(Graphics graphics, PerthroBoomerangEntity boomerang)
    {
        if (_perthroProjectileTexture == null)
        {
            return;
        }

        var size = boomerang.Radius * 2.2f;
        DrawRotatedTexture(graphics, _perthroProjectileTexture, boomerang.Position, boomerang.RotationRadians, size, size);
    }

    public void Dispose()
    {
        _fragmentBrush.Dispose();
        _eiwazProjectileTexture?.Dispose();
        _ingwazProjectileTexture?.Dispose();
        _perthroProjectileTexture?.Dispose();

        foreach (var brush in _brushCache.Values)
        {
            brush.Dispose();
        }
    }

    private void DrawThurisazFireball(Graphics graphics, ProjectileEntity projectile)
    {
        if (_thurisazFrames.Count == 0)
        {
            var brush = GetBrush(projectile.Impact.Color);
            var diameter = projectile.Flight.Radius * 2f;
            graphics.FillEllipse(
                brush,
                projectile.Transform.Position.X - projectile.Flight.Radius,
                projectile.Transform.Position.Y - projectile.Flight.Radius,
                diameter,
                diameter);
            return;
        }

        var frameIndex = GetAnimationFrameIndex(_thurisazFrames.Count, ThurisazTuning.AnimationFrameDurationSeconds);
        var frame = _thurisazFrames[frameIndex];
        var size = projectile.Flight.Radius * 2f * ThurisazTuning.VisualScaleMultiplier;
        var rotationRadians = GetProjectileRotation(projectile);
        DrawRotatedTexture(graphics, frame, projectile.Transform.Position, rotationRadians, size, size);
    }

    private void DrawEiwazProjectile(Graphics graphics, ProjectileEntity projectile)
    {
        if (_eiwazProjectileTexture == null)
        {
            var brush = GetBrush(projectile.Impact.Color);
            var diameter = projectile.Flight.Radius * 2f;
            graphics.FillEllipse(
                brush,
                projectile.Transform.Position.X - projectile.Flight.Radius,
                projectile.Transform.Position.Y - projectile.Flight.Radius,
                diameter,
                diameter);
            return;
        }

        var size = projectile.Flight.Radius * 2f * EiwazTuning.ProjectileVisualScaleMultiplier;
        var rotationRadians = GetProjectileRotation(projectile);
        DrawRotatedTexture(graphics, _eiwazProjectileTexture, projectile.Transform.Position, rotationRadians, size, size);
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

    private void DrawIngwazProjectile(Graphics graphics, ProjectileEntity projectile)
    {
        if (_ingwazProjectileTexture == null)
        {
            return;
        }

        var frameWidth = _ingwazProjectileTexture.Width / IngwazTuning.ProjectileFrameCount;
        if (frameWidth <= 0)
        {
            return;
        }

        var frameIndex = GetAnimationFrameIndex(IngwazTuning.ProjectileFrameCount, IngwazTuning.ProjectileAnimationFrameDurationSeconds);
        var sourceRectangle = new Rectangle(
            frameIndex * frameWidth,
            0,
            frameWidth,
            _ingwazProjectileTexture.Height);
        var width = frameWidth * IngwazTuning.ProjectileVisualScaleMultiplier;
        var height = _ingwazProjectileTexture.Height * IngwazTuning.ProjectileVisualScaleMultiplier;
        var rotationRadians = GetProjectileRotation(projectile);
        DrawRotatedTexture(graphics, _ingwazProjectileTexture, sourceRectangle, projectile.Transform.Position, rotationRadians, width, height);
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

    private static int GetAnimationFrameIndex(int frameCount, float frameDurationSeconds)
    {
        if (frameCount <= 1)
        {
            return 0;
        }

        var totalElapsedSeconds = Environment.TickCount64 / 1000d;
        return (int)(totalElapsedSeconds / frameDurationSeconds) % frameCount;
    }

    private static float GetProjectileRotation(ProjectileEntity projectile)
    {
        var direction = projectile.Flight.Target.Transform.Position - projectile.Transform.Position;
        if (direction.LengthSquared() <= 0.001f)
        {
            return 0f;
        }

        return MathF.Atan2(direction.Y, direction.X);
    }

    private static void DrawRotatedTexture(
        Graphics graphics,
        Bitmap texture,
        System.Numerics.Vector2 center,
        float rotationRadians,
        float sizeWidth,
        float sizeHeight)
    {
        DrawRotatedTexture(
            graphics,
            texture,
            new Rectangle(0, 0, texture.Width, texture.Height),
            center,
            rotationRadians,
            sizeWidth,
            sizeHeight);
    }

    private static void DrawRotatedTexture(
        Graphics graphics,
        Bitmap texture,
        Rectangle sourceRectangle,
        System.Numerics.Vector2 center,
        float rotationRadians,
        float sizeWidth,
        float sizeHeight)
    {
        GraphicsState state = graphics.Save();
        graphics.TranslateTransform(center.X, center.Y);
        graphics.RotateTransform(rotationRadians * (180f / MathF.PI));
        graphics.DrawImage(
            texture,
            new RectangleF(
                -(sizeWidth * 0.5f),
                -(sizeHeight * 0.5f),
                sizeWidth,
                sizeHeight),
            sourceRectangle,
            GraphicsUnit.Pixel);
        graphics.Restore(state);
    }
}
