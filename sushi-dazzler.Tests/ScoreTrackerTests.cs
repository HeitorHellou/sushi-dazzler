using SushiDazzler.Core;

namespace sushi_dazzler.Tests;

public class ScoreTrackerTests
{
    [Theory]
    [InlineData(0.0f, HitAccuracy.Excellent, 10)]
    [InlineData(0.1f, HitAccuracy.Excellent, 10)]
    [InlineData(0.2f, HitAccuracy.Great, 5)]
    [InlineData(0.25f, HitAccuracy.Great, 5)]
    [InlineData(0.4f, HitAccuracy.Good, 2)]
    [InlineData(0.5f, HitAccuracy.Good, 2)]
    [InlineData(0.7f, HitAccuracy.Bad, -10)]
    public void RecordHit_ClassifiesByTimingWindow(float diff, HitAccuracy expected, int expectedScore)
    {
        var s = new ScoreTracker();
        var acc = s.RecordHit(diff);
        Assert.Equal(expected, acc);
        Assert.Equal(expectedScore, s.TotalScore);
        Assert.Equal(1, s.TotalNotes);
    }

    [Fact]
    public void RecordHit_UsesAbsoluteTiming()
    {
        var s = new ScoreTracker();
        Assert.Equal(HitAccuracy.Great, s.RecordHit(-0.2f)); // early counts like late
    }

    [Fact]
    public void RecordMiss_PenalizesAndCountsNote()
    {
        var s = new ScoreTracker();
        s.RecordMiss();
        Assert.Equal(-10, s.TotalScore);
        Assert.Equal(1, s.BadCount);
        Assert.Equal(1, s.TotalNotes);
    }

    [Fact]
    public void RecordGhostTap_PenalizesWithoutCountingNote()
    {
        var s = new ScoreTracker();
        s.RecordGhostTap();
        Assert.Equal(-2, s.TotalScore);
        Assert.Equal(1, s.GhostTapCount);
        Assert.Equal(0, s.TotalNotes); // a ghost tap is not a chart note
        Assert.Equal(0, s.BadCount);
    }

    [Fact]
    public void GhostTap_DoesNotInflateMaxPossibleScore()
    {
        var s = new ScoreTracker();
        s.RecordHit(0f);    // one real note, max 10
        s.RecordGhostTap(); // penalty only, no extra note
        Assert.Equal(10, s.MaxPossibleScore);
    }

    [Fact]
    public void StarRating_AllExcellent_IsFive()
    {
        var s = new ScoreTracker();
        for (int i = 0; i < 10; i++) s.RecordHit(0f);
        Assert.Equal(5, s.GetStarRating());
    }

    [Fact]
    public void StarRating_NoNotes_IsZero()
    {
        Assert.Equal(0, new ScoreTracker().GetStarRating());
    }

    [Fact]
    public void StarRating_EightyPercent_IsFour()
    {
        var s = new ScoreTracker();
        for (int i = 0; i < 9; i++) s.RecordHit(0f); // 90 pts over 9 notes
        s.RecordMiss();                              // -10 => 80 / 100 = 0.8 => 4 stars
        Assert.Equal(4, s.GetStarRating());
    }

    [Fact]
    public void Reset_ClearsEverything()
    {
        var s = new ScoreTracker();
        s.RecordHit(0f);
        s.RecordGhostTap();
        s.Reset();
        Assert.Equal(0, s.TotalScore);
        Assert.Equal(0, s.TotalNotes);
        Assert.Equal(0, s.GhostTapCount);
        Assert.Equal(0, s.GetStarRating());
    }
}
