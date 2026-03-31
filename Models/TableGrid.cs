using System.Drawing;
using System.Numerics;

namespace runeforge.Models;

public sealed class TableGrid
{
    public const int Size = 4;

    private readonly GridCell[] _cells;

    public TableGrid(Rectangle tableBounds)
    {
        var cellSize = tableBounds.Width / Size;
        _cells = new GridCell[Size * Size];

        for (var row = 0; row < Size; row++)
        {
            for (var column = 0; column < Size; column++)
            {
                var bounds = new Rectangle(
                    tableBounds.Left + (column * cellSize),
                    tableBounds.Top + (row * cellSize),
                    cellSize,
                    cellSize);

                _cells[(row * Size) + column] = new GridCell(
                    row,
                    column,
                    bounds,
                    new Vector2(bounds.Left + (bounds.Width * 0.5f), bounds.Top + (bounds.Height * 0.5f)));
            }
        }
    }

    public IReadOnlyList<GridCell> Cells => _cells;

    public GridCell GetCell(int row, int column)
        => _cells[(row * Size) + column];

    public readonly struct GridCell
    {
        public GridCell(int row, int column, Rectangle bounds, Vector2 center)
        {
            Row = row;
            Column = column;
            Bounds = bounds;
            Center = center;
        }

        public int Row { get; }

        public int Column { get; }

        public Rectangle Bounds { get; }

        public Vector2 Center { get; }
    }
}
