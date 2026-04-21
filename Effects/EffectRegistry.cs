using System.Drawing;
using System.Numerics;
using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Effects;

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

    public static bool TryCreateBerkanoPoisonEffect(Vector2 position, out AnimatedEffect? effect, float? scale = null)
    {
        effect = new AnimatedEffect(Get(EffectType.BerkanoPoison), rowIndex: BerkanoTuning.PoisonEffectRowIndex, position, scale);
        return true;
    }

    public static bool TryCreateAlgizSweepEffect(Vector2 position, float rotationRadians, out AnimatedEffect? effect, float? scale = null)
    {
        effect = new AnimatedEffect(
            Get(EffectType.AlgizSweep),
            AlgizTuning.EffectRowIndex,
            position,
            scale,
            rotationRadians,
            flipHorizontally: true);
        return true;
    }

    public static bool TryCreateKenazExplosionEffect(Vector2 position, out AnimatedEffect? effect, float? scale = null)
    {
        effect = new AnimatedEffect(Get(EffectType.KenazExplosion), KenazTuning.ExplosionEffectRowIndex, position, scale);
        return true;
    }

    public static bool TryCreateAnsuzImpactEffect(Vector2 position, out AnimatedEffect? effect, float? scale = null)
    {
        effect = new AnimatedEffect(Get(EffectType.AnsuzImpact), AnsuzTuning.EffectRowIndex, position, scale);
        return true;
    }

    public static bool TryCreateLaguzExecuteEffect(Vector2 position, out AnimatedEffect? effect, float? scale = null)
    {
        effect = new AnimatedEffect(Get(EffectType.LaguzExecute), rowIndex: 1, position, scale);
        return true;
    }

    public static bool TryCreateEhwazChainHitEffect(
        Vector2 position,
        EnemyEntity? attachedEnemy,
        out AnimatedEffect? effect,
        float? scale = null)
    {
        effect = new AnimatedEffect(
            Get(EffectType.EhwazChainHit),
            EhwazTuning.ChainEffectRowIndex,
            position,
            scale,
            attachedEnemy: attachedEnemy);
        return true;
    }

    public static bool TryCreateEiwazImpactEffect(Vector2 position, out AnimatedEffect? effect, float? scale = null)
    {
        effect = new AnimatedEffect(Get(EffectType.EiwazImpact), rowIndex: 2, position, scale);
        return true;
    }

    public static bool TryCreateHagalazExplosionEffect(Vector2 position, out AnimatedEffect? effect, float? scale = null)
    {
        effect = new AnimatedEffect(
            Get(EffectType.HagalazExplosion),
            HagalazTuning.ExplosionEffectRowIndex,
            position,
            scale);
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

    public static SpriteSheetEffectDefinition GetRaidhoOverloadEffect()
    {
        return Get(EffectType.RaidhoOverload);
    }

    public static SpriteSheetEffectDefinition GetRuneHoldEffect()
    {
        return Get(EffectType.RuneHold);
    }

    public static bool TryGetEffectColorRowIndex(RuneColor color, out int rowIndex)
    {
        return TryGetColorRowIndex(color, out rowIndex);
    }

    private static SpriteSheetEffectDefinition[] CreateDefinitions()
    {
        var berkanoPoisonTexturePath = ResolveEffectTexturePath("Part 13", "berkano-effect.png");
        var algizSweepTexturePath = ResolveEffectTexturePath("Part 5", "algiz-effect.png");
        var runeHoldTexturePath = ResolveEffectTexturePath("Part 4", "rune-hold-effect.png");
        var runeSpawnTexturePath = ResolveEffectTexturePath("Part 12", "rune_spawn.png");
        var runeRemoveTexturePath = ResolveEffectTexturePath("Part 3", "rune_remove.png");
        var mergeTexturePath = ResolveEffectTexturePath("Part 9", "merge.png");
        var ehwazChainHitTexturePath = ResolveEffectTexturePath("Part 10", "ehwaz-effect.png");
        var eiwazImpactTexturePath = ResolveEffectTexturePath("Part 10", "eiwaz-effect.png");
        var kenazExplosionTexturePath = ResolveEffectTexturePath("Part 9", "kenaz-effect.png");
        var laguzBlackHoleFramePaths = ResolveEffectFramePaths("laguz-effect", Path.Combine("Pixel Art Animated Portal", "Frames"));
        var laguzExecuteTexturePath = ResolveEffectTexturePath("Part 8", "laguz-execute-effect.png");
        var ansuzImpactTexturePath = ResolveEffectTexturePath("Part 6", "ansuz-effect.png");
        var raidhoOverloadTexturePath = ResolveEffectTexturePath("Part 14", "raidho-effect.png");
        var hagalazExplosionFramePaths = ResolveEffectFramePaths("explosion-effect");
        var algizSweepFrameCount = DetermineFrameCount(algizSweepTexturePath, EffectFrameSize);
        var berkanoPoisonFrameCount = DetermineFrameCount(berkanoPoisonTexturePath, EffectFrameSize);
        var ehwazChainHitFrameCount = DetermineFrameCount(ehwazChainHitTexturePath, EffectFrameSize);
        var eiwazImpactFrameCount = DetermineFrameCount(eiwazImpactTexturePath, EffectFrameSize);
        var mergeFrameCount = DetermineFrameCount(mergeTexturePath, EffectFrameSize);
        var hagalazExplosionFrameSize = DetermineFrameSize(hagalazExplosionFramePaths[0]);
        var laguzBlackHoleFrameSize = DetermineFrameSize(laguzBlackHoleFramePaths[0]);
        var laguzExecuteFrameCount = DetermineFrameCount(laguzExecuteTexturePath, EffectFrameSize);
        var kenazExplosionFrameCount = DetermineFrameCount(kenazExplosionTexturePath, EffectFrameSize);
        var ansuzImpactFrameCount = DetermineFrameCount(ansuzImpactTexturePath, EffectFrameSize);
        var runeHoldFrameCount = DetermineFrameCount(runeHoldTexturePath, EffectFrameSize);
        var runeSpawnFrameCount = DetermineFrameCount(runeSpawnTexturePath, EffectFrameSize);
        var runeRemoveFrameCount = DetermineFrameCount(runeRemoveTexturePath, EffectFrameSize);
        var raidhoOverloadFrameCount = DetermineFrameCount(raidhoOverloadTexturePath, EffectFrameSize);

        var definitions = new List<SpriteSheetEffectDefinition>
        {
            new(
                EffectType.Merge,
                mergeTexturePath,
                EffectFrameSize,
                EffectFrameSize,
                mergeFrameCount,
                frameDuration: 0.07f,
                defaultScale: 2.2f),
            new(
                EffectType.AlgizSweep,
                algizSweepTexturePath,
                EffectFrameSize,
                EffectFrameSize,
                algizSweepFrameCount,
                frameDuration: 0.05f,
                defaultScale: AlgizTuning.EffectScale),
            new(
                EffectType.BerkanoPoison,
                berkanoPoisonTexturePath,
                EffectFrameSize,
                EffectFrameSize,
                berkanoPoisonFrameCount,
                frameDuration: 0.05f,
                defaultScale: BerkanoTuning.PoisonEffectScale),
            new(
                EffectType.EhwazChainHit,
                ehwazChainHitTexturePath,
                EffectFrameSize,
                EffectFrameSize,
                ehwazChainHitFrameCount,
                frameDuration: 0.04f,
                defaultScale: EhwazTuning.ChainEffectScale),
            new(
                EffectType.EiwazImpact,
                eiwazImpactTexturePath,
                EffectFrameSize,
                EffectFrameSize,
                eiwazImpactFrameCount,
                frameDuration: 0.035f,
                defaultScale: 1.7f),
            new(
                EffectType.HagalazExplosion,
                hagalazExplosionFramePaths,
                hagalazExplosionFrameSize.Width,
                hagalazExplosionFrameSize.Height,
                frameDuration: 0.05f,
                defaultScale: HagalazTuning.ExplosionEffectScale),
            new(
                EffectType.KenazExplosion,
                kenazExplosionTexturePath,
                EffectFrameSize,
                EffectFrameSize,
                kenazExplosionFrameCount,
                frameDuration: 0.05f,
                defaultScale: KenazTuning.ExplosionEffectScale),
            new(
                EffectType.LaguzBlackHole,
                laguzBlackHoleFramePaths,
                laguzBlackHoleFrameSize.Width,
                laguzBlackHoleFrameSize.Height,
                frameDuration: LaguzTuning.BlackHoleAnimationFrameDurationSeconds,
                defaultScale: LaguzTuning.BlackHoleEffectScale),
            new(
                EffectType.LaguzExecute,
                laguzExecuteTexturePath,
                EffectFrameSize,
                EffectFrameSize,
                laguzExecuteFrameCount,
                frameDuration: 0.05f,
                defaultScale: 2.3f),
            new(
                EffectType.AnsuzImpact,
                ansuzImpactTexturePath,
                EffectFrameSize,
                EffectFrameSize,
                ansuzImpactFrameCount,
                frameDuration: 0.05f,
                defaultScale: AnsuzTuning.AllyHitEffectScale),
            new(
                EffectType.RuneHold,
                runeHoldTexturePath,
                EffectFrameSize,
                EffectFrameSize,
                runeHoldFrameCount,
                frameDuration: 0.06f,
                defaultScale: 2.25f),
            new(
                EffectType.RuneSpawn,
                runeSpawnTexturePath,
                EffectFrameSize,
                EffectFrameSize,
                runeSpawnFrameCount,
                frameDuration: 0.05f,
                defaultScale: 3.1f),
            new(
                EffectType.RuneRemove,
                runeRemoveTexturePath,
                EffectFrameSize,
                EffectFrameSize,
                runeRemoveFrameCount,
                frameDuration: 0.05f,
                defaultScale: 3.3f),
            new(
                EffectType.RaidhoOverload,
                raidhoOverloadTexturePath,
                EffectFrameSize,
                EffectFrameSize,
                raidhoOverloadFrameCount,
                frameDuration: 0.06f,
                defaultScale: RaidhoTuning.OverloadEffectScale)
        };

        return [.. definitions];
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

    private static Size DetermineFrameSize(string texturePath)
    {
        using var image = Image.FromFile(texturePath);
        return image.Size;
    }

    private static string ResolveEffectTexturePath(string partDirectory, string fileName)
    {
        var resolvedPath = TryResolveEffectTexturePath(partDirectory, fileName);
        if (resolvedPath != null)
        {
            return resolvedPath;
        }

        throw new FileNotFoundException($"Could not locate effect texture '{fileName}' in Assets/Effects/{partDirectory}.");
    }

    private static string? TryResolveEffectTexturePath(string partDirectory, string fileName)
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

        return null;
    }

    private static IReadOnlyList<string> ResolveEffectFramePaths(params string[] effectDirectories)
    {
        foreach (var effectDirectory in effectDirectories)
        {
            string[] candidateDirectories =
            [
                Path.Combine(AppContext.BaseDirectory, "Assets", "Effects", effectDirectory),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Assets", "Effects", effectDirectory))
            ];

            foreach (var candidateDirectory in candidateDirectories)
            {
                if (!Directory.Exists(candidateDirectory))
                {
                    continue;
                }

                var framePaths = Directory
                    .GetFiles(candidateDirectory, "*.png")
                    .OrderBy(static path =>
                    {
                        var fileName = Path.GetFileNameWithoutExtension(path);
                        return ExtractTrailingNumber(fileName) ?? int.MaxValue;
                    })
                    .ThenBy(static path => path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (framePaths.Length > 0)
                {
                    return framePaths;
                }
            }
        }

        throw new DirectoryNotFoundException($"Could not locate effect frames in any of: {string.Join(", ", effectDirectories.Select(static directory => $"Assets/Effects/{directory}"))}.");
    }

    private static int? ExtractTrailingNumber(string fileNameWithoutExtension)
    {
        if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
        {
            return null;
        }

        var end = fileNameWithoutExtension.Length - 1;
        while (end >= 0 && char.IsDigit(fileNameWithoutExtension[end]))
        {
            end--;
        }

        var digitStart = end + 1;
        if (digitStart >= fileNameWithoutExtension.Length)
        {
            return null;
        }

        var numericPart = fileNameWithoutExtension[digitStart..];
        return int.TryParse(numericPart, out var value) ? value : null;
    }
}
