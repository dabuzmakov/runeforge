using System.Numerics;

namespace runeforge.Effects;

public sealed class AnimatedEffect
{
    private float _elapsed;

    public AnimatedEffect(
        SpriteSheetEffectDefinition definition,
        int rowIndex,
        Vector2 position,
        float? scale = null)
    {
        Definition = definition;
        RowIndex = rowIndex;
        Position = position;
        Scale = scale ?? definition.DefaultScale;
    }

    public SpriteSheetEffectDefinition Definition { get; }

    public int RowIndex { get; }

    public Vector2 Position { get; }

    public float Scale { get; }

    public bool IsFinished => _elapsed >= TotalDuration;

    public int CurrentFrameIndex
    {
        get
        {
            if (Definition.FrameCount <= 1)
            {
                return 0;
            }

            var frameIndex = (int)(_elapsed / Definition.FrameDuration);
            return Math.Clamp(frameIndex, 0, Definition.FrameCount - 1);
        }
    }

    private float TotalDuration => Definition.FrameCount * Definition.FrameDuration;

    public void Update(float deltaTime)
    {
        if (IsFinished)
        {
            return;
        }

        _elapsed = Math.Min(_elapsed + deltaTime, TotalDuration);
    }
}
