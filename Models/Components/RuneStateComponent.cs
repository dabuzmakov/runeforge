using System.Numerics;
using runeforge.Configs;

namespace runeforge.Models;

public sealed class RuneStateComponent
{
    private float _raidhoTimeUntilNextOverload;
    private float _raidhoOverloadRemaining;
    private float _hagalazTimeUntilNextCharge;
    private float _thurisazChargeElapsed;
    private readonly Queue<int> _algizTargetIds = new();
    private float _algizSweepStepInterval;
    private int? _eiwazTargetEnemyId;
    private float _eiwazAimElapsed;

    public RuneStateComponent(RuneData runeData, int tier)
    {
        if (runeData.Type == RuneType.Raidho)
        {
            _raidhoTimeUntilNextOverload = RaidhoTuning.OverloadIntervalSeconds;
        }

        if (runeData.Type == RuneType.Hagalaz)
        {
            _hagalazTimeUntilNextCharge = HagalazTuning.ChargeSegmentIntervalSeconds;
        }
    }

    public bool IsRaidhoOverloadActive => _raidhoOverloadRemaining > 0.001f;

    public float RaidhoOverloadElapsedSeconds { get; private set; }

    public int HagalazChargeSegments { get; private set; }

    public float ThurisazChargeProgress => ThurisazTuning.ChargeDurationSeconds <= 0f
        ? 1f
        : Math.Clamp(_thurisazChargeElapsed / ThurisazTuning.ChargeDurationSeconds, 0f, 1f);

    public bool IsThurisazCharged => ThurisazChargeProgress >= 0.999f;

    public float ThurisazAimAngleRadians { get; private set; }

    public bool IsAlgizSweepActive => _algizTargetIds.Count > 0;

    public bool IsEiwazAiming => _eiwazTargetEnemyId.HasValue;

    public int? EiwazTargetEnemyId => _eiwazTargetEnemyId;

    public float EiwazAimElapsedSeconds => _eiwazAimElapsed;

    public float EiwazAimProgress => EiwazTuning.AimDurationSeconds <= 0f
        ? 1f
        : Math.Clamp(_eiwazAimElapsed / EiwazTuning.AimDurationSeconds, 0f, 1f);

    public void Update(RuneStatsComponent runeStats, float deltaTime)
    {
        if (deltaTime <= 0f)
        {
            return;
        }

        if (runeStats.Type == RuneType.Raidho)
        {
            UpdateRaidhoState(runeStats.Tier, deltaTime);
        }

        if (runeStats.Type == RuneType.Hagalaz)
        {
            UpdateHagalazCharges(deltaTime);
        }
    }

    private void BeginRaidhoOverload(int tier)
    {
        _raidhoTimeUntilNextOverload = RaidhoTuning.OverloadIntervalSeconds;
        _raidhoOverloadRemaining = RaidhoTuning.GetOverloadDurationSeconds(tier);
        RaidhoOverloadElapsedSeconds = 0f;
    }

    private void UpdateRaidhoState(int tier, float deltaTime)
    {
        var remainingDelta = deltaTime;

        while (remainingDelta > 0f)
        {
            if (IsRaidhoOverloadActive)
            {
                var consumed = Math.Min(remainingDelta, _raidhoOverloadRemaining);
                _raidhoOverloadRemaining -= consumed;
                RaidhoOverloadElapsedSeconds += consumed;
                remainingDelta -= consumed;

                if (_raidhoOverloadRemaining <= 0.001f)
                {
                    _raidhoOverloadRemaining = 0f;
                    RaidhoOverloadElapsedSeconds = 0f;
                }

                continue;
            }

            if (_raidhoTimeUntilNextOverload <= 0.001f)
            {
                BeginRaidhoOverload(tier);
                continue;
            }

            var idleConsumed = Math.Min(remainingDelta, _raidhoTimeUntilNextOverload);
            _raidhoTimeUntilNextOverload -= idleConsumed;
            remainingDelta -= idleConsumed;

            if (_raidhoTimeUntilNextOverload <= 0.001f)
            {
                BeginRaidhoOverload(tier);
            }
        }
    }

    private void UpdateHagalazCharges(float deltaTime)
    {
        if (HagalazChargeSegments >= HagalazTuning.ChargeSegmentCount)
        {
            _hagalazTimeUntilNextCharge = 0f;
            return;
        }

        var remainingDelta = deltaTime;
        while (remainingDelta > 0f && HagalazChargeSegments < HagalazTuning.ChargeSegmentCount)
        {
            if (_hagalazTimeUntilNextCharge <= 0.001f)
            {
                HagalazChargeSegments++;
                _hagalazTimeUntilNextCharge = HagalazChargeSegments >= HagalazTuning.ChargeSegmentCount
                    ? 0f
                    : HagalazTuning.ChargeSegmentIntervalSeconds;
                continue;
            }

            var consumed = Math.Min(remainingDelta, _hagalazTimeUntilNextCharge);
            _hagalazTimeUntilNextCharge -= consumed;
            remainingDelta -= consumed;
        }
    }

    public void AdvanceThurisazCharge(float deltaTime)
    {
        if (deltaTime <= 0f || IsThurisazCharged)
        {
            return;
        }

        _thurisazChargeElapsed = MathF.Min(
            ThurisazTuning.ChargeDurationSeconds,
            _thurisazChargeElapsed + deltaTime);
    }

    public void ConsumeThurisazCharge()
    {
        _thurisazChargeElapsed = 0f;
    }

    public void UpdateThurisazAim(Vector2 fromPosition, Vector2 targetPosition)
    {
        var direction = targetPosition - fromPosition;
        if (direction.LengthSquared() <= 0.001f)
        {
            return;
        }

        ThurisazAimAngleRadians = MathF.Atan2(direction.Y, direction.X);
    }

    public void BeginAlgizSweep(IEnumerable<int> targetIds, float sweepStepInterval)
    {
        _algizTargetIds.Clear();
        _algizSweepStepInterval = Math.Max(0.001f, sweepStepInterval);

        foreach (var targetId in targetIds)
        {
            _algizTargetIds.Enqueue(targetId);
        }
    }

    public bool TryPeekAlgizTargetId(out int targetId)
    {
        if (_algizTargetIds.Count > 0)
        {
            targetId = _algizTargetIds.Peek();
            return true;
        }

        targetId = default;
        return false;
    }

    public void AdvanceAlgizSweep()
    {
        if (_algizTargetIds.Count > 0)
        {
            _algizTargetIds.Dequeue();
        }
    }

    public void EndAlgizSweep()
    {
        _algizTargetIds.Clear();
        _algizSweepStepInterval = 0f;
    }

    public float GetAlgizSweepStepInterval()
    {
        return _algizSweepStepInterval > 0f
            ? _algizSweepStepInterval
            : AlgizTuning.SweepStepIntervalSeconds;
    }

    public void StartEiwazAim(int targetEnemyId)
    {
        _eiwazTargetEnemyId = targetEnemyId;
        _eiwazAimElapsed = 0f;
    }

    public void UpdateEiwazTarget(int targetEnemyId)
    {
        _eiwazTargetEnemyId = targetEnemyId;
    }

    public void ClearEiwazAim()
    {
        _eiwazTargetEnemyId = null;
        _eiwazAimElapsed = 0f;
    }

    public void AdvanceEiwazAim(float deltaTime)
    {
        if (!_eiwazTargetEnemyId.HasValue || deltaTime <= 0f)
        {
            return;
        }

        _eiwazAimElapsed = MathF.Min(EiwazTuning.AimDurationSeconds, _eiwazAimElapsed + deltaTime);
    }
}
