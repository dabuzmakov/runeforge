namespace runeforge;

public class Rune
{
    public RuneType Type { get; init; }
    public int Tier { get; private set; }

    private IRuneBehavior _behavior;

    public Rune(RuneType type, IRuneBehavior behavior, int tier = 1)
    {
        Type = type;
        Tier = tier;
        _behavior = behavior;
    }

    public void Update(GameModel model, double dt)
    {
        _behavior.Update(this, model, dt);
    }
}