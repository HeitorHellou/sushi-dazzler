using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SushiDazzler.Core.Scenes;

public class ResultsScene : IScene
{
    private readonly GameContext _ctx;
    private readonly SongEntry _entry;
    private readonly ScoreTracker _score;

    private bool _isNewBest;
    private ChartRecord? _record;

    public ResultsScene(GameContext ctx, SongEntry entry, ScoreTracker score)
    {
        _ctx = ctx;
        _entry = entry;
        _score = score;
    }

    public void Enter()
    {
        // Record this run and remember whether it beat the stored best.
        _isNewBest = _ctx.Progress.SubmitResult(_entry.Key, _score.TotalScore, _score.GetStarRating());
        _record = _ctx.Progress.Data.GetChart(_entry.Key);
    }

    public void Exit() { }

    public void Update(GameTime gameTime)
    {
        if (_ctx.WasKeyPressed(Keys.R))
        {
            _ctx.SceneManager.ChangeScene(new PlayingScene(_ctx, _entry));
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

        DrawCentered(spriteBatch, $"{_entry.Title} - {_entry.Artist}", cx, y, Color.Gray); y += 40;
        DrawCentered(spriteBatch, "RESULTS", cx, y, Color.White); y += 50;

        int stars = _score.GetStarRating();
        DrawCentered(spriteBatch, new string('*', stars) + new string('-', 5 - stars), cx, y, Color.Gold); y += 40;

        DrawCentered(spriteBatch, $"Score: {_score.TotalScore} / {_score.MaxPossibleScore}", cx, y, Color.White); y += 30;
        DrawCentered(spriteBatch, $"Excellent: {_score.ExcellentCount}", cx, y, Color.Gold); y += 24;
        DrawCentered(spriteBatch, $"Great:     {_score.GreatCount}", cx, y, Color.LimeGreen); y += 24;
        DrawCentered(spriteBatch, $"Good:      {_score.GoodCount}", cx, y, Color.Yellow); y += 24;
        DrawCentered(spriteBatch, $"Miss/Bad:  {_score.BadCount}", cx, y, Color.Red); y += 24;
        DrawCentered(spriteBatch, $"Ghost taps: {_score.GhostTapCount}", cx, y, Color.OrangeRed); y += 34;

        if (_record != null)
        {
            DrawCentered(spriteBatch, $"Best: {_record.BestScore}  ({new string('*', _record.BestStars)})", cx, y, Color.Cyan);
            y += 28;
        }
        if (_isNewBest)
        {
            DrawCentered(spriteBatch, "NEW BEST!", cx, y, Color.Gold);
            y += 28;
        }
        y += 12;

        DrawCentered(spriteBatch, "[R] Retry    [Enter/Esc] Menu", cx, y, Color.Gray);
    }

    private void DrawCentered(SpriteBatch spriteBatch, string text, float cx, float y, Color color)
    {
        var size = _ctx.Font.MeasureString(text);
        spriteBatch.DrawString(_ctx.Font, text, new Vector2(cx - size.X / 2f, y), color);
    }
}
