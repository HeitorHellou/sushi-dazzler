using System;
using System.IO;
using SushiDazzler.Core;

namespace sushi_dazzler.Tests;

public class SaveManagerTests
{
    [Fact]
    public void SaveThenLoad_RoundTripsScoresAndSettings()
    {
        var path = Path.Combine(Path.GetTempPath(), $"sushi_save_{Guid.NewGuid():N}.json");
        try
        {
            var writer = new SaveManager(path);
            writer.SubmitResult("Yokohama/easy", 120, 4);
            writer.Data.Settings.AudioOffsetSeconds = 0.05f;
            writer.Save();

            var reader = new SaveManager(path);
            reader.Load();

            var rec = reader.Data.GetChart("Yokohama/easy");
            Assert.NotNull(rec);
            Assert.Equal(120, rec!.BestScore);
            Assert.Equal(4, rec.BestStars);
            Assert.Equal(0.05f, reader.Data.Settings.AudioOffsetSeconds, 5);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void Load_MissingFile_StartsEmpty()
    {
        var path = Path.Combine(Path.GetTempPath(), $"sushi_missing_{Guid.NewGuid():N}.json");
        var reader = new SaveManager(path);

        reader.Load();

        Assert.Empty(reader.Data.Charts);
    }
}
