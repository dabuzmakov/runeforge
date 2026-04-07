using System.Numerics;

namespace runeforge.Models;

public sealed class TransformComponent
{
    public TransformComponent(Vector2 position)
    {
        Position = position;
    }

    public Vector2 Position { get; set; }
}
