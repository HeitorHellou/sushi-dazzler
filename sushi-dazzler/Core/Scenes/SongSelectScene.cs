using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SushiDazzler.Core.Scenes;

public class SongSelectScene : IScene
{
    private readonly GameContext _ctx;
    private readonly BarEntry _bar;
    private int _selected;

    public SongSelectScene(GameContext ctx, BarEntry bar)
    {
        _ctx = ctx;
        _bar = bar;
    }

    public void Enter()
    {
        _selected = 0;
    }

    public void Exit() { }

    public void Update(GameTime gameTime)
    {
        if (_ctx.WasKeyPressed(Keys.Escape))
        {
            _ctx.SceneManager.ChangeScene(new BarSelectScene(_ctx));
            return;
        }

        if (_bar.Songs.Count == 0)
            return;

        if (_ctx.WasKeyPressed(Keys.Up))
            _selected = (_selected - 1 + _bar.Songs.Count) % _bar.Songs.Count;
        if (_ctx.WasKeyPressed(Keys.Down))
            _selected = (_selected + 1) % _bar.Songs.Count;

        if (_ctx.WasKeyPressed(Keys.Enter))
        {
            var song = _bar.Songs[_selected];
            _ctx.SceneManager.ChangeScene(new PlayingScene(_ctx, song));
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        float cx = _ctx.ScreenWidth / 2f;
        float y = 80f;

        DrawCentered(spriteBatch, _bar.Name.ToUpperInvariant(), cx, y, Color.White); y += 30;
        DrawCentered(spriteBatch, "SELECT SONG", cx, y, Color.LightGray); y += 50;

        if (_bar.Songs.Count == 0)
        {
            DrawCentered(spriteBatch, "(no songs in this bar)", cx, y, Color.Gray);
        }
        else
        {
            for (int i = 0; i < _bar.Songs.Count; i++)
            {
                var song = _bar.Songs[i];
                var record = _ctx.Progress.Data.GetChart(song.Key);
                var best = record != null
                    ? $"   [{new string('*', record.BestStars)}{new string('-', 5 - record.BestStars)}  {record.BestScore}]"
                    : "";
                var label = (i == _selected ? "> " : "  ") + $"{song.Title} - {song.Artist}{best}";
                var color = i == _selected ? Color.Yellow : Color.LightGray;
                DrawCentered(spriteBatch, label, cx, y, color);
                y += 32;
            }
        }

        y = _ctx.ScreenHeight - 60f;
        DrawCentered(spriteBatch, "[Up/Down] navigate   [Enter] play   [Esc] back", cx, y, Color.Gray);
    }

    private void DrawCentered(SpriteBatch spriteBatch, string text, float cx, float y, Color color)
    {
        var size = _ctx.Font.MeasureString(text);
        spriteBatch.DrawString(_ctx.Font, text, new Vector2(cx - size.X / 2f, y), color);
    }
}
