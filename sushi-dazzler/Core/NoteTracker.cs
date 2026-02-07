using System.Collections.Generic;
using System.Linq;

namespace SushiDazzler.Core;

public struct HitResult
{
    public bool Success;
    public float TimingDifference; // Positive = late, negative = early

    public static HitResult Miss => new() { Success = false, TimingDifference = 0 };
    public static HitResult Hit(float timing) => new() { Success = true, TimingDifference = timing };
}

public class NoteTracker
{
    private readonly List<Note> _notes;
    private readonly HashSet<Note> _activeNotes = new();
    private readonly Conductor _conductor;

    private int _nextNoteIndex;

    // Hold note tracking
    private Note? _currentHold;
    private float _holdStartTimingDiff; // Store timing diff from when hold started

    public float HitWindow { get; set; } = 0.5f; // ±0.5 beats (matches ScoreTracker.GoodWindow)
    public int HitCount { get; private set; }
    public int MissCount { get; private set; }
    public IReadOnlyCollection<Note> ActiveNotes => _activeNotes;

    // Expose hold state for Game1 to query
    public bool IsHolding => _currentHold != null;
    public char? CurrentHoldKey => _currentHold?.Key;

    public NoteTracker(Song song, Conductor conductor)
    {
        _notes = song.Notes.OrderBy(n => n.Beat).ToList();
        _conductor = conductor;
        _nextNoteIndex = 0;
    }

    /// <summary>
    /// Updates the note tracker and returns the number of notes that were missed this frame.
    /// </summary>
    public int Update()
    {
        float currentBeat = _conductor.CurrentBeat;
        float windowStart = currentBeat - HitWindow;
        float windowEnd = currentBeat + HitWindow;

        // Move notes into active window
        while (_nextNoteIndex < _notes.Count)
        {
            var note = _notes[_nextNoteIndex];
            if (note.Beat <= windowEnd)
            {
                _activeNotes.Add(note);
                _nextNoteIndex++;
            }
            else
            {
                break;
            }
        }

        // Check for missed notes (past the window)
        // Note: Don't mark the currently held note as missed
        var missedNotes = _activeNotes.Where(n => n.Beat < windowStart && n != _currentHold).ToList();
        foreach (var note in missedNotes)
        {
            _activeNotes.Remove(note);
            MissCount++;
        }

        return missedNotes.Count;
    }

    public HitResult TryHit(char key)
    {
        var note = _activeNotes.FirstOrDefault(n => n.Type == NoteType.Tap && n.Key == key);
        if (note == null)
            return HitResult.Miss;

        float timingDiff = _conductor.CurrentBeat - note.Beat;
        _activeNotes.Remove(note);
        HitCount++;
        return HitResult.Hit(timingDiff);
    }

    public HitResult TryStartHold(char key)
    {
        if (_currentHold != null)
            return HitResult.Miss; // Already holding something

        var note = _activeNotes.FirstOrDefault(n => n.Type == NoteType.Hold && n.Key == key);
        if (note == null)
            return HitResult.Miss;

        _holdStartTimingDiff = _conductor.CurrentBeat - note.Beat;
        _currentHold = note;
        _activeNotes.Remove(note); // Remove from active so it doesn't get marked as missed
        return HitResult.Hit(_holdStartTimingDiff);
    }

    public HitResult TryReleaseHold()
    {
        if (_currentHold == null)
            return HitResult.Miss;

        float currentBeat = _conductor.CurrentBeat;
        float holdEndBeat = _currentHold.Beat + _currentHold.Duration;
        float timingDiff = currentBeat - holdEndBeat;

        // Check if released within the hit window of the end beat (±HitWindow)
        bool releasedOnTime = System.Math.Abs(timingDiff) <= HitWindow;

        if (releasedOnTime)
        {
            HitCount++;
        }
        else
        {
            // Released too early or too late
            MissCount++;
        }

        _currentHold = null;
        return releasedOnTime ? HitResult.Hit(timingDiff) : HitResult.Miss;
    }

    public void Reset()
    {
        _nextNoteIndex = 0;
        _activeNotes.Clear();
        HitCount = 0;
        MissCount = 0;
        _currentHold = null;
        _holdStartTimingDiff = 0;
    }
}
