using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SushiDazzler.Core.Scenes;

namespace sushi_dazzler;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private GameContext _ctx;
    private SceneManager _sceneManager;

    private Texture2D _pixel;
    private SpriteFont _font;
    private SoundEffect _hitSound;
    private SoundEffect _missSound;

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

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _font = Content.Load<SpriteFont>("DefaultFont");

        try { _hitSound = Content.Load<SoundEffect>("SFX/hit"); }
        catch (Exception) { Console.WriteLine("Warning: Could not load SFX/hit.wav - hit sounds disabled"); }

        try { _missSound = Content.Load<SoundEffect>("SFX/miss"); }
        catch (Exception) { Console.WriteLine("Warning: Could not load SFX/miss.wav - miss sounds disabled"); }

        var saveManager = SushiDazzler.Core.SaveManager.Default();
        saveManager.Load();

        _sceneManager = new SceneManager();
        _ctx = new GameContext
        {
            Content = Content,
            GraphicsDevice = GraphicsDevice,
            Font = _font,
            Pixel = _pixel,
            ScreenWidth = _graphics.PreferredBackBufferWidth,
            ScreenHeight = _graphics.PreferredBackBufferHeight,
            SceneManager = _sceneManager,
            Progress = saveManager,
            HitSound = _hitSound,
            MissSound = _missSound,
            Exit = Exit
        };

        _sceneManager.ChangeScene(new MenuScene(_ctx));
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        _ctx.PreviousKeyboard = _previousKeyboardState;
        _ctx.Keyboard = keyboardState;

        _sceneManager.Update(gameTime);

        _previousKeyboardState = keyboardState;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();
        _sceneManager.Draw(_spriteBatch);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
