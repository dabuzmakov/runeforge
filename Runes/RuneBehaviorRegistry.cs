using runeforge.Models;

namespace runeforge.Runes;

public static class RuneBehaviorRegistry
{
    private static readonly IRuneBehavior DefaultBehavior = new DefaultRuneBehavior();

    private static readonly IReadOnlyDictionary<RuneType, IRuneBehavior> Behaviors = new Dictionary<RuneType, IRuneBehavior>
    {
        { RuneType.Algiz, new AlgizRuneBehavior() },
        { RuneType.Ansuz, new AnsuzRuneBehavior() },
        { RuneType.Berkano, new BerkanoRuneBehavior() },
        { RuneType.Dagaz, new DagazRuneBehavior() },
        { RuneType.Ehwaz, new EhwazRuneBehavior() },
        { RuneType.Eiwaz, new EiwazRuneBehavior() },
        { RuneType.Gebo, new GeboRuneBehavior() },
        { RuneType.Isa, new IsaRuneBehavior() },
        { RuneType.Kenaz, new KenazRuneBehavior() },
        { RuneType.Laguz, new LaguzRuneBehavior() },
        { RuneType.Nauthiz, new NauthizRuneBehavior() },
        { RuneType.Raidho, new RaidhoRuneBehavior() },
        { RuneType.Sowilo, new SowiloRuneBehavior() },
        { RuneType.Thurisaz, new ThurisazRuneBehavior() },
        { RuneType.Uruz, new UruzRuneBehavior() },
        { RuneType.Wunjo, new WunjoRuneBehavior() }
    };

    public static IRuneBehavior Get(RuneType runeType)
    {
        return Behaviors.TryGetValue(runeType, out var behavior)
            ? behavior
            : DefaultBehavior;
    }
}
