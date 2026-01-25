using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SushiDazzler.Core;

namespace sushi_dazzler;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Conductor _conductor;
    private Song _song;
    private NoteTracker _noteTracker;
    private NoteHighway _noteHighway;

    private Texture2D _pixel;
    private SpriteFont _font;

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
        _conductor = new Conductor();
        _noteTracker = new NoteTracker(_song, _conductor);
        _noteHighway = new NoteHighway(_song, _conductor, _noteTracker,
            _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

        Console.WriteLine($"Loaded: {_song.Title} by {_song.Artist}");
        Console.WriteLine($"BPM: {_song.BPM}, Notes: {_song.Notes.Count}");
        Console.WriteLine("Controls: Space=Tap, H=Hold");
        Console.WriteLine("Press Enter to start...");
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.Escape))
            Exit();

        // Start the song with Enter
        if (WasKeyPressed(Keys.Enter, keyboardState) && !_conductor.IsPlaying)
        {
            _conductor.Start(_song.BPM, _song.Offset);
            Console.WriteLine("Started!");
        }

        // Update highway even when not playing (for flash timers)
        _noteHighway.Update(gameTime);

        if (_conductor.IsPlaying)
        {
            _conductor.Update(gameTime);
            _noteTracker.Update();

            // Input handling
            if (WasKeyPressed(Keys.Space, keyboardState))
                HandleHit(NoteType.Tap);

            if (WasKeyPressed(Keys.H, keyboardState))
                HandleHit(NoteType.Hold);

            // Debug: show current beat every second (roughly every 60 frames)
            if ((int)(_conductor.CurrentBeat * 4) != (int)((_conductor.CurrentBeat - (float)gameTime.ElapsedGameTime.TotalSeconds / _conductor.Crotchet) * 4))
            {
                Console.WriteLine($"Beat: {_conductor.CurrentBeat:F1} | Active: {_noteTracker.ActiveNotes.Count} | Hits: {_noteTracker.HitCount} Miss: {_noteTracker.MissCount}");
            }
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

        if (success)
            Console.WriteLine($"HIT {type}!");
        else
            Console.WriteLine($"Miss... (no {type} note active)");
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
