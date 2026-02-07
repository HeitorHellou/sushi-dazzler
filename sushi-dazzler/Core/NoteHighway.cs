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
    private static readonly Color ExcellentColor = Color.Gold;
    private static readonly Color GreatColor = Color.LimeGreen;
    private static readonly Color GoodColor = Color.Yellow;
    private static readonly Color BadColor = Color.Red;

    // Hit feedback
    private float _hitFlashTimer;
    private bool _lastHitWasSuccess;
    private HitAccuracy? _lastAccuracy;
    private const float FlashDuration = 0.15f;

    // Scoring
    private readonly ScoreTracker _scoreTracker;

    public NoteHighway(Song song, Conductor conductor, NoteTracker noteTracker, ScoreTracker scoreTracker, int screenWidth, int screenHeight)
    {
        _song = song;
        _conductor = conductor;
        _noteTracker = noteTracker;
        _scoreTracker = scoreTracker;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;

        // Set up highway bounds (full width, between HUD and controls hint)
        int highwayY = HudHeight;
        int highwayHeight = screenHeight - HudHeight - ControlsHintHeight;
        HighwayBounds = new Rectangle(0, highwayY, screenWidth, highwayHeight);

        // Hit zone is on the left side
        HitZoneX = 100;
    }

    public void OnHit(bool success, HitAccuracy? accuracy = null)
    {
        _hitFlashTimer = FlashDuration;
        _lastHitWasSuccess = success;
        _lastAccuracy = accuracy;
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
        {
            // For hold notes, consider the end of the hold for visibility
            float noteEndBeat = n.Type == NoteType.Hold ? n.Beat + n.Duration : n.Beat;

            return noteEndBeat >= currentBeat - lookBehind && n.Beat <= currentBeat + LookAheadBeats;
        });
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
        DrawHitZone(spriteBatch, pixel, font);
        DrawNotes(spriteBatch, pixel, font);
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

    private void DrawHitZone(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font)
    {
        // Determine hit zone color based on flash state and accuracy
        Color zoneColor = HitZoneColor;
        if (_hitFlashTimer > 0 && _lastAccuracy.HasValue)
        {
            zoneColor = GetAccuracyColor(_lastAccuracy.Value);
        }
        else if (_hitFlashTimer > 0 && !_lastHitWasSuccess)
        {
            zoneColor = BadColor;
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

        // Draw accuracy text feedback
        if (_hitFlashTimer > 0 && _lastAccuracy.HasValue)
        {
            string accuracyText = GetAccuracyText(_lastAccuracy.Value);
            Color textColor = GetAccuracyColor(_lastAccuracy.Value);
            Vector2 textSize = font.MeasureString(accuracyText);
            Vector2 textPos = new Vector2(HitZoneX - textSize.X / 2, HighwayBounds.Y + 20);
            spriteBatch.DrawString(font, accuracyText, textPos, textColor);
        }
    }

    private static Color GetAccuracyColor(HitAccuracy accuracy)
    {
        return accuracy switch
        {
            HitAccuracy.Excellent => ExcellentColor,
            HitAccuracy.Great => GreatColor,
            HitAccuracy.Good => GoodColor,
            HitAccuracy.Bad => BadColor,
            _ => Color.White
        };
    }

    private static string GetAccuracyText(HitAccuracy accuracy)
    {
        return accuracy switch
        {
            HitAccuracy.Excellent => "EXCELLENT!",
            HitAccuracy.Great => "GREAT!",
            HitAccuracy.Good => "GOOD",
            HitAccuracy.Bad => "BAD",
            _ => ""
        };
    }

    private void DrawNotes(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font)
    {
        int noteY = GetNoteY();
        float pixelsPerBeat = (_screenWidth - HitZoneX - 50) / LookAheadBeats;

        foreach (var note in GetVisibleNotes())
        {
            int noteX = GetNoteX(note);
            Color noteColor = GetNoteColor(note.Type);

            switch (note.Type)
            {
                case NoteType.Tap:
                    // Skip tap notes that are too far off screen
                    if (noteX < -NoteSize || noteX > _screenWidth + NoteSize) continue;

                    // Dim notes that have passed the hit zone
                    if (noteX < HitZoneX - 20)
                    {
                        noteColor = new Color((byte)(noteColor.R / 3), (byte)(noteColor.G / 3), (byte)(noteColor.B / 3), noteColor.A);
                    }
                    DrawTapNote(spriteBatch, pixel, font, noteX, noteY, note.Key, noteColor);
                    break;

                case NoteType.Hold:
                    // For hold notes, calculate the end position
                    int holdWidth = Math.Max(NoteSize, (int)(note.Duration * pixelsPerBeat));
                    int noteEndX = noteX + holdWidth;

                    // Skip if entirely off screen
                    if (noteEndX < 0 || noteX > _screenWidth + NoteSize) continue;

                    // Dim if the end has passed the hit zone
                    if (noteEndX < HitZoneX - 20)
                    {
                        noteColor = new Color((byte)(noteColor.R / 3), (byte)(noteColor.G / 3), (byte)(noteColor.B / 3), noteColor.A);
                    }
                    DrawHoldNote(spriteBatch, pixel, font, noteX, noteY, note.Key, note.Duration, noteColor, pixelsPerBeat);
                    break;
            }
        }
    }

    private void DrawTapNote(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font, int x, int y, char key, Color color)
    {
        // Simple square for tap notes
        spriteBatch.Draw(pixel,
            new Rectangle(x - NoteSize / 2, y, NoteSize, NoteSize),
            color);

        // Draw key letter centered on the note
        string keyStr = key.ToString();
        Vector2 textSize = font.MeasureString(keyStr);
        Vector2 textPos = new Vector2(x - textSize.X / 2, y + (NoteSize - textSize.Y) / 2);
        spriteBatch.DrawString(font, keyStr, textPos, Color.Black);
    }

    private void DrawHoldNote(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font, int x, int y, char key, float duration, Color color, float pixelsPerBeat)
    {
        int holdWidth = Math.Max(NoteSize, (int)(duration * pixelsPerBeat));
        int barHeight = NoteSize / 2;

        // Calculate the actual drawing bounds, clipping to screen
        int drawStartX = x - NoteSize / 2;
        int drawEndX = drawStartX + holdWidth;

        // Clip to left edge of screen (or just past hit zone)
        int clipLeft = 0;
        if (drawStartX < clipLeft)
        {
            drawStartX = clipLeft;
        }

        // Only draw if there's something visible
        int clippedWidth = drawEndX - drawStartX;
        if (clippedWidth <= 0) return;

        // Draw the hold bar (clipped)
        spriteBatch.Draw(pixel,
            new Rectangle(drawStartX, y + NoteSize / 4, clippedWidth, barHeight),
            color);

        // Draw start cap only if start is visible
        if (x - NoteSize / 2 >= clipLeft)
        {
            spriteBatch.Draw(pixel,
                new Rectangle(x - NoteSize / 2, y, 4, NoteSize),
                color);

            // Draw key letter at the start of the hold
            string keyStr = key.ToString();
            Vector2 textSize = font.MeasureString(keyStr);
            Vector2 textPos = new Vector2(x - textSize.X / 2, y + (NoteSize - textSize.Y) / 2);
            spriteBatch.DrawString(font, keyStr, textPos, Color.Black);
        }

        // Draw end cap
        spriteBatch.Draw(pixel,
            new Rectangle(drawEndX - 4, y, 4, NoteSize),
            color);
    }

    private void DrawHud(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font)
    {
        // Background for HUD
        spriteBatch.Draw(pixel, new Rectangle(0, 0, _screenWidth, HudHeight), new Color(20, 20, 30));

        // Draw score
        string scoreText = $"Score: {_scoreTracker.TotalScore}";
        int padding = 20;
        spriteBatch.DrawString(font, scoreText, new Vector2(padding, 10), Color.White);

        // Draw accuracy breakdown
        string accuracyText = $"E:{_scoreTracker.ExcellentCount} G:{_scoreTracker.GreatCount} OK:{_scoreTracker.GoodCount} B:{_scoreTracker.BadCount}";
        spriteBatch.DrawString(font, accuracyText, new Vector2(padding + 150, 10), Color.LightGray);

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
        string controls = "[A S D F J K L = Notes]  [Enter = Start]  [R = Restart]  [Esc = Quit]";
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
