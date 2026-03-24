namespace runeforge;

public class GameTable
{
    private Rune[,] cells = new Rune[4, 4];

    public Rune Get(int x, int y)
        => cells[x, y];

    public void Place(int x, int y, Rune rune)
        => cells[x, y] = rune;

    public void Remove(int x, int y)
        => cells[x, y] = null;

    public int Size => 4;
}