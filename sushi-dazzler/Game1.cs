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

    private Texture2D _pixel;
    private SpriteFont _font;

    private Microsoft.Xna.Framework.Media.Song _musicTrack;

    private KeyboardState _previousKeyboardState;

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
        _noteHighway = new NoteHighway(_song, _conductor, _noteTracker, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
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
            _noteTracker.Update();

            // Input handling - Tap notes
            if (WasKeyPressed(Keys.Space, keyboardState))
                HandleHit(NoteType.Tap);

            // Input handling - Hold notes
            HandleHoldInput(keyboardState);
        }

        _previousKeyboardState = keyboardState;
        base.Update(gameTime);
    }

    private bool WasKeyPressed(Keys key, KeyboardState currentState)
    {
        return currentState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
    }

    private void HandleHit(NoteType type)
    {
        bool success = _noteTracker.TryHit(type);
        _noteHighway.OnHit(success);
    }

    private void HandleHoldInput(KeyboardState keyboardState)
    {
        bool holdKeyDown = keyboardState.IsKeyDown(Keys.H);
        bool holdKeyWasDown = _previousKeyboardState.IsKeyDown(Keys.H);

        if (holdKeyDown && !holdKeyWasDown)
        {
            // Key just pressed - try to start a hold
            bool success = _noteTracker.TryStartHold();
            _noteHighway.OnHit(success);
        }
        else if (!holdKeyDown && holdKeyWasDown && _noteTracker.IsHolding)
        {
            // Key released - check if release timing is correct (within ±0.2 of end beat)
            bool success = _noteTracker.TryReleaseHold();
            _noteHighway.OnHit(success);
        }
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
