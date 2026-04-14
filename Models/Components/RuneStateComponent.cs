using runeforge.Configs;

namespace runeforge.Models;

public sealed class RuneStateComponent
{
    private float _raidoTimeUntilNextOverload;
    private float _raidoOverloadRemaining;

    public RuneStateComponent(RuneData runeData, int tier)
    {
        if (runeData.Type == RuneType.Raidho)
        {
            _raidoTimeUntilNextOverload = RaidoTuning.OverloadIntervalSeconds;
        }
    }

    public bool IsRaidoOverloadActive => _raidoOverloadRemaining > 0.001f;

    public float RaidoOverloadElapsedSeconds { get; private set; }

    public void Update(RuneStatsComponent runeStats, float deltaTime)
    {
        if (runeStats.Type != RuneType.Raidho || deltaTime <= 0f)
        {
            return;
        }

        var remainingDelta = deltaTime;

        while (remainingDelta > 0f)
        {
            if (IsRaidoOverloadActive)
            {
                var consumed = Math.Min(remainingDelta, _raidoOverloadRemaining);
                _raidoOverloadRemaining -= consumed;
                RaidoOverloadElapsedSeconds += consumed;
                remainingDelta -= consumed;

                if (_raidoOverloadRemaining <= 0.001f)
                {
                    _raidoOverloadRemaining = 0f;
                    RaidoOverloadElapsedSeconds = 0f;
                }

                continue;
            }

            if (_raidoTimeUntilNextOverload <= 0.001f)
            {
                BeginRaidoOverload(runeStats.Tier);
                continue;
            }

            var idleConsumed = Math.Min(remainingDelta, _raidoTimeUntilNextOverload);
            _raidoTimeUntilNextOverload -= idleConsumed;
            remainingDelta -= idleConsumed;

            if (_raidoTimeUntilNextOverload <= 0.001f)
            {
                BeginRaidoOverload(runeStats.Tier);
            }
        }
    }

    private void BeginRaidoOverload(int tier)
    {
        _raidoTimeUntilNextOverload = RaidoTuning.OverloadIntervalSeconds;
        _raidoOverloadRemaining = RaidoTuning.GetOverloadDurationSeconds(tier);
        RaidoOverloadElapsedSeconds = 0f;
    }
}
