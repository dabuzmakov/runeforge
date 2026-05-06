using runeforge.Configs;

namespace runeforge.Models;

public sealed class EnemyStatusEffectsComponent
{
    private const float SecondarySlowContributionFactor = 0.5f;
    private readonly List<EnemyStatusEffect> _activeEffects = new(4);
    private readonly List<float> _shatterStackBonusPercents = new(NauthizTuning.MaxShatterStacks);
    private float _movementSlowPercent;
    private float _burnRemainingDurationSeconds;
    private float _burnTimeUntilNextTickSeconds;
    private float _burnCurrentHealthDamagePercentPerTick;
    private float _burnBaseDamagePerTick;
    private RuneEntity? _burnSourceRune;
    private float _auraHealthRegenerationPercentPerSecond;
    private float _auraMoveSpeedPercent;
    private bool _hasAuraImmunity;

    public float MovementSpeedMultiplier => (1f - _movementSlowPercent) * (1f + _auraMoveSpeedPercent);

    public float AuraHealthRegenerationPercentPerSecond => _auraHealthRegenerationPercentPerSecond;

    public bool HasAuraImmunity => _hasAuraImmunity;

    public int ShatterStackCount => _shatterStackBonusPercents.Count;

    public float IncomingDamageMultiplier => 1f + (_shatterStackBonusPercents.Sum() / 100f);

    public bool IsIsaSlowed { get; private set; }

    public bool IsLaguzSlowed { get; private set; }

    public bool IsBurning => BurnStackCount > 0 && _burnRemainingDurationSeconds > 0.001f;

    public int BurnStackCount { get; private set; }

    public bool IsPerthroMarked { get; private set; }

    public bool IsMannazStormMarked { get; private set; }

    public bool IsFehuMarked => FehuMarkedByRune != null && FehuBonusRunePointPercent > 0.001f;

    public RuneEntity? FehuMarkedByRune { get; private set; }

    public float FehuBonusRunePointPercent { get; private set; }

    public EnemyStatusTickResult Update(float deltaTime, float currentHealth)
    {
        var strongestIsaSlow = 0f;
        var strongestLaguzSlow = 0f;
        var totalPoisonDamage = 0f;
        RuneEntity? poisonSourceRune = null;

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
                    poisonSourceRune ??= effect.SourceRune;
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

        if (_hasAuraImmunity)
        {
            IsIsaSlowed = false;
            IsLaguzSlowed = false;
            _movementSlowPercent = 0f;
        }

        var totalBurnDamage = UpdateBurning(deltaTime, currentHealth);
        return new EnemyStatusTickResult(totalPoisonDamage, totalBurnDamage, poisonSourceRune, _burnSourceRune);
    }

    public void ApplyOrRefreshIsaSlow(float slowPercent, float durationSeconds)
    {
        if (_hasAuraImmunity)
        {
            return;
        }

        ApplyOrRefreshSlow(EnemyStatusEffectType.IsaSlow, slowPercent, durationSeconds);
    }

    public void ApplyOrRefreshLaguzSlow(float slowPercent, float durationSeconds)
    {
        if (_hasAuraImmunity)
        {
            return;
        }

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

            if (slowPercent + 0.001f < _activeEffects[i].Strength)
            {
                return;
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

    public void ApplyPoison(float damagePerTick, float durationSeconds, float tickIntervalSeconds, RuneEntity? sourceRune = null)
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
                RemainingDurationSeconds = Math.Max(_activeEffects[i].RemainingDurationSeconds, durationSeconds),
                SourceRune = sourceRune ?? _activeEffects[i].SourceRune
            };
            return;
        }

