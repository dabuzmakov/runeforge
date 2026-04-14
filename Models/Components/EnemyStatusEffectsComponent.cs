namespace runeforge.Models;

public sealed class EnemyStatusEffectsComponent
{
    private readonly List<EnemyStatusEffect> _activeEffects = new(4);

    public float MovementSpeedMultiplier => 1f - StrongestMovementSlow;

    public float StrongestMovementSlow { get; private set; }

    public bool IsSlowed => StrongestMovementSlow > 0f;

    public void Update(float deltaTime)
    {
        var strongestMovementSlow = 0f;

        for (var i = _activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = _activeEffects[i];
            var remainingDuration = effect.RemainingDurationSeconds - deltaTime;
            if (remainingDuration <= 0f)
            {
                _activeEffects.RemoveAt(i);
                continue;
            }

            effect = effect with { RemainingDurationSeconds = remainingDuration };
            _activeEffects[i] = effect;

            if (effect.Type == EnemyStatusEffectType.MovementSlow)
            {
                strongestMovementSlow = Math.Max(strongestMovementSlow, effect.Strength);
            }
        }

        StrongestMovementSlow = strongestMovementSlow;
    }

    public void ApplyOrRefreshMovementSlow(float slowPercent, float durationSeconds)
    {
        slowPercent = Math.Clamp(slowPercent, 0f, 0.95f);
        durationSeconds = Math.Max(0f, durationSeconds);

        for (var i = 0; i < _activeEffects.Count; i++)
        {
            if (_activeEffects[i].Type != EnemyStatusEffectType.MovementSlow)
            {
                continue;
            }

            _activeEffects[i] = new EnemyStatusEffect(
                EnemyStatusEffectType.MovementSlow,
                Math.Max(_activeEffects[i].Strength, slowPercent),
                Math.Max(_activeEffects[i].RemainingDurationSeconds, durationSeconds));

            StrongestMovementSlow = Math.Max(StrongestMovementSlow, _activeEffects[i].Strength);
            return;
        }

        _activeEffects.Add(new EnemyStatusEffect(
            EnemyStatusEffectType.MovementSlow,
            slowPercent,
            durationSeconds));

        StrongestMovementSlow = Math.Max(StrongestMovementSlow, slowPercent);
    }
}
