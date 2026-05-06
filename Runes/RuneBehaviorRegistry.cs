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
        { RuneType.Fehu, new FehuRuneBehavior() },
        { RuneType.Gebo, new GeboRuneBehavior() },
        { RuneType.Ingwaz, new IngwazRuneBehavior() },
        { RuneType.Isa, new IsaRuneBehavior() },
        { RuneType.Jera, new JeraRuneBehavior() },
        { RuneType.Kenaz, new KenazRuneBehavior() },
        { RuneType.Laguz, new LaguzRuneBehavior() },
        { RuneType.Mannaz, new MannazRuneBehavior() },
        { RuneType.Nauthiz, new NauthizRuneBehavior() },
        { RuneType.Othala, new OthalaRuneBehavior() },
        { RuneType.Perthro, new PerthroRuneBehavior() },
        { RuneType.Raidho, new RaidhoRuneBehavior() },
        { RuneType.Sowilo, new SowiloRuneBehavior() },
        { RuneType.Thurisaz, new ThurisazRuneBehavior() },
        { RuneType.Tiwaz, new TiwazRuneBehavior() },
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
