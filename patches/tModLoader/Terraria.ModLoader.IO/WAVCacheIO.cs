using System;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework.Audio;

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
			var mod = ModLoader.GetMod(modName);
			if (mod != null)
			{
				foreach (var item in mod.musics)
				{
					item.Value.Dispose();
				}
			}
			var dir = Directory.CreateDirectory(ModCachePath);
			foreach (var file in dir.EnumerateFiles(Path.GetFileNameWithoutExtension(modName) + "_*.wav"))
			{
				file.Delete();
			}
		}

		internal static void DeleteIfOlder(string modFilename, string wavCacheFilename)
		{
			FileInfo modFile = new FileInfo(modFilename);
			var dir = Directory.CreateDirectory(ModCachePath);
			foreach (var file in dir.EnumerateFiles(Path.GetFileNameWithoutExtension(wavCacheFilename.Substring(0, wavCacheFilename.LastIndexOf('_'))) + "_*.wav"))
			{
				if (file.Name == wavCacheFilename)
				{
					if (file.LastWriteTime < modFile.LastWriteTime)
					{
						file.Delete();
					}
				}
				else
				{
					file.Delete();
				}
			}
		}

		public static SoundEffect CacheMP3(string wavCacheFilename, byte[] data)
		{
			ushort nChannels;
			uint nSamplesPerSec;
			uint nAvgBytesPerSec;
			ushort nBlockAlign;
			const ushort wFormatTag = 1;
			const ushort wBitsPerSample = 16;
			const int headerSize = 44;

			using (MemoryStream output = new MemoryStream(),
								datastream = new MemoryStream(data))
			using (var input = new MP3Sharp.MP3Stream(datastream))
			using (var writer = new BinaryWriter(output, Encoding.UTF8))
			{
				output.Position = headerSize;
				input.CopyTo(output);
				uint wavDataLength = (uint)output.Length - headerSize;
				output.Position = 0;
				nChannels = (ushort)input.ChannelCount;
				nSamplesPerSec = (uint)input.Frequency;
				nBlockAlign = (ushort)(nChannels * (wBitsPerSample / 8));
				nAvgBytesPerSec = (uint)(nSamplesPerSec * nChannels * (wBitsPerSample / 8));
				//write the header
				writer.Write("RIFF".ToCharArray()); //4
				writer.Write((uint)(wavDataLength + 36)); // 4
				writer.Write("WAVE".ToCharArray()); //4
				writer.Write("fmt ".ToCharArray()); //4
				writer.Write(16); //4
				writer.Write(wFormatTag); //
				writer.Write((ushort)nChannels);
				writer.Write(nSamplesPerSec);
				writer.Write(nAvgBytesPerSec);
				writer.Write(nBlockAlign);
				writer.Write(wBitsPerSample);
				writer.Write("data".ToCharArray());
				writer.Write((uint)wavDataLength);
				output.Position = 0;
				SaveWavStream(output, wavCacheFilename);
				return SoundEffect.FromStream(output);
			}
		}
	}
}
