using System.Drawing;

namespace runeforge.Configs;

public static class AuraEnemyTuning
{
    public const float AuraRadius = 60f;
    public const float RegenerationPercentPerSecond = 0.015f;
    public const float SpeedBonusPercent = 0.18f;
    public const float ImmunityIgnoreChance = 0.04f;

    public static readonly Color RegenerationCoreColor = Color.FromArgb(102, 214, 126);
    public static readonly Color RegenerationGlowColor = Color.FromArgb(88, 232, 140);
    public static readonly Color SpeedCoreColor = Color.FromArgb(232, 204, 86);
    public static readonly Color SpeedGlowColor = Color.FromArgb(246, 222, 110);
    public static readonly Color ImmunityCoreColor = Color.FromArgb(108, 202, 244);
    public static readonly Color ImmunityGlowColor = Color.FromArgb(136, 222, 255);
}
