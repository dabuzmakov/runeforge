using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;
using runeforge.Configs;
using runeforge.Models;

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

    private static GraphicsPath[] CreateCellPaths(IReadOnlyList<TableGrid.GridCell> cells)
    {
        var paths = new GraphicsPath[cells.Count];

        for (var i = 0; i < cells.Count; i++)
        {
            var cellBounds = Inflate(cells[i].Bounds, -5, -5);
            paths[i] = CreateRoundedRectanglePath(cellBounds, 12);
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

    private static Dictionary<string, Bitmap> LoadRuneTextures()
    {
        var spriteDirectory = ResolveSpriteDirectory();
        var textures = new Dictionary<string, Bitmap>();

        foreach (var runeDefinition in RuneDatabase.All)
        {
            var textureKey = runeDefinition.TextureKey;
            var texturePath = Path.Combine(spriteDirectory, textureKey + ".png");
            textures.Add(textureKey, LoadBitmap(texturePath));
        }

        return textures;
    }

    private static Bitmap LoadTexture(string textureName)
    {
        var texturePath = Path.Combine(ResolveSpriteDirectory(), textureName + ".png");
        return LoadBitmap(texturePath);
    }

    private static List<Bitmap> LoadAnimationFrames(string effectDirectoryName)
    {
        var framesDirectory = Path.Combine(ResolveEffectsDirectory(), effectDirectoryName);
        if (!Directory.Exists(framesDirectory))
        {
            return [];
        }

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

    private static string ResolveSpriteDirectory()
    {
        string[] candidateDirectories =
        [
            Path.Combine(AppContext.BaseDirectory, "Assets", "Sprites"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Assets", "Sprites"))
        ];

        foreach (var candidateDirectory in candidateDirectories)
        {
            if (Directory.Exists(candidateDirectory))
            {
                return candidateDirectory;
            }
        }

        throw new DirectoryNotFoundException("Could not locate Assets/Sprites for rune textures.");
    }

    private static string ResolveEffectsDirectory()
    {
        string[] candidateDirectories =
        [
            Path.Combine(AppContext.BaseDirectory, "Assets", "Effects"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Assets", "Effects"))
        ];

        foreach (var candidateDirectory in candidateDirectories)
        {
            if (Directory.Exists(candidateDirectory))
            {
                return candidateDirectory;
            }
        }

        throw new DirectoryNotFoundException("Could not locate Assets/Effects.");
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
