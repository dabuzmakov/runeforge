using System.Drawing;
using runeforge.Models;

namespace runeforge.Configs;

public static class RuneDatabase
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

    private static readonly IReadOnlyDictionary<RuneType, RuneData> Entries = CreateEntries();

    public static IReadOnlyList<RuneData> All { get; } = Entries.Values.OrderBy(static rune => rune.Type).ToArray();

    public static IReadOnlyList<RuneType> AllTypes { get; } = All.Select(static rune => rune.Type).ToArray();

    public static RuneData Get(RuneType type)
    {
        if (Entries.TryGetValue(type, out var runeData))
        {
            return runeData;
        }

        throw new InvalidOperationException($"No rune data registered for {type}.");
    }

    private static IReadOnlyDictionary<RuneType, RuneData> CreateEntries()
    {
        var entries = new Dictionary<RuneType, RuneData>();

        Add(entries, RuneType.Kenaz, RuneColor.Blue, baseAttackRate: 0.78f, baseDamage: 0.98f, effectType: RuneEffectType.KenazSplash, effectPower: KenazTuning.SplashDamageMultiplier);
        Add(entries, RuneType.Eiwaz, RuneColor.Blue);
        Add(entries, RuneType.Mannaz, RuneColor.Blue);

        Add(entries, RuneType.Fehu, RuneColor.Red);
        Add(entries, RuneType.Wunjo, RuneColor.Red);
        Add(entries, RuneType.Algiz, RuneColor.Red);
        Add(entries, RuneType.Ingwaz, RuneColor.Red);

        Add(entries, RuneType.Ansuz, RuneColor.Green);
        Add(entries, RuneType.Isa, RuneColor.Green, baseAttackRate: 0.82f, baseDamage: 1.08f, effectType: RuneEffectType.IsaSlow, effectPower: 1f);
        Add(entries, RuneType.Berkano, RuneColor.Green);

        Add(entries, RuneType.Raidho, RuneColor.Cyan, baseAttackRate: 0.72f, baseDamage: 0.92f);
        Add(entries, RuneType.Jera, RuneColor.Cyan);
        Add(entries, RuneType.Ehwaz, RuneColor.Cyan);

        Add(entries, RuneType.Uruz, RuneColor.Orange);
        Add(entries, RuneType.Hagalaz, RuneColor.Orange);
        Add(entries, RuneType.Sowilo, RuneColor.Orange, baseAttackRate: 0.78f, baseDamage: 0.92f);
        Add(entries, RuneType.Dagaz, RuneColor.Orange);

        Add(entries, RuneType.Thurisaz, RuneColor.Yellow);
        Add(entries, RuneType.Nauthiz, RuneColor.Yellow);
        Add(entries, RuneType.Tiwaz, RuneColor.Yellow);
        Add(entries, RuneType.Othala, RuneColor.Yellow);

        Add(entries, RuneType.Gebo, RuneColor.Purple, baseAttackRate: 0.82f, baseDamage: 0.78f);
        Add(entries, RuneType.Perthro, RuneColor.Purple);
        Add(entries, RuneType.Laguz, RuneColor.Purple);

        return entries;
    }

    private static void Add(
        IDictionary<RuneType, RuneData> entries,
        RuneType type,
        RuneColor color,
        float? baseAttackRate = null,
        float? baseDamage = null,
        RuneEffectType effectType = RuneEffectType.None,
        float effectPower = 0f)
    {
        entries.Add(type, new RuneData(
            type,
            color,
            type.ToString(),
            baseAttackRate ?? DefaultAttackRate,
            baseDamage ?? DefaultDamage,
            ProjectileColors[color],
            DefaultProjectileSpeed,
            DefaultProjectileRadius,
            DefaultRuneRadius,
            effectType,
            effectPower));
    }
}
