namespace FlappyBird;

/// <summary>An obstacle made of a top and a bottom pipe with a gap between them.</summary>
public class PipePair
{
    public const int Width = 70;

    public float X { get; private set; }
    public int GapY { get; private set; }
    public int GapHeight { get; }
    public bool Passed { get; set; }

    public PipePair(float x, int gapY, int gapHeight)
    {
        X = x;
        GapY = gapY;
        GapHeight = gapHeight;
    }

    public Rectangle TopRect => new((int)X, 0, Width, GapY);

    public Rectangle BottomRect(int groundY) =>
        new((int)X, GapY + GapHeight, Width, groundY - (GapY + GapHeight));

    public bool IsOffScreen => X + Width < 0;

    public void Move(float speed) => X -= speed;

    /// <summary>Puts the pipe back on the right side with a new random gap.</summary>
    public void Reposition(float newX, int newGapY)
    {
        X = newX;
        GapY = newGapY;
        Passed = false;
    }
}
