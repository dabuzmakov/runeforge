namespace runeforge.Models;

public sealed class GridPositionComponent
{
    public GridPositionComponent(int row, int column)
    {
        Row = row;
        Column = column;
    }

    public int Row { get; }

    public int Column { get; }
}
