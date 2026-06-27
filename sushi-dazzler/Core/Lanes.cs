using Microsoft.Xna.Framework.Input;

namespace SushiDazzler.Core;

/// <summary>
/// Canonical set of playable lanes — the keyboard key and the chart note character it maps to.
/// Single source of truth for "what counts as a lane" shared by input handling and charts.
/// </summary>
public static class Lanes
{
    public static readonly (Keys Key, char Note)[] All =
    {
        (Keys.A, 'A'),
        (Keys.S, 'S'),
        (Keys.D, 'D'),
        (Keys.F, 'F'),
        (Keys.J, 'J'),
        (Keys.K, 'K'),
        (Keys.L, 'L'),
    };
}
