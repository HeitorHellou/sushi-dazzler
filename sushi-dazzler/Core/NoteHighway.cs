using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SushiDazzler.Core;

public class NoteHighway
{
    private readonly Song _song;
    private readonly Conductor _conductor;
    private readonly NoteTracker _noteTracker;

    // Layout
    private readonly int _screenWidth;
    private readonly int _screenHeight;

    // Highway configuration
    public float LookAheadBeats { get; set; } = 4f;
    public int HitZoneX { get; private set; }
    public Rectangle HighwayBounds { get; private set; }

    // Visual settings
    private const int HudHeight = 40;
    private const int ControlsHintHeight = 30;
    private const int NoteSize = 24;
    private const int HitZoneWidth = 4;

    // Colors
    private static readonly Color TapColor = Color.Cyan;
    private static readonly Color HoldColor = Color.LimeGreen;
    private static readonly Color HighwayBackgroundColor = new Color(30, 30, 40);
    private static readonly Color HitZoneColor = Color.White;
    private static readonly Color HitFlashColor = Color.Yellow;
    private static readonly Color MissFlashColor = Color.Red;

    // Hit feedback
    private float _hitFlashTimer;
    private bool _lastHitWasSuccess;
    private const float FlashDuration = 0.15f;

    public NoteHighway(Song song, Conductor conductor, NoteTracker noteTracker, int screenWidth, int screenHeight)
    {
        _song = song;
        _conductor = conductor;
        _noteTracker = noteTracker;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;

        // Set up highway bounds (full width, between HUD and controls hint)
        int highwayY = HudHeight;
        int highwayHeight = screenHeight - HudHeight - ControlsHintHeight;
        HighwayBounds = new Rectangle(0, highwayY, screenWidth, highwayHeight);

        // Hit zone is on the left side
        HitZoneX = 100;
    }

    public void OnHit(bool success)
    {
        _hitFlashTimer = FlashDuration;
        _lastHitWasSuccess = success;
    }

