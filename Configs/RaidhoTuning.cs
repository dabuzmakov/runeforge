namespace runeforge.Configs;

public static class RaidhoTuning
{
    public const int OverloadEffectRowIndex = 2;
    public const float OverloadIntervalSeconds = 5f;
    public const float OverloadEffectScale = 1.95f;

    private static readonly float[] BaseAttackIntervalSecondsByTier =
    [
        0.5f,
        0.45f,
        0.40f,
        0.35f,
        0.30f
    ];

    private static readonly float[] OverloadDurationSecondsByTier =
    [
        3.0f,
        5.0f,
        7.0f,
        9.0f,
        11.0f
    ];

    private static readonly float[] OverloadAttackSpeedMultiplierByTier =
    [
        2.0f,
        2.5f,
        3.0f,
        3.5f,
        4.0f
    ];

    public static float GetBaseAttackIntervalSeconds(int tier)
    {
        var clampedTier = RuneTierTuning.Clamp(tier);
        return BaseAttackIntervalSecondsByTier[clampedTier - 1];
    }

    public static float GetOverloadDurationSeconds(int tier)
    {
        var clampedTier = RuneTierTuning.Clamp(tier);
        return OverloadDurationSecondsByTier[clampedTier - 1];
    }

    public static float GetOverloadAttackSpeedMultiplier(int tier)
    {
        var clampedTier = RuneTierTuning.Clamp(tier);
        return OverloadAttackSpeedMultiplierByTier[clampedTier - 1];
    }
}
