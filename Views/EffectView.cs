using System.Drawing;
using System.Numerics;
using runeforge.Effects;

namespace runeforge.Views;

public sealed class EffectView : IDisposable
{
    private readonly Dictionary<EffectType, Bitmap> _textures;

    public EffectView()
    {
        _textures = new Dictionary<EffectType, Bitmap>();

        foreach (var definition in EffectRegistry.All)
        {
            _textures[definition.Type] = LoadBitmap(definition.TexturePath);
        }
    }

    public void Draw(Graphics graphics, AnimatedEffect effect)
    {
        if (!_textures.TryGetValue(effect.Definition.Type, out var texture))
        {
            return;
        }

        Draw(graphics, effect.Definition, effect.RowIndex, effect.Position, effect.Scale, effect.CurrentFrameIndex);
    }

    public void Draw(
        Graphics graphics,
        SpriteSheetEffectDefinition definition,
        int rowIndex,
        Vector2 position,
        float scale,
        int frameIndex)
    {
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

        graphics.DrawImage(texture, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);
    }

    public void Dispose()
    {
        foreach (var texture in _textures.Values)
        {
            texture.Dispose();
        }
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
