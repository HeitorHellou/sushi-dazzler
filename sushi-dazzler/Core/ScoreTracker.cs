namespace SushiDazzler.Core;

public enum HitAccuracy
{
    Excellent,  // ±0.1 beats  → 10 pts
    Great,      // ±0.25 beats → 5 pts
    Good,       // ±0.5 beats  → 2 pts
    Bad         // beyond      → -10 pts (miss)
}

public class ScoreTracker
{
    // Timing windows in beats
    public float ExcellentWindow { get; set; } = 0.1f;
    public float GreatWindow { get; set; } = 0.25f;
    public float GoodWindow { get; set; } = 0.5f;

    // Points per accuracy tier
    public int ExcellentPoints { get; set; } = 10;
    public int GreatPoints { get; set; } = 5;
    public int GoodPoints { get; set; } = 2;
    public int BadPoints { get; set; } = -10;

    // Penalty for a press that matched no note (a "ghost tap"). Small so mashing
    // isn't free, but it is NOT a missed note: it doesn't touch the note counts.
    public int GhostTapPenalty { get; set; } = 2;

    // Stats
    public int TotalScore { get; private set; }
    public int ExcellentCount { get; private set; }
    public int GreatCount { get; private set; }
    public int GoodCount { get; private set; }
    public int BadCount { get; private set; }
    public int GhostTapCount { get; private set; }
    public int TotalNotes { get; private set; }

    public int MaxPossibleScore => TotalNotes * ExcellentPoints;

    public HitAccuracy RecordHit(float timingDifference)
    {
        float absTimingDiff = System.Math.Abs(timingDifference);
        HitAccuracy accuracy;
        int points;

        if (absTimingDiff <= ExcellentWindow)
        {
            accuracy = HitAccuracy.Excellent;
            points = ExcellentPoints;
            ExcellentCount++;
        }
        else if (absTimingDiff <= GreatWindow)
        {
            accuracy = HitAccuracy.Great;
            points = GreatPoints;
            GreatCount++;
        }
        else if (absTimingDiff <= GoodWindow)
        {
            accuracy = HitAccuracy.Good;
            points = GoodPoints;
            GoodCount++;
        }
        else
        {
            accuracy = HitAccuracy.Bad;
            points = BadPoints;
            BadCount++;
        }

        TotalScore += points;
        TotalNotes++;
        return accuracy;
    }

    public void RecordMiss()
    {
        TotalScore += BadPoints;
        BadCount++;
        TotalNotes++;
    }

    /// <summary>
    /// A key press that hit nothing. Applies a small penalty without counting as a
    /// chart note, so the star denominator and accuracy breakdown stay accurate.
    /// </summary>
    public void RecordGhostTap()
    {
        TotalScore -= GhostTapPenalty;
        GhostTapCount++;
    }

    public int GetStarRating()
    {
        if (TotalNotes == 0)
            return 0;

        float percentage = (float)TotalScore / MaxPossibleScore;

        if (percentage >= 0.9f)
            return 5;
        if (percentage >= 0.75f)
            return 4;
        if (percentage >= 0.5f)
            return 3;
        if (percentage >= 0.25f)
            return 2;
        return 1;
    }

    public void Reset()
    {
        TotalScore = 0;
        ExcellentCount = 0;
        GreatCount = 0;
        GoodCount = 0;
        BadCount = 0;
        GhostTapCount = 0;
        TotalNotes = 0;
    }
}
