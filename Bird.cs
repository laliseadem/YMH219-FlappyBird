namespace FlappyBird;

/// <summary>The player. Holds position and vertical velocity.</summary>
public class Bird
{
    public const int Size = 34;
    private const float Gravity = 0.55f;
    private const float FlapStrength = -8.5f;

    public float X { get; }
    public float Y { get; private set; }
    public float Velocity { get; private set; }

    public Bird(float x, float startY)
    {
        X = x;
        Y = startY;
    }

    /// <summary>Centre point used for drawing the sprite.</summary>
    public PointF Center => new(X + Size / 2f, Y + Size / 2f);

    /// <summary>Collision box, a little smaller than the sprite so hits feel fair.</summary>
    public Rectangle Bounds => new((int)X + 4, (int)Y + 4, Size - 8, Size - 8);

    public void Flap() => Velocity = FlapStrength;

    public void Update()
    {
        Velocity += Gravity;
        Y += Velocity;
    }

    public void Reset(float startY)
    {
        Y = startY;
        Velocity = 0;
    }
}
