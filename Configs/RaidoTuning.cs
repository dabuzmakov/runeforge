namespace runeforge.Configs;

public static class RaidoTuning
{
    public const int OverloadEffectRowIndex = 2;
    public const float OverloadIntervalSeconds = 5f;
    public const float OverloadEffectScale = 1.95f;

    private static readonly float[] BaseAttackIntervalSecondsByTier =
    [
        0.72f,
        0.70f,
        0.68f,
        0.66f,
        0.64f,
        0.62f,
        0.60f,
        0.58f,
        0.56f,
        0.54f
    ];

    private static readonly float[] OverloadDurationSecondsByTier =
    [
        2.0f,
        3.0f,
        4.0f,
        5.0f,
        6.0f,
        7.0f,
        8.0f,
        9.0f,
        10.0f,
        11.0f
    ];

    private static readonly float[] OverloadAttackSpeedMultiplierByTier =
    [
        1.70f,
        1.75f,
        1.80f,
        1.85f,
        1.90f,
        1.95f,
        2.00f,
        2.05f,
        2.10f,
        2.15f
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
