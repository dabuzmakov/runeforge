using System.Drawing;
using runeforge.Models;

namespace runeforge.Configs;

public static class RuneCatalog
{
    private const float DefaultAttackRate = 0.8f;
    private const float DefaultDamage = 1f;
    private const float DefaultProjectileSpeed = 600f;
    private const float DefaultProjectileRadius = 7f;
    private const float DefaultRuneRadius = 22f;

    private static readonly IReadOnlyDictionary<RuneColor, Color> ProjectileColors = new Dictionary<RuneColor, Color>
    {
        { RuneColor.Blue, Color.FromArgb(25, 119, 241) },
        { RuneColor.Red, Color.FromArgb(214, 56, 56) },
        { RuneColor.Green, Color.FromArgb(86, 179, 0) },
        { RuneColor.Cyan, Color.FromArgb(67, 197, 255) },
        { RuneColor.Orange, Color.FromArgb(237, 140, 34) },
        { RuneColor.Yellow, Color.FromArgb(234, 204, 52) },
        { RuneColor.Purple, Color.FromArgb(156, 92, 255) }
    };

    private static readonly IReadOnlyDictionary<RuneType, RuneConfig> Configs = CreateConfigs();

    public static IReadOnlyList<RuneConfig> All { get; } = Configs.Values.OrderBy(static config => config.Type).ToArray();

    public static IReadOnlyList<RuneType> AllTypes { get; } = All.Select(static config => config.Type).ToArray();

    public static RuneConfig Get(RuneType type)
    {
        if (Configs.TryGetValue(type, out var config))
        {
            return config;
        }

        throw new InvalidOperationException($"No rune config registered for {type}.");
    }

    private static IReadOnlyDictionary<RuneType, RuneConfig> CreateConfigs()
    {
        var configs = new Dictionary<RuneType, RuneConfig>();

        Add(configs, RuneType.Kenaz, RuneColor.Blue, RuneEffectType.KenazSplash, 0.2f);
        Add(configs, RuneType.Eiwaz, RuneColor.Blue);
        Add(configs, RuneType.Mannaz, RuneColor.Blue);

        Add(configs, RuneType.Fehu, RuneColor.Red);
        Add(configs, RuneType.Wunjo, RuneColor.Red);
        Add(configs, RuneType.Algiz, RuneColor.Red);
        Add(configs, RuneType.Ingwaz, RuneColor.Red);

        Add(configs, RuneType.Ansuz, RuneColor.Green);
        Add(configs, RuneType.Isa, RuneColor.Green, RuneEffectType.IsaSlow, 1f);
        Add(configs, RuneType.Berkano, RuneColor.Green);

        Add(configs, RuneType.Raidho, RuneColor.Cyan);
        Add(configs, RuneType.Jera, RuneColor.Cyan);
        Add(configs, RuneType.Ehwaz, RuneColor.Cyan);

        Add(configs, RuneType.Uruz, RuneColor.Orange);
        Add(configs, RuneType.Hagalaz, RuneColor.Orange);
        Add(configs, RuneType.Sowilo, RuneColor.Orange);
        Add(configs, RuneType.Dagaz, RuneColor.Orange);

        Add(configs, RuneType.Thurisaz, RuneColor.Yellow);
        Add(configs, RuneType.Nauthiz, RuneColor.Yellow);
        Add(configs, RuneType.Tiwaz, RuneColor.Yellow);
        Add(configs, RuneType.Othala, RuneColor.Yellow);

        Add(configs, RuneType.Gebo, RuneColor.Purple);
        Add(configs, RuneType.Perthro, RuneColor.Purple);
        Add(configs, RuneType.Laguz, RuneColor.Purple);

        return configs;
    }

    private static void Add(
        IDictionary<RuneType, RuneConfig> configs,
        RuneType type,
        RuneColor color,
        RuneEffectType effectType = RuneEffectType.None,
        float effectPower = 0f)
    {
        configs.Add(type, new RuneConfig(
            type,
            color,
            type.ToString(),
            DefaultAttackRate,
            DefaultDamage,
            ProjectileColors[color],
            DefaultProjectileSpeed,
            DefaultProjectileRadius,
            DefaultRuneRadius,
            effectType,
            effectPower));
    }
}
