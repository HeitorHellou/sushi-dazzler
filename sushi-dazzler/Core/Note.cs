namespace SushiDazzler.Core;

public enum NoteType
{
    Tap,
    Hold
}

public class Note
{
    public float Beat { get; set; }
    public NoteType Type { get; set; }
    public float Duration { get; set; }
    public char Key { get; set; }  // A, S, D, F, J, K, L
}
