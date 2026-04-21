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

    public EnemySystem(EnemyFactory enemyFactory, WaveGenerator waveGenerator, DamagePopupSystem damagePopupSystem)
    {
        _enemyFactory = enemyFactory;
        _waveGenerator = waveGenerator;
        _damagePopupSystem = damagePopupSystem;
    }

    public void Update(GameState gameState, IReadOnlyList<Vector2> path, float deltaTime)
    {
        StartNextWaveIfNeeded(gameState);
        UpdateSpawning(gameState, path, deltaTime);
        UpdateMovement(gameState, path, deltaTime);
        Cleanup(gameState);
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
            gameState.Enemies.Add(_enemyFactory.Create(spawnEntry.Archetype, path[0], spawnEntry.Tier));
            waveState.SpawnedEnemiesInWave++;
            waveState.TimeUntilNextSpawn += waveState.ActiveWave.SpawnIntervalSeconds;
        }
    }

    private void UpdateMovement(GameState gameState, IReadOnlyList<Vector2> path, float deltaTime)
    {
        foreach (var enemy in gameState.Enemies)
        {
            enemy.UpdatePresentation(deltaTime);

            if (!enemy.Data.IsAlive)
            {
                continue;
            }

            var poisonDamage = enemy.StatusEffects.Update(deltaTime);
            if (poisonDamage > 0f)
            {
                var modifiedPoisonDamage = enemy.StatusEffects.ApplyIncomingDamageMultiplier(poisonDamage);
                _damagePopupSystem.Spawn(gameState, enemy, modifiedPoisonDamage, DamagePopupStyle.Poison);
                enemy.Data.TakeDamage(modifiedPoisonDamage);
                if (!enemy.Data.IsAlive)
                {
                    continue;
                }
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

    private static void Cleanup(GameState gameState)
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
                gameState.Economy.GrantRunePoints(EconomyTuning.GetEnemyKillRunePointReward(enemy.Data.Type, enemy.Data.Tier));
            }

            gameState.Enemies.RemoveAt(i);
        }
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

        var nextWaveNumber = waveState.CurrentWaveNumber + 1;
        waveState.CurrentWaveNumber = nextWaveNumber;
        waveState.ActiveWave = _waveGenerator.Generate(nextWaveNumber);
        waveState.SpawnedEnemiesInWave = 0;
        waveState.TimeUntilNextSpawn = 0f;
    }
}
