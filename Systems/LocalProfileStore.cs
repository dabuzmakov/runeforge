using System.Text.Json;

namespace runeforge.Systems;

public readonly record struct LocalProfileSnapshot(
    int BestCompletedWaveRecord,
    long TotalKilledEnemyCount,
    double TotalPlayTimeSeconds);

public sealed class LocalProfileStore
{
    private readonly string _profilePath;

    public LocalProfileStore()
    {
        _profilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "runeforge",
            "profile.json");
    }

    public LocalProfileSnapshot LoadProfile()
    {
        try
        {
            if (!File.Exists(_profilePath))
            {
                return default;
            }

            var json = File.ReadAllText(_profilePath);
            var profile = JsonSerializer.Deserialize<LocalProfileData>(json);
            return new LocalProfileSnapshot(
                Math.Max(0, profile?.BestCompletedWaveRecord ?? 0),
                Math.Max(0, profile?.TotalKilledEnemyCount ?? 0),
                Math.Max(0d, profile?.TotalPlayTimeSeconds ?? 0d));
        }
        catch
        {
            return default;
        }
    }

    public void SaveProfile(LocalProfileSnapshot profile)
    {
        try
        {
            var directoryPath = Path.GetDirectoryName(_profilePath);
            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var json = JsonSerializer.Serialize(new LocalProfileData
            {
                BestCompletedWaveRecord = profile.BestCompletedWaveRecord,
                TotalKilledEnemyCount = Math.Max(0, profile.TotalKilledEnemyCount),
                TotalPlayTimeSeconds = Math.Max(0d, profile.TotalPlayTimeSeconds)
            });

            File.WriteAllText(_profilePath, json);
        }
        catch
        {
        }
    }

    private sealed class LocalProfileData
    {
        public int? BestCompletedWaveRecord { get; init; }

        public long TotalKilledEnemyCount { get; init; }

        public double TotalPlayTimeSeconds { get; init; }
    }
}
