using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;

namespace tModLoader.BuildTools.ModFile;

public static class ContentConverters
{
	private const int Version = 1;

	public static bool Convert(ref string resourceName, FileStream src, MemoryStream dst)
	{
		switch (Path.GetExtension(resourceName).ToLower()) {
			case ".png":
				if (resourceName != "icon.png") {
					ToRaw(src, dst);
					resourceName = Path.ChangeExtension(resourceName, "rawimg");
					return true;
				}

				src.Position = 0;
				return false;
			default:
				return false;
		}
	}

	public static void ToRaw(Stream src, Stream dst)
	{
		using Image<Rgba32> image = Image.Load<Rgba32>(src);

		using BinaryWriter writer = new(dst);
		writer.Write(Version);
		writer.Write(image.Width);
		writer.Write(image.Height);

		for (int y = 0; y < image.Height; y++) {
			for (int x = 0; x < image.Width; x++) {
				Rgba32 color = image[x, y];

				if (color.A == 0) color = new Rgba32(0, 0, 0, 0);
				dst.WriteByte(color.R);
				dst.WriteByte(color.G);
				dst.WriteByte(color.B);
				dst.WriteByte(color.A);
			}
		}
	}
}