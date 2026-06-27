using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SushiDazzler.Core;

public static class SongLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static Song Load(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<Song>(json, Options)
            ?? throw new InvalidDataException($"Failed to parse song from {path}");
    }

    /// <summary>
    /// Attempts to load a chart, returning false instead of throwing on a missing or
    /// malformed file. Used by catalog discovery so one bad chart can't crash the game.
    /// </summary>
    public static bool TryLoad(string path, out Song song)
    {
        try
        {
            song = Load(path);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: could not load chart '{path}': {ex.Message}");
            song = null;
            return false;
        }
    }
}
