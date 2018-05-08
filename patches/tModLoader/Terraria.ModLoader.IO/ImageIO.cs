using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria.ModLoader.IO
{
	public class ImageIO
	{
		public const int VERSION = 1;

		public static bool ToRaw(Stream src, Stream dst)
		{
			using (var img = new Bitmap(src))
			{
				//XNA has a strange interaction where large size PNGs can be loaded, but not created via any other means
				if (img.Width > 2048 || img.Height > 2048)
					return false;

				var bitmapData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
				var rawdata = new int[img.Width * img.Height];
				Marshal.Copy(bitmapData.Scan0, rawdata, 0, rawdata.Length);
				var w = new BinaryWriter(dst);
				w.Write(VERSION);
				w.Write(img.Width);
				w.Write(img.Height);
				foreach (int c in rawdata)
				{
					//Bitmap is in ABGR
					int a = c >> 24 & 0xFF;
					int b = c >> 16 & 0xFF;
					int g = c >> 8 & 0xFF;
					int r = c >> 0 & 0xFF;

					//special note, mirror XNA behaviour of zeroing out textures with full alpha zero
					//this means that an author doesn't have to set their fully transparent pixels to black
					//if they want additive blending they need to use alpha 1/255
					if (a == 0)
					{
						w.Write(0);
						continue;
					}

					//write ARGB, note that the texture is assumed pre-multiplied, allowing for extra blending effects
					w.Write((byte)b);
					w.Write((byte)g);
					w.Write((byte)r);
					w.Write((byte)a);
				}
				return true;
			}
		}

		public static byte[] ToRawBytes(Stream src)
		{
			using (var ms = new MemoryStream())
			{
				return ToRaw(src, ms) ? ms.ToArray() : null;
			}
		}

		public static Texture2D RawToTexture2D(GraphicsDevice graphicsDevice, Stream src) =>
			RawToTexture2D(graphicsDevice, new BinaryReader(src, Encoding.UTF8));

		public static Texture2D RawToTexture2D(GraphicsDevice graphicsDevice, BinaryReader r, bool async = false)
		{
			int v = r.ReadInt32();
			if (v != VERSION)
				throw new Exception("Unknown RawImg Format Version: " + v);

			int width = r.ReadInt32();
			int height = r.ReadInt32();
			var rawdata = r.ReadBytes(width * height * 4);
			var tex = new Texture2D(graphicsDevice, width, height);
			tex.SetData(rawdata);
			return tex;
		}

		public static Task<Texture2D> RawToTexture2DAsync(GraphicsDevice graphicsDevice, BinaryReader r)
		{
			int v = r.ReadInt32();
			if (v != VERSION)
				throw new Exception("Unknown RawImg Format Version: " + v);

			int width = r.ReadInt32();
			int height = r.ReadInt32();
			var rawdata = r.ReadBytes(width * height * 4);
			return Task.Factory.StartNew(() =>
			{
				var tex = new Texture2D(graphicsDevice, width, height);
				tex.SetData(rawdata);
				return tex;
			});
		}

		public static Task<Texture2D> PngToTexture2DAsync(GraphicsDevice graphicsDevice, Stream stream)
		{
#if WINDOWS
			return Task.Factory.StartNew(() => Texture2D.FromStream(graphicsDevice, stream));
#else
			Texture2D.TextureDataFromStreamEXT(stream, out int width, out int height, out byte[] rawdata, -1, -1, false);
			return Task.Factory.StartNew(() =>
			{
				var tex = new Texture2D(graphicsDevice, width, height);
				tex.SetData(rawdata);
				return tex;
			});
#endif
		}
	}
}
