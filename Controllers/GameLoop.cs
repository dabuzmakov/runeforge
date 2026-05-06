using System.Diagnostics;
using System.Runtime.InteropServices;

namespace runeforge.Controllers;

public sealed class GameLoop : IDisposable
{
    private const double MaxElapsedSeconds = 0.12;
    private const double TargetFrameSeconds = 1d / 144d;
    private const int TimerIntervalMilliseconds = 1;
    private const int MaxUpdateStepsPerTick = 8;

    private readonly Stopwatch _stopwatch;
    private readonly Action<float> _update;
    private readonly Action _render;
    private readonly System.Windows.Forms.Timer _timer;

    private TimeSpan _lastTime;
    private double _accumulatedSeconds;
    private bool _isRunning;
    private bool _isTimerResolutionRaised;

    public GameLoop(Action<float> update, Action render)
    {
        _update = update;
        _render = render;

        _stopwatch = Stopwatch.StartNew();
        _isTimerResolutionRaised = TimeBeginPeriod(1) == 0;
        _timer = new System.Windows.Forms.Timer
        {
            Interval = TimerIntervalMilliseconds
        };
        _timer.Tick += OnTimerTick;
    }

    public void Start()
    {
        if (_isRunning)
        {
            return;
        }

        _lastTime = _stopwatch.Elapsed;
        _accumulatedSeconds = 0d;
        _isRunning = true;
        _timer.Start();
    }

    public void Dispose()
    {
        _isRunning = false;
        _timer.Stop();
        _timer.Tick -= OnTimerTick;
        _timer.Dispose();
        if (_isTimerResolutionRaised)
        {
            TimeEndPeriod(1);
            _isTimerResolutionRaised = false;
        }
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (!_isRunning)
        {
            return;
        }

        RunFrame();
    }

    private void RunFrame()
    {
        var currentTime = _stopwatch.Elapsed;
        var elapsedTime = currentTime - _lastTime;
        _lastTime = currentTime;
        _accumulatedSeconds += Math.Min(elapsedTime.TotalSeconds, MaxElapsedSeconds);

        var updateStepCount = 0;
        while (_accumulatedSeconds >= TargetFrameSeconds && updateStepCount < MaxUpdateStepsPerTick)
        {
            _update((float)TargetFrameSeconds);
            _accumulatedSeconds -= TargetFrameSeconds;
            updateStepCount++;
        }

        if (updateStepCount >= MaxUpdateStepsPerTick)
        {
            _accumulatedSeconds = 0d;
        }

        if (updateStepCount > 0)
        {
            _render();
        }
    }

    [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
    private static extern uint TimeBeginPeriod(uint periodMilliseconds);

    [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
    private static extern uint TimeEndPeriod(uint periodMilliseconds);
}
