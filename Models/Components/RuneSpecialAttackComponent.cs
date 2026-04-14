namespace runeforge.Models;

public sealed class RuneSpecialAttackComponent
{
    public int AttackCount { get; private set; }

    public void RegisterAttack()
    {
        AttackCount++;
    }

    public bool ShouldTriggerOnNextAttack(int frequency)
    {
        var clampedFrequency = Math.Max(1, frequency);
        return ((AttackCount + 1) % clampedFrequency) == 0;
    }
}
