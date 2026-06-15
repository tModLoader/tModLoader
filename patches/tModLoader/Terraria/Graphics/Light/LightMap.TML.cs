#nullable enable

using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria.Graphics.Light;

partial class LightMap
{
	private Texture2D? bufferTexture;
	private bool dirtyBuffer;
	private Rectangle tileArea;

	public void MarkDirty()
	{
		dirtyBuffer = true;
	}

	public void UpdateArea(Rectangle area)
	{
		tileArea = area;
		dirtyBuffer = true;
	}

	public unsafe LightMapBuffer GetBufferTexture()
	{
		var width = Width + 1;
		var height = Height + 1;

		if (bufferTexture is null) {
			bufferTexture = InitBufferTexture(width, height);
			dirtyBuffer = true;
		}
		else if (bufferTexture.Width != width || bufferTexture.Height != height) {
			bufferTexture?.Dispose();
			bufferTexture = InitBufferTexture(width, height);
			dirtyBuffer = true;
		}

		if (dirtyBuffer) {
			fixed(Vector4* pColors = &_colors[0]) {
				bufferTexture.SetDataPointerEXT(0, null, (nint)pColors, width * height * sizeof(Vector4));
			}

			dirtyBuffer = false;
		}

		return new LightMapBuffer
		{
			Texture = bufferTexture,
			TileArea = tileArea
		};
	}

	private static Texture2D InitBufferTexture(int width, int height)
	{
		return new Texture2D(Main.instance.GraphicsDevice, width, height, mipMap: false, format: SurfaceFormat.Vector4);
	}

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
