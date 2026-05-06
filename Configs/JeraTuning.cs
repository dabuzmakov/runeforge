namespace runeforge.Configs;

public static class JeraTuning
{
    public const float DamageBonusPercentPerStack = 3f;
    public const float AttackSpeedBonusPercentPerStack = 1.5f;
    public const int EffectRowIndex = 2;
    public const float EffectScale = 2.1f;

    public static float GetDamageMultiplier(int sharedStacks)
    {
        return 1f + ((Math.Max(0, sharedStacks) * DamageBonusPercentPerStack) / 100f);
    }

    public static float GetAttackSpeedMultiplier(int sharedStacks)
    {
        return 1f + ((Math.Max(0, sharedStacks) * AttackSpeedBonusPercentPerStack) / 100f);
    }
}
