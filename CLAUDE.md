# SUSHI DAZZLER

- PC

A rhythm game developed for desktop, where the player plays as a sushi chef working in different bars in Japan.
Players prepare sushi dishes by synchronizing their actions with the rhythm of the music. The actions performed may have a corresponding animation as feedback.

## Gameplay
- TAP (cut sushi)
- HOLD (shape sushi)

## Structure
Sushi bars spread across Japan
- Each bar has its own theme and music style
- No errors during the song (only decreases the final score)
- 1-5 star feedback
- three difficulties per bar (easy/medium/hard)

## Bars
- Yokohama: City pop (musical reference: Goyeol - dosii)
- Osaka: J-Rock (musical reference: rock 'n' roll wa shinanai with totsuzen shounen - haru nemuri)

## Art
- 2D Pixel
- Camera focused on the "chef" to view the animations

The focus of the game is to make something small and concise in a 6 month timeframe. For this we have chosen the MonoGame framework, and we will be programming using C#. The main target is desktop, but it would be nice to have a mobile version. This project will be handled by 2 programmers, with a potential artist coming in later. The main focus right now is to build the underlying system and gameplay.

## Technical Stack
- Framework: MonoGame.Framework.DesktopGL v3.8.*
- Language: C# / .NET 9.0
- IDE: Visual Studio Code
- Content Pipeline: MGCB (MonoGame Content Builder)

## Build & Run
```bash
dotnet build sushi-dazzler/sushi-dazzler.csproj
dotnet run --project sushi-dazzler/sushi-dazzler.csproj
```

## Core Systems

### Conductor
The single source of truth for music timing. All rhythm-based systems query the Conductor.

```
┌─────────────────────────────────────────────────────┐
│                     CONDUCTOR                       │
├─────────────────────────────────────────────────────┤
│  SongPosition (seconds)      →  "Where in the song" │
│  BPM                         →  "How fast"          │
│  CurrentBeat (float)         →  "Which beat"        │
│  Crotchet (sec per beat)     →  60 / BPM            │
│  Offset                      →  Audio latency fix   │
├─────────────────────────────────────────────────────┤
│  IsPlaying                   →  Song state          │
│  Start() / Stop() / Pause()  →  Control             │
└─────────────────────────────────────────────────────┘
```

Location: `Core/Conductor.cs`

### Song & Note
Song data structure for charts. Loaded from JSON files.

```
┌─────────────────────────────────────────────────────┐
│                       SONG                          │
├─────────────────────────────────────────────────────┤
│  Title                  →  "Yokohama Nights"        │
│  Artist                 →  "dosii"                  │
│  BPM                    →  120                      │
│  AudioFile              →  "yokohama.ogg"           │
│  Offset                 →  0.0                      │
│  Notes[]                →  List of notes            │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│                       NOTE                          │
├─────────────────────────────────────────────────────┤
│  Beat                   →  4.0 (which beat)         │
│  Type                   →  Tap / Hold               │
│  Duration               →  2.0 (beats, for Hold)    │
└─────────────────────────────────────────────────────┘
```

Location: `Core/Song.cs`, `Core/Note.cs`, `Core/SongLoader.cs`

Charts stored at: `Content/Songs/{bar}/{difficulty}.json`

### NoteTracker
Bridges Conductor and Song. Tracks which notes are active and handles hit detection.

```
┌─────────────────────────────────────────────────────┐
│                   NOTE TRACKER                      │
├─────────────────────────────────────────────────────┤
│  Inputs:                                            │
│    - Song (all notes)                               │
│    - Conductor (current beat)                       │
├─────────────────────────────────────────────────────┤
│  HitWindow              →  ±0.2 beats (adjustable)  │
│  ActiveNotes            →  Notes in hit window      │
│  HitCount / MissCount   →  Stats                    │
├─────────────────────────────────────────────────────┤
│  Update()               →  Move notes in/out window │
│  TryHit(NoteType)       →  Attempt to hit a note    │
│  Reset()                →  Reset for replay         │
└─────────────────────────────────────────────────────┘
```

