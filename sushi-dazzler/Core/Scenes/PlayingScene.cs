using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace SushiDazzler.Core.Scenes;

public class PlayingScene : IScene
{
    private readonly GameContext _ctx;
    private readonly SongEntry _entry;

    private Conductor _conductor;
    private Song _song;
    private NoteTracker _noteTracker;
    private NoteHighway _noteHighway;
    private ScoreTracker _scoreTracker;
    private Microsoft.Xna.Framework.Media.Song _musicTrack;

    private float _songEndBeat;
    private bool _finished;
    private bool _paused;
    private float _resumeTimer;
    private bool _loadFailed;

    private const float ResumeCountdownSeconds = 3f;

    public PlayingScene(GameContext ctx, SongEntry entry)
    {
        _ctx = ctx;
        _entry = entry;
    }

    public void Enter()
    {
        _song = _entry.Song;

        try
        {
            _musicTrack = _ctx.Content.Load<Microsoft.Xna.Framework.Media.Song>(_entry.AudioAssetPath);
        }
        catch (Exception ex)
        {
            // A chart pointing at audio that isn't in the content pipeline shouldn't crash to desktop.
            Console.WriteLine($"Failed to load audio '{_entry.AudioAssetPath}': {ex.Message}");
            _loadFailed = true;
            return;
        }

        _conductor = new Conductor();
        _scoreTracker = new ScoreTracker();
        _noteTracker = new NoteTracker(_song, _conductor)
        {
            // Single source of truth: a note is hittable exactly within the widest scoring tier.
            HitWindow = _scoreTracker.GoodWindow
        };
        _noteHighway = new NoteHighway(_song, _conductor, _scoreTracker, _ctx.ScreenWidth, _ctx.ScreenHeight);

        // Compute when the song ends (last note's end beat + buffer for results hand-off).
        float lastNoteEndBeat = _song.Notes
            .Select(n => n.Type == NoteType.Hold ? n.Beat + n.Duration : n.Beat)
            .DefaultIfEmpty(0f)
            .Max();
        _songEndBeat = lastNoteEndBeat + 2f;
        _finished = false;
        _paused = false;
        _resumeTimer = 0f;

        _conductor.Start(_song.BPM, _song.Offset);
        MediaPlayer.Play(_musicTrack);
        MediaPlayer.IsRepeating = false;
    }

    public void Exit()
    {
        MediaPlayer.Stop();
    }

    public void Update(GameTime gameTime)
    {
        if (_loadFailed)
        {
            _ctx.SceneManager.ChangeScene(new MenuScene(_ctx));
            return;
        }

        if (_resumeTimer > 0f)
        {
            UpdateResuming(gameTime);
            return;
        }

        if (_paused)
        {
            UpdatePaused();
            return;
        }

        if (_ctx.WasKeyPressed(Keys.Escape))
        {
            Pause();
            return;
        }

        if (_ctx.WasKeyPressed(Keys.R))
        {
            Restart();
            return;
        }

        _noteHighway.Update(gameTime);

        if (_finished)
            return;

        _conductor.Update(gameTime);

        int missedCount = _noteTracker.Update();
        for (int i = 0; i < missedCount; i++)
        {
            _scoreTracker.RecordMiss();
            _ctx.MissSound?.Play();
        }

        HandleInput();

        if (_conductor.CurrentBeat >= _songEndBeat)
        {
            _finished = true;
            _ctx.SceneManager.ChangeScene(new ResultsScene(_ctx, _entry, _scoreTracker));
        }
    }

    private void HandleInput()
    {
        foreach (var (key, note) in Lanes.All)
        {
            if (_ctx.WasKeyPressed(key))
            {
                var tapResult = _noteTracker.TryHit(note);
                if (tapResult.Success)
                {
                    RegisterHit(tapResult.TimingDifference);
                }
                else
                {
                    var holdResult = _noteTracker.TryStartHold(note);
                    if (holdResult.Success)
                        RegisterHit(holdResult.TimingDifference);
                    else
                        RegisterGhostTap();
                }
            }
            else if (_ctx.WasKeyReleased(key) && _noteTracker.CurrentHoldKey == note)
            {
                var releaseResult = _noteTracker.TryReleaseHold();
                if (releaseResult.Success)
                {
                    RegisterHit(releaseResult.TimingDifference);
                }
                else
                {
                    _scoreTracker.RecordMiss();
                    _noteHighway.OnHit(false, HitAccuracy.Bad);
                    _ctx.MissSound?.Play();
                }
            }
        }
    }

