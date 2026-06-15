using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Terraria.Graphics.Light;

partial class LightMap
{
	// PERF: If we ever need these to be faster:
	// private static Vector3 ToVector3(in Vector4 value)
	//     return *(Vector3*)&value;
	//
	// private static void FromVector3(in Vector3 value, ref Vector4 dst)
	//   *(Vector3*)&dst = value;
	//
	// The reason I don't currently do this is it relies on the memory layout
	// of Vector3 and Vector4 to be Explicit or Sequential, which it currently
	// isn't.  (In practice, this would probably work fine on 64-bit machines).
	//
	// As it currently stands, I don't notice a massive performance impact, so
	// the JIT likely handles these cases quite well already.

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Vector3 ToVector3(in Vector4 value)
	{
		return new Vector3(value.X, value.Y, value.Z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Vector4 FromVector3(in Vector3 value)
	{
		return new Vector4(value.X, value.Y, value.Z, 1f);
	}
}
