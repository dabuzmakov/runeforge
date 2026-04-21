using runeforge.Configs;
using runeforge.Models;
using runeforge.Runes;

namespace runeforge.Systems;

public sealed class RuneEffectSystem
{
    private readonly DamagePopupSystem _damagePopupSystem;
    private readonly AnsuzAllySystem _ansuzAllySystem;
    private readonly EffectAnimationSystem _effectAnimationSystem;

    public RuneEffectSystem(
        DamagePopupSystem damagePopupSystem,
        AnsuzAllySystem ansuzAllySystem,
        EffectAnimationSystem effectAnimationSystem)
    {
        _damagePopupSystem = damagePopupSystem;
        _ansuzAllySystem = ansuzAllySystem;
        _effectAnimationSystem = effectAnimationSystem;
    }

    public void ApplyHitEffects(
        GameState gameState,
        IReadOnlyList<System.Numerics.Vector2> path,
        ProjectileEntity projectile,
        EffectAnimationSystem effectAnimationSystem)
    {
        var targetEnemy = projectile.Flight.HitTarget;
        if (targetEnemy == null)
        {
            return;
        }

        if (!TryApplyExternalRuneAttackKill(
                gameState,
                targetEnemy,
                projectile.Impact.SourceRuneType,
                projectile.Impact.SourceRuneTier))
        {
            ApplyDamage(
                gameState,
                targetEnemy,
                projectile.Impact.Damage,
                projectile.Impact.IsCriticalHit ? DamagePopupStyle.Critical : DamagePopupStyle.Normal,
                projectile.Impact.IsCriticalHit);
        }

        RuneBehaviorRegistry.Get(projectile.Impact.SourceRuneType).OnProjectileHit(
            new RuneHitContext(gameState, projectile, targetEnemy, path, this, effectAnimationSystem));
    }

    public void ApplyDirectDamage(
        GameState gameState,
        EnemyEntity targetEnemy,
        float damage,
        bool isCriticalHit = false,
        RuneType? sourceRuneType = null,
        int sourceRuneTier = 1)
    {
        if (!targetEnemy.Data.IsAlive || targetEnemy.Path.HasReachedGoal || damage <= 0f)
        {
            return;
        }

        if (sourceRuneType.HasValue &&
            TryApplyExternalRuneAttackKill(gameState, targetEnemy, sourceRuneType.Value, sourceRuneTier))
        {
            return;
        }

        ApplyDamage(
            gameState,
            targetEnemy,
            damage,
            isCriticalHit ? DamagePopupStyle.Critical : DamagePopupStyle.Normal,
            isCriticalHit);
    }

    public void ApplyIsaLaneSlow(GameState gameState)
    {
        var activeIsaTiers = gameState.Runes
            .Where(static rune => rune.Stats.Type == RuneType.Isa && rune.Presentation.IsCombatActive)
            .Select(static rune => rune.Stats.Tier)
            .ToArray();

        if (activeIsaTiers.Length == 0)
        {
            return;
        }

        var slowPercent = IsaTuning.GetCombinedSlowPercent(activeIsaTiers);
        var durationSeconds = IsaTuning.GetCombinedSlowDurationSeconds(activeIsaTiers);

        for (var i = 0; i < gameState.Enemies.Count; i++)
        {
            var enemy = gameState.Enemies[i];
            if (!enemy.Data.IsAlive || enemy.Path.HasReachedGoal)
            {
                continue;
            }

            enemy.StatusEffects.ApplyOrRefreshIsaSlow(slowPercent, durationSeconds);
        }
    }

    public void TrySpawnAnsuzAllyFromKilledEnemy(
        GameState gameState,
        IReadOnlyList<System.Numerics.Vector2> path,
        EnemyEntity sourceEnemy,
        int runeTier)
    {
        _ansuzAllySystem.TrySpawnFromKilledEnemy(
            gameState,
            path,
            PathGeometry.ComputeLength(path),
            sourceEnemy,
            runeTier);
    }

    public void ApplyKenazSplashDamage(
        GameState gameState,
        ProjectileEntity projectile,
        EnemyEntity primaryTarget,
        EffectAnimationSystem effectAnimationSystem)
    {
        var impactPosition = projectile.Transform.Position;
        effectAnimationSystem.TrySpawnKenazExplosionAnimation(gameState, impactPosition);

        var splashDamage = projectile.Impact.BaseDamage * KenazTuning.SplashDamageMultiplier;
        ApplyRadialDamage(
            gameState,
            gameState.Enemies,
            impactPosition,
            KenazTuning.SplashRadius,
            splashDamage,
            projectile.Impact.SourceRuneType,
            projectile.Impact.SourceRuneTier,
            KenazTuning.IncludePrimaryTargetInSplash ? null : primaryTarget);
    }

