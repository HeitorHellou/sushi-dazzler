using System;
using System.IO;
using System.Text.Json;

namespace SushiDazzler.Core;

/// <summary>
/// Loads and saves <see cref="SaveData"/> as JSON in the user's app-data folder.
/// File IO is best-effort: a missing or corrupt save starts fresh rather than crashing.
/// </summary>
public class SaveManager
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private readonly string _path;
    public SaveData Data { get; private set; } = new();

    public SaveManager(string path) => _path = path;

    public static SaveManager Default()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SushiDazzler");
        return new SaveManager(Path.Combine(dir, "progress.json"));
    }

    public void Load()
    {
        try
        {
            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                Data = JsonSerializer.Deserialize<SaveData>(json, Options) ?? new SaveData();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: could not read save '{_path}': {ex.Message}. Starting fresh.");
            Data = new SaveData();
        }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonSerializer.Serialize(Data, Options));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: could not write save '{_path}': {ex.Message}");
        }
    }

    /// <summary>Submits a result, persists immediately, and reports whether it was a new best.</summary>
    public bool SubmitResult(string key, int score, int stars)
    {
        bool isNewBest = Data.SubmitResult(key, score, stars);
        Save();
        return isNewBest;
    }
}
