using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SushiDazzler.Core.Scenes;

public class GameContext
{
    public ContentManager Content { get; set; }
    public GraphicsDevice GraphicsDevice { get; set; }
    public SpriteFont Font { get; set; }
    public Texture2D Pixel { get; set; }
    public int ScreenWidth { get; set; }
    public int ScreenHeight { get; set; }
    public SceneManager SceneManager { get; set; }
    public SoundEffect HitSound { get; set; }
    public SoundEffect MissSound { get; set; }
    public Action Exit { get; set; }

    public KeyboardState Keyboard { get; set; }
    public KeyboardState PreviousKeyboard { get; set; }

    public bool WasKeyPressed(Keys key) =>
        Keyboard.IsKeyDown(key) && !PreviousKeyboard.IsKeyDown(key);
}
