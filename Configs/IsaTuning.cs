namespace runeforge.Configs;

public readonly record struct IsaTierValues(float TriggerIntervalSeconds, float SlowPercent, float SlowDurationSeconds);

public static class IsaTuning
{
    private static readonly IsaTierValues[] TierValues =
    [
        new IsaTierValues(5.1f, 0.08f, 0.95f),
        new IsaTierValues(4.7f, 0.09f, 1.05f),
        new IsaTierValues(4.3f, 0.11f, 1.20f),
        new IsaTierValues(3.9f, 0.13f, 1.35f),
        new IsaTierValues(3.5f, 0.15f, 1.50f)
    ];

    private static readonly float[] AdditionalIsaContributionWeights =
    [
        1f,
        0.45f,
        0.25f,
        0.15f,
        0.10f,
        0.06f,
        0.04f,
        0.03f
    ];

    private static readonly float[] MaxCombinedSlowPercentByHighestTier =
    [
        0.10f,
        0.13f,
        0.16f,
        0.19f,
        0.22f
    ];

    public static IsaTierValues GetValues(int tier)
    {
        var index = Math.Clamp(tier, 1, TierValues.Length) - 1;
        return TierValues[index];
    }

    public static float GetCombinedSlowPercent(IEnumerable<int> isaTiers)
    {
        var normalizedIsaTiers = isaTiers.ToArray();
        if (normalizedIsaTiers.Length == 0)
        {
            return 0f;
        }

        var orderedSlowValues = normalizedIsaTiers
            .Select(static tier => GetValues(tier).SlowPercent)
            .OrderByDescending(static slowPercent => slowPercent)
            .ToArray();

        var combinedSlowPercent = 0f;
        for (var i = 0; i < orderedSlowValues.Length; i++)
        {
            var weight = i < AdditionalIsaContributionWeights.Length
                ? AdditionalIsaContributionWeights[i]
                : AdditionalIsaContributionWeights[^1];

            combinedSlowPercent += orderedSlowValues[i] * weight;
        }

        return Math.Min(combinedSlowPercent, GetMaxCombinedSlowPercent(normalizedIsaTiers.Max()));
    }

    public static float GetCombinedSlowDurationSeconds(IEnumerable<int> isaTiers)
    {
        return isaTiers
            .Select(static tier => GetValues(tier).SlowDurationSeconds)
            .DefaultIfEmpty(0f)
            .Max();
    }

    public static float GetPulseIntervalSeconds(IEnumerable<int> isaTiers)
    {
        return isaTiers
            .Select(static tier => GetValues(tier).TriggerIntervalSeconds)
            .DefaultIfEmpty(float.MaxValue)
            .Min();
    }

    public static float GetMaxCombinedSlowPercent(int highestIsaTier)
    {
        var index = Math.Clamp(highestIsaTier, 1, MaxCombinedSlowPercentByHighestTier.Length) - 1;
        return MaxCombinedSlowPercentByHighestTier[index];
    }
}
