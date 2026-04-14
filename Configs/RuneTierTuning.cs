namespace runeforge.Configs;

public static class RuneTierTuning
{
    public const int MinTier = 1;
    public const int MaxTier = 5;

    public static int Clamp(int tier)
    {
        return Math.Clamp(tier, MinTier, MaxTier);
    }
}
