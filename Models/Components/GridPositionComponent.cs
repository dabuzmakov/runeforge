namespace runeforge.Models;

public sealed class GridPositionComponent
{
    public GridPositionComponent(int row, int column)
    {
        Row = row;
        Column = column;
    }

    public int Row { get; private set; }

    public int Column { get; private set; }

    public void MoveTo(int row, int column)
    {
        Row = row;
        Column = column;
    }
}
