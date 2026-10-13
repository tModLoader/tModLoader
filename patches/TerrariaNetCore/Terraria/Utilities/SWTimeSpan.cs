using System.Diagnostics;

namespace Terraria.Utilities;

// TML#5330:
/// <summary>
/// A minimal TimeSpan that operates on the same level of precision as a <see cref="Stopwatch" />.
/// <br/> Added by the <b>TerrariaNetCore</b> stage. See https://github.com/tModLoader/tModLoader/issues/5330 for details.
/// </summary>
public readonly struct SWTimeSpan(long swTicks)
{
    public readonly long Ticks = swTicks;

    public readonly double TotalSeconds => Ticks / (double)Stopwatch.Frequency;
    public readonly double TotalMilliseconds => Ticks / (double)(Stopwatch.Frequency * 1000);

    public static SWTimeSpan operator -(SWTimeSpan a, SWTimeSpan b) => new(a.Ticks - b.Ticks);
    public static SWTimeSpan operator +(SWTimeSpan a, SWTimeSpan b) => new(a.Ticks + b.Ticks);
}