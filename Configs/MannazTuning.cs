namespace runeforge.Configs;

public static class MannazTuning
{
    public const float StormCooldownSeconds = 11.5f;
    public const int LightningEffectRowIndex = 8;
    public const float LightningTickFrameDurationSeconds = 0.1f;
    public const float LightningEffectScale = 3f;
    public static readonly System.Drawing.Color StormAuraCoreColor = System.Drawing.Color.FromArgb(24, 126, 245);
    public static readonly System.Drawing.Color StormAuraGlowColor = System.Drawing.Color.FromArgb(110, 178, 255);

    private static readonly float[] LightningCurrentHealthDamagePercentByTier =
    [
        0.045f,
        0.060f,
        0.075f,
        0.090f,
        0.105f
    ];

    private static readonly float[] LightningBaseDamageByTier =
    [
        2.6f,
        3.8f,
        5.4f,
        7.2f,
        9.4f
    ];

    public static float GetLightningCurrentHealthDamagePercent(int tier)
    {
        return LightningCurrentHealthDamagePercentByTier[RuneTierTuning.Clamp(tier) - 1];
    }

    public static float GetLightningBaseDamage(int tier)
    {
        return LightningBaseDamageByTier[RuneTierTuning.Clamp(tier) - 1] * RuneCombatTuning.GlobalDamageMultiplier;
    }

    public static int GetTargetCount(int tier)
    {
        return RuneTierTuning.Clamp(tier) switch
        {
            1 => 1,
            2 => 2,
            3 => 2,
            4 => 3,
            _ => 3
        };
    }
}
