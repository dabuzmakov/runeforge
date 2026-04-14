namespace runeforge.Models;

public sealed class WaveState
{
    public int CurrentWaveNumber { get; set; }

    public WaveDefinition? ActiveWave { get; set; }

    public int SpawnedEnemiesInWave { get; set; }

    public float TimeUntilNextSpawn { get; set; }

    public string CurrentWaveSummary => ActiveWave?.BuildSummary() ?? "Wave system idle.";

    public bool HasActiveWave => ActiveWave != null;

    public bool IsWaveSpawnFinished =>
        ActiveWave != null &&
        SpawnedEnemiesInWave >= ActiveWave.TotalEnemyCount;
}
