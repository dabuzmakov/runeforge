using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Numerics;
using runeforge.Configs;
using runeforge.Models;
using runeforge.Systems;

namespace runeforge.Views;

public sealed partial class GameRenderer
{
    private static PointF[] ToPointArray(IReadOnlyList<Vector2> points)
    {
        var result = new PointF[points.Count];

        for (var i = 0; i < points.Count; i++)
        {
            result[i] = new PointF(points[i].X, points[i].Y);
        }

        return result;
    }

    private static GraphicsPath[] CreateRoundedCellPaths(
        IReadOnlyList<TableGrid.GridCell> cells,
        int inflateX,
        int inflateY,
        int radius)
    {
        var paths = new GraphicsPath[cells.Count];

        for (var i = 0; i < cells.Count; i++)
        {
            var bounds = Inflate(cells[i].Bounds, inflateX, inflateY);
            paths[i] = CreateRoundedRectanglePath(bounds, radius);
        }

        return paths;
    }

    private static Rectangle Inflate(Rectangle rectangle, int amountX, int amountY)
    {
        return new Rectangle(
            rectangle.X - amountX,
            rectangle.Y - amountY,
            rectangle.Width + (amountX * 2),
            rectangle.Height + (amountY * 2));
    }

    private static RectangleF CreateCenteredSquareF(Vector2 center, float size)
    {
        return new RectangleF(
            center.X - (size * 0.5f),
            center.Y - (size * 0.5f),
            size,
            size);
    }

    private static Vector2 Rotate(Vector2 vector, float radians)
    {
        var sin = MathF.Sin(radians);
        var cos = MathF.Cos(radians);
        return new Vector2(
            (vector.X * cos) - (vector.Y * sin),
            (vector.X * sin) + (vector.Y * cos));
    }

    private static float SmoothStep(float value)
    {
        var clamped = Math.Clamp(value, 0f, 1f);
        return clamped * clamped * (3f - (2f * clamped));
    }

    private static Rectangle ScaleRectangle(Rectangle rectangle, float scale)
    {
        var centerX = rectangle.Left + (rectangle.Width * 0.5f);
        var centerY = rectangle.Top + (rectangle.Height * 0.5f);
        var width = rectangle.Width * scale;
        var height = rectangle.Height * scale;

        return Rectangle.Round(new RectangleF(
            centerX - (width * 0.5f),
            centerY - (height * 0.5f),
            width,
            height));
    }

    private static RectangleF ScaleRectangle(RectangleF rectangle, float scale)
    {
        var centerX = rectangle.Left + (rectangle.Width * 0.5f);
        var centerY = rectangle.Top + (rectangle.Height * 0.5f);
        var width = rectangle.Width * scale;
        var height = rectangle.Height * scale;

        return new RectangleF(
            centerX - (width * 0.5f),
            centerY - (height * 0.5f),
            width,
            height);
    }

    private static Rectangle CreateCenteredAspectRectangle(Rectangle bounds, Bitmap texture, int extraWidth = 0, int extraHeight = 0)
    {
        var targetWidth = bounds.Width + (extraWidth * 2);
        var targetHeight = bounds.Height + (extraHeight * 2);
        var scale = Math.Min(
            targetWidth / (float)texture.Width,
            targetHeight / (float)texture.Height);
        var width = Math.Max(1, (int)MathF.Round(texture.Width * scale));
        var height = Math.Max(1, (int)MathF.Round(texture.Height * scale));
        return new Rectangle(
            bounds.Left + ((bounds.Width - width) / 2),
            bounds.Top + ((bounds.Height - height) / 2),
            width,
            height);
    }

