namespace runeforge.Models;

public sealed class RuneBuffComponent
{
    public float AttackSpeedBonusPercent { get; private set; }

    public bool HasAttackSpeedBuff => AttackSpeedBonusPercent > 0.001f;

    public void Reset()
    {
        AttackSpeedBonusPercent = 0f;
    }

    public void ApplyAttackSpeedBonusPercent(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        AttackSpeedBonusPercent = Math.Max(AttackSpeedBonusPercent, amount);
    }
}
