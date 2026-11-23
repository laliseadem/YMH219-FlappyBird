using System.Drawing.Drawing2D;

namespace FlappyBird;

/// <summary>Draws the bird sprite with GDI+ shapes (body, wing, tail, beak, eye).</summary>
public static class BirdRenderer
{
    public static void Draw(Graphics g, Bird bird, int frame, bool flapping)
    {
        var saved = g.Save();
        g.TranslateTransform(bird.Center.X, bird.Center.Y);
        g.RotateTransform(Math.Clamp(bird.Velocity * 4f, -25f, 70f)); // tilt with speed

        // Tail feathers
        using (var tail = new SolidBrush(Color.FromArgb(190, 85, 40)))
            g.FillPolygon(tail, new PointF[]
            {
                new(-12, -3), new(-26, -9), new(-22, -1), new(-26, 6), new(-12, 3)
            });

        // Head crest
        using (var crest = new SolidBrush(Color.FromArgb(210, 60, 40)))
            g.FillPolygon(crest, new PointF[] { new(0, -12), new(-2, -21), new(4, -14), new(8, -20), new(9, -11) });

        // Body
        var bodyRect = new RectangleF(-17, -13, 34, 26);
        using (var body = new LinearGradientBrush(bodyRect, Color.FromArgb(255, 220, 70),
                   Color.FromArgb(240, 150, 40), LinearGradientMode.Vertical))
            g.FillEllipse(body, bodyRect);
        using (var outline = new Pen(Color.FromArgb(170, 90, 20), 1.5f))
            g.DrawEllipse(outline, bodyRect);

        // Belly
        using (var belly = new SolidBrush(Color.FromArgb(255, 238, 180)))
            g.FillEllipse(belly, -8, 2, 22, 10);

        // Wing (flaps up and down)
        float wingAngle = flapping ? (float)Math.Sin(frame * 0.55) * 35f : 15f;
        var wingState = g.Save();
        g.TranslateTransform(-4, 1);
        g.RotateTransform(wingAngle);
        using (var wing = new SolidBrush(Color.FromArgb(220, 110, 30)))
        using (var wingPen = new Pen(Color.FromArgb(150, 70, 15), 1.2f))
        {
            g.FillEllipse(wing, -13, -4, 20, 9);
            g.DrawEllipse(wingPen, -13, -4, 20, 9);
        }
        g.Restore(wingState);

        // Eye
        g.FillEllipse(Brushes.White, 6, -10, 11, 11);
        g.FillEllipse(Brushes.Black, 11, -8, 5, 5);

        // Beak
        using (var beak = new SolidBrush(Color.FromArgb(245, 105, 30)))
            g.FillPolygon(beak, new PointF[] { new(15, -3), new(27, 1), new(15, 5) });

        g.Restore(saved);
    }
}
