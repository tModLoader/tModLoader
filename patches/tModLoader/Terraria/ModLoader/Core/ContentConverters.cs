using System;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Core;

internal static class ContentConverters
{
	// Keep as PNG so they can be fetched easily from the modfile without loading the mod
	private static List<String> keepAsPng = ["icon.png", "icon_small.png"];

	internal static bool Convert(ref string resourceName, FileStream src, MemoryStream dst)
	{
		switch (Path.GetExtension(resourceName).ToLower()) {
			case ".png":
				if (!keepAsPng.Contains(resourceName) && ImageIO.ToRaw(src, dst)) {
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
