using System.Numerics;
using System.Threading;
using runeforge.Configs;

namespace runeforge.Models;

public sealed class EnemyEntity
{
    private const float SpawnAnimationDurationSeconds = 0.12f;
    private static int _nextId;
    private float _spawnAnimationElapsed;
    private Vector2 _lastKnownPosition;

    public EnemyEntity(EnemyConfig config, Vector2 position, int tier = 1)
    {
        Id = Interlocked.Increment(ref _nextId);
        Transform = new TransformComponent(position);
        Data = new EnemyDataComponent(config, tier);
        Path = new PathFollowComponent();
        StatusEffects = new EnemyStatusEffectsComponent();
        DamagePopup = new EnemyDamagePopupComponent();
        Data.DamageTaken += HandleDamageTaken;
        _lastKnownPosition = position;
    }

    public int Id { get; }

    public TransformComponent Transform { get; }

    public EnemyDataComponent Data { get; }

    public PathFollowComponent Path { get; }

    public EnemyStatusEffectsComponent StatusEffects { get; }

    public EnemyDamagePopupComponent DamagePopup { get; }

    public Vector2 CurrentVelocity { get; private set; }

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
        DamagePopup.Update(deltaTime);
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
}
