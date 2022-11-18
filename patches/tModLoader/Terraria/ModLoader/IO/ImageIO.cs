using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace Terraria.ModLoader.IO;

public class ImageIO
{
	public const int VERSION = 1;

	public static unsafe bool ToRaw(Stream src, Stream dst)
	{
		IntPtr img = FNA3D.ReadImageStream(src, out int width, out int height, out int len);
		if (img == IntPtr.Zero)
			return false; // throw exception?

		byte* colors = (byte*)img.ToPointer();

		using var w = new BinaryWriter(dst);
		w.Write(VERSION);
		w.Write(width);
		w.Write(height);

		for (int i = 0; i < len; i += 4) {
			//special note, mirror XNA behaviour of zeroing out textures with full alpha zero
			//this means that an author doesn't have to set their fully transparent pixels to black
			//if they want additive blending they need to use alpha 1/255
			if (colors[i + 3] == 0) {
				w.Write(0);
				continue;
			}

			w.Write(colors[i]);
			w.Write(colors[i + 1]);
			w.Write(colors[i + 2]);
			w.Write(colors[i + 3]);
		}

		FNA3D.FNA3D_Image_Free(img);
		return true;
	}

	public static unsafe void RawToPng(Stream src, Stream dst)
	{
		byte[] data = ReadRaw(src, out int width, out int height);
		fixed (byte* pixels = data)
			FNA3D.WritePNGStream(dst, width, height, width, height, (IntPtr)pixels);
	}

	public static byte[] ReadRaw(Stream stream, out int width, out int height)
	{
		using var r = new BinaryReader(stream);

		int v = r.ReadInt32();
		if (v != VERSION)
			throw new Exception("Unknown RawImg Format Version: " + v);

		width = r.ReadInt32();
		height = r.ReadInt32();
		return r.ReadBytes(width * height * 4);
	}
}