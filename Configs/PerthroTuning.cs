namespace runeforge.Configs;

public static class PerthroTuning
{
    public const float OvershootDistance = 62f;
    public const float MinimumOutboundDistance = 148f;
    public const float BoomerangRadius = 20f;
    public const float RotationSpeedRadiansPerSecond = 18.5f;
    public const float CooldownSeconds = 1.85f;
    public const float MinLateralOffsetDistance = 28f;
    public const float MaxLateralOffsetDistance = 74f;
    public const float MinLateralOffsetFactor = 0.16f;
    public const float MaxLateralOffsetFactor = 0.30f;
    public static readonly System.Drawing.Color MarkOutlineColor = System.Drawing.Color.FromArgb(232, 72, 228);

    private static readonly float[] DamageByTier =
    [
        0.76f,
        1.16f,
        1.70f,
        2.32f,
        3.08f
    ];

    private static readonly float[] SpeedByTier =
    [
        360f,
        420f,
        485f,
        555f,
        630f
    ];

    private static readonly float[] ExecuteHealthPercentByTier =
    [
        0.0065f,
        0.0115f,
        0.0165f,
        0.0220f,
        0.0280f
    ];

    public static float GetDamage(int tier)
    {
        return DamageByTier[RuneTierTuning.Clamp(tier) - 1] * RuneCombatTuning.GlobalDamageMultiplier;
    }

    public static float GetSpeed(int tier)
    {
        return SpeedByTier[RuneTierTuning.Clamp(tier) - 1];
    }

    public static float GetExecuteHealthPercentThreshold(int tier)
    {
        return ExecuteHealthPercentByTier[RuneTierTuning.Clamp(tier) - 1];
    }
}
