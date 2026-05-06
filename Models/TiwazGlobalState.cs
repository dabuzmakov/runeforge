using runeforge.Configs;

namespace runeforge.Models;

public enum TiwazMode
{
    Charging,
    Discharging
}

public sealed class TiwazGlobalState
{
    public TiwazMode Mode { get; private set; } = TiwazMode.Charging;

    public float RemainingDischargeSeconds { get; private set; }

    public bool IsCharging => Mode == TiwazMode.Charging;

    public bool IsDischarging => Mode == TiwazMode.Discharging;

    public void ToggleMode()
    {
        if (IsCharging)
        {
            StartDischarge();
            return;
        }

        ReturnToCharging();
    }

    public void StartDischarge()
    {
        Mode = TiwazMode.Discharging;
        RemainingDischargeSeconds = TiwazTuning.DischargeDurationSeconds;
    }

    public void ReturnToCharging()
    {
        Mode = TiwazMode.Charging;
        RemainingDischargeSeconds = 0f;
    }

    public void Update(float deltaTime)
    {
        if (!IsDischarging || deltaTime <= 0f)
        {
            return;
        }

        RemainingDischargeSeconds = Math.Max(0f, RemainingDischargeSeconds - deltaTime);
        if (RemainingDischargeSeconds <= 0.001f)
        {
            ReturnToCharging();
        }
    }
}
