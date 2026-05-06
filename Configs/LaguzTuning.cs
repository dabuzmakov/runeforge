using System.Drawing;
using runeforge.Models;

namespace runeforge.Configs;

public static class LaguzTuning
{
    public const int MaxBlackHoleCount = 8;
    public const float CooldownSeconds = 4.25f;
    public const float BlackHoleRadius = 56f;
    public const float BlackHoleInnerRadius = 14f;
    public const float BlackHoleSpawnScaleDurationSeconds = 0.22f;
    public const float BlackHoleEffectScale = 2.45f;
    public const float BlackHoleAnimationFrameDurationSeconds = 0.07f;
    public const float OrbSpeedPixelsPerSecond = 540f;
    public const float OrbRadius = 8f;
    public const float OrbTailLength = 22f;
    public static readonly Color OrbColor = Color.FromArgb(180, 46, 169);
    public static readonly Color OrbCoreColor = Color.FromArgb(247, 184, 255);

    private static readonly float[] AdditionalBlackHoleChanceByTier =
    [
        0f,
        0.02f,
        0.04f,
        0.06f,
        0.08f
    ];

    private static readonly float[] ThirdBlackHoleChanceByTier =
    [
        0f,
        0f,
        0f,
        0.02f,
        0.03f
    ];

    private static readonly float[] ExecuteChanceByTier =
    [
        0.0017f,
        0.0028f,
        0.0038f,
        0.0052f,
        0.0069f
    ];

    private static readonly float[] SlowPercentByTier =
    [
        0.10f,
        0.14f,
        0.18f,
        0.23f,
        0.28f
    ];

    private static readonly float[] LifetimeByTier =
    [
        1.4f,
        1.7f,
        2.0f,
        2.3f,
        2.6f
    ];

    public static float GetExecuteChance(int tier)
    {
        return ExecuteChanceByTier[RuneTierTuning.Clamp(tier) - 1];
    }

    public static float GetSlowPercent(int tier)
    {
        return SlowPercentByTier[RuneTierTuning.Clamp(tier) - 1];
    }

    public static float GetBlackHoleLifetime(int tier)
    {
        return LifetimeByTier[RuneTierTuning.Clamp(tier) - 1];
    }

    public static bool CanRuneTriggerExecute(RuneType runeType)
    {
        return runeType != RuneType.Laguz;
    }

    public static int GetBlackHoleCount(int tier)
    {
        var clampedTier = RuneTierTuning.Clamp(tier);
        var blackHoleCount = 1;

        if (Random.Shared.NextSingle() < AdditionalBlackHoleChanceByTier[clampedTier - 1])
        {
            blackHoleCount++;
        }

        if (Random.Shared.NextSingle() < ThirdBlackHoleChanceByTier[clampedTier - 1])
        {
            blackHoleCount++;
        }

        return Math.Min(3, blackHoleCount);
    }
}
