using System.Numerics;
using System.Threading;
using runeforge.Configs;

namespace runeforge.Models;

public sealed class EnemyEntity
{
    private const float SpawnAnimationDurationSeconds = 0.12f;
    private const float TeleportShrinkDurationSeconds = 0.12f;
    private static int _nextId;
    private float _spawnAnimationElapsed;
    private Vector2 _lastKnownPosition;
    private float _timeUntilNextTeleport;
    private float _teleportShrinkElapsed;

    public EnemyEntity(
        EnemyConfig config,
        Vector2 position,
        int tier = 1,
        EnemySpawnRank rank = EnemySpawnRank.Normal,
        float healthMultiplier = 1f,
        float sizeMultiplier = 1f,
        float speedMultiplier = 1f)
    {
        Id = Interlocked.Increment(ref _nextId);
        Transform = new TransformComponent(position);
        Data = new EnemyDataComponent(config, tier, rank, healthMultiplier, sizeMultiplier, speedMultiplier);
        AuraType = CreateAuraType(config.Type);
        Path = new PathFollowComponent();
        StatusEffects = new EnemyStatusEffectsComponent();
        DamagePopup = new EnemyDamagePopupComponent();
        Data.DamageTaken += HandleDamageTaken;
        _lastKnownPosition = position;
        RemainingTeleports = config.MaxTeleportCount;
        _timeUntilNextTeleport = CreateRandomTeleportDelay(config);
    }

    public int Id { get; }

    public TransformComponent Transform { get; }

    public EnemyDataComponent Data { get; }

    public EnemyAuraType AuraType { get; }

    public PathFollowComponent Path { get; }

    public EnemyStatusEffectsComponent StatusEffects { get; }

    public EnemyDamagePopupComponent DamagePopup { get; }

    public Vector2 CurrentVelocity { get; private set; }

    public int RemainingTeleports { get; private set; }

    public bool CanTeleportNow =>
        RemainingTeleports > 0 &&
        Data.Config.TeleportDistance > 0f &&
        _timeUntilNextTeleport <= 0f;

    public float SpawnScale
    {
        get
        {
            if (_spawnAnimationElapsed >= SpawnAnimationDurationSeconds)
            {
                return 1f;
            }

            return _spawnAnimationElapsed / SpawnAnimationDurationSeconds;
        }
    }

    public float PresentationScale => SpawnScale * GetTeleportScaleMultiplier();

    public bool IsTeleportShrinking { get; private set; }

    public bool CanCompleteTeleportShrink =>
        IsTeleportShrinking &&
        _teleportShrinkElapsed >= TeleportShrinkDurationSeconds;

    public void UpdateSpawnAnimation(float deltaTime)
    {
        if (_spawnAnimationElapsed >= SpawnAnimationDurationSeconds)
        {
            return;
        }

        _spawnAnimationElapsed = MathF.Min(
            SpawnAnimationDurationSeconds,
            _spawnAnimationElapsed + deltaTime);
    }

    public void UpdatePresentation(float deltaTime)
    {
        UpdateSpawnAnimation(deltaTime);
        UpdateTeleportShrinkAnimation(deltaTime);
        DamagePopup.Update(deltaTime);
    }

    public void UpdateTeleportTimer(float deltaTime)
    {
        if (RemainingTeleports <= 0 || Data.Config.TeleportDistance <= 0f)
        {
            return;
        }

        _timeUntilNextTeleport -= deltaTime;
    }

    public void ConsumeTeleport()
    {
        if (RemainingTeleports <= 0)
        {
            return;
        }

        RemainingTeleports--;
        _timeUntilNextTeleport = RemainingTeleports > 0
            ? CreateRandomTeleportDelay(Data.Config)
            : float.MaxValue;
    }

    public void BeginTeleportShrink()
    {
        if (IsTeleportShrinking)
        {
            return;
        }

        IsTeleportShrinking = true;
        _teleportShrinkElapsed = 0f;
    }

    public void CompleteTeleportShrink()
    {
        IsTeleportShrinking = false;
        _teleportShrinkElapsed = 0f;
    }

    public void SyncMovementVelocity(float deltaTime)
    {
        if (deltaTime <= 0f)
        {
            CurrentVelocity = Vector2.Zero;
            _lastKnownPosition = Transform.Position;
            return;
        }

        CurrentVelocity = (Transform.Position - _lastKnownPosition) / deltaTime;
        _lastKnownPosition = Transform.Position;
    }

    private void HandleDamageTaken(float damage, bool isCriticalHit)
    {
        if (damage <= 0f)
        {
            return;
        }

        DamagePopup.Show(damage, isCriticalHit);
    }

    private void UpdateTeleportShrinkAnimation(float deltaTime)
    {
        if (!IsTeleportShrinking)
        {
            return;
        }

        _teleportShrinkElapsed = MathF.Min(
            TeleportShrinkDurationSeconds,
            _teleportShrinkElapsed + deltaTime);
    }

    private float GetTeleportScaleMultiplier()
    {
        if (!IsTeleportShrinking)
        {
            return 1f;
        }

        return 1f - Math.Clamp(_teleportShrinkElapsed / TeleportShrinkDurationSeconds, 0f, 1f);
    }

    private static float CreateRandomTeleportDelay(EnemyConfig config)
    {
        if (config.MaxTeleportCount <= 0 || config.TeleportDistance <= 0f)
        {
            return float.MaxValue;
        }

        if (config.MaxTeleportDelaySeconds <= config.MinTeleportDelaySeconds)
        {
            return config.MinTeleportDelaySeconds;
        }

        return config.MinTeleportDelaySeconds +
            (Random.Shared.NextSingle() * (config.MaxTeleportDelaySeconds - config.MinTeleportDelaySeconds));
    }

    private static EnemyAuraType CreateAuraType(EnemyType enemyType)
    {
        if (enemyType != EnemyType.Aura)
        {
            return EnemyAuraType.None;
        }

        return Random.Shared.Next(3) switch
        {
            0 => EnemyAuraType.Regeneration,
            1 => EnemyAuraType.Speed,
            _ => EnemyAuraType.Immunity
        };
    }
}
