namespace runeforge.Configs;

public static class AnsuzTuning
{
    public const int EffectRowIndex = 3;
    public const float AllyHitEffectScale = 1.6f;

    private static readonly float[] SpawnChanceByTier =
    [
        0.18f,
        0.30f,
        0.42f,
        0.54f,
        0.66f
    ];

    public static bool ShouldSpawnAlly(int tier)
    {
        var clampedTier = RuneTierTuning.Clamp(tier);
        return Random.Shared.NextSingle() < SpawnChanceByTier[clampedTier - 1];
    }
}
