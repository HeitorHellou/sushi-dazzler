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
        if (!IsPlaying)
            return;

        _songPosition += (float)gameTime.ElapsedGameTime.TotalSeconds;
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
