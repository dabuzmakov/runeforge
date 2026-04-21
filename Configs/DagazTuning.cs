using runeforge.Models;

namespace runeforge.Configs;

public static class DagazTuning
{
    private static readonly float[] MultiShotChancePercentByTier =
    [
        16f,
        22f,
        28f,
        34f,
        40f
    ];

    private static readonly float[] AdditionalProjectileDamageMultiplierByTier =
    [
        0.5f,
        0.3f,
        0.2333f,
        0.2f,
        0.2f
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
