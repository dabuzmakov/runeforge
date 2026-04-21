using System.Numerics;

namespace runeforge.Models;

public sealed class DamagePopupInstance
{
    private const float ScaleInDurationSeconds = 0.08f;
    private const float HoldDurationSeconds = 0.24f;
    private const float ScaleOutDurationSeconds = 0.10f;
    private const float TotalDurationSeconds = ScaleInDurationSeconds + HoldDurationSeconds + ScaleOutDurationSeconds;

    private readonly EnemyEntity _sourceEnemy;

    public DamagePopupInstance(EnemyEntity sourceEnemy, float damage, DamagePopupStyle style)
    {
        _sourceEnemy = sourceEnemy;
        Position = sourceEnemy.Transform.Position;
        SourceRadius = sourceEnemy.Data.Radius;
        Velocity = sourceEnemy.CurrentVelocity;
        Text = ((int)MathF.Round(damage)).ToString();
        Style = style;
    }

    public Vector2 Position { get; private set; }

    public float SourceRadius { get; }

    public string Text { get; }

    public DamagePopupStyle Style { get; }

    public Vector2 Velocity { get; private set; }

    public float ElapsedSeconds { get; private set; }

    public bool IsExpired => ElapsedSeconds >= TotalDurationSeconds;

    public float Scale
    {
        get
        {
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

    public void Update(float deltaTime)
    {
        if (_sourceEnemy.Data.IsAlive && !_sourceEnemy.Path.HasReachedGoal)
        {
            var currentPosition = _sourceEnemy.Transform.Position;
            if (deltaTime > 0f)
            {
                Velocity = (currentPosition - Position) / deltaTime;
            }

            Position = currentPosition;
        }
        else
        {
            Position += Velocity * deltaTime;
        }

        ElapsedSeconds = MathF.Min(TotalDurationSeconds, ElapsedSeconds + deltaTime);
    }
}
