using runeforge.Configs;

namespace runeforge.Models;

public static class RuneMergeRules
{
    public static bool CanMerge(RuneEntity sourceRune, RuneEntity targetRune)
    {
        if (sourceRune.Stats.Tier >= RuneTierTuning.MaxTier || targetRune.Stats.Tier >= RuneTierTuning.MaxTier)
        {
            return false;
        }

        if (sourceRune.Stats.Tier != targetRune.Stats.Tier)
        {
            return false;
        }

        return sourceRune.Stats.Type == targetRune.Stats.Type;
    }
}
