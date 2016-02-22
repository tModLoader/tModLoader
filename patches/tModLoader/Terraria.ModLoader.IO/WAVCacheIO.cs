using System;
using System.IO;

namespace Terraria.ModLoader.IO
{
	internal static class WAVCacheIO
	{
		public static readonly string ModCachePath = Main.SavePath + Path.DirectorySeparatorChar + "Mods" + Path.DirectorySeparatorChar + "Cache";

		internal static bool WAVCacheAvailable(string wavCachePath)
		{
			if (File.Exists(ModCachePath + Path.DirectorySeparatorChar + wavCachePath))
			{
				return true;
			}
			return false;
		}

		internal static void SaveWavStream(MemoryStream output, string wavCachePath)
		{
			Directory.CreateDirectory(ModCachePath);
			using (FileStream fileStream = File.Create(ModCachePath + Path.DirectorySeparatorChar + wavCachePath))
			{
				output.WriteTo(fileStream);
			}
		}

		internal static Stream GetWavStream(string wavCachePath)
		{
			return File.OpenRead(ModCachePath + Path.DirectorySeparatorChar + wavCachePath);
		}

		internal static void ClearCache(string modName)
		{
			var dir = Directory.CreateDirectory(ModCachePath);
			foreach (var file in dir.EnumerateFiles(Path.GetFileNameWithoutExtension(modName) + "_*.wav"))
			{
				file.Delete();
			}
		}

		internal static void DeleteIfOlder(string modFilename, string wavCacheFilename)
		{
			FileInfo modFile = new FileInfo(modFilename);
			FileInfo wavFile = new FileInfo(ModCachePath + Path.DirectorySeparatorChar + wavCacheFilename);
			if (wavFile.Exists)
			{
				if (wavFile.LastWriteTime < modFile.LastWriteTime)
				{
					wavFile.Delete();
				}
			}
		}
	}
}
