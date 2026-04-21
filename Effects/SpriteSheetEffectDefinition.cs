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
        FramePaths = [];
    }

    public SpriteSheetEffectDefinition(
        EffectType type,
        IReadOnlyList<string> framePaths,
        int frameWidth,
        int frameHeight,
        float frameDuration,
        float defaultScale)
    {
        if (framePaths.Count == 0)
        {
            throw new ArgumentException("Frame sequence must contain at least one frame.", nameof(framePaths));
        }

        Type = type;
        TexturePath = framePaths[0];
        FrameWidth = frameWidth;
        FrameHeight = frameHeight;
        FrameCount = framePaths.Count;
        FrameDuration = Math.Max(0.001f, frameDuration);
        DefaultScale = Math.Max(0.01f, defaultScale);
        FramePaths = [.. framePaths];
    }

    public EffectType Type { get; }

    public string TexturePath { get; }

    public int FrameWidth { get; }

    public int FrameHeight { get; }

    public int FrameCount { get; }

    public float FrameDuration { get; }

    public float DefaultScale { get; }

    public IReadOnlyList<string> FramePaths { get; }

    public bool UsesFrameSequence => FramePaths.Count > 0;
}
