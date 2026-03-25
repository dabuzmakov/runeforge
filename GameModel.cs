namespace runeforge;

public class GameModel
{
    public GameTable Table { get; } = new();
    public List<Enemy> Enemies { get; } = new();

    private Random _random = new();

    public void SpawnRandomRune()
    {
        var x = _random.Next(0, Table.Size);
        var y = _random.Next(0, Table.Size);

        if (Table.Get(x, y) == null)
        {
            RuneType type = (RuneType)_random.Next(0, 2);
            var rune = new Rune(type);

            Table.Place(x, y, rune);
        }
    }

    public void SpawnEnemy()
    {
        Enemies.Add(new Enemy(50));
    }

    public void Update()
    {
        if (Enemies.Count == 0) SpawnEnemy();

        var enemy = Enemies.FirstOrDefault(e => !e.IsDead);

        if (enemy == null) return;

        for (var y = 0; y < Table.Size; y++)
        for (var x = 0; x < Table.Size; x++)
        {
            var rune = Table.Get(x, y);
            if (rune != null)
                enemy.TakeDamage(1);
        }

        Enemies.RemoveAll(e => e.IsDead);
    }
}