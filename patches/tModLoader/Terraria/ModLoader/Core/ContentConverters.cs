using System;
using System.IO;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Core;

internal static class ContentConverters
{
	internal static bool Convert(ref string resourceName, FileStream src, MemoryStream dst)
	{
		switch (Path.GetExtension(resourceName).ToLower()) {
			case ".png":
				if (resourceName != "icon.png" && ImageIO.ToRaw(src, dst)) {
					resourceName = Path.ChangeExtension(resourceName, "rawimg");
					return true;
				}
				src.Position = 0;
				return false;
			default:
				return false;
		}
	}

	internal static bool Reverse(ref string resourceName, out Action<Stream, Stream> converter)
	{
		if(resourceName == "Info") {
			resourceName = "build.txt";
			converter = BuildProperties.InfoToBuildTxt;
			return true;
		}
		switch (Path.GetExtension(resourceName).ToLower()) {
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
