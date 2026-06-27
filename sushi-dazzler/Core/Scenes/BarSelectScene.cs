using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SushiDazzler.Core.Scenes;

public class BarSelectScene : IScene
{
    private readonly GameContext _ctx;
    private IReadOnlyList<BarEntry> _bars;
    private int _selected;

    public BarSelectScene(GameContext ctx)
    {
        _ctx = ctx;
    }

    public void Enter()
    {
        _bars = SongCatalog.Discover();
        _selected = 0;
    }

    public void Exit() { }

    public void Update(GameTime gameTime)
    {
        if (_ctx.WasKeyPressed(Keys.Escape))
        {
            _ctx.SceneManager.ChangeScene(new MenuScene(_ctx));
            return;
        }

        if (_bars.Count == 0)
            return;

        if (_ctx.WasKeyPressed(Keys.Up))
            _selected = (_selected - 1 + _bars.Count) % _bars.Count;
        if (_ctx.WasKeyPressed(Keys.Down))
            _selected = (_selected + 1) % _bars.Count;

        if (_ctx.WasKeyPressed(Keys.Enter))
        {
            var bar = _bars[_selected];
            if (bar.Songs.Count > 0)
                _ctx.SceneManager.ChangeScene(new SongSelectScene(_ctx, bar));
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        float cx = _ctx.ScreenWidth / 2f;
        float y = 80f;

        DrawCentered(spriteBatch, "SELECT BAR", cx, y, Color.White); y += 60;

        if (_bars.Count == 0)
        {
            DrawCentered(spriteBatch, "(no bars found)", cx, y, Color.Gray);
        }
        else
        {
            for (int i = 0; i < _bars.Count; i++)
            {
                var bar = _bars[i];
                var label = i == _selected ? $"> {bar.Name}" : $"  {bar.Name}";
                var color = i == _selected ? Color.Yellow : Color.LightGray;
                if (bar.Songs.Count == 0) color = Color.DarkGray;
                DrawCentered(spriteBatch, label, cx, y, color);
                y += 32;
            }
        }

        y = _ctx.ScreenHeight - 60f;
        DrawCentered(spriteBatch, "[Up/Down] navigate   [Enter] select   [Esc] back", cx, y, Color.Gray);
    }

    private void DrawCentered(SpriteBatch spriteBatch, string text, float cx, float y, Color color)
    {
        var size = _ctx.Font.MeasureString(text);
        spriteBatch.DrawString(_ctx.Font, text, new Vector2(cx - size.X / 2f, y), color);
    }
}
