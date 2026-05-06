namespace runeforge.Configs;

public static class TiwazTuning
{
    public const float DischargeDurationSeconds = 4.4f;
    public const int ChargeEffectRowIndex = 4;
    public const float ChargeEffectScale = 1.5f;

    private static readonly float[] ChargeFractionByTier =
    [
        0.56f,
        0.60f,
        0.63f,
        0.66f,
        0.70f
    ];

    private static readonly float[] DischargeShotDamageByTier =
    [
        9.6f,
        14.2f,
        18.8f,
        23.2f,
        27.8f
    ];

    public static float GetChargeFraction(int tier)
    {
        return ChargeFractionByTier[RuneTierTuning.Clamp(tier) - 1];
    }

    public static float GetDischargeShotDamage(int tier)
    {
        return DischargeShotDamageByTier[RuneTierTuning.Clamp(tier) - 1];
    }
}
