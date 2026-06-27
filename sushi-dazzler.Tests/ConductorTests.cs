using SushiDazzler.Core;

namespace sushi_dazzler.Tests;

public class ConductorTests
{
    [Fact]
    public void Crotchet_IsSecondsPerBeat()
    {
        var c = new Conductor();
        c.Start(120f);
        Assert.Equal(0.5f, c.Crotchet, 5);
    }

    [Fact]
    public void Advance_ConvertsSecondsToBeats()
    {
        var c = new Conductor();
        c.Start(120f);   // 0.5s per beat
        c.Advance(1.0);  // 1 second => 2 beats
        Assert.Equal(2f, c.CurrentBeat, 5);
    }

    [Fact]
    public void Offset_DelaysBeatZero()
    {
        var c = new Conductor();
        c.Start(120f, offset: 0.5f); // clock starts half a second before beat 0
        Assert.Equal(-1f, c.CurrentBeat, 5); // -0.5s / 0.5 = -1 beat
        c.Advance(0.5);
        Assert.Equal(0f, c.CurrentBeat, 5);
    }

    [Fact]
    public void Pause_FreezesClock_Resume_Continues()
    {
        var c = new Conductor();
        c.Start(120f);
        c.Advance(0.5);   // beat 1
        c.Pause();
        c.Advance(10.0);  // ignored while paused
        Assert.Equal(1f, c.CurrentBeat, 5);
        c.Resume();
        c.Advance(0.5);   // beat 2
        Assert.Equal(2f, c.CurrentBeat, 5);
    }

    [Fact]
    public void Stop_ResetsPositionAndState()
    {
        var c = new Conductor();
        c.Start(120f);
        c.Advance(1.0);
        c.Stop();
        Assert.False(c.IsPlaying);
        Assert.Equal(0f, c.SongPosition, 5);
    }
}
