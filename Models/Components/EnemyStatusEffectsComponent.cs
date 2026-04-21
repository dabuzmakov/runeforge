using runeforge.Configs;

namespace runeforge.Models;

public sealed class EnemyStatusEffectsComponent
{
    private const float SecondarySlowContributionFactor = 0.5f;
    private readonly List<EnemyStatusEffect> _activeEffects = new(4);
    private readonly List<float> _shatterStackBonusPercents = new(NauthizTuning.MaxShatterStacks);
    private float _movementSlowPercent;

    public float MovementSpeedMultiplier => 1f - _movementSlowPercent;

    public int ShatterStackCount => _shatterStackBonusPercents.Count;

    public float IncomingDamageMultiplier => 1f + (_shatterStackBonusPercents.Sum() / 100f);

    public bool IsIsaSlowed { get; private set; }

    public bool IsLaguzSlowed { get; private set; }

    public float Update(float deltaTime)
    {
        var strongestIsaSlow = 0f;
        var strongestLaguzSlow = 0f;
        var totalPoisonDamage = 0f;

        for (var i = _activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = _activeEffects[i];
            var remainingDuration = effect.RemainingDurationSeconds - deltaTime;
            var timeUntilNextTrigger = effect.TimeUntilNextTriggerSeconds - deltaTime;

            if (effect.Type == EnemyStatusEffectType.Poison)
            {
                while (timeUntilNextTrigger <= 0f && remainingDuration > 0f)
                {
                    totalPoisonDamage += effect.Strength;
                    timeUntilNextTrigger += effect.TriggerIntervalSeconds;
                }
            }

            if (remainingDuration <= 0f)
            {
                _activeEffects.RemoveAt(i);
                continue;
            }

            effect = effect with
            {
                RemainingDurationSeconds = remainingDuration,
                TimeUntilNextTriggerSeconds = effect.Type == EnemyStatusEffectType.Poison
                    ? timeUntilNextTrigger
                    : effect.TimeUntilNextTriggerSeconds
            };
            _activeEffects[i] = effect;

            if (effect.Type == EnemyStatusEffectType.IsaSlow)
            {
                strongestIsaSlow = Math.Max(strongestIsaSlow, effect.Strength);
            }
            else if (effect.Type == EnemyStatusEffectType.LaguzSlow)
            {
                strongestLaguzSlow = Math.Max(strongestLaguzSlow, effect.Strength);
            }
        }

        IsIsaSlowed = strongestIsaSlow > 0f;
        IsLaguzSlowed = strongestLaguzSlow > 0f;

        var primarySlow = Math.Max(strongestIsaSlow, strongestLaguzSlow);
        var secondarySlow = Math.Min(strongestIsaSlow, strongestLaguzSlow);
        _movementSlowPercent = Math.Clamp(
            primarySlow + (secondarySlow * (1f - primarySlow) * SecondarySlowContributionFactor),
            0f,
            0.95f);
        return totalPoisonDamage;
    }

    public void ApplyOrRefreshIsaSlow(float slowPercent, float durationSeconds)
    {
        ApplyOrRefreshSlow(EnemyStatusEffectType.IsaSlow, slowPercent, durationSeconds);
    }

    public void ApplyOrRefreshLaguzSlow(float slowPercent, float durationSeconds)
    {
        ApplyOrRefreshSlow(EnemyStatusEffectType.LaguzSlow, slowPercent, durationSeconds);
    }

    private void ApplyOrRefreshSlow(EnemyStatusEffectType effectType, float slowPercent, float durationSeconds)
    {
        slowPercent = Math.Clamp(slowPercent, 0f, 0.95f);
        durationSeconds = Math.Max(0f, durationSeconds);

        for (var i = 0; i < _activeEffects.Count; i++)
        {
            if (_activeEffects[i].Type != effectType)
            {
                continue;
            }

            _activeEffects[i] = new EnemyStatusEffect(
                effectType,
                Math.Max(_activeEffects[i].Strength, slowPercent),
                Math.Max(_activeEffects[i].RemainingDurationSeconds, durationSeconds),
                0f,
                0f);

            return;
        }

        _activeEffects.Add(new EnemyStatusEffect(
            effectType,
            slowPercent,
            durationSeconds,
            0f,
            0f));
    }

    public void ApplyPoison(float damagePerTick, float durationSeconds, float tickIntervalSeconds)
    {
        damagePerTick = Math.Max(0f, damagePerTick);
        durationSeconds = Math.Max(0f, durationSeconds);
        tickIntervalSeconds = Math.Max(0.01f, tickIntervalSeconds);

        for (var i = 0; i < _activeEffects.Count; i++)
        {
            if (_activeEffects[i].Type != EnemyStatusEffectType.Poison)
            {
                continue;
            }

            _activeEffects[i] = _activeEffects[i] with
            {
                RemainingDurationSeconds = Math.Max(_activeEffects[i].RemainingDurationSeconds, durationSeconds)
            };
            return;
        }

        _activeEffects.Add(new EnemyStatusEffect(
            EnemyStatusEffectType.Poison,
            damagePerTick,
            durationSeconds,
            tickIntervalSeconds,
            tickIntervalSeconds));
    }

    public void ApplyOrUpgradeShatter(float incomingDamageBonusPercent)
    {
        var clampedBonus = Math.Max(0f, incomingDamageBonusPercent);
        if (clampedBonus <= 0.001f)
        {
            return;
        }

        if (_shatterStackBonusPercents.Count < NauthizTuning.MaxShatterStacks)
        {
            _shatterStackBonusPercents.Add(clampedBonus);
            return;
        }

        var weakestStackIndex = 0;
        for (var i = 1; i < _shatterStackBonusPercents.Count; i++)
        {
            if (_shatterStackBonusPercents[i] < _shatterStackBonusPercents[weakestStackIndex])
            {
                weakestStackIndex = i;
            }
        }

        if (clampedBonus > _shatterStackBonusPercents[weakestStackIndex])
        {
            _shatterStackBonusPercents[weakestStackIndex] = clampedBonus;
        }
    }

    public float ApplyIncomingDamageMultiplier(float damage)
    {
        return Math.Max(0f, damage) * IncomingDamageMultiplier;
    }
}
