using System.Numerics;

namespace runeforge.Models;

public sealed class GameBoard
{
    private const int TableSize = 400;
    private const int PathHorizontalMargin = 72;
    private const int PathTopMargin = 70;
    private const int PathBottomMargin = 70;
    private const int PathCornerRadius = 64;
    private const int PathCornerSegments = 6;
    private const int BagHalfSize = 80;
    private const int BottomOffset = 200;

    public GameBoard(int screenWidth, int screenHeight)
    {
        ViewportBounds = new Rectangle(0, 0, screenWidth, screenHeight);
        TableBounds = CreateTableBounds(screenWidth, screenHeight);
        Grid = new TableGrid(TableBounds);
        BagBounds = CreateBagBounds(screenWidth, screenHeight);
        Path = CreatePath(TableBounds);
        PathLength = PathGeometry.ComputeLength(Path);
    }

    public Rectangle ViewportBounds { get; }

    public Rectangle TableBounds { get; }

    public Rectangle BagBounds { get; }

    public TableGrid Grid { get; }

    public IReadOnlyList<Vector2> Path { get; }

    public float PathLength { get; }

    private static Rectangle CreateTableBounds(int screenWidth, int screenHeight)
    {
        var centerX = screenWidth / 2;
        var centerY = (screenHeight / 2) + 50;

        return new Rectangle(
            centerX - (TableSize / 2),
            centerY - (TableSize / 2),
            TableSize,
            TableSize);
    }

    private static Rectangle CreateBagBounds(int screenWidth, int screenHeight)
    {
        var center = new Vector2(screenWidth * 0.5f, screenHeight - BottomOffset);

        return new Rectangle(
            (int)(center.X - BagHalfSize),
            (int)(center.Y - BagHalfSize),
            BagHalfSize * 2,
            BagHalfSize * 2);
    }

    private static IReadOnlyList<Vector2> CreatePath(Rectangle tableBounds)
    {
        var left = tableBounds.Left - PathHorizontalMargin;
        var top = tableBounds.Top - PathTopMargin;
        var right = tableBounds.Right + PathHorizontalMargin;
        var bottom = tableBounds.Bottom + PathBottomMargin;
        var radius = PathCornerRadius;

        var path = new List<Vector2>(32)
        {
            new(tableBounds.Left + 50f, bottom),
            new(left + radius, bottom)
        };

        AddArcPoints(path, new Vector2(left + radius, bottom - radius), radius, 90f, 180f, PathCornerSegments);
        path.Add(new Vector2(left, top + radius));

        AddArcPoints(path, new Vector2(left + radius, top + radius), radius, 180f, 270f, PathCornerSegments);
        path.Add(new Vector2(right - radius, top));

        AddArcPoints(path, new Vector2(right - radius, top + radius), radius, 270f, 360f, PathCornerSegments);
        path.Add(new Vector2(right, bottom - radius));

        AddArcPoints(path, new Vector2(right - radius, bottom - radius), radius, 0f, 90f, PathCornerSegments);
        path.Add(new Vector2(tableBounds.Right - 50f, bottom));

        return path;
    }

    private static void AddArcPoints(
        List<Vector2> path,
        Vector2 center,
        float radius,
        float startAngleDegrees,
        float endAngleDegrees,
        int segments)
    {
        var step = (endAngleDegrees - startAngleDegrees) / segments;

        for (var i = 1; i <= segments; i++)
        {
            var angle = (startAngleDegrees + (step * i)) * (MathF.PI / 180f);
            path.Add(new Vector2(
                center.X + (MathF.Cos(angle) * radius),
                center.Y + (MathF.Sin(angle) * radius)));
        }
    }
}