        _activeEffects.Add(new EnemyStatusEffect(
            EnemyStatusEffectType.Poison,
            damagePerTick,
            durationSeconds,
            tickIntervalSeconds,
            tickIntervalSeconds,
            sourceRune));
    }

    public void ApplyBurn(
        float currentHealthDamagePercentPerTick,
        float baseDamagePerTick,
        float durationSeconds,
        RuneEntity? sourceRune = null)
    {
        currentHealthDamagePercentPerTick = Math.Max(0f, currentHealthDamagePercentPerTick);
        baseDamagePerTick = Math.Max(0f, baseDamagePerTick);
        durationSeconds = Math.Max(0f, durationSeconds);
        if (currentHealthDamagePercentPerTick <= 0f && baseDamagePerTick <= 0f)
        {
            return;
        }

        BurnStackCount = Math.Min(IngwazTuning.MaxBurnStacks, BurnStackCount + 1);
        _burnRemainingDurationSeconds = Math.Max(_burnRemainingDurationSeconds, durationSeconds);
        _burnCurrentHealthDamagePercentPerTick = Math.Max(_burnCurrentHealthDamagePercentPerTick, currentHealthDamagePercentPerTick);
        _burnBaseDamagePerTick = Math.Max(_burnBaseDamagePerTick, baseDamagePerTick);
        _burnSourceRune = sourceRune ?? _burnSourceRune;
        _burnTimeUntilNextTickSeconds = _burnTimeUntilNextTickSeconds <= 0.001f
            ? IngwazTuning.BurnTickIntervalSeconds
            : Math.Min(_burnTimeUntilNextTickSeconds, IngwazTuning.BurnTickIntervalSeconds);
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

    public void ResetAuraEffects()
    {
        _auraHealthRegenerationPercentPerSecond = 0f;
        _auraMoveSpeedPercent = 0f;
        _hasAuraImmunity = false;
    }

    public void ApplyAuraRegeneration(float maxHealthPercentPerSecond)
    {
        _auraHealthRegenerationPercentPerSecond = Math.Max(
            _auraHealthRegenerationPercentPerSecond,
            Math.Max(0f, maxHealthPercentPerSecond));
    }

    public void ApplyAuraMoveSpeed(float moveSpeedPercent)
    {
        _auraMoveSpeedPercent = Math.Max(_auraMoveSpeedPercent, Math.Max(0f, moveSpeedPercent));
    }

    public void ApplyAuraImmunity()
    {
        _hasAuraImmunity = true;
    }

    public bool TryIgnoreIncomingAttackOrEffect()
    {
        return _hasAuraImmunity && Random.Shared.NextSingle() < AuraEnemyTuning.ImmunityIgnoreChance;
    }

    public void ApplyPerthroMark()
    {
        IsPerthroMarked = true;
    }

    public void ClearPerthroMark()
    {
        IsPerthroMarked = false;
    }

    public void ApplyMannazStormMark()
    {
        IsMannazStormMarked = true;
    }

    public void ClearMannazStormMark()
    {
        IsMannazStormMarked = false;
    }

    public void ApplyFehuMark(RuneEntity sourceRune, float bonusRunePointPercent)
    {
        if (bonusRunePointPercent <= 0f)
        {
            return;
        }

        FehuMarkedByRune = sourceRune;
        FehuBonusRunePointPercent = Math.Max(FehuBonusRunePointPercent, bonusRunePointPercent);
    }

    public int ConsumeFehuBonusRunePoints(int baseRunePoints)
    {
        if (!IsFehuMarked || baseRunePoints <= 0)
        {
            ClearFehuMark();
            return 0;
        }

        var bonusRunePoints = (int)MathF.Round(baseRunePoints * (FehuBonusRunePointPercent / 100f));
        ClearFehuMark();
        return Math.Max(0, bonusRunePoints);
    }

    public void ClearFehuMark()
    {
        FehuMarkedByRune = null;
        FehuBonusRunePointPercent = 0f;
    }

    private float UpdateBurning(float deltaTime, float currentHealth)
    {
        if (BurnStackCount <= 0 || _burnRemainingDurationSeconds <= 0.001f)
        {
            ResetBurn();
            return 0f;
        }

        _burnRemainingDurationSeconds = Math.Max(0f, _burnRemainingDurationSeconds - deltaTime);
        _burnTimeUntilNextTickSeconds -= deltaTime;

        var totalBurnDamage = 0f;
        var simulatedHealth = Math.Max(0f, currentHealth);
        while (_burnTimeUntilNextTickSeconds <= 0f && _burnRemainingDurationSeconds > 0f && simulatedHealth > 0.001f)
        {
            var singleStackDamage = (simulatedHealth * _burnCurrentHealthDamagePercentPerTick) + _burnBaseDamagePerTick;
            var tickDamage = singleStackDamage * BurnStackCount;
            totalBurnDamage += tickDamage;
            simulatedHealth = Math.Max(0f, simulatedHealth - tickDamage);
            _burnTimeUntilNextTickSeconds += IngwazTuning.BurnTickIntervalSeconds;
        }

        if (_burnRemainingDurationSeconds <= 0.001f)
        {
            ResetBurn();
        }

        return totalBurnDamage;
    }

    private void ResetBurn()
    {
        BurnStackCount = 0;
        _burnRemainingDurationSeconds = 0f;
        _burnTimeUntilNextTickSeconds = 0f;
        _burnCurrentHealthDamagePercentPerTick = 0f;
        _burnBaseDamagePerTick = 0f;
        _burnSourceRune = null;
    }
}
