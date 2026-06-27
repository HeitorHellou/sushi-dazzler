using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SushiDazzler.Core.Scenes;

public class ResultsScene : IScene
{
    private readonly GameContext _ctx;
    private readonly Song _song;
    private readonly ScoreTracker _score;
    private readonly string _chartPath;
    private readonly string _audioAssetPath;

    public ResultsScene(GameContext ctx, Song song, ScoreTracker score, string chartPath, string audioAssetPath)
    {
        _ctx = ctx;
        _song = song;
        _score = score;
        _chartPath = chartPath;
        _audioAssetPath = audioAssetPath;
    }

    public void Enter() { }
    public void Exit() { }

    public void Update(GameTime gameTime)
    {
        if (_ctx.WasKeyPressed(Keys.R))
        {
            _ctx.SceneManager.ChangeScene(new PlayingScene(_ctx, _chartPath, _audioAssetPath));
            return;
        }

        if (_ctx.WasKeyPressed(Keys.Escape) || _ctx.WasKeyPressed(Keys.Enter))
        {
            _ctx.SceneManager.ChangeScene(new MenuScene(_ctx));
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        float cx = _ctx.ScreenWidth / 2f;
        float y = 80f;

        DrawCentered(spriteBatch, $"{_song.Title} - {_song.Artist}", cx, y, Color.Gray); y += 40;
        DrawCentered(spriteBatch, "RESULTS", cx, y, Color.White); y += 50;

        int stars = _score.GetStarRating();
        DrawCentered(spriteBatch, new string('*', stars) + new string('-', 5 - stars), cx, y, Color.Gold); y += 40;

        DrawCentered(spriteBatch, $"Score: {_score.TotalScore} / {_score.MaxPossibleScore}", cx, y, Color.White); y += 30;
        DrawCentered(spriteBatch, $"Excellent: {_score.ExcellentCount}", cx, y, Color.Gold); y += 24;
        DrawCentered(spriteBatch, $"Great:     {_score.GreatCount}", cx, y, Color.LimeGreen); y += 24;
        DrawCentered(spriteBatch, $"Good:      {_score.GoodCount}", cx, y, Color.Yellow); y += 24;
        DrawCentered(spriteBatch, $"Miss/Bad:  {_score.BadCount}", cx, y, Color.Red); y += 40;

        DrawCentered(spriteBatch, "[R] Retry    [Enter/Esc] Menu", cx, y, Color.Gray);
    }

    private void DrawCentered(SpriteBatch spriteBatch, string text, float cx, float y, Color color)
    {
        var size = _ctx.Font.MeasureString(text);
        spriteBatch.DrawString(_ctx.Font, text, new Vector2(cx - size.X / 2f, y), color);
    }
}
