using System.Drawing.Drawing2D;
using System.Numerics;
using runeforge.Configs;
using runeforge.Models;
using runeforge.Systems;

namespace runeforge.Views;

public sealed class SowiloBeamView : IDisposable
{
    private readonly Bitmap _texture;
    private readonly Pen _outerGlowPen;
    private readonly Pen _innerGlowPen;

    public SowiloBeamView()
    {
        _texture = LoadBitmap(ResolveTexturePath());
        _outerGlowPen = new Pen(Color.FromArgb(78, 255, 202, 84), 1f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        _innerGlowPen = new Pen(Color.FromArgb(166, 255, 244, 188), 1f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
    }

    public void Draw(Graphics graphics, SowiloBeamInstance beam)
    {
        var beamVector = beam.CurrentEndPoint - beam.StartPoint;
        var beamLength = beamVector.Length();
        if (beamLength <= 1f)
        {
            return;
        }

        var angleDegrees = MathF.Atan2(beamVector.Y, beamVector.X) * (180f / MathF.PI);
        var thickness = SowiloTuning.BeamThickness * beam.Intensity;
        var glowAlpha = (int)(78f * beam.Intensity);
        var coreAlpha = (int)(166f * beam.Intensity);
        var beamDirection = Vector2.Normalize(beamVector);
        var visualEndOvershoot = Math.Min(SowiloTuning.BeamVisualEndOvershoot, beamLength * 0.4f);
        var visualEndPoint = beam.StartPoint + (beamDirection * (beamLength + visualEndOvershoot));
        var outerGlowWidth = thickness * 0.62f;
        var innerGlowWidth = thickness * 0.34f;
        var outerGlowStartPoint = beam.StartPoint + (beamDirection * (outerGlowWidth * 0.5f));
        var outerGlowEndPoint = visualEndPoint - (beamDirection * (outerGlowWidth * 0.5f));
        var innerGlowStartPoint = beam.StartPoint + (beamDirection * (innerGlowWidth * 0.5f));
        var innerGlowEndPoint = visualEndPoint - (beamDirection * (innerGlowWidth * 0.5f));
        _outerGlowPen.Color = Color.FromArgb(glowAlpha, 255, 202, 84);
        _outerGlowPen.Width = outerGlowWidth;
        _innerGlowPen.Color = Color.FromArgb(coreAlpha, 255, 244, 188);
        _innerGlowPen.Width = innerGlowWidth;

        graphics.DrawLine(_outerGlowPen, outerGlowStartPoint.X, outerGlowStartPoint.Y, outerGlowEndPoint.X, outerGlowEndPoint.Y);
        graphics.DrawLine(_innerGlowPen, innerGlowStartPoint.X, innerGlowStartPoint.Y, innerGlowEndPoint.X, innerGlowEndPoint.Y);

        var state = graphics.Save();
        graphics.TranslateTransform(beam.StartPoint.X, beam.StartPoint.Y);
        graphics.RotateTransform(angleDegrees);
        graphics.TranslateTransform(beamLength, 0f);
        graphics.RotateTransform(180f);

        var sourceY = beam.CurrentFrameIndex * SowiloTuning.SpriteFrameHeight;
        var destinationRectangle = new Rectangle(
            -(int)MathF.Round(visualEndOvershoot),
            (int)MathF.Round(-thickness * 0.5f),
            Math.Max(1, (int)MathF.Round(beamLength + visualEndOvershoot)),
            Math.Max(1, (int)MathF.Round(thickness)));

        graphics.DrawImage(
            _texture,
            destinationRectangle,
            0f,
            sourceY,
            _texture.Width,
            SowiloTuning.SpriteFrameHeight,
            GraphicsUnit.Pixel);

        graphics.Restore(state);
    }

    public void Dispose()
    {
        _outerGlowPen.Dispose();
        _innerGlowPen.Dispose();
        _texture.Dispose();
    }

    private static string ResolveTexturePath()
    {
        return AssetResolver.ResolveFile("Effects", "SpriteSheets", "sowilo-beam.png");
    }

    private static Bitmap LoadBitmap(string path)
    {
        using var stream = File.OpenRead(path);
        using var image = Image.FromStream(stream);
        return new Bitmap(image);
    }
}
