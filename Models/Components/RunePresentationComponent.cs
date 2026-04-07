using System.Numerics;

namespace runeforge.Models;

public sealed class RunePresentationComponent
{
    private enum RuneActionState
    {
        Idle,
        SpawnFromBag,
        MergeIntoRune,
        InsertIntoBag
    }

    private const float AttackPulseDuration = 0.13f;
    private const float AttackPulseScale = 0.12f;
    private const float AttackPulseChargeRatio = 0.62f;
    private const float MergePopDuration = 0.14f;
    private const float MergePopScale = 0.15f;
    private const float MergePopChargeRatio = 0.45f;
    private const float DragScaleBonus = 0.1f;
    private const float HoverScaleBonus = 0.14f;
    private const float VisualApproachSpeed = 14f;
    private const float MergeTravelDuration = 0.12f;
    private const float BagInsertDuration = 0.15f;
    private const float BagSpawnDuration = 0.16f;
    private const float MergeEndScale = 0.28f;
    private const float BagInsertEndScale = 0.16f;
    private const float BagSpawnStartScale = 0.38f;

    private float _pulseElapsed = AttackPulseDuration;
    private float _pulseDuration = AttackPulseDuration;
    private float _pulsePeakScale;
    private float _pulseChargeRatio = AttackPulseChargeRatio;
    private float _dragEmphasis;
    private float _hoverEmphasis;
    private bool _isDragged;
    private bool _isMergeHoverTarget;
    private bool _isReservedForMerge;
    private RuneActionState _actionState;
    private Vector2 _visualPosition;
    private Vector2 _actionStartPosition;
    private Vector2 _actionTargetPosition;
    private float _actionElapsed;
    private float _actionDuration;
    private float _actionStartScale = 1f;
    private float _actionEndScale = 1f;
    private float _actionStartAlpha = 1f;
    private float _actionEndAlpha = 1f;

    public RunePresentationComponent(Vector2 worldPosition)
    {
        _visualPosition = worldPosition;
    }

    public Vector2 VisualPosition { get; private set; }

    public float VisualScale { get; private set; } = 1f;

    public float VisualAlpha { get; private set; } = 1f;

    public bool IsCombatActive => _actionState == RuneActionState.Idle && !_isDragged && !_isReservedForMerge;

    public bool IsInteractable => _actionState == RuneActionState.Idle && !_isReservedForMerge;

    public bool ShouldRenderAboveBag => _actionState != RuneActionState.Idle;

    public bool IsTransientAnimationComplete => _actionState != RuneActionState.Idle && _actionElapsed >= _actionDuration;

    public void TriggerAttackPulse()
    {
        StartPulse(AttackPulseDuration, AttackPulseScale, AttackPulseChargeRatio);
    }

    public void TriggerMergePop()
    {
        StartPulse(MergePopDuration, MergePopScale, MergePopChargeRatio);
    }

    public void SetDragged(bool isDragged)
    {
        _isDragged = isDragged;
    }

    public void SetMergeHoverTarget(bool isMergeHoverTarget)
    {
        _isMergeHoverTarget = isMergeHoverTarget;
    }

    public void SetReservedForMerge(bool isReservedForMerge)
    {
        _isReservedForMerge = isReservedForMerge;
    }

    public void BeginMergeInto(Vector2 startPosition, Vector2 targetPosition)
    {
        BeginAction(
            RuneActionState.MergeIntoRune,
            startPosition,
            targetPosition,
            MergeTravelDuration,
            CaptureCurrentScale(),
            MergeEndScale,
            1f,
            0f);
    }

    public void BeginBagInsert(Vector2 startPosition, Vector2 targetPosition)
    {
        BeginAction(
            RuneActionState.InsertIntoBag,
            startPosition,
            targetPosition,
            BagInsertDuration,
            CaptureCurrentScale(),
            BagInsertEndScale,
            1f,
            0f);
    }

    public void BeginSpawnFromBag(Vector2 startPosition, Vector2 targetPosition)
    {
        BeginAction(
            RuneActionState.SpawnFromBag,
            startPosition,
            targetPosition,
            BagSpawnDuration,
            BagSpawnStartScale,
            1f,
            0f,
            1f);
    }

