using System;
using System.IO;

namespace Terraria.ModLoader.IO
{
	public static class ContentConverters
	{
		internal static bool Convert(ref string resourceName, FileStream src, MemoryStream dst) {
			switch (Path.GetExtension(resourceName)) {
				case ".png":
					if (resourceName != "icon.png" && ImageIO.ToRaw(src, dst)) {
						resourceName = Path.ChangeExtension(resourceName, "rawimg");
						return true;
					}
					return false;
				default:
					return false;
			}
		}

		internal static bool Reverse(ref string resourceName, out Action<Stream, Stream> converter) {
			if(resourceName == "Info") {
				resourceName = "build.txt";
				converter = BuildProperties.InfoToBuildTxt;
				return true;
			}
			switch (Path.GetExtension(resourceName)) {
				case ".rawimg":
					resourceName = Path.ChangeExtension(resourceName, "png");
					converter = ImageIO.RawToPng;
					return true;
				default:
					converter = null;
					return false;
			}
		}
	}
}
