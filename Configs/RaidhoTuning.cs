namespace runeforge.Configs;

public static class RaidhoTuning
{
    public const int OverloadEffectRowIndex = 2;
    public const float OverloadIntervalSeconds = 5f;
    public const float OverloadEffectScale = 1.95f;

    private static readonly float[] BaseAttackIntervalSecondsByTier =
    [
        0.62f,
        0.58f,
        0.54f,
        0.50f,
        0.46f
    ];

    private static readonly float[] OverloadDurationSecondsByTier =
    [
        2.5f,
        3.5f,
        4.5f,
        5.5f,
        6.5f
    ];

    private static readonly float[] OverloadAttackSpeedMultiplierByTier =
    [
        1.35f,
        1.50f,
        1.70f,
        1.90f,
        2.10f
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
