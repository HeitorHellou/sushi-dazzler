using System;
using System.Collections.Generic;
using System.IO;

namespace SushiDazzler.Core;

public record SongEntry(string Title, string Artist, string ChartPath, string AudioAssetPath, Song Song, string Key);

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
                // Skip (don't crash on) a chart that fails to parse — it never reaches gameplay.
                if (!SongLoader.TryLoad(jsonPath, out var song))
                    continue;

                var audioName = Path.GetFileNameWithoutExtension(song.AudioFile);
                var audioAssetPath = $"Songs/{barName}/{audioName}";
                // Stable identity for save data, independent of absolute path: e.g. "Yokohama/easy".
                var key = $"{barName}/{Path.GetFileNameWithoutExtension(jsonPath)}";
                songs.Add(new SongEntry(song.Title, song.Artist, jsonPath, audioAssetPath, song, key));
            }

            bars.Add(new BarEntry(barName, songs));
        }
        return bars;
    }
}
