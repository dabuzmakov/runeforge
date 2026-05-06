using runeforge.Models;

namespace runeforge.Configs;

public static class DagazTuning
{
    private static readonly float[] MultiShotChancePercentByTier =
    [
        12f,
        17f,
        22f,
        27f,
        32f
    ];

    private static readonly float[] AdditionalProjectileDamageMultiplierByTier =
    [
        0.42f,
        0.26f,
        0.20f,
        0.18f,
        0.18f
    ];

    public static float GetMultiShotChancePercent(int tier)
    {
        return MultiShotChancePercentByTier[RuneTierTuning.Clamp(tier) - 1];
    }

    public static int GetAdditionalProjectileCount(int tier)
    {
        return RuneTierTuning.Clamp(tier);
    }

    public static float GetAdditionalProjectileDamageMultiplier(int tier)
    {
        return AdditionalProjectileDamageMultiplierByTier[RuneTierTuning.Clamp(tier) - 1];
    }

    public static bool CanReceiveMultiShotBuff(RuneType runeType)
    {
        return runeType is not RuneType.Algiz
            and not RuneType.Dagaz
            and not RuneType.Eiwaz
            and not RuneType.Gebo
            and not RuneType.Isa
            and not RuneType.Thurisaz
            and not RuneType.Wunjo;
    }
}
