namespace runeforge.Configs;

public static class AnsuzTuning
{
    public const int EffectRowIndex = 3;
    public const float AllyHitEffectScale = 1.6f;
    public const float AllyHealthMultiplier = 0.35f;
    public const float AllySpeedMultiplier = 0.75f;
    public const float AllyRadiusMultiplier = 0.85f;

    private static readonly float[] SpawnChanceByTier =
    [
        0.04f,
        0.06f,
        0.08f,
        0.10f,
        0.13f
    ];

    public static bool ShouldSpawnAlly(int tier)
    {
        var clampedTier = RuneTierTuning.Clamp(tier);
        return Random.Shared.NextSingle() < SpawnChanceByTier[clampedTier - 1];
    }
}
