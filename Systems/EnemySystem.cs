using System.Numerics;
using runeforge.Configs;
using runeforge.Factories;
using runeforge.Models;

namespace runeforge.Systems;

public sealed class EnemySystem
{
    private readonly EnemyFactory _enemyFactory;
    private readonly WaveGenerator _waveGenerator;
    private readonly DamagePopupSystem _damagePopupSystem;
    private readonly RuneEffectSystem _runeEffectSystem;
    private readonly EffectAnimationSystem _effectAnimationSystem;

    public EnemySystem(
        EnemyFactory enemyFactory,
        WaveGenerator waveGenerator,
        DamagePopupSystem damagePopupSystem,
        RuneEffectSystem runeEffectSystem,
        EffectAnimationSystem effectAnimationSystem)
    {
        _enemyFactory = enemyFactory;
        _waveGenerator = waveGenerator;
        _damagePopupSystem = damagePopupSystem;
        _runeEffectSystem = runeEffectSystem;
        _effectAnimationSystem = effectAnimationSystem;
    }

    public void ForceStartWave(GameState gameState, int waveNumber)
    {
        var clampedWaveNumber = Math.Max(1, waveNumber);
        gameState.Waves.CurrentWaveNumber = clampedWaveNumber;
        gameState.Waves.ActiveWave = _waveGenerator.Generate(clampedWaveNumber);
        gameState.Waves.SpawnedEnemiesInWave = 0;
        gameState.Waves.TimeUntilNextSpawn = 0f;
    }

    public void Update(GameState gameState, IReadOnlyList<Vector2> path, float pathLength, float deltaTime)
    {
        StartNextWaveIfNeeded(gameState);
        UpdateSpawning(gameState, path, deltaTime);
        UpdateAuraEffects(gameState);
        UpdateMovement(gameState, path, pathLength, deltaTime);
        Cleanup(gameState, path, pathLength);
        StartNextWaveIfNeeded(gameState);
    }

    private void UpdateSpawning(GameState gameState, IReadOnlyList<Vector2> path, float deltaTime)
    {
        if (gameState.IsDefeated || path.Count == 0 || gameState.Waves.ActiveWave == null)
        {
            return;
        }

        var waveState = gameState.Waves;
        waveState.TimeUntilNextSpawn -= deltaTime;
        if (waveState.TimeUntilNextSpawn > 0f)
        {
            return;
        }

        while (waveState.TimeUntilNextSpawn <= 0f && !waveState.IsWaveSpawnFinished)
        {
            var spawnEntry = waveState.ActiveWave.SpawnEntries[waveState.SpawnedEnemiesInWave];
            gameState.Enemies.Add(_enemyFactory.Create(spawnEntry, path[0]));
            waveState.SpawnedEnemiesInWave++;
            waveState.TimeUntilNextSpawn += waveState.ActiveWave.SpawnIntervalSeconds;
        }
    }

    private void UpdateMovement(GameState gameState, IReadOnlyList<Vector2> path, float pathLength, float deltaTime)
    {
        foreach (var enemy in gameState.Enemies)
        {
            enemy.UpdatePresentation(deltaTime);

            if (!enemy.Data.IsAlive)
            {
                continue;
            }

            var statusTickResult = enemy.StatusEffects.Update(deltaTime, enemy.Data.Health);
            if (statusTickResult.PoisonDamage > 0f)
            {
                var modifiedPoisonDamage = enemy.StatusEffects.ApplyIncomingDamageMultiplier(statusTickResult.PoisonDamage);
                _damagePopupSystem.Spawn(gameState, enemy, modifiedPoisonDamage, DamagePopupStyle.Poison);
                enemy.Data.TakeDamage(modifiedPoisonDamage);
                _runeEffectSystem.RegisterRuneDamageDealt(gameState, statusTickResult.PoisonSourceRune, modifiedPoisonDamage);
                if (!enemy.Data.IsAlive)
                {
                    continue;
                }
            }

            if (statusTickResult.BurnDamage > 0f)
            {
                var modifiedBurnDamage = enemy.StatusEffects.ApplyIncomingDamageMultiplier(statusTickResult.BurnDamage);
                _damagePopupSystem.Spawn(gameState, enemy, modifiedBurnDamage, DamagePopupStyle.Burn);
                enemy.Data.TakeDamage(modifiedBurnDamage);
                _runeEffectSystem.RegisterRuneDamageDealt(gameState, statusTickResult.BurnSourceRune, modifiedBurnDamage);
                if (!enemy.Data.IsAlive)
                {
                    continue;
                }
            }

            var intrinsicHealthRegenerationPercent =
                enemy.Data.Config.MaxHealthRegenerationPerSecond *
                BossTuning.GetIntrinsicHealthRegenerationMultiplier(enemy.Data.Type, enemy.Data.Rank);
            var totalHealthRegenerationPercent = intrinsicHealthRegenerationPercent +
                enemy.StatusEffects.AuraHealthRegenerationPercentPerSecond;
            if (totalHealthRegenerationPercent > 0f)
            {
                enemy.Data.RestoreHealth(enemy.Data.MaxHealth * totalHealthRegenerationPercent * deltaTime);
            }

            enemy.UpdateTeleportTimer(deltaTime);
            if (TryProcessTeleport(gameState, enemy, path, pathLength, deltaTime))
            {
                continue;
            }

            if (enemy.IsTeleportShrinking)
            {
                enemy.SyncMovementVelocity(deltaTime);
                continue;
            }

            var effectiveSpeed = enemy.Data.Speed * enemy.StatusEffects.MovementSpeedMultiplier;
            enemy.Path.Update(enemy.Transform, effectiveSpeed, deltaTime, path);
            enemy.SyncMovementVelocity(deltaTime);
            if (enemy.Path.HasReachedGoal)
            {
                gameState.EscapedEnemyCount++;
                enemy.Data.MarkDead();
            }
        }
    }

