using System.Drawing;
using System.Numerics;
using runeforge.Effects;

namespace runeforge.Views;

public sealed class EffectView : IDisposable
{
    private readonly Dictionary<EffectType, Bitmap> _textures;
    private readonly Dictionary<EffectType, List<Bitmap>> _frameSequences;

    public EffectView()
    {
        _textures = new Dictionary<EffectType, Bitmap>();
        _frameSequences = new Dictionary<EffectType, List<Bitmap>>();

        foreach (var definition in EffectRegistry.All)
        {
            if (definition.UsesFrameSequence)
            {
                _frameSequences[definition.Type] = definition.FramePaths.Select(LoadBitmap).ToList();
                continue;
            }

            _textures[definition.Type] = LoadBitmap(definition.TexturePath);
        }
    }

    public void Draw(Graphics graphics, AnimatedEffect effect)
    {
        if (!effect.Definition.UsesFrameSequence &&
            !_textures.TryGetValue(effect.Definition.Type, out _))
        {
            return;
        }

        Draw(
            graphics,
            effect.Definition,
            effect.RowIndex,
            effect.Position,
            effect.Scale,
            effect.CurrentFrameIndex,
            effect.RotationRadians,
            effect.FlipHorizontally);
    }

    public void Draw(
        Graphics graphics,
        SpriteSheetEffectDefinition definition,
        int rowIndex,
        Vector2 position,
        float scale,
        int frameIndex,
        float rotationRadians = 0f,
        bool flipHorizontally = false)
    {
        if (definition.UsesFrameSequence)
        {
            DrawFrameSequence(
                graphics,
                definition,
                position,
                scale,
                frameIndex,
                rotationRadians,
                flipHorizontally);
            return;
        }

        if (!_textures.TryGetValue(definition.Type, out var texture))
        {
            return;
        }

        var clampedFrameIndex = definition.FrameCount <= 0
            ? 0
            : ((frameIndex % definition.FrameCount) + definition.FrameCount) % definition.FrameCount;

        var sourceRectangle = new Rectangle(
            clampedFrameIndex * definition.FrameWidth,
            rowIndex * definition.FrameHeight,
            definition.FrameWidth,
            definition.FrameHeight);

        if (sourceRectangle.Right > texture.Width || sourceRectangle.Bottom > texture.Height)
        {
            return;
        }

        var destinationRectangle = CreateDestinationRectangle(
            position,
            definition.FrameWidth,
            definition.FrameHeight,
            scale);

        if (MathF.Abs(rotationRadians) <= 0.0001f && !flipHorizontally)
        {
            graphics.DrawImage(texture, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);
            return;
        }

        var state = graphics.Save();
        graphics.TranslateTransform(position.X, position.Y);
        if (MathF.Abs(rotationRadians) > 0.0001f)
        {
            var angleDegrees = rotationRadians * (180f / MathF.PI);
            graphics.RotateTransform(angleDegrees);
        }

        if (flipHorizontally)
        {
            graphics.ScaleTransform(-1f, 1f);
        }

        var centeredDestination = new RectangleF(
            -(destinationRectangle.Width * 0.5f),
            -(destinationRectangle.Height * 0.5f),
            destinationRectangle.Width,
            destinationRectangle.Height);
        graphics.DrawImage(texture, centeredDestination, sourceRectangle, GraphicsUnit.Pixel);
        graphics.Restore(state);
    }

    public void Dispose()
    {
        foreach (var texture in _textures.Values)
        {
            texture.Dispose();
        }

        foreach (var frameSequence in _frameSequences.Values)
        {
            foreach (var frame in frameSequence)
            {
                frame.Dispose();
            }
        }
    }

    private void DrawFrameSequence(
        Graphics graphics,
        SpriteSheetEffectDefinition definition,
        Vector2 position,
        float scale,
        int frameIndex,
        float rotationRadians,
        bool flipHorizontally)
    {
        if (!_frameSequences.TryGetValue(definition.Type, out var frames) || frames.Count == 0)
        {
            return;
        }

        var clampedFrameIndex = definition.FrameCount <= 0
            ? 0
            : ((frameIndex % definition.FrameCount) + definition.FrameCount) % definition.FrameCount;
        var frame = frames[Math.Min(clampedFrameIndex, frames.Count - 1)];
        var destinationRectangle = CreateDestinationRectangle(position, frame.Width, frame.Height, scale);

        if (MathF.Abs(rotationRadians) <= 0.0001f && !flipHorizontally)
        {
            graphics.DrawImage(frame, destinationRectangle);
            return;
        }

        var state = graphics.Save();
        graphics.TranslateTransform(position.X, position.Y);
        if (MathF.Abs(rotationRadians) > 0.0001f)
        {
            var angleDegrees = rotationRadians * (180f / MathF.PI);
            graphics.RotateTransform(angleDegrees);
        }

        if (flipHorizontally)
        {
            graphics.ScaleTransform(-1f, 1f);
        }

        var centeredDestination = new RectangleF(
            -(destinationRectangle.Width * 0.5f),
            -(destinationRectangle.Height * 0.5f),
            destinationRectangle.Width,
            destinationRectangle.Height);
        graphics.DrawImage(frame, centeredDestination);
        graphics.Restore(state);
    }

    private static RectangleF CreateDestinationRectangle(Vector2 center, int frameWidth, int frameHeight, float scale)
    {
        var drawWidth = frameWidth * scale;
        var drawHeight = frameHeight * scale;

        return new RectangleF(
            center.X - (drawWidth * 0.5f),
            center.Y - (drawHeight * 0.5f),
            drawWidth,
            drawHeight);
    }

    private static Bitmap LoadBitmap(string path)
    {
        using var stream = File.OpenRead(path);
        using var image = Image.FromStream(stream);
        return new Bitmap(image);
    }
}
