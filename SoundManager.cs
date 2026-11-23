using System.Media;
using System.Text;

namespace FlappyBird;

/// <summary>Creates and plays small sound effects generated in code (no audio files needed).</summary>
public class SoundManager : IDisposable
{
    private const int SampleRate = 22050;

    private readonly SoundPlayer? _flap;
    private readonly SoundPlayer? _score;
    private readonly SoundPlayer? _hit;

    public bool Muted { get; set; }

    public SoundManager()
    {
        try
        {
            _flap = Create(Tone(350, 650, 90, 0.35));
            _score = Create(Concat(Tone(880, 880, 70, 0.35), Tone(1320, 1320, 110, 0.35)));
            _hit = Create(Tone(300, 60, 350, 0.6, square: true));
        }
        catch (Exception)
        {
            // If audio is unavailable the game still works silently.
        }
    }

    public void PlayFlap() => Play(_flap);
    public void PlayScore() => Play(_score);
    public void PlayHit() => Play(_hit);

    private void Play(SoundPlayer? player)
    {
        if (Muted || player == null) return;
        try { player.Play(); }
        catch (Exception) { /* ignore audio errors */ }
    }

    private static SoundPlayer Create(short[] samples)
    {
        var player = new SoundPlayer(new MemoryStream(ToWav(samples)));
        player.Load();
        return player;
    }

    private static short[] Tone(double f0, double f1, int ms, double volume, bool square = false)
    {
        int n = SampleRate * ms / 1000;
        var data = new short[n];
        double phase = 0;
        for (int i = 0; i < n; i++)
        {
            double t = (double)i / n;
            double freq = f0 + (f1 - f0) * t;
            phase += 2 * Math.PI * freq / SampleRate;
            double wave = square ? Math.Sign(Math.Sin(phase)) * 0.6 : Math.Sin(phase);
            double envelope = Math.Min(1.0, t * 12) * (1 - t);
            data[i] = (short)(wave * envelope * volume * short.MaxValue);
        }
        return data;
    }

    private static short[] Concat(params short[][] parts) => parts.SelectMany(p => p).ToArray();

    private static byte[] ToWav(short[] samples)
    {
        int dataLength = samples.Length * 2;
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);
        w.Write(Encoding.ASCII.GetBytes("RIFF"));
        w.Write(36 + dataLength);
        w.Write(Encoding.ASCII.GetBytes("WAVE"));
        w.Write(Encoding.ASCII.GetBytes("fmt "));
        w.Write(16);
        w.Write((short)1);           // PCM
        w.Write((short)1);           // mono
        w.Write(SampleRate);
        w.Write(SampleRate * 2);     // byte rate
        w.Write((short)2);           // block align
        w.Write((short)16);          // bits per sample
        w.Write(Encoding.ASCII.GetBytes("data"));
        w.Write(dataLength);
        foreach (var s in samples) w.Write(s);
        w.Flush();
        return ms.ToArray();
    }

    public void Dispose()
    {
        _flap?.Dispose();
        _score?.Dispose();
        _hit?.Dispose();
    }
}
