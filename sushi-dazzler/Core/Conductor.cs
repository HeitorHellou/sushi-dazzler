using Microsoft.Xna.Framework;

namespace SushiDazzler.Core;

public class Conductor
{
    private float _songPosition;
    private float _offset;

    public float BPM { get; private set; }
    public float Crotchet => 60f / BPM;
    public float SongPosition => _songPosition;
    public float CurrentBeat => _songPosition / Crotchet;
    public bool IsPlaying { get; private set; }

    public void Start(float bpm, float offset = 0f)
    {
        BPM = bpm;
        _offset = offset;
        _songPosition = -offset;
        IsPlaying = true;
    }

    public void Update(GameTime gameTime)
    {
        Advance(gameTime.ElapsedGameTime.TotalSeconds);
    }

    /// <summary>
    /// Advances the song clock by a raw number of seconds. Frame-loop code uses
    /// <see cref="Update"/>; tests drive this directly without a GameTime.
    /// </summary>
    public void Advance(double seconds)
    {
        if (!IsPlaying)
            return;

        _songPosition += (float)seconds;
    }

    public void Stop()
    {
        IsPlaying = false;
        _songPosition = 0f;
    }

    public void Pause()
    {
        IsPlaying = false;
    }

    public void Resume()
    {
        IsPlaying = true;
    }
}
