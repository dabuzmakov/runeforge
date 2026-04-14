using System.Drawing;
using System.Numerics;
using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Effects;

public sealed class SpriteSheetEffectDefinition
{
    public SpriteSheetEffectDefinition(
        EffectType type,
        string texturePath,
        int frameWidth,
        int frameHeight,
        int frameCount,
        float frameDuration,
        float defaultScale)
    {
        Type = type;
        TexturePath = texturePath;
        FrameWidth = frameWidth;
        FrameHeight = frameHeight;
        FrameCount = Math.Max(1, frameCount);
        FrameDuration = Math.Max(0.001f, frameDuration);
        DefaultScale = Math.Max(0.01f, defaultScale);
    }

    public EffectType Type { get; }

    public string TexturePath { get; }

    public int FrameWidth { get; }

    public int FrameHeight { get; }

    public int FrameCount { get; }

    public float FrameDuration { get; }

    public float DefaultScale { get; }
}

public static class EffectRegistry
{
    private const int EffectFrameSize = 64;

    private static readonly Lazy<SpriteSheetEffectDefinition[]> Definitions = new(CreateDefinitions);

    public static IReadOnlyList<SpriteSheetEffectDefinition> All => Definitions.Value;

    public static SpriteSheetEffectDefinition Get(EffectType type)
    {
        foreach (var definition in Definitions.Value)
        {
            if (definition.Type == type)
            {
                return definition;
            }
        }

        throw new InvalidOperationException($"No effect definition registered for {type}.");
    }

    public static bool TryCreateMergeEffect(Vector2 position, RuneColor color, out AnimatedEffect? effect)
    {
        effect = null;
        if (!TryGetColorRowIndex(color, out var rowIndex))
        {
            return false;
        }

        effect = new AnimatedEffect(Get(EffectType.Merge), rowIndex, position);
        return true;
    }

    public static bool TryCreateKenazExplosionEffect(Vector2 position, out AnimatedEffect? effect, float? scale = null)
    {
        effect = new AnimatedEffect(Get(EffectType.KenazExplosion), KenazTuning.ExplosionEffectRowIndex, position, scale);
        return true;
    }

    public static bool TryCreateRuneSpawnEffect(Vector2 position, RuneColor color, out AnimatedEffect? effect)
    {
        effect = null;
        if (!TryGetColorRowIndex(color, out var rowIndex))
        {
            return false;
        }

        effect = new AnimatedEffect(Get(EffectType.RuneSpawn), rowIndex, position);
        return true;
    }

    public static bool TryCreateRuneRemoveEffect(Vector2 position, RuneColor color, out AnimatedEffect? effect)
    {
        effect = null;
        if (!TryGetColorRowIndex(color, out var rowIndex))
        {
            return false;
        }

        effect = new AnimatedEffect(Get(EffectType.RuneRemove), rowIndex, position);
        return true;
    }

    public static SpriteSheetEffectDefinition GetRaidoOverloadEffect()
    {
        return Get(EffectType.RaidoOverload);
    }

    private static SpriteSheetEffectDefinition[] CreateDefinitions()
    {
        var runeSpawnTexturePath = ResolveEffectTexturePath("Part 12", "rune_spawn.png");
        var runeRemoveTexturePath = ResolveEffectTexturePath("Part 3", "rune_remove.png");
        var mergeTexturePath = ResolveEffectTexturePath("Part 9", "merge.png");
        var kenazExplosionTexturePath = ResolveEffectTexturePath("Part 9", "kenaz-effect.png");
        var raidoOverloadTexturePath = ResolveEffectTexturePath("Part 14", "raidho-effect.png");
        var mergeFrameCount = DetermineFrameCount(mergeTexturePath, EffectFrameSize);
        var kenazExplosionFrameCount = DetermineFrameCount(kenazExplosionTexturePath, EffectFrameSize);
        var runeSpawnFrameCount = DetermineFrameCount(runeSpawnTexturePath, EffectFrameSize);
        var runeRemoveFrameCount = DetermineFrameCount(runeRemoveTexturePath, EffectFrameSize);
        var raidoOverloadFrameCount = DetermineFrameCount(raidoOverloadTexturePath, EffectFrameSize);

        return
        [
            new SpriteSheetEffectDefinition(
                EffectType.Merge,
                mergeTexturePath,
                EffectFrameSize,
                EffectFrameSize,
                mergeFrameCount,
                frameDuration: 0.07f,
                defaultScale: 2.2f),
            new SpriteSheetEffectDefinition(
                EffectType.KenazExplosion,
                kenazExplosionTexturePath,
                EffectFrameSize,
                EffectFrameSize,
                kenazExplosionFrameCount,
                frameDuration: 0.05f,
                defaultScale: KenazTuning.ExplosionEffectScale),
            new SpriteSheetEffectDefinition(
                EffectType.RuneSpawn,
                runeSpawnTexturePath,
                EffectFrameSize,
                EffectFrameSize,
                runeSpawnFrameCount,
                frameDuration: 0.05f,
                defaultScale: 3.1f),
            new SpriteSheetEffectDefinition(
                EffectType.RuneRemove,
                runeRemoveTexturePath,
                EffectFrameSize,
                EffectFrameSize,
                runeRemoveFrameCount,
                frameDuration: 0.05f,
                defaultScale: 3.3f),
            new SpriteSheetEffectDefinition(
                EffectType.RaidoOverload,
                raidoOverloadTexturePath,
                EffectFrameSize,
                EffectFrameSize,
                raidoOverloadFrameCount,
                frameDuration: 0.06f,
                defaultScale: RaidoTuning.OverloadEffectScale)
        ];
    }

    private static bool TryGetColorRowIndex(RuneColor color, out int rowIndex)
    {
        rowIndex = color switch
        {
            RuneColor.Blue => 8,
            RuneColor.Red => 7,
            RuneColor.Green => 3,
            RuneColor.Cyan => 2,
            RuneColor.Orange => 0,
            RuneColor.Yellow => 4,
            RuneColor.Purple => 1,
            _ => -1
        };

        return rowIndex >= 0;
    }

    private static int DetermineFrameCount(string texturePath, int frameWidth)
    {
        using var image = Image.FromFile(texturePath);
        return image.Width / frameWidth;
    }

    private static string ResolveEffectTexturePath(string partDirectory, string fileName)
    {
        string[] candidatePaths =
        [
            Path.Combine(AppContext.BaseDirectory, "Assets", "Effects", partDirectory, fileName),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Assets", "Effects", partDirectory, fileName))
        ];

        foreach (var candidatePath in candidatePaths)
        {
            if (File.Exists(candidatePath))
            {
                return candidatePath;
            }
        }

        throw new FileNotFoundException($"Could not locate effect texture '{fileName}' in Assets/Effects/{partDirectory}.");
    }

}
