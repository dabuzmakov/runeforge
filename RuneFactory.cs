namespace runeforge;

public class RuneFactory
{
    private Random _random = new();

    public Rune CreateRandom()
    {
        var type = (RuneType)_random.Next(0, 2);

        return type switch
        {
            RuneType.Uruz => new Rune(type, new AttackBehavior()),
            RuneType.Isa => new Rune(type, new SlowBehavior()),
            _ => throw new Exception()
        };
    }
}