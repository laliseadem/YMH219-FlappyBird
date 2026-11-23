using System.Drawing.Drawing2D;

namespace FlappyBird;

/// <summary>Forest background: sky, sun, clouds, parallax pine layers and ground.</summary>
public class Scenery
{
    private class Tree { public float X; public int W; public int H; }

    private class Layer
    {
        public float Factor;      // parallax speed relative to game speed
        public Color Dark;
        public Color Light;
        public int Period;
        public float Offset;
        public List<Tree> Trees = new();
    }

    private class Cloud { public float X; public float Y; public float Scale; }

    private readonly int _width;
    private readonly List<Layer> _layers = new();
    private readonly List<Cloud> _clouds = new();
    private readonly Random _rnd = new(7);
    private float _groundOffset;

    public Scenery(int width)
    {
        _width = width;

        // far -> near
        _layers.Add(MakeLayer(0.15f, Color.FromArgb(58, 110, 100), Color.FromArgb(80, 135, 118), 70, 150, 230, 60));
        _layers.Add(MakeLayer(0.35f, Color.FromArgb(34, 95, 60), Color.FromArgb(52, 125, 70), 60, 120, 190, 70));
        _layers.Add(MakeLayer(0.70f, Color.FromArgb(20, 70, 40), Color.FromArgb(34, 100, 52), 80, 140, 220, 120));

        for (int i = 0; i < 4; i++)
            _clouds.Add(new Cloud
            {
                X = i * 150 + _rnd.Next(60),
                Y = 30 + _rnd.Next(110),
                Scale = 0.7f + (float)_rnd.NextDouble() * 0.7f
            });
    }

    private Layer MakeLayer(float factor, Color dark, Color light, int w, int hMin, int hMax, int spacing)
    {
        int period = spacing * (int)Math.Ceiling((_width + 160) / (double)spacing);
        var layer = new Layer { Factor = factor, Dark = dark, Light = light, Period = period };
        for (int x = 0; x < period; x += spacing)
        {
            layer.Trees.Add(new Tree
            {
                X = x + _rnd.Next(-spacing / 4, spacing / 4),
                W = w + _rnd.Next(-10, 15),
                H = _rnd.Next(hMin, hMax)
            });
        }
        return layer;
    }

    /// <summary>Advance the scenery. Pass the game speed (0 keeps only the clouds drifting).</summary>
    public void Tick(float speed)
    {
        foreach (var l in _layers)
            l.Offset = (l.Offset + speed * l.Factor) % l.Period;

        _groundOffset = (_groundOffset + speed) % 24;

        foreach (var c in _clouds)
        {
            c.X -= 0.25f + speed * 0.05f;
            if (c.X < -120) c.X = _width + 40 + _rnd.Next(60);
        }
    }

    public void Draw(Graphics g, int groundY, int height)
    {
        // Sky
        var skyRect = new Rectangle(0, 0, _width, groundY);
        using (var sky = new LinearGradientBrush(skyRect, Color.FromArgb(110, 190, 235),
                   Color.FromArgb(225, 245, 215), LinearGradientMode.Vertical))
            g.FillRectangle(sky, skyRect);

        // Sun
        using (var glow = new SolidBrush(Color.FromArgb(70, 255, 245, 170)))
            g.FillEllipse(glow, _width - 150, 20, 110, 110);
        using (var sun = new SolidBrush(Color.FromArgb(255, 250, 200)))
            g.FillEllipse(sun, _width - 130, 40, 70, 70);

        // Clouds
        using (var cloud = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
            foreach (var c in _clouds)
            {
                float s = c.Scale;
                g.FillEllipse(cloud, c.X, c.Y, 60 * s, 24 * s);
                g.FillEllipse(cloud, c.X + 16 * s, c.Y - 10 * s, 36 * s, 26 * s);
                g.FillEllipse(cloud, c.X + 34 * s, c.Y + 2 * s, 46 * s, 20 * s);
            }

        // Pine tree layers (parallax)
        foreach (var layer in _layers)
        {
            foreach (var t in layer.Trees)
            {
                float x = ((t.X - layer.Offset) % layer.Period + layer.Period) % layer.Period - 80;
                if (x < -t.W || x > _width + t.W) continue;
                DrawPine(g, x, groundY, t.W, t.H, layer.Dark, layer.Light);
            }
        }

        // Ground
        using (var dirt = new SolidBrush(Color.FromArgb(110, 75, 40)))
            g.FillRectangle(dirt, 0, groundY, _width, height - groundY);
        using (var grass = new SolidBrush(Color.FromArgb(70, 160, 60)))
            g.FillRectangle(grass, 0, groundY, _width, 16);
        using (var tuft = new Pen(Color.FromArgb(40, 120, 40), 2))
            for (float x = -_groundOffset; x < _width; x += 24)
                g.DrawLine(tuft, x, groundY + 16, x + 8, groundY + 4);
    }

    private static void DrawPine(Graphics g, float cx, int groundY, int w, int h, Color dark, Color light)
    {
        float trunkH = h * 0.16f, trunkW = w * 0.14f;
        using var trunk = new SolidBrush(Color.FromArgb(80, 55, 30));
        g.FillRectangle(trunk, cx - trunkW / 2, groundY - trunkH, trunkW, trunkH);

        float baseY = groundY - trunkH + 4;
        float tierH = (h - trunkH) / 2.2f;
        using var darkBrush = new SolidBrush(dark);
        using var lightBrush = new SolidBrush(light);

        for (int i = 0; i < 3; i++)
        {
            float yBottom = baseY - i * tierH * 0.7f;
            float yTop = yBottom - tierH;
            float tw = w * (1 - i * 0.26f);
            var pts = new[]
            {
                new PointF(cx - tw / 2, yBottom),
                new PointF(cx + tw / 2, yBottom),
                new PointF(cx, yTop)
            };
            g.FillPolygon(i == 2 ? lightBrush : darkBrush, pts);
        }
    }
}