    public void Update(GameTime gameTime)
    {
        if (_hitFlashTimer > 0)
        {
            _hitFlashTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    public IEnumerable<Note> GetVisibleNotes()
    {
        float currentBeat = _conductor.CurrentBeat;
        float lookBehind = 0.5f; // Show notes slightly past the hit zone

        return _song.Notes.Where(n =>
            n.Beat >= currentBeat - lookBehind &&
            n.Beat <= currentBeat + LookAheadBeats);
    }

    public int GetNoteX(Note note)
    {
        float currentBeat = _conductor.CurrentBeat;
        float beatsAhead = note.Beat - currentBeat;

        // Calculate pixels per beat based on available highway width
        int availableWidth = _screenWidth - HitZoneX - 50; // Leave some margin on the right
        float pixelsPerBeat = availableWidth / LookAheadBeats;

        return HitZoneX + (int)(beatsAhead * pixelsPerBeat);
    }

    public int GetNoteY()
    {
        // Center notes vertically in the highway
        return HighwayBounds.Y + HighwayBounds.Height / 2 - NoteSize / 2;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font)
    {
        DrawHighwayBackground(spriteBatch, pixel);
        DrawHitZone(spriteBatch, pixel);
        DrawNotes(spriteBatch, pixel);
        DrawHud(spriteBatch, pixel, font);
        DrawControlsHint(spriteBatch, pixel, font);
    }

    private void DrawHighwayBackground(SpriteBatch spriteBatch, Texture2D pixel)
    {
        // Draw highway background
        spriteBatch.Draw(pixel, HighwayBounds, HighwayBackgroundColor);

        // Draw beat lines for visual reference
        float currentBeat = _conductor.CurrentBeat;
        int startBeat = (int)Math.Floor(currentBeat);

        for (int i = startBeat; i <= startBeat + (int)LookAheadBeats + 1; i++)
        {
            float beatPos = i - currentBeat;
            if (beatPos < -0.5f || beatPos > LookAheadBeats) continue;

            int lineX = HitZoneX + (int)(beatPos * ((_screenWidth - HitZoneX - 50) / LookAheadBeats));

            // Make every 4th beat brighter
            Color lineColor = i % 4 == 0 ? new Color(80, 80, 100) : new Color(50, 50, 60);

            spriteBatch.Draw(pixel,
                new Rectangle(lineX, HighwayBounds.Y, 1, HighwayBounds.Height),
                lineColor);
        }
    }

    private void DrawHitZone(SpriteBatch spriteBatch, Texture2D pixel)
    {
        // Determine hit zone color based on flash state
        Color zoneColor = HitZoneColor;
        if (_hitFlashTimer > 0)
        {
            zoneColor = _lastHitWasSuccess ? HitFlashColor : MissFlashColor;
        }

        // Draw the hit zone line
        spriteBatch.Draw(pixel,
            new Rectangle(HitZoneX - HitZoneWidth / 2, HighwayBounds.Y, HitZoneWidth, HighwayBounds.Height),
            zoneColor);

        // Draw a subtle glow area around the hit zone
        Color glowColor = new Color(zoneColor.R, zoneColor.G, zoneColor.B, (byte)30);
        int glowWidth = 30;
        spriteBatch.Draw(pixel,
            new Rectangle(HitZoneX - glowWidth, HighwayBounds.Y, glowWidth * 2, HighwayBounds.Height),
            glowColor);
    }

    private void DrawNotes(SpriteBatch spriteBatch, Texture2D pixel)
    {
        int noteY = GetNoteY();

        foreach (var note in GetVisibleNotes())
        {
            int noteX = GetNoteX(note);

            // Skip notes that are too far off screen
            if (noteX < -NoteSize || noteX > _screenWidth + NoteSize) continue;

            Color noteColor = GetNoteColor(note.Type);

            // Dim notes that have passed the hit zone
            if (noteX < HitZoneX - 20)
            {
                noteColor = new Color((byte)(noteColor.R / 3), (byte)(noteColor.G / 3), (byte)(noteColor.B / 3), noteColor.A);
            }

            switch (note.Type)
            {
                case NoteType.Tap:
                    DrawTapNote(spriteBatch, pixel, noteX, noteY, noteColor);
                    break;
                case NoteType.Hold:
                    DrawHoldNote(spriteBatch, pixel, noteX, noteY, note.Duration, noteColor);
                    break;
            }
        }
    }

    private void DrawTapNote(SpriteBatch spriteBatch, Texture2D pixel, int x, int y, Color color)
    {
        // Simple square for tap notes
        spriteBatch.Draw(pixel,
            new Rectangle(x - NoteSize / 2, y, NoteSize, NoteSize),
            color);
    }

    private void DrawHoldNote(SpriteBatch spriteBatch, Texture2D pixel, int x, int y, float duration, Color color)
    {
        // Calculate the width based on duration
        float pixelsPerBeat = (_screenWidth - HitZoneX - 50) / LookAheadBeats;
        int holdWidth = Math.Max(NoteSize, (int)(duration * pixelsPerBeat));

        // Draw the hold bar
        int barHeight = NoteSize / 2;
        spriteBatch.Draw(pixel,
            new Rectangle(x - NoteSize / 2, y + NoteSize / 4, holdWidth, barHeight),
            color);

        // Draw caps at start and end
        spriteBatch.Draw(pixel,
            new Rectangle(x - NoteSize / 2, y, 4, NoteSize),
            color);
        spriteBatch.Draw(pixel,
            new Rectangle(x - NoteSize / 2 + holdWidth - 4, y, 4, NoteSize),
            color);
    }

    private void DrawHud(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font)
    {
        // Background for HUD
        spriteBatch.Draw(pixel, new Rectangle(0, 0, _screenWidth, HudHeight), new Color(20, 20, 30));

        // Draw stats
        string beatText = $"Beat: {_conductor.CurrentBeat:F1}";
        string hitsText = $"Hits: {_noteTracker.HitCount}";
        string missText = $"Misses: {_noteTracker.MissCount}";

        int padding = 20;

        spriteBatch.DrawString(font, beatText, new Vector2(padding, 10), Color.White);
        spriteBatch.DrawString(font, hitsText, new Vector2(padding + 150, 10), HitFlashColor);
        spriteBatch.DrawString(font, missText, new Vector2(padding + 280, 10), MissFlashColor);

        // Song title on the right
        string titleText = $"{_song.Title} - {_song.Artist}";
        Vector2 titleSize = font.MeasureString(titleText);
        spriteBatch.DrawString(font, titleText, new Vector2(_screenWidth - titleSize.X - padding, 10), Color.Gray);
    }

    private void DrawControlsHint(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font)
    {
        int y = _screenHeight - ControlsHintHeight;

        // Background
        spriteBatch.Draw(pixel, new Rectangle(0, y, _screenWidth, ControlsHintHeight), new Color(20, 20, 30));

        // Controls text
        string controls = "[Space = Tap]  [H = Hold]  [Enter = Start]  [Esc = Quit]";
        Vector2 controlsSize = font.MeasureString(controls);
        float controlsX = (_screenWidth - controlsSize.X) / 2;

        spriteBatch.DrawString(font, controls, new Vector2(controlsX, y + 6), Color.Gray);
    }

    private static Color GetNoteColor(NoteType type)
    {
        return type switch
        {
            NoteType.Tap => TapColor,
            NoteType.Hold => HoldColor,
            _ => Color.White
        };
    }
}
