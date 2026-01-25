using System.Collections.Generic;

namespace SushiDazzler.Core;

public class Song
{
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public float BPM { get; set; }
    public string AudioFile { get; set; } = string.Empty;
    public float Offset { get; set; }
    public List<Note> Notes { get; set; } = new();
}
