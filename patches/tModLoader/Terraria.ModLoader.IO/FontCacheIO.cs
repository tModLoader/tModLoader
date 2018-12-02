using System.IO;

namespace Terraria.ModLoader.IO
{
	internal static class FontCacheIO
	{
		public static readonly string FontCachePath = "Content" + Path.DirectorySeparatorChar + "Fonts" + Path.DirectorySeparatorChar + "ModFonts";

		internal static bool FontCacheAvailable(string fontCachePath) {
			if (File.Exists(FontCachePath + Path.DirectorySeparatorChar + fontCachePath)) {
				return true;
			}
			return false;
		}

		internal static void DeleteIfOlder(string modFilename, string fontCacheFilename) {
			FileInfo modFile = new FileInfo(modFilename);
			var dir = Directory.CreateDirectory(FontCachePath);
			foreach (var file in dir.EnumerateFiles(Path.GetFileNameWithoutExtension(fontCacheFilename.Substring(0, fontCacheFilename.LastIndexOf('_'))) + "_*.xnb")) {
				if (file.Name == fontCacheFilename) {
					if (file.LastWriteTime < modFile.LastWriteTime) {
						file.Delete();
					}
				}
				else {
					file.Delete();
				}
			}
		}
	}
}
