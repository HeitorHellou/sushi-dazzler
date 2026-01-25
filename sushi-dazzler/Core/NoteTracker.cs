using System.Collections.Generic;
using System.Linq;

namespace SushiDazzler.Core;

public class NoteTracker
{
    private readonly List<Note> _notes;
    private readonly HashSet<Note> _activeNotes = new();
    private readonly Conductor _conductor;

    private int _nextNoteIndex;

    public float HitWindow { get; set; } = 0.2f; // ±0.2 beats
    public int HitCount { get; private set; }
    public int MissCount { get; private set; }
    public IReadOnlyCollection<Note> ActiveNotes => _activeNotes;

    public NoteTracker(Song song, Conductor conductor)
    {
        _notes = song.Notes.OrderBy(n => n.Beat).ToList();
        _conductor = conductor;
        _nextNoteIndex = 0;
    }

    public void Update()
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
        var missedNotes = _activeNotes.Where(n => n.Beat < windowStart).ToList();
        foreach (var note in missedNotes)
        {
            _activeNotes.Remove(note);
            MissCount++;
        }
    }

    public bool TryHit(NoteType type)
    {
        var note = _activeNotes.FirstOrDefault(n => n.Type == type);
        if (note == null)
            return false;

        _activeNotes.Remove(note);
        HitCount++;
        return true;
    }

    public void Reset()
    {
        _nextNoteIndex = 0;
        _activeNotes.Clear();
        HitCount = 0;
        MissCount = 0;
    }
}