    private static void UpdateAuraEffects(GameState gameState)
    {
        for (var i = 0; i < gameState.Enemies.Count; i++)
        {
            gameState.Enemies[i].StatusEffects.ResetAuraEffects();
        }

        for (var sourceIndex = 0; sourceIndex < gameState.Enemies.Count; sourceIndex++)
        {
            var sourceEnemy = gameState.Enemies[sourceIndex];
            if (!sourceEnemy.Data.IsAlive || sourceEnemy.Path.HasReachedGoal || sourceEnemy.AuraType == EnemyAuraType.None)
            {
                continue;
            }

            for (var targetIndex = 0; targetIndex < gameState.Enemies.Count; targetIndex++)
            {
                var targetEnemy = gameState.Enemies[targetIndex];
                if (!targetEnemy.Data.IsAlive || targetEnemy.Path.HasReachedGoal)
                {
                    continue;
                }

                var distanceThreshold = AuraEnemyTuning.AuraRadius + targetEnemy.Data.Radius;
                if (Vector2.DistanceSquared(sourceEnemy.Transform.Position, targetEnemy.Transform.Position) > distanceThreshold * distanceThreshold)
                {
                    continue;
                }

                var auraEffectMultiplier = sourceEnemy.Data.IsBoss
                    ? BossTuning.AuraEffectMultiplier
                    : 1f;

                switch (sourceEnemy.AuraType)
                {
                    case EnemyAuraType.Regeneration:
                        targetEnemy.StatusEffects.ApplyAuraRegeneration(
                            AuraEnemyTuning.RegenerationPercentPerSecond * auraEffectMultiplier);
                        break;
                    case EnemyAuraType.Speed:
                        targetEnemy.StatusEffects.ApplyAuraMoveSpeed(
                            AuraEnemyTuning.SpeedBonusPercent * auraEffectMultiplier);
                        break;
                    case EnemyAuraType.Immunity:
                        targetEnemy.StatusEffects.ApplyAuraImmunity();
                        break;
                }
            }
        }
    }

    private bool TryProcessTeleport(
        GameState gameState,
        EnemyEntity enemy,
        IReadOnlyList<Vector2> path,
        float pathLength,
        float deltaTime)
    {
        if (!enemy.CanTeleportNow || path.Count < 2 || IsOnFinalRightPathSection(enemy, path))
        {
            if (enemy.IsTeleportShrinking && enemy.CanCompleteTeleportShrink)
            {
                return CompleteTeleport(gameState, enemy, path, pathLength, deltaTime);
            }

            if (enemy.IsTeleportShrinking)
            {
                return true;
            }

            return false;
        }

        if (!enemy.IsTeleportShrinking)
        {
            enemy.BeginTeleportShrink();
            return true;
        }

        if (!enemy.CanCompleteTeleportShrink)
        {
            return true;
        }

        return CompleteTeleport(gameState, enemy, path, pathLength, deltaTime);
    }

