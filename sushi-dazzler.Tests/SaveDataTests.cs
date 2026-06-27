using SushiDazzler.Core;

namespace sushi_dazzler.Tests;

public class SaveDataTests
{
    [Fact]
    public void SubmitResult_FirstPlay_SetsBest_EvenIfNegative()
    {
        var data = new SaveData();

        bool isNew = data.SubmitResult("Yokohama/easy", -5, 1);

        Assert.True(isNew);
        var rec = data.GetChart("Yokohama/easy");
        Assert.NotNull(rec);
        Assert.Equal(-5, rec!.BestScore);
        Assert.Equal(1, rec.BestStars);
        Assert.Equal(1, rec.TimesPlayed);
    }

    [Fact]
    public void SubmitResult_HigherScore_UpdatesBest()
    {
        var data = new SaveData();
        data.SubmitResult("k", 100, 3);

        bool isNew = data.SubmitResult("k", 150, 4);

        Assert.True(isNew);
        var rec = data.GetChart("k")!;
        Assert.Equal(150, rec.BestScore);
        Assert.Equal(4, rec.BestStars);
        Assert.Equal(2, rec.TimesPlayed);
    }

    [Fact]
    public void SubmitResult_LowerScore_KeepsBest_ButCountsPlay()
    {
        var data = new SaveData();
        data.SubmitResult("k", 100, 3);

        bool isNew = data.SubmitResult("k", 50, 2);

        Assert.False(isNew);
        var rec = data.GetChart("k")!;
        Assert.Equal(100, rec.BestScore);
        Assert.Equal(3, rec.BestStars);
        Assert.Equal(2, rec.TimesPlayed);
    }

    [Fact]
    public void GetChart_Unknown_ReturnsNull()
    {
        Assert.Null(new SaveData().GetChart("nope"));
    }
}
