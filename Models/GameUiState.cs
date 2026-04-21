using System.Numerics;
using runeforge.Configs;

namespace runeforge.Models;

public enum BuildSelectionAnimationKind
{
    Add,
    Remove
}

public sealed class BuildSelectionAnimation
{
    private const float DurationSeconds = 0.24f;

    private float _elapsed;

    public BuildSelectionAnimation(
        BuildSelectionAnimationKind kind,
        RuneType runeType,
        int slotIndex,
        Vector2 startPosition,
        Vector2 endPosition)
    {
        Kind = kind;
        RuneType = runeType;
        SlotIndex = slotIndex;
        StartPosition = startPosition;
        EndPosition = endPosition;
    }

    public BuildSelectionAnimationKind Kind { get; }

    public RuneType RuneType { get; }

    public int SlotIndex { get; }

    public Vector2 StartPosition { get; }

    public Vector2 EndPosition { get; }

    public float Progress => Math.Clamp(_elapsed / DurationSeconds, 0f, 1f);

    public bool IsFinished => _elapsed >= DurationSeconds;

    public Vector2 CurrentPosition => Vector2.Lerp(StartPosition, EndPosition, EaseOutCubic(Progress));

    public void Update(float deltaTime)
    {
        _elapsed = Math.Min(_elapsed + deltaTime, DurationSeconds);
    }

    private static float EaseOutCubic(float value)
    {
        var clamped = Math.Clamp(value, 0f, 1f);
        var inverse = 1f - clamped;
        return 1f - (inverse * inverse * inverse);
    }
}

public sealed class BuildSelectionState
{
    public const int BuildSize = 5;

    public List<RuneType> SelectedRunes { get; } = new(BuildSize);

    public Dictionary<RuneType, float> OptionHoverAmounts { get; } = RuneDatabase.AllTypes.ToDictionary(static runeType => runeType, static _ => 0f);

    public bool IsOpen { get; set; } = true;

    public BuildSelectionAnimation? ActiveAnimation { get; set; }

    public bool CanStart => SelectedRunes.Count == BuildSize;
}

public sealed class GameUiState
{
    public RuneEntity? DraggedRune { get; set; }

    public Vector2 DraggedRunePosition { get; set; }

    public Vector2 DraggedRuneGrabOffset { get; set; }

    public bool IsHagalazPathPreviewVisible { get; set; }

    public Vector2[] HagalazPathPreviewPoints { get; set; } = [];

    public Vector2 HagalazPathPreviewCenter { get; set; }

    public bool UseOpenBagSprite { get; set; }

    public float BagScale { get; set; } = 1f;

    public BuildSelectionState BuildSelection { get; } = new();
}
