using ReLogic.OS;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace Terraria.ModLoader.Engine
{
	internal static class GoGVerifier
	{
		private static bool? isGoG;
		public static bool IsGoG => isGoG ?? (isGoG = GoGCheck()).Value;

		private static bool HashMatchesFile(byte[] hash, string path) {
			using (var md5 = MD5.Create())
			using (var stream = File.OpenRead(path))
				return hash.SequenceEqual(md5.ComputeHash(stream));
		}

		private static byte[] ToByteArray(string hexString) {
			byte[] retval = new byte[hexString.Length / 2];
			for (int i = 0; i < hexString.Length; i += 2)
				retval[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
			return retval;
		}

		private static void GetPlatformCheckInfo(out string steamAPIpath, out byte[] vanillaGoGhash) {
			// I'd like it if you couldn't just open tML in a hex editor and replace the hash
			// but whether I make it byte array or string doesn't change that. I could obfuscate the hash a bit
			// but I can't really make it more effort than just nuking the steam check with dnspy (which could be done before we added GoG support)
			if (Platform.IsWindows) {
				steamAPIpath = "steam_api.dll";
				vanillaGoGhash = ToByteArray("81ef4a9337ae6d7a1698fdeb3137580d");
			}
			else if (Platform.IsOSX) {
				steamAPIpath = "osx/libCSteamworks";
				vanillaGoGhash = ToByteArray("e8dfb127721edc4ceb32381f41ece7b8");
			}
			else if (Platform.IsLinux) {
				steamAPIpath = "lib/libCSteamworks.so";
				vanillaGoGhash = ToByteArray("942ab061e854c74db3a6b1efe2dc24d0");
			}
			else {
				throw new Exception("Unknown OS platform");
			}
		}

		private static bool GoGCheck() {
			GetPlatformCheckInfo(out var steamAPIpath, out var vanillaGoGhash);
			if (File.Exists(steamAPIpath)) {
				VerifySteamAPI();
				return false;
			}

			var vanillaPath = Path.GetFileName(Assembly.GetExecutingAssembly().Location) != "Terraria.exe" ? "Terraria.exe" : "Terraria_v1.3.5.3.exe";
			if (!File.Exists(vanillaPath)) {
				Logging.tML.Info("Vanilla Terraria.exe not found.");
				return false;
			}

			if (!HashMatchesFile(vanillaGoGhash, vanillaPath))
				return false;

			Logging.tML.Info("GoG detected. Disabled steam check.");
			return true;
		}

		private static void VerifySteamAPI() {
			if (!Platform.IsWindows)
				return;

			var steamAPIhash = new byte[] { 123, 133, 124, 137, 123, 198, 147, 19, 228, 147, 109, 195, 220, 206, 81, 147 };
			if (!HashMatchesFile(steamAPIhash, "steam_api.dll")) {
				Logging.tML.Fatal("Steam API hash mismatch, assumed pirated");
				Environment.Exit(1);
			}
		}
	}
}
