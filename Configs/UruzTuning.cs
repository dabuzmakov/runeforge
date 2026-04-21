namespace runeforge.Configs;

public static class UruzTuning
{
    public const float BaseAttackIntervalSeconds = 0.88f;
    public const float BaseDamage = 0.92f;
    public const float TornadoSpeedPixelsPerSecond = 280f;
    public const float TornadoHitRadius = 26f;
    public const float TornadoFrameDurationSeconds = 0.08f;
    public const float TornadoScale = 1.7f;

    private static readonly float[] TornadoCooldownSecondsByTier =
    [
        10f,
        9f,
        8f,
        7f,
        6f
    ];

    private static readonly float[] TornadoDamageByTier =
    [
        2.0f,
        4.0f,
        6.0f,
        8.0f,
        10.0f
    ];

    private static readonly float[] MarkedHealthDamagePercentByTier =
    [
        0.02f,
        0.04f,
        0.06f,
        0.08f,
        0.10f
    ];

    public static float GetTornadoCooldownSeconds(int tier)
    {
        return TornadoCooldownSecondsByTier[RuneTierTuning.Clamp(tier) - 1];
    }

    public static float GetTornadoDamage(int tier)
    {
        return TornadoDamageByTier[RuneTierTuning.Clamp(tier) - 1];
    }

    public static float GetMarkedHealthDamagePercent(int tier)
    {
        return MarkedHealthDamagePercentByTier[RuneTierTuning.Clamp(tier) - 1];
    }
}