    public void Update(float deltaTime, Vector2 worldPosition)
    {
        _pulseElapsed = Math.Min(_pulseDuration, _pulseElapsed + deltaTime);
        _dragEmphasis = Approach(_dragEmphasis, _isDragged ? 1f : 0f, deltaTime * VisualApproachSpeed);
        _hoverEmphasis = Approach(_hoverEmphasis, _isMergeHoverTarget ? 1f : 0f, deltaTime * VisualApproachSpeed);

        if (_actionState == RuneActionState.Idle)
        {
            VisualPosition = worldPosition;
            VisualScale = 1f + EvaluatePulseScale() + (_dragEmphasis * DragScaleBonus) + (_hoverEmphasis * HoverScaleBonus);
            VisualAlpha = 1f;
            return;
        }

        _actionElapsed = Math.Min(_actionDuration, _actionElapsed + deltaTime);
        VisualPosition = Vector2.Lerp(_actionStartPosition, _actionTargetPosition, EaseOutCubic(GetActionProgress()));
        VisualScale = Lerp(_actionStartScale, _actionEndScale, GetActionProgress());
        VisualAlpha = Lerp(_actionStartAlpha, _actionEndAlpha, GetActionProgress());

        if (_actionState == RuneActionState.SpawnFromBag && _actionElapsed >= _actionDuration)
        {
            _actionState = RuneActionState.Idle;
            VisualPosition = worldPosition;
            VisualScale = 1f + EvaluatePulseScale() + (_dragEmphasis * DragScaleBonus) + (_hoverEmphasis * HoverScaleBonus);
            VisualAlpha = 1f;
        }
    }

    private void StartPulse(float duration, float peakScale, float chargeRatio)
    {
        _pulseElapsed = 0f;
        _pulseDuration = duration;
        _pulsePeakScale = peakScale;
        _pulseChargeRatio = chargeRatio;
    }

    private void BeginAction(
        RuneActionState actionState,
        Vector2 startPosition,
        Vector2 targetPosition,
        float duration,
        float startScale,
        float endScale,
        float startAlpha,
        float endAlpha)
    {
        _actionState = actionState;
        _actionStartPosition = startPosition;
        _actionTargetPosition = targetPosition;
        VisualPosition = startPosition;
        _actionDuration = duration;
        _actionElapsed = 0f;
        _actionStartScale = startScale;
        _actionEndScale = endScale;
        _actionStartAlpha = startAlpha;
        _actionEndAlpha = endAlpha;
        _isDragged = false;
        _isMergeHoverTarget = false;
    }

    private float EvaluatePulseScale()
    {
        if (_pulseElapsed >= _pulseDuration || _pulsePeakScale <= 0f)
        {
            return 0f;
        }

        var progress = _pulseElapsed / _pulseDuration;
        if (progress <= _pulseChargeRatio)
        {
            var chargeProgress = progress / _pulseChargeRatio;
            return SmoothStep(chargeProgress) * _pulsePeakScale;
        }

        var releaseProgress = (progress - _pulseChargeRatio) / (1f - _pulseChargeRatio);
        return (1f - releaseProgress) * _pulsePeakScale;
    }

    private float CaptureCurrentScale()
    {
        return 1f + EvaluatePulseScale() + (_dragEmphasis * DragScaleBonus) + (_hoverEmphasis * HoverScaleBonus);
    }

    private float GetActionProgress()
    {
        return _actionDuration <= 0f ? 1f : Math.Clamp(_actionElapsed / _actionDuration, 0f, 1f);
    }

    private static float Approach(float value, float target, float step)
    {
        if (value < target)
        {
            return Math.Min(value + step, target);
        }

        return Math.Max(value - step, target);
    }

    private static float SmoothStep(float value)
    {
        var clamped = Math.Clamp(value, 0f, 1f);
        return clamped * clamped * (3f - (2f * clamped));
    }

    private static float EaseOutCubic(float value)
    {
        var clamped = Math.Clamp(value, 0f, 1f);
        var inverse = 1f - clamped;
        return 1f - (inverse * inverse * inverse);
    }

    private static float Lerp(float start, float end, float amount)
    {
        return start + ((end - start) * Math.Clamp(amount, 0f, 1f));
    }
}
