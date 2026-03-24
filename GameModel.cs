namespace runeforge;

public class GameModel
{
    public GameTable Table { get; } = new GameTable();
    private Random random = new Random();

    public void SpawnRandomRune()
    {
        var x = random.Next(0, Table.Size);
        var y = random.Next(0, Table.Size);

        if (Table.Get(x, y) == null)
        {
            RuneType type = (RuneType)random.Next(0, 3);
            var rune = new Rune(type);

            Table.Place(x, y, rune);
        }
    }
}