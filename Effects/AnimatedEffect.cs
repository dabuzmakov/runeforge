using System.Numerics;
using runeforge.Models;

namespace runeforge.Effects;

public sealed class AnimatedEffect
{
    private float _elapsed;
    private readonly Vector2 _initialPosition;

    public AnimatedEffect(
        SpriteSheetEffectDefinition definition,
        int rowIndex,
        Vector2 position,
        float? scale = null,
        float rotationRadians = 0f,
        bool flipHorizontally = false,
        EnemyEntity? attachedEnemy = null,
        bool anchorBottomCenter = false,
        float attachedVerticalOffset = 0f)
    {
        Definition = definition;
        RowIndex = rowIndex;
        _initialPosition = position;
        Scale = scale ?? definition.DefaultScale;
        RotationRadians = rotationRadians;
        FlipHorizontally = flipHorizontally;
        AttachedEnemy = attachedEnemy;
        AnchorBottomCenter = anchorBottomCenter;
        AttachedVerticalOffset = attachedVerticalOffset;
    }

    public SpriteSheetEffectDefinition Definition { get; }

    public int RowIndex { get; }

    public Vector2 Position
    {
        get
        {
            if (AttachedEnemy == null)
            {
                return _initialPosition;
            }

            var attachedPosition = AttachedEnemy.Transform.Position;
            if (AnchorBottomCenter)
            {
                attachedPosition.Y += AttachedEnemy.Data.Radius * AttachedEnemy.PresentationScale;
            }

            attachedPosition.Y += AttachedVerticalOffset;
            return attachedPosition;
        }
    }

    public float Scale { get; }

    public float RotationRadians { get; }

    public bool FlipHorizontally { get; }

    public EnemyEntity? AttachedEnemy { get; }

    public bool AnchorBottomCenter { get; }

    public float AttachedVerticalOffset { get; }

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
