using System.Collections.Generic;

namespace SushiDazzler.Core;

public class ChartRecord
{
    public int BestScore { get; set; }
    public int BestStars { get; set; }
    public int TimesPlayed { get; set; }
}

public class GameSettings
{
    // Global audio/input latency offset in seconds. The future calibration screen writes this;
    // it lives here so persistence and calibration share one save file.
    public float AudioOffsetSeconds { get; set; }
}

/// <summary>
/// Pure, serializable player progress: best score per chart plus shared settings.
/// All "is this a new best?" logic lives here so it can be tested without touching disk.
/// </summary>
public class SaveData
{
    public Dictionary<string, ChartRecord> Charts { get; set; } = new();
    public GameSettings Settings { get; set; } = new();

    public ChartRecord? GetChart(string key) =>
        Charts.TryGetValue(key, out var record) ? record : null;

    /// <summary>
    /// Records a completed play and updates the stored best. Returns true if this run set a
    /// new best score — always true on a chart's first play, even with a negative score.
    /// </summary>
    public bool SubmitResult(string key, int score, int stars)
    {
        if (!Charts.TryGetValue(key, out var record))
        {
            Charts[key] = new ChartRecord { BestScore = score, BestStars = stars, TimesPlayed = 1 };
            return true;
        }

        record.TimesPlayed++;
        if (score > record.BestScore)
        {
            record.BestScore = score;
            record.BestStars = stars;
            return true;
        }
        return false;
    }
}