    private Bitmap GetScaledTexture(string cacheKey, Bitmap texture, Size targetSize)
    {
        if (targetSize.Width <= 0 || targetSize.Height <= 0)
        {
            return texture;
        }

        var key = $"{cacheKey}:{targetSize.Width}x{targetSize.Height}";
        if (_scaledTextureCache.TryGetValue(key, out var cachedTexture))
        {
            return cachedTexture;
        }

        var scaledTexture = new Bitmap(targetSize.Width, targetSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        using var scaledGraphics = Graphics.FromImage(scaledTexture);
        scaledGraphics.CompositingMode = CompositingMode.SourceCopy;
        scaledGraphics.CompositingQuality = CompositingQuality.HighQuality;
        scaledGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        scaledGraphics.PixelOffsetMode = PixelOffsetMode.Half;
        scaledGraphics.SmoothingMode = SmoothingMode.HighQuality;
        scaledGraphics.DrawImage(texture, new Rectangle(Point.Empty, targetSize));
        _scaledTextureCache.Add(key, scaledTexture);
        return scaledTexture;
    }

    private static GraphicsPath CreateRoundedRectanglePath(Rectangle rectangle, int radius)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2;

        path.AddArc(rectangle.X, rectangle.Y, diameter, diameter, 180, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Y, diameter, diameter, 270, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rectangle.X, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }

    private static ImageAttributes CreateOpacityImageAttributes(float opacity)
    {
        var clampedOpacity = Math.Clamp(opacity, 0f, 1f);
        var colorMatrix = new ColorMatrix(
        [
            [1f, 0f, 0f, 0f, 0f],
            [0f, 1f, 0f, 0f, 0f],
            [0f, 0f, 1f, 0f, 0f],
            [0f, 0f, 0f, clampedOpacity, 0f],
            [0f, 0f, 0f, 0f, 1f]
        ]);
        var imageAttributes = new ImageAttributes();
        imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        return imageAttributes;
    }

    private ImageAttributes GetOpacityImageAttributes(float opacity)
    {
        var alphaKey = Math.Clamp((int)MathF.Round(Math.Clamp(opacity, 0f, 1f) * 255f), 0, 255);
        if (_opacityImageAttributesCache.TryGetValue(alphaKey, out var imageAttributes))
        {
            return imageAttributes;
        }

        imageAttributes = CreateOpacityImageAttributes(alphaKey / 255f);
        _opacityImageAttributesCache.Add(alphaKey, imageAttributes);
        return imageAttributes;
    }

    private static Dictionary<string, Bitmap> LoadRuneTextures()
    {
        var runeDirectory = AssetResolver.ResolveDirectory("Runes");
        var textures = new Dictionary<string, Bitmap>();

        foreach (var runeDefinition in RuneDatabase.All)
        {
            var textureKey = runeDefinition.TextureKey;
            var texturePath = Path.Combine(runeDirectory, textureKey + ".png");
            textures.Add(textureKey, LoadBitmap(texturePath));
        }

        return textures;
    }

    private static IReadOnlyDictionary<BackgroundId, Bitmap> LoadBackgroundTextures()
    {
        return new Dictionary<BackgroundId, Bitmap>
        {
            { BackgroundId.Initial, LoadBackgroundTexture("initial-background") },
            { BackgroundId.Main, LoadBackgroundTexture("main-background") },
            { BackgroundId.Selection, LoadBackgroundTexture("selection-background") }
        };
    }

    private void DrawBackground(Graphics graphics, BackgroundId backgroundId)
    {
        var backgroundTexture = GetPreparedBackgroundTexture(backgroundId, _board.ViewportBounds.Size);
        if (backgroundTexture == null)
        {
            graphics.Clear(BackgroundColor);
            return;
        }

        graphics.DrawImageUnscaled(backgroundTexture, _board.ViewportBounds.Location);
    }

    private Bitmap? GetPreparedBackgroundTexture(BackgroundId backgroundId, Size targetSize)
    {
        if (!_backgroundTextures.TryGetValue(backgroundId, out var sourceTexture))
        {
            return null;
        }

        var key = $"{backgroundId}:{targetSize.Width}x{targetSize.Height}";
        if (_preparedBackgroundTextureCache.TryGetValue(key, out var cachedTexture))
        {
            return cachedTexture;
        }

        var preparedTexture = new Bitmap(targetSize.Width, targetSize.Height, PixelFormat.Format32bppPArgb);
        using var preparedGraphics = Graphics.FromImage(preparedTexture);
        preparedGraphics.CompositingMode = CompositingMode.SourceCopy;
        preparedGraphics.CompositingQuality = CompositingQuality.HighQuality;
        preparedGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        preparedGraphics.PixelOffsetMode = PixelOffsetMode.Half;
        preparedGraphics.SmoothingMode = SmoothingMode.HighQuality;

        if (backgroundId == BackgroundId.Selection)
        {
            using var imageAttributes = CreateBackgroundImageAttributes();
            preparedGraphics.DrawImage(
                sourceTexture,
                new Rectangle(Point.Empty, targetSize),
                0,
                0,
                sourceTexture.Width,
                sourceTexture.Height,
                GraphicsUnit.Pixel,
                imageAttributes);
        }
        else
        {
            preparedGraphics.DrawImage(sourceTexture, new Rectangle(Point.Empty, targetSize));
        }

        _preparedBackgroundTextureCache.Add(key, preparedTexture);
        return preparedTexture;
    }

    private static ImageAttributes CreateBackgroundImageAttributes()
    {
        const float contrast = 1.08f;
        const float brightness = 0.08f;
        const float offset = brightness + ((1f - contrast) * 0.5f);
        var colorMatrix = new ColorMatrix(
        [
            [contrast, 0f, 0f, 0f, 0f],
            [0f, contrast, 0f, 0f, 0f],
            [0f, 0f, contrast, 0f, 0f],
            [0f, 0f, 0f, 1f, 0f],
            [offset, offset, offset, 0f, 1f]
        ]);
        var imageAttributes = new ImageAttributes();
        imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        return imageAttributes;
    }

    private static BackgroundId ResolveBackgroundId(GameState gameState)
    {
        if (gameState.Ui.IsStartScreenOpen)
        {
            return BackgroundId.Initial;
        }

        return gameState.Ui.BuildSelection.IsOpen
            ? BackgroundId.Selection
            : BackgroundId.Main;
    }

    private static Bitmap LoadTexture(string textureName)
    {
        var texturePath = AssetResolver.ResolveFileByName("UI", textureName + ".png", SearchOption.AllDirectories);
        return LoadBitmap(texturePath);
    }

    private static Bitmap LoadBackgroundTexture(string textureName)
    {
        var texturePath = AssetResolver.ResolveFile("Backgrounds", textureName + ".png");
        return LoadBitmap(texturePath);
    }

    private static Bitmap LoadEffectTexture(string textureName)
    {
        var texturePath = AssetResolver.ResolveFileByName("Effects", textureName + ".png", SearchOption.AllDirectories);
        return LoadBitmap(texturePath);
    }

    private static List<Bitmap> LoadAnimationFrames(string effectDirectoryName)
    {
        var framesDirectory = AssetResolver.ResolveDirectory("Effects", "FrameAnimations", effectDirectoryName);

        return Directory
            .GetFiles(framesDirectory, "*.png")
            .OrderBy(static path =>
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                var trailingNumber = ExtractTrailingNumber(fileName);
                return trailingNumber ?? int.MaxValue;
            })
            .ThenBy(static path => path, StringComparer.OrdinalIgnoreCase)
            .Select(LoadBitmap)
            .ToList();
    }

    private static int? ExtractTrailingNumber(string fileNameWithoutExtension)
    {
        if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
        {
            return null;
        }

        var end = fileNameWithoutExtension.Length - 1;
        while (end >= 0 && char.IsDigit(fileNameWithoutExtension[end]))
        {
            end--;
        }

        var digitStart = end + 1;
        if (digitStart >= fileNameWithoutExtension.Length)
        {
            return null;
        }

        var numericPart = fileNameWithoutExtension[digitStart..];
        return int.TryParse(numericPart, out var value) ? value : null;
    }

    private static Bitmap LoadBitmap(string path)
    {
        using var stream = File.OpenRead(path);
        using var image = Image.FromStream(stream);
        return new Bitmap(image);
    }

    private static Pen CreatePathPen(Color color, float width)
    {
        return new Pen(color, width)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
    }

    private static bool ShouldDimForDraggedMerge(RuneEntity draggedRune, RuneEntity candidateRune)
    {
        return !RuneMergeRules.CanMerge(draggedRune, candidateRune);
    }
}
