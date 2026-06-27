using System.Collections.Generic;
using SushiDazzler.Core;

namespace sushi_dazzler.Tests;

public class NoteTrackerTests
{
    private const float Bpm = 120f;          // 0.5s per beat
    private const double SecPerBeat = 0.5;

    private static Conductor ConductorAtBeat(float beat)
    {
        var c = new Conductor();
        c.Start(Bpm);
        c.Advance(beat * SecPerBeat);
        return c;
    }

    private static Song SongWith(params Note[] notes) =>
        new Song { BPM = Bpm, Notes = new List<Note>(notes) };

    [Fact]
    public void TryHit_OnActiveTap_Succeeds()
    {
        var song = SongWith(new Note { Beat = 4f, Type = NoteType.Tap, Key = 'A' });
        var tracker = new NoteTracker(song, ConductorAtBeat(4f));
        tracker.Update();

        var result = tracker.TryHit('A');

        Assert.True(result.Success);
        Assert.Equal(0f, result.TimingDifference, 3);
        Assert.Equal(1, tracker.HitCount);
    }

    [Fact]
    public void TryHit_WrongKey_Fails()
    {
        var song = SongWith(new Note { Beat = 4f, Type = NoteType.Tap, Key = 'A' });
        var tracker = new NoteTracker(song, ConductorAtBeat(4f));
        tracker.Update();

        Assert.False(tracker.TryHit('S').Success);
        Assert.Equal(0, tracker.HitCount);
    }

    [Fact]
    public void Note_PastHitWindow_IsMissed()
    {
        var song = SongWith(new Note { Beat = 4f, Type = NoteType.Tap, Key = 'A' });
        var tracker = new NoteTracker(song, ConductorAtBeat(5f)); // beyond the ±0.5 window

        int missed = tracker.Update();

        Assert.Equal(1, missed);
        Assert.Equal(1, tracker.MissCount);
    }

    [Fact]
    public void Hold_StartThenRelease_Succeeds()
    {
        var song = SongWith(new Note { Beat = 2f, Type = NoteType.Hold, Duration = 2f, Key = 'J' });
        var c = ConductorAtBeat(2f);
        var tracker = new NoteTracker(song, c);
        tracker.Update();

        Assert.True(tracker.TryStartHold('J').Success);
        Assert.True(tracker.IsHolding);
        Assert.Equal('J', tracker.CurrentHoldKey);

        c.Advance(2f * SecPerBeat); // advance to beat 4 — the hold's end
        Assert.True(tracker.TryReleaseHold().Success);
        Assert.False(tracker.IsHolding);
        Assert.Equal(1, tracker.HitCount); // the successful release scores the hit
    }

    [Fact]
    public void HeldNote_IsNotMissedWhileHeld()
    {
        var song = SongWith(new Note { Beat = 2f, Type = NoteType.Hold, Duration = 4f, Key = 'J' });
        var c = ConductorAtBeat(2f);
        var tracker = new NoteTracker(song, c);
        tracker.Update();
        tracker.TryStartHold('J');

        c.Advance(2f * SecPerBeat); // beat 4 — well past the start window
        int missed = tracker.Update();

        Assert.Equal(0, missed);
        Assert.True(tracker.IsHolding);
    }
}
