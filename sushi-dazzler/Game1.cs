using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SushiDazzler.Core;

namespace sushi_dazzler;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Conductor _conductor;
    private SushiDazzler.Core.Song _song;
    private NoteTracker _noteTracker;
    private NoteHighway _noteHighway;
    private ScoreTracker _scoreTracker;

    private Texture2D _pixel;
    private SpriteFont _font;

    private Microsoft.Xna.Framework.Media.Song _musicTrack;

    private KeyboardState _previousKeyboardState;

    // Note keys: A, S, D, F, J, K, L
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

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Create 1x1 pixel texture for drawing shapes
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        // Load font
        _font = Content.Load<SpriteFont>("DefaultFont");

        _song = SongLoader.Load("Content/Songs/yokohama/easy.json");

        // Load audio file through content pipeline
        string audioAssetPath = "Songs/yokohama/" + Path.GetFileNameWithoutExtension(_song.AudioFile);
        _musicTrack = Content.Load<Microsoft.Xna.Framework.Media.Song>(audioAssetPath);

        _conductor = new Conductor();
        _noteTracker = new NoteTracker(_song, _conductor);
        _scoreTracker = new ScoreTracker();
        _noteHighway = new NoteHighway(_song, _conductor, _noteTracker, _scoreTracker, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            MediaPlayer.Stop();
            Exit();
        }

        // Start the song with Enter
        if (WasKeyPressed(Keys.Enter, keyboardState) && !_conductor.IsPlaying)
        {
            _conductor.Start(_song.BPM, _song.Offset);
            MediaPlayer.Play(_musicTrack);
            MediaPlayer.IsRepeating = false;
            Console.WriteLine("Started!");
        }

        // Restart the song with R
        if (WasKeyPressed(Keys.R, keyboardState))
        {
            MediaPlayer.Stop();
            _conductor.Stop();
            _noteTracker.Reset();
            _scoreTracker.Reset();
            _conductor.Start(_song.BPM, _song.Offset);
            MediaPlayer.Play(_musicTrack);
            MediaPlayer.IsRepeating = false;
            Console.WriteLine("Restarted!");
        }

        // Update highway even when not playing (for flash timers)
        _noteHighway.Update(gameTime);

        if (_conductor.IsPlaying)
        {
            _conductor.Update(gameTime);
            int missedCount = _noteTracker.Update();
            for (int i = 0; i < missedCount; i++)
            {
                _scoreTracker.RecordMiss();
            }

            // Input handling - check all note keys
            foreach (var (key, note) in NoteKeys)
            {
                bool keyDown = keyboardState.IsKeyDown(key);
                bool keyWasDown = _previousKeyboardState.IsKeyDown(key);

                if (keyDown && !keyWasDown)
                {
                    // Key just pressed - try Tap first, then Hold
                    var tapResult = _noteTracker.TryHit(note);
                    if (tapResult.Success)
                    {
                        var accuracy = _scoreTracker.RecordHit(tapResult.TimingDifference);
                        _noteHighway.OnHit(true, accuracy);
                    }
                    else
                    {
                        var holdResult = _noteTracker.TryStartHold(note);
                        if (holdResult.Success)
                        {
                            var accuracy = _scoreTracker.RecordHit(holdResult.TimingDifference);
                            _noteHighway.OnHit(true, accuracy);
                        }
                        else
                        {
                            _noteHighway.OnHit(false, null);
                        }
                    }
                }
                else if (!keyDown && keyWasDown && _noteTracker.CurrentHoldKey == note)
                {
                    // Key released - check if this is the held note's key
                    var releaseResult = _noteTracker.TryReleaseHold();
                    if (releaseResult.Success)
                    {
                        var accuracy = _scoreTracker.RecordHit(releaseResult.TimingDifference);
                        _noteHighway.OnHit(true, accuracy);
                    }
                    else
                    {
                        _scoreTracker.RecordMiss();
                        _noteHighway.OnHit(false, HitAccuracy.Bad);
                    }
                }
            }
        }

        _previousKeyboardState = keyboardState;
        base.Update(gameTime);
    }

    private bool WasKeyPressed(Keys key, KeyboardState currentState)
    {
        return currentState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
    }


    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();
        _noteHighway.Draw(_spriteBatch, _pixel, _font);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
