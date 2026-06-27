using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SushiDazzler.Core.Scenes;

public class MenuScene : IScene
{
    private readonly GameContext _ctx;

    public MenuScene(GameContext ctx)
    {
        _ctx = ctx;
    }

    public void Enter() { }
    public void Exit() { }

    public void Update(GameTime gameTime)
    {
        if (_ctx.WasKeyPressed(Keys.Escape))
        {
            _ctx.Exit?.Invoke();
            return;
        }

        if (_ctx.WasKeyPressed(Keys.Enter))
        {
            _ctx.SceneManager.ChangeScene(new BarSelectScene(_ctx));
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var title = "SUSHI DAZZLER";
        var prompt = "Press ENTER to play";
        var quit = "Press ESC to quit";

        var titleSize = _ctx.Font.MeasureString(title);
        var promptSize = _ctx.Font.MeasureString(prompt);
        var quitSize = _ctx.Font.MeasureString(quit);

        float cx = _ctx.ScreenWidth / 2f;
        float cy = _ctx.ScreenHeight / 2f;

        spriteBatch.DrawString(_ctx.Font, title,
            new Vector2(cx - titleSize.X / 2f, cy - 80), Color.White);
        spriteBatch.DrawString(_ctx.Font, prompt,
            new Vector2(cx - promptSize.X / 2f, cy), Color.LightGray);
        spriteBatch.DrawString(_ctx.Font, quit,
            new Vector2(cx - quitSize.X / 2f, cy + 40), Color.Gray);
    }
}
