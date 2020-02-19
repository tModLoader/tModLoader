using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Engine;

namespace Terraria.ModLoader.IO
{
	public class ImageIO
	{
		public const int VERSION = 1;

		public static bool ToRaw(Stream src, Stream dst) {
			using (var img = new Bitmap(src)) {
				// now that the hi-def profile is always enabled, the max texture size is 4096
				// if we get bug reports with old graphics cards forcing fallback to Reach and failing to load
				// large textures, we can implement a slower path where the rawimg is converted to png before loading

				//if (img.Width > 2048 || img.Height > 2048)
				//	return false;

				var bitmapData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
				var rawdata = new int[img.Width * img.Height];
				Marshal.Copy(bitmapData.Scan0, rawdata, 0, rawdata.Length);
				var w = new BinaryWriter(dst);
				w.Write(VERSION);
				w.Write(img.Width);
				w.Write(img.Height);
				foreach (int c in rawdata) {
					//Bitmap is in ABGR
					int a = c >> 24 & 0xFF;
					int b = c >> 16 & 0xFF;
					int g = c >> 8 & 0xFF;
					int r = c >> 0 & 0xFF;

					//special note, mirror XNA behaviour of zeroing out textures with full alpha zero
					//this means that an author doesn't have to set their fully transparent pixels to black
					//if they want additive blending they need to use alpha 1/255
					if (a == 0) {
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

		public static byte[] ToRawBytes(Stream src) {
			using (var ms = new MemoryStream()) {
				return ToRaw(src, ms) ? ms.ToArray() : null;
			}
		}

		public static Texture2D RawToTexture2D(GraphicsDevice graphicsDevice, Stream src) =>
			RawToTexture2D(graphicsDevice, new BinaryReader(src, Encoding.UTF8));

		public static void RawToPng(Stream src, Stream dst) {
			using (var img = RawToTexture2D(Main.instance.GraphicsDevice, src))
				img.SaveAsPng(dst, img.Width, img.Height);
		}

		public static Tuple<int, int, byte[]> ReadRaw(Stream src) =>
			ReadRaw(new BinaryReader(src, Encoding.UTF8));

		public static Tuple<int, int, byte[]> ReadRaw(BinaryReader r) {
			int v = r.ReadInt32();
			if (v != VERSION)
				throw new Exception("Unknown RawImg Format Version: " + v);

			int width = r.ReadInt32();
			int height = r.ReadInt32();
			var rawdata = r.ReadBytes(width * height * 4);
			return new Tuple<int, int, byte[]>(width, height, rawdata);
		}

		public static Texture2D RawToTexture2D(GraphicsDevice graphicsDevice, BinaryReader r) {
			var rawData = ReadRaw(r);
			var tex = new Texture2D(graphicsDevice, rawData.Item1, rawData.Item2);
			tex.SetData(rawData.Item3);
			return tex;
		}

		public static Task<Texture2D> RawToTexture2DAsync(GraphicsDevice graphicsDevice, BinaryReader r) {
			var rawData = ReadRaw(r);
			return GLCallLocker.InvokeAsync(() => {
				var tex = new Texture2D(graphicsDevice, rawData.Item1, rawData.Item2);
				tex.SetData(rawData.Item3);
				return tex;
			});
		}

		public static Task<Texture2D> PngToTexture2DAsync(GraphicsDevice graphicsDevice, Stream stream) {
#if XNA
			if (!(stream is MemoryStream)) {
				var ms = new MemoryStream((int)stream.Length);
				stream.CopyTo(ms);
				ms.Position = 0;
				stream = ms;
			}
			return GLCallLocker.InvokeAsync(() => Texture2D.FromStream(graphicsDevice, stream));
#else
			Texture2D.TextureDataFromStreamEXT(stream, out int width, out int height, out byte[] rawdata, -1, -1, false);
			return GLCallLocker.InvokeAsync(() => {
				var tex = new Texture2D(graphicsDevice, width, height);
				tex.SetData(rawdata);
				return tex;
			});
#endif
		}
	}
}