    private bool CompleteTeleport(
        GameState gameState,
        EnemyEntity enemy,
        IReadOnlyList<Vector2> path,
        float pathLength,
        float deltaTime)
    {
        var sourcePosition = enemy.Transform.Position;
        var targetDistance = Math.Min(pathLength, enemy.Path.Progress + enemy.Data.Config.TeleportDistance);
                enemy.Path.MoveToDistance(enemy.Transform, targetDistance, path, pathLength);
                enemy.ConsumeTeleport();
                enemy.CompleteTeleportShrink();
                var effectScale = MathF.Max(0.8f, enemy.Data.Radius / 18f);
        _effectAnimationSystem.TrySpawnTeleportAnimation(gameState, sourcePosition, effectScale);
        _effectAnimationSystem.TrySpawnTeleportAnimation(gameState, enemy.Transform.Position, effectScale);

        if (!enemy.Path.HasReachedGoal)
        {
            enemy.SyncMovementVelocity(deltaTime);
            return true;
        }

        enemy.SyncMovementVelocity(deltaTime);
        gameState.EscapedEnemyCount++;
        enemy.Data.MarkDead();
        return true;
    }

    private static bool IsOnFinalRightPathSection(EnemyEntity enemy, IReadOnlyList<Vector2> path)
    {
        if (path.Count < 2)
        {
            return false;
        }

        var pathPoint = PathGeometry.GetClosestPointResult(path, enemy.Transform.Position);
        var segment = pathPoint.SegmentEnd - pathPoint.SegmentStart;
        if (MathF.Abs(segment.Y) <= MathF.Abs(segment.X))
        {
            return false;
        }

        var rightMostX = path.Max(static point => point.X);
        return MathF.Abs(pathPoint.SegmentStart.X - rightMostX) <= 0.001f &&
            MathF.Abs(pathPoint.SegmentEnd.X - rightMostX) <= 0.001f;
    }

    private void Cleanup(GameState gameState, IReadOnlyList<Vector2> path, float pathLength)
    {
        for (var i = gameState.Enemies.Count - 1; i >= 0; i--)
        {
            var enemy = gameState.Enemies[i];
            if (enemy.Data.IsAlive)
            {
                continue;
            }

            if (!enemy.Path.HasReachedGoal)
            {
                gameState.KilledEnemyCount++;

                if (enemy.Data.Type == EnemyType.Cluster)
                {
                    SpawnClusterShards(gameState, enemy, path, pathLength);
                }

                var baseRunePoints = EconomyTuning.GetEnemyKillRunePointReward(enemy.Data.Type, enemy.Data.Tier);
                var bonusRunePoints = enemy.StatusEffects.ConsumeFehuBonusRunePoints(baseRunePoints);
                gameState.Economy.GrantRunePoints(baseRunePoints + bonusRunePoints);
            }

            gameState.Enemies.RemoveAt(i);
        }
    }

    private void SpawnClusterShards(GameState gameState, EnemyEntity clusterEnemy, IReadOnlyList<Vector2> path, float pathLength)
    {
        if (path.Count < 2)
        {
            return;
        }

        var shardSpawnEntry = CreateClusterShardSpawnEntry(clusterEnemy);
        var shardRadius = EnemyCatalog.Get(EnemyType.ClusterShard).Radius * shardSpawnEntry.SizeMultiplier;
        var shardSpacing = (shardRadius * 2f) + 4f;
        var firstShardDistance = clusterEnemy.Path.Progress + (shardSpacing * 1.5f);

        for (var shardIndex = 0; shardIndex < 4; shardIndex++)
        {
            var shard = _enemyFactory.Create(shardSpawnEntry, clusterEnemy.Transform.Position);
            var spawnDistance = Math.Max(0f, firstShardDistance - (shardIndex * shardSpacing));
            shard.Path.MoveToDistance(shard.Transform, spawnDistance, path, pathLength);
            shard.SyncMovementVelocity(0f);
            gameState.Enemies.Add(shard);
        }
    }

    private static EnemySpawnEntry CreateClusterShardSpawnEntry(EnemyEntity clusterEnemy)
    {
        if (!clusterEnemy.Data.IsBoss)
        {
            return new EnemySpawnEntry(
                EnemyType.ClusterShard,
                clusterEnemy.Data.Tier,
                SpeedMultiplier: clusterEnemy.Data.SpeedMultiplier);
        }

        return new EnemySpawnEntry(
            EnemyType.ClusterShard,
            clusterEnemy.Data.Tier,
            EnemySpawnRank.BossShard,
            BossTuning.GetClusterShardHealthMultiplier(clusterEnemy.Data.HealthMultiplier),
            BossTuning.ClusterShardSizeMultiplier,
            clusterEnemy.Data.SpeedMultiplier);
    }

    private void StartNextWaveIfNeeded(GameState gameState)
    {
        if (gameState.IsDefeated)
        {
            return;
        }

        var waveState = gameState.Waves;
        if (waveState.ActiveWave != null)
        {
            var waveResolved = waveState.IsWaveSpawnFinished && gameState.Enemies.Count == 0;
            if (!waveResolved)
            {
                return;
            }
        }

        ForceStartWave(gameState, waveState.CurrentWaveNumber + 1);
    }
}
