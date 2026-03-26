namespace runeforge;

public class GameModel
{
    public GameTable Table { get; } = new();
    public List<Enemy> Enemies { get; } = new();

    private Random _random = new();
    private RuneFactory _factory = new();

    public void SpawnRandomRune()
    {
        var emptyCells = new List<(int x, int y)>();
        for (var y = 0; y < Table.Size; y++)
            for (var x = 0; x < Table.Size; x++)
                if (Table.Get(x, y) == null)
                    emptyCells.Add((x, y));

        if (emptyCells.Count == 0)
            return;

        var index = _random.Next(emptyCells.Count);
        var (randomX, randomY) = emptyCells[index];

        var rune = _factory.CreateRandom();
        Table.Place(randomX, randomY, rune);
    }

    public void SpawnEnemy()
    {
        Enemies.Add(new Enemy(50));
    }

    public void Update(double dt)
    {
        if (Enemies.Count < 5)
            SpawnEnemy();

        foreach (var enemy in Enemies)
            enemy.Update(dt);

        foreach (var (x, y, rune) in GetAllRunes())
            rune.Update(this, dt);

        Enemies.RemoveAll(e => e.IsDead);
    }

    public IEnumerable<(int x, int y, Rune rune)> GetAllRunes()
    {
        for (var y = 0; y < Table.Size; y++)
        for (var x = 0; x < Table.Size; x++)
        {
            var rune = Table.Get(x, y);
            if (rune != null) 
                yield return (x, y, rune);
        }
    }
}