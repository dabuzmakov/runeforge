using runeforge.Models;

namespace runeforge.Runes;

public static class RuneBehaviorRegistry
{
    private static readonly IRuneBehavior DefaultBehavior = new DefaultRuneBehavior();

    private static readonly IReadOnlyDictionary<RuneType, IRuneBehavior> Behaviors = new Dictionary<RuneType, IRuneBehavior>
    {
        { RuneType.Gebo, new GeboRuneBehavior() },
        { RuneType.Isa, new IsaRuneBehavior() },
        { RuneType.Kenaz, new KenazRuneBehavior() },
        { RuneType.Raidho, new RaidhoRuneBehavior() },
        { RuneType.Sowilo, new SowiloRuneBehavior() }
    };

    public static IRuneBehavior Get(RuneType runeType)
    {
        return Behaviors.TryGetValue(runeType, out var behavior)
            ? behavior
            : DefaultBehavior;
    }
}