Location: `Core/NoteTracker.cs`

### Understanding Beats and BPM

#### What is a Beat?
A **beat** is the fundamental unit of musical time. In our system, beats are the primary way we define when notes should appear and be hit. Rather than specifying note timing in seconds or milliseconds, we use beats because they map directly to how music is structured.

#### Why Beats Instead of Time?
Using beats as the driving force for note placement has several advantages:
1. **Musical Intuition**: Charts are created by thinking in musical terms ("this note hits on beat 4") rather than raw time ("this note hits at 2000ms")
2. **BPM Independence**: If you change the song's BPM, all notes automatically adjust to the new tempo without recalculating times
3. **Easier Charting**: Aligning notes to beats 1, 2, 3, 4 (or subdivisions like 1.5, 2.25) is more natural than calculating milliseconds

#### The Math: Converting Between Beats and Time

The **Conductor** handles all timing conversions using these formulas:

```
┌─────────────────────────────────────────────────────────────────┐
│  BPM (Beats Per Minute)     The song's tempo                    │
│                                                                 │
│  Crotchet = 60 / BPM        Seconds per beat                    │
│                             At 120 BPM: 60/120 = 0.5 sec/beat   │
│                             At 60 BPM:  60/60  = 1.0 sec/beat   │
│                                                                 │
│  CurrentBeat = SongPosition / Crotchet                          │
│                             Converts elapsed seconds to beats   │
│                                                                 │
│  NoteTime = Note.Beat × Crotchet                                │
│                             When a note should be hit (seconds) │
└─────────────────────────────────────────────────────────────────┘
```

**Example at 120 BPM:**
- Crotchet = 60 / 120 = 0.5 seconds per beat
- A note at beat 4.0 should be hit at: 4.0 × 0.5 = 2.0 seconds
- A note at beat 8.0 should be hit at: 8.0 × 0.5 = 4.0 seconds
- If 1.5 seconds have elapsed: CurrentBeat = 1.5 / 0.5 = beat 3.0

#### Chart JSON Format

Song charts are stored as JSON files with this structure:

```json
{
  "title": "Yokohama Nights",
  "artist": "dosii",
  "bpm": 120,
  "audioFile": "yokohama.ogg",
  "offset": 0.0,
  "notes": [
    { "beat": 1.0, "type": "Tap" },
    { "beat": 2.0, "type": "Tap" },
    { "beat": 4.0, "type": "Tap" },
    { "beat": 8.0, "type": "Hold", "duration": 2.0 }
  ]
}
```

| Field      | Description                                                    |
|------------|----------------------------------------------------------------|
| `title`    | Song display name                                              |
| `artist`   | Artist name                                                    |
| `bpm`      | Beats per minute (tempo)                                       |
| `audioFile`| Audio file path (relative to song folder)                      |
| `offset`   | Seconds to wait before beat 0 (for audio sync)                 |
| `notes`    | Array of note objects                                          |

**Note object fields:**
| Field      | Description                                                    |
|------------|----------------------------------------------------------------|
| `beat`     | When the note should be hit (in beats, can be decimal)         |
| `type`     | `"Tap"` or `"Hold"`                                            |
| `duration` | For Hold notes only: how long to hold (in beats)               |

#### Beat Subdivisions

Notes don't have to land on whole beats. Common subdivisions:

```
Beat:    1.0    1.25   1.5    1.75   2.0
         │      │      │      │      │
         ┼──────┼──────┼──────┼──────┼
         ▼      ▼      ▼      ▼      ▼
         1    1+1/4  1+1/2  1+3/4    2

1.0   = On the beat (quarter note)
1.5   = Half-beat (eighth note)
1.25  = Quarter of a beat (sixteenth note)
```