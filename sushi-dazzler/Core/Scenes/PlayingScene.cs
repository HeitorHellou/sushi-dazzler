using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace SushiDazzler.Core.Scenes;

public class PlayingScene : IScene
{
    private readonly GameContext _ctx;
    private readonly string _chartPath;

    private Conductor _conductor;
    private Song _song;
    private NoteTracker _noteTracker;
    private NoteHighway _noteHighway;
    private ScoreTracker _scoreTracker;
    private Microsoft.Xna.Framework.Media.Song _musicTrack;

    private float _songEndBeat;
    private bool _finished;

    private static readonly (Keys key, char note)[] NoteKeys = new[]
    {
        (Keys.A, 'A'),
        (Keys.S, 'S'),
        (Keys.D, 'D'),
        (Keys.F, 'F'),
        (Keys.J, 'J'),
        (Keys.K, 'K'),
        (Keys.L, 'L')
    };

    public PlayingScene(GameContext ctx, string chartPath)
    {
        _ctx = ctx;
        _chartPath = chartPath;
    }

    public void Enter()
    {
        _song = SongLoader.Load(_chartPath);

        string audioAssetPath = "Songs/yokohama/" + Path.GetFileNameWithoutExtension(_song.AudioFile);
        _musicTrack = _ctx.Content.Load<Microsoft.Xna.Framework.Media.Song>(audioAssetPath);

        _conductor = new Conductor();
        _noteTracker = new NoteTracker(_song, _conductor);
        _scoreTracker = new ScoreTracker();
        _noteHighway = new NoteHighway(_song, _conductor, _noteTracker, _scoreTracker, _ctx.ScreenWidth, _ctx.ScreenHeight);

        // Compute when the song ends (last note's end beat + buffer for results hand-off).
        float lastNoteEndBeat = _song.Notes
            .Select(n => n.Type == NoteType.Hold ? n.Beat + n.Duration : n.Beat)
            .DefaultIfEmpty(0f)
            .Max();
        _songEndBeat = lastNoteEndBeat + 2f;
        _finished = false;

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
        if (_ctx.WasKeyPressed(Keys.Escape))
        {
            _ctx.SceneManager.ChangeScene(new MenuScene(_ctx));
            return;
        }

        if (_ctx.WasKeyPressed(Keys.R))
        {
            _ctx.SceneManager.ChangeScene(new PlayingScene(_ctx, _chartPath));
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
            _ctx.SceneManager.ChangeScene(new ResultsScene(_ctx, _song, _scoreTracker, _chartPath));
        }
    }

    private void HandleInput()
    {
        foreach (var (key, note) in NoteKeys)
        {
            bool keyDown = _ctx.Keyboard.IsKeyDown(key);
            bool keyWasDown = _ctx.PreviousKeyboard.IsKeyDown(key);

            if (keyDown && !keyWasDown)
            {
                var tapResult = _noteTracker.TryHit(note);
                if (tapResult.Success)
                {
                    var accuracy = _scoreTracker.RecordHit(tapResult.TimingDifference);
                    _noteHighway.OnHit(true, accuracy);
                    _ctx.HitSound?.Play();
                }
                else
                {
                    var holdResult = _noteTracker.TryStartHold(note);
                    if (holdResult.Success)
                    {
                        var accuracy = _scoreTracker.RecordHit(holdResult.TimingDifference);
                        _noteHighway.OnHit(true, accuracy);
                        _ctx.HitSound?.Play();
                    }
                    else
                    {
                        _noteHighway.OnHit(false, null);
                        _ctx.MissSound?.Play();
                    }
                }
            }
            else if (!keyDown && keyWasDown && _noteTracker.CurrentHoldKey == note)
            {
                var releaseResult = _noteTracker.TryReleaseHold();
                if (releaseResult.Success)
                {
                    var accuracy = _scoreTracker.RecordHit(releaseResult.TimingDifference);
                    _noteHighway.OnHit(true, accuracy);
                    _ctx.HitSound?.Play();
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

    public void Draw(SpriteBatch spriteBatch)
    {
        _noteHighway.Draw(spriteBatch, _ctx.Pixel, _ctx.Font);
    }
}
