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
    public const float TooltipShowDelaySeconds = 0.3f;

    public List<RuneType> SelectedRunes { get; } = new(BuildSize);

    public Dictionary<RuneType, float> OptionHoverAmounts { get; } = RuneDatabase.AllTypes.ToDictionary(static runeType => runeType, static _ => 0f);

    public RuneType? HoveredRuneType { get; set; }

    public RuneType? PendingTooltipRuneType { get; set; }

    public float PendingTooltipHoverSeconds { get; set; }

    public Point TooltipAnchor { get; set; }

    public Rectangle HoveredCardBounds { get; set; }

    public float TooltipOpacity { get; set; }

    public float StartButtonHoverAmount { get; set; }

    public bool IsOpen { get; set; } = true;

    public BuildSelectionAnimation? ActiveAnimation { get; set; }

    public bool CanStart => SelectedRunes.Count == BuildSize;
}

public sealed class GameUiState
{
    private const float HeartLossFeedbackDurationSeconds = 0.72f;
    private const float HeartLossShakeFrequency = 34f;
    private const float HeartLossShakeAmplitude = 7f;

    private float _heartLossElapsed = HeartLossFeedbackDurationSeconds;
    private int _heartLossStartIndex = -1;
    private int _heartLossCount;

    public RuneEntity? DraggedRune { get; set; }

    public Vector2 DraggedRunePosition { get; set; }

    public Vector2 DraggedRuneGrabOffset { get; set; }

    public bool IsHagalazPathPreviewVisible { get; set; }

    public Vector2[] HagalazPathPreviewPoints { get; set; } = [];

    public Vector2 HagalazPathPreviewCenter { get; set; }

    public bool UseOpenBagSprite { get; set; }

    public bool UseActiveBagSprite { get; set; }

    public float BagScale { get; set; } = 1f;

    public bool UseActiveRerollButtonSprite { get; set; }

    public float RerollScale { get; set; } = 1f;

    public float PauseButtonHoverAmount { get; set; }

    public float HomeButtonHoverAmount { get; set; }

    public float ExitButtonHoverAmount { get; set; }

    public bool IsStartScreenOpen { get; set; } = true;

    public float StartScreenPlayButtonHoverAmount { get; set; }

    public float PausePopupRestartHoverAmount { get; set; }

    public float PausePopupHomeHoverAmount { get; set; }

    public float PausePopupResumeHoverAmount { get; set; }

    public float PausePopupVisibility { get; set; }

    public float GameOverPopupVisibility { get; set; }

    public float GameOverPopupRestartHoverAmount { get; set; }

    public float GameOverPopupHomeHoverAmount { get; set; }

    public BuildSelectionState BuildSelection { get; } = new();

    public int HeartLossCount => IsHeartLossFeedbackActive ? _heartLossCount : 0;

    public float HeartLossPanelShakeOffset => !IsHeartLossFeedbackActive
        ? 0f
        : MathF.Sin(_heartLossElapsed * HeartLossShakeFrequency) * GetHeartLossEmphasis() * HeartLossShakeAmplitude;

    public float HeartLossPanelFlashOpacity => GetHeartLossEmphasis() * 0.9f;

    public float HeartLossScreenFlashOpacity => GetHeartLossEmphasis() * 0.22f;

    public float HeartLossTextOpacity => GetHeartLossEmphasis();

    public float HeartLossTextRiseOffset => HeartLossProgress * 20f;

    public void TriggerHeartLoss(int heartsBefore, int heartsAfter)
    {
        var lostHeartCount = Math.Max(0, heartsBefore - heartsAfter);
        if (lostHeartCount <= 0)
        {
            return;
        }

        _heartLossStartIndex = Math.Clamp(heartsAfter, 0, GameState.MaxHearts - 1);
        _heartLossCount = lostHeartCount;
        _heartLossElapsed = 0f;
    }

    public void Update(float deltaTime)
    {
        _heartLossElapsed = Math.Min(
            HeartLossFeedbackDurationSeconds,
            _heartLossElapsed + Math.Max(0f, deltaTime));

        if (_heartLossElapsed < HeartLossFeedbackDurationSeconds)
        {
            return;
        }

        _heartLossStartIndex = -1;
        _heartLossCount = 0;
    }

    public bool IsHeartLossHighlightActive(int heartIndex)
    {
        return IsHeartLossFeedbackActive &&
            heartIndex >= _heartLossStartIndex &&
            heartIndex < (_heartLossStartIndex + _heartLossCount);
    }

    public float GetHeartLossScale(int heartIndex)
    {
        if (!IsHeartLossHighlightActive(heartIndex))
        {
            return 1f;
        }

        var pulse = 0.76f + (0.24f * MathF.Sin(HeartLossProgress * MathF.PI * 4f));
        return 1f + (GetHeartLossEmphasis() * 0.34f * pulse);
    }

    public float GetHeartLossGlowOpacity(int heartIndex)
    {
        return IsHeartLossHighlightActive(heartIndex)
            ? GetHeartLossEmphasis()
            : 0f;
    }

    private bool IsHeartLossFeedbackActive =>
        _heartLossCount > 0 &&
        _heartLossElapsed < HeartLossFeedbackDurationSeconds;

    private float HeartLossProgress => Math.Clamp(_heartLossElapsed / HeartLossFeedbackDurationSeconds, 0f, 1f);

    private float GetHeartLossEmphasis()
    {
        if (!IsHeartLossFeedbackActive)
        {
            return 0f;
        }

        var inverseProgress = 1f - HeartLossProgress;
        return inverseProgress * inverseProgress;
    }
}
