namespace runeforge.Models;

public sealed class EnemyDamagePopupComponent
{
    private const float ScaleInDurationSeconds = 0.08f;
    private const float HoldDurationSeconds = 0.24f;
    private const float ScaleOutDurationSeconds = 0.10f;
    private const float TotalDurationSeconds = ScaleInDurationSeconds + HoldDurationSeconds + ScaleOutDurationSeconds;

    public string Text { get; private set; } = string.Empty;

    public bool IsCriticalHit { get; private set; }

    public bool IsVisible => ElapsedSeconds < TotalDurationSeconds && !string.IsNullOrEmpty(Text);

    public float ElapsedSeconds { get; private set; } = TotalDurationSeconds;

    public float Scale
    {
        get
        {
            if (!IsVisible)
            {
                return 0f;
            }

            if (ElapsedSeconds < ScaleInDurationSeconds)
            {
                return ElapsedSeconds / ScaleInDurationSeconds;
            }

            if (ElapsedSeconds < ScaleInDurationSeconds + HoldDurationSeconds)
            {
                return 1f;
            }

            var scaleOutElapsed = ElapsedSeconds - ScaleInDurationSeconds - HoldDurationSeconds;
            return 1f - (scaleOutElapsed / ScaleOutDurationSeconds);
        }
    }

    public void Show(float damage, bool isCriticalHit)
    {
        Text = ((int)MathF.Round(damage)).ToString();
        IsCriticalHit = isCriticalHit;
        ElapsedSeconds = 0f;
    }

    public void Update(float deltaTime)
    {
        if (!IsVisible)
        {
            return;
        }

        ElapsedSeconds = MathF.Min(TotalDurationSeconds, ElapsedSeconds + deltaTime);
    }
}
