using System.Numerics;

namespace runeforge.Models;

public sealed class GameBoard
{
    private const int TableSize = 400;
    private const int PathMargin = 70;
    private const int BagHalfSize = 80;
    private const int BottomOffset = 200;

    public GameBoard(int screenWidth, int screenHeight)
    {
        TableBounds = CreateTableBounds(screenWidth, screenHeight);
        Grid = new TableGrid(TableBounds);
        BagBounds = CreateBagBounds(screenWidth, screenHeight);
        Path = CreatePath(TableBounds, screenHeight);
    }

    public Rectangle TableBounds { get; }

    public Rectangle BagBounds { get; }

    public TableGrid Grid { get; }

    public IReadOnlyList<Vector2> Path { get; }

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

    private static IReadOnlyList<Vector2> CreatePath(Rectangle tableBounds, int screenHeight)
    {
        var left = tableBounds.Left - PathMargin;
        var top = tableBounds.Top - PathMargin;
        var right = tableBounds.Right + PathMargin;
        var bottom = tableBounds.Bottom + PathMargin;

        return
        [
            new Vector2(tableBounds.Left + 50, bottom),
            new Vector2(left, bottom),
            new Vector2(left, top),
            new Vector2(right, top),
            new Vector2(right, bottom),
            new Vector2(tableBounds.Right - 50, bottom)
        ];
    }
}