    private void ApplyRadialDamage(
        GameState gameState,
        IReadOnlyList<EnemyEntity> enemies,
        System.Numerics.Vector2 center,
        float radius,
        float damage,
        RuneType? sourceRuneType = null,
        int sourceRuneTier = 1,
        EnemyEntity? excludedEnemy = null)
    {
        for (var i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (!enemy.Data.IsAlive || enemy.Path.HasReachedGoal || ReferenceEquals(enemy, excludedEnemy))
            {
                continue;
            }

            var distanceThreshold = radius + enemy.Data.Radius;
            var delta = enemy.Transform.Position - center;
            if (delta.LengthSquared() > distanceThreshold * distanceThreshold)
            {
                continue;
            }

            if (sourceRuneType.HasValue &&
                TryApplyExternalRuneAttackKill(gameState, enemy, sourceRuneType.Value, sourceRuneTier))
            {
                continue;
            }

            ApplyDamage(gameState, enemy, damage);
        }
    }

    public void TryApplyBerkanoPoison(
        GameState gameState,
        ProjectileEntity projectile,
        EnemyEntity primaryTarget,
        EffectAnimationSystem effectAnimationSystem)
    {
        var runeTier = projectile.Impact.SourceRuneTier;
        if (Random.Shared.NextSingle() > BerkanoTuning.GetPoisonChance(runeTier))
        {
            return;
        }

        var availableEnemies = gameState.Enemies
            .Where(static enemy => enemy.Data.IsAlive && !enemy.Path.HasReachedGoal)
            .ToArray();
        if (availableEnemies.Length == 0)
        {
            return;
        }

        var radius = BerkanoTuning.GetPoisonRadius(runeTier);
        var durationSeconds = BerkanoTuning.GetPoisonDurationSeconds(runeTier);
        var damagePerTick = BerkanoTuning.GetPoisonDamagePerTick(runeTier) * projectile.Impact.EffectDamageMultiplier;
        var epicenterEnemy = availableEnemies[Random.Shared.Next(availableEnemies.Length)];
        var center = epicenterEnemy.Transform.Position;
        var appliedToAnyEnemy = false;

        for (var i = 0; i < gameState.Enemies.Count; i++)
        {
            var enemy = gameState.Enemies[i];
            if (!enemy.Data.IsAlive || enemy.Path.HasReachedGoal)
            {
                continue;
            }

            var distanceThreshold = radius + enemy.Data.Radius;
            var delta = enemy.Transform.Position - center;
            if (delta.LengthSquared() > distanceThreshold * distanceThreshold)
            {
                continue;
            }

            enemy.StatusEffects.ApplyPoison(
                damagePerTick,
                durationSeconds,
                BerkanoTuning.PoisonTickIntervalSeconds);
            appliedToAnyEnemy = true;
        }

        if (appliedToAnyEnemy)
        {
            effectAnimationSystem.TrySpawnBerkanoPoisonAnimation(gameState, center);
        }
    }

    public void ApplyNauthizShatter(EnemyEntity targetEnemy, int runeTier)
    {
        if (!targetEnemy.Data.IsAlive || targetEnemy.Path.HasReachedGoal)
        {
            return;
        }

        targetEnemy.StatusEffects.ApplyOrUpgradeShatter(
            NauthizTuning.GetIncomingDamageBonusPercentPerStack(runeTier));
    }

    public void ApplyDamage(
        GameState gameState,
        EnemyEntity targetEnemy,
        float rawDamage,
        DamagePopupStyle style = DamagePopupStyle.Normal,
        bool isCriticalHit = false)
    {
        if (!targetEnemy.Data.IsAlive || targetEnemy.Path.HasReachedGoal || rawDamage <= 0f)
        {
            return;
        }

        var modifiedDamage = targetEnemy.StatusEffects.ApplyIncomingDamageMultiplier(rawDamage);
        _damagePopupSystem.Spawn(gameState, targetEnemy, modifiedDamage, style);
        targetEnemy.Data.TakeDamage(modifiedDamage, isCriticalHit);
    }

    public bool TryApplyExternalRuneAttackKill(
        GameState gameState,
        EnemyEntity targetEnemy,
        RuneType sourceRuneType,
        int sourceRuneTier)
    {
        if (!targetEnemy.Data.IsAlive || targetEnemy.Path.HasReachedGoal)
        {
            return false;
        }

        if (!LaguzTuning.CanRuneTriggerExecute(sourceRuneType))
        {
            return false;
        }

        var executeChance = 0f;
        for (var i = 0; i < gameState.LaguzBlackHoles.Count; i++)
        {
            var blackHole = gameState.LaguzBlackHoles[i];
            var influenceRadius = blackHole.Radius + targetEnemy.Data.Radius;
            if (System.Numerics.Vector2.DistanceSquared(targetEnemy.Transform.Position, blackHole.Position) > influenceRadius * influenceRadius)
            {
                continue;
            }

            executeChance = Math.Max(executeChance, LaguzTuning.GetExecuteChance(blackHole.SourceRuneTier));
        }

        if (executeChance <= 0f || Random.Shared.NextSingle() > executeChance)
        {
            return false;
        }

        _effectAnimationSystem.TrySpawnLaguzExecuteAnimation(gameState, targetEnemy.Transform.Position);
        targetEnemy.Data.MarkDead();
        return true;
    }
}
