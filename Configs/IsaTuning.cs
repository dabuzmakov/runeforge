namespace runeforge.Configs;

public readonly record struct IsaTierValues(float TriggerIntervalSeconds, float SlowPercent, float SlowDurationSeconds);

public static class IsaTuning
{
    private static readonly IsaTierValues[] TierValues =
    [
        new IsaTierValues(4.6f, 0.18f, 1.8f),
        new IsaTierValues(4.2f, 0.21f, 2.0f),
        new IsaTierValues(3.8f, 0.24f, 2.2f),
        new IsaTierValues(3.4f, 0.27f, 2.4f),
        new IsaTierValues(3.0f, 0.30f, 2.6f)
    ];

    private static readonly float[] AdditionalIsaContributionWeights =
    [
        1f,
        0.6f,
        0.4f,
        0.28f,
        0.2f,
        0.14f,
        0.1f,
        0.07f
    ];

    private static readonly float[] MaxCombinedSlowPercentByHighestTier =
    [
        0.28f,
        0.34f,
        0.40f,
        0.47f,
        0.55f
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

    public static float GetMaxCombinedSlowPercent(int highestIsaTier)
    {
        var index = Math.Clamp(highestIsaTier, 1, MaxCombinedSlowPercentByHighestTier.Length) - 1;
        return MaxCombinedSlowPercentByHighestTier[index];
    }
}
