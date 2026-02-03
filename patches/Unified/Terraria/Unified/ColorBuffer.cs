using System;
using System.Runtime.CompilerServices;

namespace Terraria.Unified;

internal static class ColorBuffer
{
	private static readonly (int x, int y)[] plus_offsets = [
		(+0, -1),
		(+0, +1),
		(-1, +0),
		(+1, +0),
	];

	private static readonly (int x, int y)[] square_offsets = [
		(-1, -1),
		(+0, -1),
		(+1, -1),
		(-1, +0),
		(+0, +0),
		(+1, +0),
		(-1, +1),
		(+0, +1),
		(+1, +1),
	];

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GetPlus(
		int x,
		int y,
		Span<Vector3> colors
	)
	{
		GetBuffer(x, y, colors, plus_offsets);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GetSquare(
		int x,
		int y,
		Span<Vector3> colors
	)
	{
		GetBuffer(x, y, colors, square_offsets);
	}

	private static void GetBuffer(
		int x,
		int y,
		Span<Vector3> colors,
		(int x, int y)[] offsets
	)
	{
		var export = Lighting.EngineExport;

		if (export.Area.Contains(x - 1, y - 1)
		 && export.Area.Contains(x + 1, y + 1)) {
			for (var i = 0; i < offsets.Length; i++) {
				var offset = offsets[i];
				var localX = x + offset.x;
				var localY = y + offset.y;

				colors[i] = export.GetColorUnsafe(localX, localY);
			}
		}
		else {
			for (var i = 0; i < offsets.Length; i++) {
				var offset = offsets[i];
				var localX = x + offset.x;
				var localY = y + offset.y;

				colors[i] = export.GetColor(localX, localY);
			}
		}
	}
}
