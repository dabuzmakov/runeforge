using System.Diagnostics;

namespace runeforge.Controllers;

public sealed class GameLoop : IDisposable
{
    private const float FixedDeltaSeconds = 1f / 144f;
    private const double MaxElapsedSeconds = 0.12;

    private readonly System.Windows.Forms.Timer _timer;
    private readonly Stopwatch _stopwatch;
    private readonly Action<float> _update;
    private readonly Action _render;

    private TimeSpan _lastTime;
    private double _accumulator;

    public GameLoop(Action<float> update, Action render)
    {
        _update = update;
        _render = render;

        _stopwatch = Stopwatch.StartNew();

        _timer = new System.Windows.Forms.Timer
        {
            Interval = 1
        };

        _timer.Tick += OnTick;
    }

    public void Start()
    {
        _lastTime = _stopwatch.Elapsed;
        _timer.Start();
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= OnTick;
        _timer.Dispose();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        var currentTime = _stopwatch.Elapsed;
        var elapsedTime = currentTime - _lastTime;
        _lastTime = currentTime;

        _accumulator += Math.Min(elapsedTime.TotalSeconds, MaxElapsedSeconds);

        while (_accumulator >= FixedDeltaSeconds)
        {
            _update(FixedDeltaSeconds);
            _accumulator = 0;
        }

        _render();
    }
}