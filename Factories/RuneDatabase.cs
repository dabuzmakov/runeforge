using System.Drawing;
using runeforge.Models;

namespace runeforge.Factories;

public static class RuneDatabase
{
    private static readonly Dictionary<RuneColor, Color> ProjectileColors = new()
    {
        { RuneColor.Blue, Color.FromArgb(25, 119, 241) },
        { RuneColor.Red, Color.FromArgb(214, 56, 56) },
        { RuneColor.Green, Color.FromArgb(86, 179, 0) },
        { RuneColor.Cyan, Color.FromArgb(67, 197, 255) },
        { RuneColor.Orange, Color.FromArgb(237, 140, 34) },
        { RuneColor.Yellow, Color.FromArgb(234, 204, 52) },
        { RuneColor.Purple, Color.FromArgb(156, 92, 255) }
    };

    private static readonly RuneConfig DefaultConfig = CreateConfig(RuneColor.Blue);

    private static readonly Dictionary<RuneType, RuneConfig> Configs = new()
    {
        { RuneType.Kenaz, CreateConfig(RuneColor.Blue) },
        { RuneType.Eiwaz, CreateConfig(RuneColor.Blue) },
        { RuneType.Mannaz, CreateConfig(RuneColor.Blue) },

        { RuneType.Fehu, CreateConfig(RuneColor.Red) },
        { RuneType.Wunjo, CreateConfig(RuneColor.Red) },
        { RuneType.Algiz, CreateConfig(RuneColor.Red) },
        { RuneType.Ingwaz, CreateConfig(RuneColor.Red) },

        { RuneType.Ansuz, CreateConfig(RuneColor.Green) },
        { RuneType.Isa, CreateConfig(RuneColor.Green) },
        { RuneType.Berkano, CreateConfig(RuneColor.Green) },

        { RuneType.Raidho, CreateConfig(RuneColor.Cyan) },
        { RuneType.Jera, CreateConfig(RuneColor.Cyan) },
        { RuneType.Ehwaz, CreateConfig(RuneColor.Cyan) },

        { RuneType.Uruz, CreateConfig(RuneColor.Orange) },
        { RuneType.Hagalaz, CreateConfig(RuneColor.Orange) },
        { RuneType.Sowilo, CreateConfig(RuneColor.Orange) },
        { RuneType.Dagaz, CreateConfig(RuneColor.Orange) },

        { RuneType.Thurisaz, CreateConfig(RuneColor.Yellow) },
        { RuneType.Nauthiz, CreateConfig(RuneColor.Yellow) },
        { RuneType.Tiwaz, CreateConfig(RuneColor.Yellow) },
        { RuneType.Othala, CreateConfig(RuneColor.Yellow) },

        { RuneType.Gebo, CreateConfig(RuneColor.Purple) },
        { RuneType.Perthro, CreateConfig(RuneColor.Purple) },
        { RuneType.Laguz, CreateConfig(RuneColor.Purple) }
    };

    public static RuneConfig Get(RuneType type)
    {
        if (Configs.TryGetValue(type, out var config))
        {
            return config;
        }

        return DefaultConfig;
    }

    private static RuneConfig CreateConfig(RuneColor color)
    {
        return new RuneConfig(
            color,
            attackRate: 0.4f,
            damage: 1f,
            projectileColor: ProjectileColors[color],
            projectileSpeed: 1000f,
            radius: 22f);
    }
}
