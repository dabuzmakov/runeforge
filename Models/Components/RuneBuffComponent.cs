using runeforge.Configs;

namespace runeforge.Models;

public sealed class RuneBuffComponent
{
    public float AttackSpeedBonusPercent { get; private set; }
    public float CriticalHitBonusPercent { get; private set; }
    public int MultiShotDagazTier { get; private set; }

    public bool HasAttackSpeedBuff => AttackSpeedBonusPercent > 0.001f;

    public bool HasCriticalHitBuff => CriticalHitBonusPercent > 0.001f;

    public bool HasMultiShotBuff => MultiShotDagazTier > 0;

    public float MultiShotChancePercent => HasMultiShotBuff
        ? DagazTuning.GetMultiShotChancePercent(MultiShotDagazTier)
        : 0f;

    public int AdditionalProjectileCount => HasMultiShotBuff
        ? DagazTuning.GetAdditionalProjectileCount(MultiShotDagazTier)
        : 0;

    public float AdditionalProjectileDamageMultiplier => HasMultiShotBuff
        ? DagazTuning.GetAdditionalProjectileDamageMultiplier(MultiShotDagazTier)
        : 0f;

    public void Reset()
    {
        AttackSpeedBonusPercent = 0f;
        CriticalHitBonusPercent = 0f;
        MultiShotDagazTier = 0;
    }

    public void ApplyAttackSpeedBonusPercent(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        AttackSpeedBonusPercent = Math.Max(AttackSpeedBonusPercent, amount);
    }

    public void ApplyCriticalHitBonusPercent(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        CriticalHitBonusPercent = Math.Max(CriticalHitBonusPercent, amount);
    }

    public void ApplyMultiShotDagazTier(int dagazTier)
    {
        MultiShotDagazTier = Math.Max(MultiShotDagazTier, dagazTier);
    }
}
