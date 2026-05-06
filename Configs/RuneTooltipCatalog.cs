using System.Globalization;
using runeforge.Models;

namespace runeforge.Configs;

public enum RuneTooltipTone
{
    Normal,
    Damage,
    Percent,
    Cooldown,
    Buff,
    Debuff
}

public readonly record struct RuneTooltipSegment(string Text, RuneTooltipTone Tone = RuneTooltipTone.Normal);

public sealed class RuneTooltipInfo
{
    public RuneTooltipInfo(string baseAttackText, string baseAttackSpeedText, params RuneTooltipSegment[] effectSegments)
    {
        BaseAttackText = baseAttackText;
        BaseAttackSpeedText = baseAttackSpeedText;
        EffectSegments = effectSegments;
    }

    public string BaseAttackText { get; }

    public string BaseAttackSpeedText { get; }

    public IReadOnlyList<RuneTooltipSegment> EffectSegments { get; }
}

public static class RuneTooltipCatalog
{
    private static readonly CultureInfo RuCulture = CultureInfo.GetCultureInfo("ru-RU");

    public static RuneTooltipInfo Get(RuneType runeType)
    {
        return runeType switch
        {
            RuneType.Algiz => new RuneTooltipInfo(
                FormatDamage(GetTierOneBaseAttackDamage(runeType)),
                "1 удар раз в 1,22 с",
                N("Работает только на внешних клетках поля. Сметает участок пути длиной"),
                Cd($"{FormatNumber(AlgizTuning.AttackPathLength, 0)} ед."),
                N("и по очереди наносит всем врагам на нём"),
                Dmg($"{FormatDamage(GetTierOneBaseAttackDamage(runeType))} урона"),
                N(".")),
            RuneType.Ansuz => new RuneTooltipInfo(
                FormatDamage(GetTierOneBaseAttackDamage(runeType)),
                FormatShotsPerSecond(RuneDatabase.Get(runeType).BaseAttackRate),
                N("Если руна добивает врага, она может призвать из него союзника. Шанс:"),
                Buff(FormatTierList(GetSpawnChancePercent, "%", 0)),
                N(". Союзник получает"),
                Buff("35% здоровья"),
                N(","),
                Buff("75% скорости"),
                N("и"),
                Buff("85% размера"),
                N("от исходного врага.")),
            RuneType.Berkano => new RuneTooltipInfo(
                FormatDamage(GetTierOneBaseAttackDamage(runeType)),
                FormatShotsPerSecond(RuneDatabase.Get(runeType).BaseAttackRate),
                N("Попадание может отравить цель. Шанс:"),
                Debuff(FormatTierList(tier => BerkanoTuning.GetPoisonChance(tier) * 100f, "%", 0)),
                N(". Яд наносит"),
                Dmg(FormatTierList(BerkanoTuning.GetPoisonDamagePerTick, " ед.", 1)),
                N("каждые"),
                Cd("0,25 с"),
                N(", длится"),
                Cd(FormatTierList(BerkanoTuning.GetPoisonDurationSeconds, " с", 1)),
                N("и задевает врагов в радиусе"),
                Debuff(FormatTierList(BerkanoTuning.GetPoisonRadius, " ед.", 0)),
                N(".")),
            RuneType.Dagaz => new RuneTooltipInfo(
                "нет",
                "пассивная руна",
                N("Даёт соседним рунам мультивыстрел. Шанс срабатывания:"),
                Buff(FormatTierList(DagazTuning.GetMultiShotChancePercent, "%", 0)),
                N(". Дополнительных снарядов:"),
                Buff(FormatTierList(tier => tier, string.Empty, 0)),
                N(". Урон каждого дополнительного снаряда:"),
                Dmg(FormatTierList(tier => DagazTuning.GetAdditionalProjectileDamageMultiplier(tier) * 100f, "%", 0)),
                N("от основного выстрела.")),
            RuneType.Ehwaz => new RuneTooltipInfo(
                FormatDamage(GetTierOneBaseAttackDamage(runeType)),
                FormatShotsPerSecond(RuneDatabase.Get(runeType).BaseAttackRate),
                N("После попадания цепная молния перескакивает ещё на"),
                Buff($"{EhwazTuning.ChainTargetCount} цели"),
                N(". Каждый дополнительный удар наносит"),
                Dmg($"{FormatPercent(EhwazTuning.ChainDamageMultiplier * 100f, 0)} урона"),
                N("от силы исходного попадания.")),
            RuneType.Eiwaz => new RuneTooltipInfo(
                "94 ед.",
                "наведение 3,35 с",
                N("Наводится на врага с самым большим запасом здоровья, затем наносит"),
                Dmg("94 ед. урона"),
                N("и дополнительно"),
                Pct(FormatTierList(tier => EiwazTuning.GetBonusMaxHealthDamagePercent(tier) * 100f, "% от макс. HP", 2)),
                N(".")),
            RuneType.Fehu => new RuneTooltipInfo(
                FormatDamage(GetTierOneBaseAttackDamage(runeType)),
                FormatShotsPerSecond(RuneDatabase.Get(runeType).BaseAttackRate),
                N("Помечает цель. Если помеченный враг погибает, вы получаете дополнительно"),
                Buff(FormatTierList(FehuTuning.GetBonusRunePointPercent, "% энергии", 0)),
                N("от обычной награды за убийство.")),
            RuneType.Gebo => new RuneTooltipInfo(
                "нет",
                "пассивная руна",
                N("Усиливает все соседние руны по стороне. Бонус скорости атаки:"),
                Buff(FormatTierList(GeboTuning.GetAttackSpeedBonusPercent, "%", 0)),
                N(".")),
            RuneType.Hagalaz => new RuneTooltipInfo(
                FormatDamage(GetTierOneBaseAttackDamage(runeType)),
                FormatShotsPerSecond(RuneDatabase.Get(runeType).BaseAttackRate),
                N("Если перетащить руну на путь, она взорвётся в радиусе"),
                Debuff($"{FormatNumber(HagalazTuning.ExplosionRadius, 0)} ед."),
                N("и нанесёт"),
                Dmg(FormatTierList(HagalazTuning.GetExplosionDamage, " ед.", 0)),
                N(". Заряд растёт по 1 делению каждые"),
                Cd("6 с"),
                N(", а итоговый множитель взрыва увеличивается от"),
                Buff("10%"),
                N("до"),
                Buff("100%"),
                N(".")),
            RuneType.Ingwaz => new RuneTooltipInfo(
                "0 ед.",
                FormatShotsPerSecond(IngwazTuning.AttackIntervalSeconds),
                N("Поджигает цель и накапливает до"),
                Buff($"{IngwazTuning.MaxBurnStacks} стаков"),
                N(". Каждый стак раз в"),
                Cd("0,5 с"),
                N("наносит"),
                Dmg(FormatTierList(IngwazTuning.GetBurnBaseDamagePerTick, " ед.", 1)),
                N("плюс"),
                Pct(FormatTierList(tier => IngwazTuning.GetBurnCurrentHealthDamagePercentPerTick(tier) * 100f, "% от текущего HP", 2)),
                N(". Длительность горения:"),
                Cd(FormatTierList(IngwazTuning.GetBurnDurationSeconds, " с", 1)),
                N(".")),
            RuneType.Isa => new RuneTooltipInfo(
                FormatDamage(GetTierOneBaseAttackDamage(runeType)),
                FormatShotsPerSecond(RuneDatabase.Get(runeType).BaseAttackRate),
                N("Все руны Isa вместе пульсируют по дорожке. Интервал срабатывания:"),
                Cd(FormatTierList(tier => IsaTuning.GetValues(tier).TriggerIntervalSeconds, " с", 1)),
                N(". Замедление:"),
                Debuff(FormatTierList(tier => IsaTuning.GetValues(tier).SlowPercent * 100f, "%", 0)),
                N(". Длительность эффекта:"),
                Cd(FormatTierList(tier => IsaTuning.GetValues(tier).SlowDurationSeconds, " с", 2)),
                N(". Максимальный суммарный эффект:"),
                Debuff(FormatTierList(tier => IsaTuning.GetMaxCombinedSlowPercent(tier) * 100f, "%", 0)),
                N(".")),
            RuneType.Jera => new RuneTooltipInfo(
                FormatDamage(GetTierOneBaseAttackDamage(runeType)),
                FormatShotsPerSecond(RuneDatabase.Get(runeType).BaseAttackRate),
                N("Все руны Jera делят общий прогресс. Каждый новый стак даёт"),
                Buff("+3% урона"),
                N("и"),
                Buff("+1,5% скорости атаки"),
                N(". Первый стак требует"),
                Cd("16 убийств"),
                N(", а каждый следующий требует ещё на"),
                Cd("2 убийства"),
                N("больше.")),
            RuneType.Kenaz => new RuneTooltipInfo(
                FormatDamage(GetTierOneBaseAttackDamage(runeType)),
                FormatShotsPerSecond(RuneDatabase.Get(runeType).BaseAttackRate),
                N("После попадания происходит небольшой взрыв. Враги рядом получают"),
                Dmg($"{FormatPercent(KenazTuning.SplashDamageMultiplier * 100f, 0)} урона"),
                N("от основного попадания в радиусе"),
                Debuff($"{FormatNumber(KenazTuning.SplashRadius, 0)} ед."),
                N(".")),
            RuneType.Laguz => new RuneTooltipInfo(
                "нет",
                "1 запуск раз в 4,25 с",
                N("Создаёт от"),
                Buff("1 до 3 чёрных дыр"),
                N(", которые существуют"),
                Cd(FormatTierList(LaguzTuning.GetBlackHoleLifetime, " с", 1)),
                N(". Они замедляют врагов на"),
                Debuff(FormatTierList(tier => LaguzTuning.GetSlowPercent(tier) * 100f, "%", 0)),
                N("и дают внешним атакам шанс казни"),
                Pct(FormatTierList(tier => LaguzTuning.GetExecuteChance(tier) * 100f, "%", 2)),
                N(". Радиус чёрной дыры:"),
                Debuff($"{FormatNumber(LaguzTuning.BlackHoleRadius, 0)} ед."),
                N(".")),
            RuneType.Mannaz => new RuneTooltipInfo(
                FormatDamage(GetTierOneBaseAttackDamage(runeType)),
                FormatShotsPerSecond(RuneDatabase.Get(runeType).BaseAttackRate),
                N("Раз в"),
                Cd($"{FormatNumber(MannazTuning.StormCooldownSeconds, 1)} с"),
                N("вызывает грозовой удар по"),
                Buff(FormatTierList(tier => MannazTuning.GetTargetCount(tier), " целям", 0)),
                N(". Урон молнии:"),
                Dmg(FormatTierList(MannazTuning.GetLightningBaseDamage, " ед.", 0)),
                N("плюс"),
                Pct(FormatTierList(tier => MannazTuning.GetLightningCurrentHealthDamagePercent(tier) * 100f, "% от текущего HP", 1)),
                N(".")),
            RuneType.Nauthiz => new RuneTooltipInfo(
                FormatDamage(GetTierOneBaseAttackDamage(runeType)),
                FormatShotsPerSecond(RuneDatabase.Get(runeType).BaseAttackRate),
                N("Попадание накладывает раскол. На цели может быть до"),
                Debuff($"{NauthizTuning.MaxShatterStacks} стаков"),
                N(". На максимуме она получает"),
                Debuff(FormatTierList(tier => NauthizTuning.GetIncomingDamageBonusPercentPerStack(tier) * NauthizTuning.MaxShatterStacks, "% больше урона", 1)),
                N("от всех источников.")),
            RuneType.Othala => new RuneTooltipInfo(
                FormatDamage(GetTierOneBaseAttackDamage(runeType)),
                "1,67 выстр./с",
                N("За каждую соседнюю Othala по стороне получает"),
                Buff(FormatTierList(tier => OthalaTuning.GetDamageBonusPercent(tier, 2), "% урона", 0)),
                N("и"),
                Buff(FormatTierList(tier => OthalaTuning.GetAttackSpeedBonusPercent(tier, 2), "% скорости атаки", 0)),
                N(".")),
            RuneType.Perthro => new RuneTooltipInfo(
                "нет",
                "1 бросок раз в 1,85 с",
                N("Запускает бумеранг, который наносит"),
                Dmg(FormatTierList(PerthroTuning.GetDamage, " ед.", 1)),
                N("урона. Скорость полёта:"),
                Cd(FormatTierList(PerthroTuning.GetSpeed, " ед./с", 0)),
                N(". Если у врага осталось меньше"),
                Pct(FormatTierList(tier => PerthroTuning.GetExecuteHealthPercentThreshold(tier) * 100f, "% HP", 2)),
                N(", цель мгновенно погибает.")),
            RuneType.Raidho => new RuneTooltipInfo(
                FormatDamage(GetTierOneBaseAttackDamage(runeType)),
                "1,61 выстр./с",
                N("Базовый интервал атаки:"),
                Cd(FormatTierList(RaidhoTuning.GetBaseAttackIntervalSeconds, " с", 2)),
                N(". Каждые"),
                Cd("5 с"),
                N("руна входит в перегрузку на"),
                Cd(FormatTierList(RaidhoTuning.GetOverloadDurationSeconds, " с", 1)),
                N("и ускоряет атаки в"),
                Buff(FormatTierList(RaidhoTuning.GetOverloadAttackSpeedMultiplier, " раза", 2)),
                N(".")),
            RuneType.Sowilo => new RuneTooltipInfo(
                FormatDamage(GetTierOneBaseAttackDamage(runeType)),
                FormatShotsPerSecond(RuneDatabase.Get(runeType).BaseAttackRate),
                N("Каждая"),
                Buff($"{SowiloTuning.SpecialAttackFrequency}-я атака"),
                N("выпускает солнечный луч и наносит"),
                Dmg(FormatTierList(SowiloTuning.GetBeamDamage, " ед.", 0)),
                N(".")),
            RuneType.Thurisaz => new RuneTooltipInfo(
                "46 ед.",
                "подготовка 3,2 с",
                N("После полной зарядки выпускает огненный шар:"),
                Dmg("46 ед. урона"),
                N("плюс"),
                Pct(FormatTierList(tier => ThurisazTuning.GetBonusMaxHealthDamagePercent(tier) * 100f, "% от макс. HP", 2)),
                N(".")),
            RuneType.Tiwaz => new RuneTooltipInfo(
                "96 ед.",
                "динамическая",
                N("В режиме зарядки руна сохраняет"),
                Buff(FormatTierList(tier => TiwazTuning.GetChargeFraction(tier) * 100f, "% урона", 0)),
                N(", наносимого соседними рунами. В режиме разрядки наносит весь сохранённый урон в течение"),
                Cd($"{FormatNumber(TiwazTuning.DischargeDurationSeconds, 1)} с"),
                N(".")),
            RuneType.Uruz => new RuneTooltipInfo(
                FormatDamage(GetTierOneBaseAttackDamage(runeType)),
                "1,14 выстр./с",
                N("Попадание помечает врага. Раз в"),
                Cd(FormatTierList(UruzTuning.GetTornadoCooldownSeconds, " с", 0)),
                N("запускает торнадо, которое наносит"),
                Dmg(FormatTierList(UruzTuning.GetTornadoDamage, " ед.", 1)),
                N(". По помеченным целям торнадо дополнительно бьёт на"),
                Pct(FormatTierList(tier => UruzTuning.GetMarkedHealthDamagePercent(tier) * 100f, "% от текущего HP", 1)),
                N(".")),
            RuneType.Wunjo => new RuneTooltipInfo(
                "нет",
                "пассивная руна",
                N("Усиливает соседние руны по стороне. Бонус к шансу критического удара:"),
                Buff(FormatTierList(WunjoTuning.GetCriticalHitBonusPercent, "%", 0)),
                N(".")),
            _ => new RuneTooltipInfo(
                FormatDamage(GetTierOneBaseAttackDamage(runeType)),
                FormatShotsPerSecond(RuneDatabase.Get(runeType).BaseAttackRate),
                N("Описание пока не заполнено."))
        };
    }

