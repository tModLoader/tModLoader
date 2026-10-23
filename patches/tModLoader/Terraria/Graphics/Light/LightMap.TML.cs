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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Vector3 ToVector3(ref Vector4 value)
	{
		return Unsafe.As<Vector4, Vector3>(ref value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void FromVector3(Vector3 src, ref Vector4 dst)
	{
		Unsafe.As<Vector4, Vector3>(ref dst) = src;
	}
}
