namespace runeforge.Models;

public sealed class IsaGlobalState
{
    public float RemainingPulseCooldownSeconds { get; private set; }

    public bool IsPulseScheduled { get; private set; }

    public void Reset()
    {
        RemainingPulseCooldownSeconds = 0f;
        IsPulseScheduled = false;
    }

    public void EnsureScheduled(float cooldownSeconds)
    {
        var normalizedCooldownSeconds = Math.Max(0.01f, cooldownSeconds);
        if (!IsPulseScheduled)
        {
            RemainingPulseCooldownSeconds = normalizedCooldownSeconds;
            IsPulseScheduled = true;
            return;
        }

        RemainingPulseCooldownSeconds = Math.Min(RemainingPulseCooldownSeconds, normalizedCooldownSeconds);
    }

    public bool Advance(float deltaTime)
    {
        if (!IsPulseScheduled || deltaTime <= 0f)
        {
            return false;
        }

        RemainingPulseCooldownSeconds = Math.Max(0f, RemainingPulseCooldownSeconds - deltaTime);
        return RemainingPulseCooldownSeconds <= 0.001f;
    }

    public void ScheduleNextPulse(float cooldownSeconds)
    {
        RemainingPulseCooldownSeconds = Math.Max(0.01f, cooldownSeconds);
        IsPulseScheduled = true;
    }
}