    private static RuneTooltipSegment N(string text) => new(text);
    private static RuneTooltipSegment Dmg(string text) => new(text, RuneTooltipTone.Damage);
    private static RuneTooltipSegment Pct(string text) => new(text, RuneTooltipTone.Percent);
    private static RuneTooltipSegment Cd(string text) => new(text, RuneTooltipTone.Cooldown);
    private static RuneTooltipSegment Buff(string text) => new(text, RuneTooltipTone.Buff);
    private static RuneTooltipSegment Debuff(string text) => new(text, RuneTooltipTone.Debuff);

    private static float GetTierOneBaseAttackDamage(RuneType runeType)
    {
        var runeData = RuneDatabase.Get(runeType);
        return runeData.BaseDamage * RuneTierTuning.GetDamageMultiplier(1) * RuneCombatTuning.GlobalDamageMultiplier;
    }

    private static string FormatDamage(float value)
    {
        return $"{FormatNumber(value, value >= 10f ? 0 : 1)} ед.";
    }

    private static string FormatShotsPerSecond(float intervalSeconds)
    {
        if (intervalSeconds <= 0.001f)
        {
            return "нет";
        }

        return $"{FormatNumber(1f / intervalSeconds, 2)} выстр./с";
    }

    private static string FormatTierList(Func<int, float> selector, string suffix, int decimals)
    {
        var values = string.Join(" / ", Enumerable.Range(RuneTierTuning.MinTier, RuneTierTuning.MaxTier)
            .Select(tier => FormatNumber(selector(tier), decimals)));

        return string.IsNullOrWhiteSpace(suffix)
            ? values
            : values + suffix;
    }

    private static float GetSpawnChancePercent(int tier)
    {
        return tier switch
        {
            1 => 4f,
            2 => 6f,
            3 => 8f,
            4 => 10f,
            _ => 13f
        };
    }

    private static string FormatPercent(float value, int decimals)
    {
        return $"{FormatNumber(value, decimals)}%";
    }

    private static string FormatNumber(float value, int decimals)
    {
        return value.ToString(decimals switch
        {
            <= 0 => "0",
            1 => "0.0",
            2 => "0.00",
            _ => "0.###"
        }, RuCulture);
    }
}
