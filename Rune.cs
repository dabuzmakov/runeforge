namespace runeforge;

public class Rune
{
    public RuneType Type { get; init; }
    public List<IRuneBehavior> Behaviors { get; init; }
    public int Tier { get; private set; }

    public Rune(RuneType type, int tier = 1)
    {
        Type = type;
        Tier = tier;
    }
}