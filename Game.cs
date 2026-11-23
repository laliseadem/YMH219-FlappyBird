namespace FlappyBird;

/// <summary>Game logic: state, score, speed, pipes and collisions. No drawing here.</summary>
public class Game
{
    private const int PipeCount = 3;
    private const int PipeSpacing = 220;
    private const int GapHeight = 150;
    private const int GroundHeight = 80;
    private const float BaseSpeed = 3f;
    private const float MaxSpeed = 8f;
    private const int PointsPerLevel = 5;

    private readonly Random _random = new();
    private readonly string _highScorePath =
        Path.Combine(AppContext.BaseDirectory, "highscore.txt");

    public int Width { get; }
    public int Height { get; }
    public int GroundY => Height - GroundHeight;

    public Bird Bird { get; private set; }
    public List<PipePair> Pipes { get; } = new();
    public event Action? Flapped;
    public event Action? Scored;
    public event Action? Died;

    public GameState State { get; private set; } = GameState.Ready;
    public int Score { get; private set; }
    public int HighScore { get; private set; }
    public int Level => Score / PointsPerLevel + 1;
    public float Speed => Math.Min(BaseSpeed + (Level - 1) * 0.5f, MaxSpeed);

    public Game(int width, int height)
    {
        Width = width;
        Height = height;
        Bird = new Bird(100, height / 2f);
        HighScore = LoadHighScore();
        Reset();
    }

    public void Reset()
    {
        Score = 0;
        State = GameState.Ready;
        Bird.Reset(Height / 2f);

        Pipes.Clear();
        for (int i = 0; i < PipeCount; i++)
            Pipes.Add(new PipePair(Width + i * PipeSpacing, RandomGapY(), GapHeight));
    }

    public void Flap()
    {
        if (State == GameState.Ready)
            State = GameState.Running;

        if (State == GameState.Running)
        {
            Bird.Flap();
            Flapped?.Invoke();
        }
    }

    public void Update()
    {
        if (State != GameState.Running) return;

        Bird.Update();

        foreach (var pipe in Pipes)
        {
            pipe.Move(Speed);

            if (!pipe.Passed && pipe.X + PipePair.Width < Bird.X)
            {
                pipe.Passed = true;
                Score++;
                Scored?.Invoke();
            }
        }

        // Recycle pipes that left the screen
        foreach (var pipe in Pipes.Where(p => p.IsOffScreen).ToList())
        {
            float rightmost = Pipes.Max(p => p.X);
            pipe.Reposition(rightmost + PipeSpacing, RandomGapY());
        }

        if (HasCollision())
            EndGame();
    }

    private bool HasCollision()
    {
        var birdRect = Bird.Bounds;

        if (birdRect.Bottom >= GroundY || birdRect.Top <= 0)
            return true;

        return Pipes.Any(p =>
            birdRect.IntersectsWith(p.TopRect) ||
            birdRect.IntersectsWith(p.BottomRect(GroundY)));
    }

    private void EndGame()
    {
        State = GameState.GameOver;
        Died?.Invoke();
        if (Score > HighScore)
        {
            HighScore = Score;
            SaveHighScore();
        }
    }

    private int RandomGapY() => _random.Next(60, GroundY - GapHeight - 60);

    private int LoadHighScore()
    {
        try
        {
            return File.Exists(_highScorePath) &&
                   int.TryParse(File.ReadAllText(_highScorePath), out int v) ? v : 0;
        }
        catch (IOException) { return 0; }
    }

    private void SaveHighScore()
    {
        try { File.WriteAllText(_highScorePath, HighScore.ToString()); }
        catch (IOException) { /* ignore: high score is optional */ }
    }
}
