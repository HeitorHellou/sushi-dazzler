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
}
