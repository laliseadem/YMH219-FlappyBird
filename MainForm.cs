using System.Drawing.Drawing2D;

namespace FlappyBird;

/// <summary>Window, input handling, game loop timer and drawing.</summary>
public class MainForm : Form
{
    private readonly Game _game;
    private readonly Scenery _scenery;
    private readonly SoundManager _sound = new();
    private readonly System.Windows.Forms.Timer _timer;
    private readonly Button _restartButton;

    private readonly Font _hudFont = new("Segoe UI", 14, FontStyle.Bold);
    private readonly Font _smallFont = new("Segoe UI", 10, FontStyle.Bold);
    private readonly Font _bigFont = new("Segoe UI", 30, FontStyle.Bold);

    private int _frame;

    public MainForm()
    {
        Text = "Flappy Bird - YMH219";
        ClientSize = new Size(480, 640);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        DoubleBuffered = true;
        KeyPreview = true;

        _game = new Game(ClientSize.Width, ClientSize.Height);
        _scenery = new Scenery(ClientSize.Width);

        _game.Flapped += _sound.PlayFlap;
        _game.Scored += _sound.PlayScore;
        _game.Died += _sound.PlayHit;

        _restartButton = new Button
        {
            Text = "Yeni Oyun",
            Size = new Size(120, 36),
            Location = new Point((ClientSize.Width - 120) / 2, 380),
            TabStop = false,
            Visible = false
        };
        _restartButton.Click += (_, _) => RestartGame();
        Controls.Add(_restartButton);

        _timer = new System.Windows.Forms.Timer { Interval = 16 }; // ~60 FPS
        _timer.Tick += GameLoop;
        _timer.Start();

        KeyDown += OnKeyDown;
    }

    private void GameLoop(object? sender, EventArgs e)
    {
        _frame++;
        _game.Update();
        _scenery.Tick(_game.State == GameState.Running ? _game.Speed : 0f);
        _restartButton.Visible = _game.State == GameState.GameOver;
        Invalidate();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Space:
            case Keys.Up:
                if (_game.State != GameState.GameOver) _game.Flap();
                e.Handled = true;
                break;
            case Keys.R:
            case Keys.Enter:
                if (_game.State == GameState.GameOver) RestartGame();
                break;
            case Keys.M:
                _sound.Muted = !_sound.Muted;
                break;
        }
    }

    private void RestartGame()
    {
        _game.Reset();
        _restartButton.Visible = false;
        Focus();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        _scenery.Draw(g, _game.GroundY, _game.Height);

        foreach (var pipe in _game.Pipes)
        {
            DrawLog(g, pipe.TopRect, capOnBottom: true);
            DrawLog(g, pipe.BottomRect(_game.GroundY), capOnBottom: false);
        }

        BirdRenderer.Draw(g, _game.Bird, _frame, _game.State != GameState.GameOver);

        // HUD
        DrawText(g, $"Skor: {_game.Score}", _hudFont, 10, 10);
        DrawText(g, $"En Yüksek: {_game.HighScore}", _hudFont, 10, 38);
        DrawText(g, $"Seviye: {_game.Level}   Hız: {_game.Speed:0.0}", _hudFont, 10, 66);
        DrawText(g, _sound.Muted ? "Ses: Kapalı (M)" : "Ses: Açık (M)", _smallFont, 10, 98);

        if (_game.State == GameState.Ready)
        {
            DrawCentered(g, "Başlamak için SPACE", _hudFont, Brushes.White, 280);
        }
        else if (_game.State == GameState.GameOver)
        {
            using var overlay = new SolidBrush(Color.FromArgb(110, 0, 0, 0));
            g.FillRectangle(overlay, 0, 0, _game.Width, _game.Height);
            DrawCentered(g, "GAME OVER", _bigFont, Brushes.Crimson, 250);
            DrawCentered(g, $"Skor: {_game.Score}", _hudFont, Brushes.White, 320);
        }
    }

    /// <summary>Draws a wooden log obstacle with a cap at the end facing the gap.</summary>
    private static void DrawLog(Graphics g, Rectangle r, bool capOnBottom)
    {
        if (r.Height <= 0) return;

        using (var body = new LinearGradientBrush(r, Color.FromArgb(150, 100, 50),
                   Color.FromArgb(95, 60, 30), LinearGradientMode.Horizontal))
            g.FillRectangle(body, r);

        using (var bark = new Pen(Color.FromArgb(70, 60, 35, 15), 2))
            for (int x = r.Left + 14; x < r.Right - 6; x += 16)
                g.DrawLine(bark, x, r.Top, x, r.Bottom);

        const int capHeight = 22, overhang = 5;
        var cap = capOnBottom
            ? new Rectangle(r.Left - overhang, r.Bottom - capHeight, r.Width + 2 * overhang, capHeight)
            : new Rectangle(r.Left - overhang, r.Top, r.Width + 2 * overhang, capHeight);

        using (var capBrush = new LinearGradientBrush(cap, Color.FromArgb(120, 80, 40),
                   Color.FromArgb(75, 48, 24), LinearGradientMode.Horizontal))
            g.FillRectangle(capBrush, cap);
        using (var capPen = new Pen(Color.FromArgb(55, 35, 15), 2))
            g.DrawRectangle(capPen, cap);

        // moss strip on the cap edge
        using var moss = new SolidBrush(Color.FromArgb(80, 150, 60));
        int mossY = capOnBottom ? cap.Bottom - 5 : cap.Top;
        g.FillRectangle(moss, cap.Left + 1, mossY, cap.Width - 1, 5);
    }

    private static void DrawText(Graphics g, string text, Font font, float x, float y)
    {
        g.DrawString(text, font, Brushes.Black, x + 1.5f, y + 1.5f); // shadow
        g.DrawString(text, font, Brushes.White, x, y);
    }

    private void DrawCentered(Graphics g, string text, Font font, Brush brush, float y)
    {
        var size = g.MeasureString(text, font);
        float x = (_game.Width - size.Width) / 2;
        g.DrawString(text, font, Brushes.Black, x + 2, y + 2);
        g.DrawString(text, font, brush, x, y);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Dispose();
            _sound.Dispose();
            _hudFont.Dispose();
            _smallFont.Dispose();
            _bigFont.Dispose();
        }
        base.Dispose(disposing);
    }
}