    private void RegisterHit(float timingDifference)
    {
        var accuracy = _scoreTracker.RecordHit(timingDifference);
        _noteHighway.OnHit(true, accuracy);
        _ctx.HitSound?.Play();
    }

    private void RegisterGhostTap()
    {
        // A press that matched no note: small score penalty so mashing isn't free.
        _scoreTracker.RecordGhostTap();
        _noteHighway.OnHit(false, null);
        _ctx.MissSound?.Play();
    }

    private void Restart()
    {
        _ctx.SceneManager.ChangeScene(new PlayingScene(_ctx, _entry));
    }

    private void Pause()
    {
        _paused = true;
        _conductor.Pause();
        MediaPlayer.Pause();
    }

    private void Resume()
    {
        _conductor.Resume();
        MediaPlayer.Resume();
    }

    private void UpdatePaused()
    {
        if (_ctx.WasKeyPressed(Keys.Escape) || _ctx.WasKeyPressed(Keys.Enter))
        {
            // Leave the pause menu and run the countdown; song stays frozen until it ends.
            _paused = false;
            _resumeTimer = ResumeCountdownSeconds;
            return;
        }

        if (_ctx.WasKeyPressed(Keys.R))
        {
            Restart();
            return;
        }

        if (_ctx.WasKeyPressed(Keys.Q))
        {
            _ctx.SceneManager.ChangeScene(new MenuScene(_ctx));
        }
    }

    private void UpdateResuming(GameTime gameTime)
    {
        // Esc during the countdown jumps back to the pause menu.
        if (_ctx.WasKeyPressed(Keys.Escape))
        {
            _resumeTimer = 0f;
            _paused = true;
            return;
        }

        _resumeTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_resumeTimer <= 0f)
        {
            _resumeTimer = 0f;
            Resume();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (_loadFailed)
            return;

        _noteHighway.Draw(spriteBatch, _ctx.Pixel, _ctx.Font);

        if (_paused)
            DrawPauseOverlay(spriteBatch);
        else if (_resumeTimer > 0f)
            DrawResumeCountdown(spriteBatch);
    }

    private void DrawResumeCountdown(SpriteBatch spriteBatch)
    {
        // Lightly dim the frozen highway; the big number reads as a "get ready" beat.
        spriteBatch.Draw(_ctx.Pixel,
            new Rectangle(0, 0, _ctx.ScreenWidth, _ctx.ScreenHeight),
            new Color(0, 0, 0, 120));

        int count = (int)System.Math.Ceiling(_resumeTimer);
        string text = count.ToString();
        const float scale = 4f;
        Vector2 size = _ctx.Font.MeasureString(text) * scale;
        Vector2 pos = new Vector2((_ctx.ScreenWidth - size.X) / 2, (_ctx.ScreenHeight - size.Y) / 2);
        spriteBatch.DrawString(_ctx.Font, text, pos, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    private void DrawPauseOverlay(SpriteBatch spriteBatch)
    {
        // Dim the frozen highway behind the menu.
        spriteBatch.Draw(_ctx.Pixel,
            new Rectangle(0, 0, _ctx.ScreenWidth, _ctx.ScreenHeight),
            new Color(0, 0, 0, 180));

        DrawCenteredText(spriteBatch, "PAUSED", _ctx.ScreenHeight / 2 - 60, Color.White);
        DrawCenteredText(spriteBatch, "Esc / Enter = Resume", _ctx.ScreenHeight / 2 - 10, Color.LightGray);
        DrawCenteredText(spriteBatch, "R = Restart", _ctx.ScreenHeight / 2 + 20, Color.LightGray);
        DrawCenteredText(spriteBatch, "Q = Quit to Menu", _ctx.ScreenHeight / 2 + 50, Color.LightGray);
    }

    private void DrawCenteredText(SpriteBatch spriteBatch, string text, int y, Color color)
    {
        Vector2 size = _ctx.Font.MeasureString(text);
        spriteBatch.DrawString(_ctx.Font, text, new Vector2((_ctx.ScreenWidth - size.X) / 2, y), color);
    }
}
