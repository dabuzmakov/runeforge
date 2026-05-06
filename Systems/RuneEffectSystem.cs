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
        float pathLength,
        ProjectileEntity projectile,
        EffectAnimationSystem effectAnimationSystem)
    {
        var targetEnemy = projectile.Flight.HitTarget;
        if (targetEnemy == null)
        {
            return;
        }

        if (TryIgnoreIncomingAttackOrEffect(targetEnemy))
        {
            return;
        }

        if (projectile.Impact.SourceRuneType == RuneType.Ingwaz)
        {
            RuneBehaviorRegistry.Get(projectile.Impact.SourceRuneType).OnProjectileHit(
                new RuneHitContext(gameState, projectile, targetEnemy, path, pathLength, this, effectAnimationSystem));
            return;
        }

        if (!TryApplyExternalRuneAttackKill(
                gameState,
                targetEnemy,
                projectile.Impact.SourceRuneType,
                projectile.Impact.SourceRuneTier,
                checkIgnore: false))
        {
            ApplyDamage(
                gameState,
                targetEnemy,
                projectile.Impact.Damage,
                projectile.Impact.IsCriticalHit ? DamagePopupStyle.Critical : DamagePopupStyle.Normal,
                projectile.Impact.IsCriticalHit,
                projectile.Impact.SourceRuneType,
                projectile.OwnerRune,
                checkIgnore: false);
        }

        RuneBehaviorRegistry.Get(projectile.Impact.SourceRuneType).OnProjectileHit(
            new RuneHitContext(gameState, projectile, targetEnemy, path, pathLength, this, effectAnimationSystem));
    }

    public void ApplyDirectDamage(
        GameState gameState,
        EnemyEntity targetEnemy,
        float damage,
        bool isCriticalHit = false,
        RuneType? sourceRuneType = null,
        int sourceRuneTier = 1,
        RuneEntity? sourceRune = null,
        bool checkIgnore = true)
    {
        if (!targetEnemy.Data.IsAlive || targetEnemy.Path.HasReachedGoal || damage <= 0f)
        {
            return;
        }

        if (checkIgnore && TryIgnoreIncomingAttackOrEffect(targetEnemy))
        {
            return;
        }

        if (sourceRuneType.HasValue &&
            TryApplyExternalRuneAttackKill(gameState, targetEnemy, sourceRuneType.Value, sourceRuneTier, checkIgnore: false))
        {
            return;
        }

        ApplyDamage(
            gameState,
            targetEnemy,
            damage,
            isCriticalHit ? DamagePopupStyle.Critical : DamagePopupStyle.Normal,
            isCriticalHit,
            sourceRuneType,
            sourceRune,
            checkIgnore: false);
    }

    public void ApplyIsaLaneSlow(GameState gameState, float slowPercent, float durationSeconds)
    {
        if (slowPercent <= 0f || durationSeconds <= 0f)
        {
            return;
        }

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
        float pathLength,
        EnemyEntity sourceEnemy,
        int runeTier)
    {
        _ansuzAllySystem.TrySpawnFromKilledEnemy(
            gameState,
            path,
            pathLength,
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
            projectile.OwnerRune,
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
        RuneEntity? sourceRune = null,
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

            if (TryIgnoreIncomingAttackOrEffect(enemy))
            {
                continue;
            }

            if (sourceRuneType.HasValue &&
                TryApplyExternalRuneAttackKill(gameState, enemy, sourceRuneType.Value, sourceRuneTier, checkIgnore: false))
            {
                continue;
            }

            ApplyDamage(gameState, enemy, damage, sourceRuneType: sourceRuneType, sourceRune: sourceRune, checkIgnore: false);
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

        var epicenterEnemy = EnemyQuery.SelectRandomTargetableEnemy(gameState.Enemies);
        if (epicenterEnemy == null)
        {
            return;
        }

        var radius = BerkanoTuning.GetPoisonRadius(runeTier);
        var durationSeconds = BerkanoTuning.GetPoisonDurationSeconds(runeTier);
        var damagePerTick = BerkanoTuning.GetPoisonDamagePerTick(runeTier) * projectile.Impact.EffectDamageMultiplier;
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

            if (TryIgnoreIncomingAttackOrEffect(enemy))
            {
                continue;
            }

            enemy.StatusEffects.ApplyPoison(
                damagePerTick,
                durationSeconds,
                BerkanoTuning.PoisonTickIntervalSeconds,
                projectile.OwnerRune);
            appliedToAnyEnemy = true;
        }

        if (appliedToAnyEnemy)
        {
            effectAnimationSystem.TrySpawnBerkanoPoisonAnimation(gameState, center);
        }
    }

    public void ApplyIngwazBurn(EnemyEntity targetEnemy, int runeTier, RuneEntity? sourceRune = null)
    {
        if (!targetEnemy.Data.IsAlive || targetEnemy.Path.HasReachedGoal)
        {
            return;
        }

        if (TryIgnoreIncomingAttackOrEffect(targetEnemy))
        {
            return;
        }

        targetEnemy.StatusEffects.ApplyBurn(
            IngwazTuning.GetBurnCurrentHealthDamagePercentPerTick(runeTier),
            IngwazTuning.GetBurnBaseDamagePerTick(runeTier),
            IngwazTuning.GetBurnDurationSeconds(runeTier),
            sourceRune);
    }

    public void ApplyNauthizShatter(EnemyEntity targetEnemy, int runeTier)
    {
        if (!targetEnemy.Data.IsAlive || targetEnemy.Path.HasReachedGoal)
        {
            return;
        }

        if (TryIgnoreIncomingAttackOrEffect(targetEnemy))
        {
            return;
        }

        targetEnemy.StatusEffects.ApplyOrUpgradeShatter(
            NauthizTuning.GetIncomingDamageBonusPercentPerStack(runeTier));
    }

    public void ApplyPerthroBoomerangHit(
        GameState gameState,
        EnemyEntity targetEnemy,
        float damage,
        int runeTier)
    {
        if (!targetEnemy.Data.IsAlive || targetEnemy.Path.HasReachedGoal)
        {
            return;
        }

        if (TryIgnoreIncomingAttackOrEffect(targetEnemy))
        {
            return;
        }

        var wasMarked = targetEnemy.StatusEffects.IsPerthroMarked;
        var healthThreshold = targetEnemy.Data.MaxHealth * PerthroTuning.GetExecuteHealthPercentThreshold(runeTier);
        if (wasMarked && targetEnemy.Data.Health <= healthThreshold)
        {
            _effectAnimationSystem.TrySpawnLaguzExecuteAnimation(gameState, targetEnemy.Transform.Position);
            targetEnemy.StatusEffects.ClearPerthroMark();
            targetEnemy.Data.MarkDead();
            return;
        }

        ApplyDamage(gameState, targetEnemy, damage);
        if (targetEnemy.Data.IsAlive)
        {
            targetEnemy.StatusEffects.ApplyPerthroMark();
        }
    }

    public void ApplyDamage(
        GameState gameState,
        EnemyEntity targetEnemy,
        float rawDamage,
        DamagePopupStyle style = DamagePopupStyle.Normal,
        bool isCriticalHit = false,
        RuneType? sourceRuneType = null,
        RuneEntity? sourceRune = null,
        bool checkIgnore = true)
    {
        if (!targetEnemy.Data.IsAlive || targetEnemy.Path.HasReachedGoal || rawDamage <= 0f)
        {
            return;
        }

        if (checkIgnore && TryIgnoreIncomingAttackOrEffect(targetEnemy))
        {
            return;
        }

        var modifiedDamage = targetEnemy.StatusEffects.ApplyIncomingDamageMultiplier(rawDamage);
        _damagePopupSystem.Spawn(gameState, targetEnemy, modifiedDamage, style);
        targetEnemy.Data.TakeDamage(modifiedDamage, isCriticalHit);
        TryChargeAdjacentTiwazRunes(gameState, sourceRune, modifiedDamage);

        if (!targetEnemy.Data.IsAlive && !targetEnemy.Path.HasReachedGoal && sourceRuneType == RuneType.Jera)
        {
            RegisterJeraKill(gameState);
        }
    }

    public void RegisterRuneDamageDealt(GameState gameState, RuneEntity? sourceRune, float dealtDamage)
    {
        TryChargeAdjacentTiwazRunes(gameState, sourceRune, dealtDamage);
    }

    public bool TryApplyExternalRuneAttackKill(
        GameState gameState,
        EnemyEntity targetEnemy,
        RuneType sourceRuneType,
        int sourceRuneTier,
        bool checkIgnore = true)
    {
        if (!targetEnemy.Data.IsAlive || targetEnemy.Path.HasReachedGoal)
        {
            return false;
        }

        if (checkIgnore && TryIgnoreIncomingAttackOrEffect(targetEnemy))
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

        if (sourceRuneType == RuneType.Jera)
        {
            RegisterJeraKill(gameState);
        }

        return true;
    }

    public bool TryIgnoreIncomingAttackOrEffect(EnemyEntity targetEnemy)
    {
        return targetEnemy.StatusEffects.TryIgnoreIncomingAttackOrEffect();
    }

    private void RegisterJeraKill(GameState gameState)
    {
        if (!gameState.Jera.RegisterKill())
        {
            return;
        }

        for (var i = 0; i < gameState.Runes.Count; i++)
        {
            var rune = gameState.Runes[i];
            if (rune.Stats.Type != RuneType.Jera)
            {
                continue;
            }

            rune.State.SetJeraSharedStacks(gameState.Jera.SharedStacks);
            rune.Presentation.TriggerMergePop();
            _effectAnimationSystem.TrySpawnJeraUpgradeAnimation(gameState, rune.Transform.Position);
        }
    }

    private static void TryChargeAdjacentTiwazRunes(GameState gameState, RuneEntity? sourceRune, float dealtDamage)
    {
        if (sourceRune == null || dealtDamage <= 0.001f || !gameState.Tiwaz.IsCharging || sourceRune.Stats.Type == RuneType.Tiwaz)
        {
            return;
        }

        for (var i = 0; i < gameState.Runes.Count; i++)
        {
            var rune = gameState.Runes[i];
            if (rune.Stats.Type != RuneType.Tiwaz)
            {
                continue;
            }

            var rowDistance = Math.Abs(rune.Grid.Row - sourceRune.Grid.Row);
            var columnDistance = Math.Abs(rune.Grid.Column - sourceRune.Grid.Column);
            if ((rowDistance + columnDistance) != 1)
            {
                continue;
            }

            rune.State.AddTiwazStoredDamage(dealtDamage * TiwazTuning.GetChargeFraction(rune.Stats.Tier));
        }
    }
}
