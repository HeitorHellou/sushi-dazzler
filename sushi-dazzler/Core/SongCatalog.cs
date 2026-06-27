using System;
using System.Collections.Generic;
using System.IO;

namespace SushiDazzler.Core;

public record SongEntry(string Title, string Artist, string ChartPath, string AudioAssetPath);

public record BarEntry(string Name, IReadOnlyList<SongEntry> Songs);

public static class SongCatalog
{
    private static readonly string SongsRoot =
        Path.Combine(AppContext.BaseDirectory, "Content", "Songs");

    public static IReadOnlyList<BarEntry> Discover()
    {
        if (!Directory.Exists(SongsRoot))
            return Array.Empty<BarEntry>();

        var bars = new List<BarEntry>();
        foreach (var barDir in Directory.EnumerateDirectories(SongsRoot))
        {
            var barName = Path.GetFileName(barDir);
            var songs = new List<SongEntry>();

            foreach (var jsonPath in Directory.EnumerateFiles(barDir, "*.json"))
            {
                var song = SongLoader.Load(jsonPath);
                var audioName = Path.GetFileNameWithoutExtension(song.AudioFile);
                var audioAssetPath = $"Songs/{barName}/{audioName}";
                songs.Add(new SongEntry(song.Title, song.Artist, jsonPath, audioAssetPath));
            }

            bars.Add(new BarEntry(barName, songs));
        }
        return bars;
    }
}
