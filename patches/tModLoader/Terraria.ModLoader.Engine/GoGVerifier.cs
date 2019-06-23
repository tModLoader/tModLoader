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

		private static void GetPlatformCheckInfo(out string steamAPIpath, out byte[] vanillaGoGhash) {
			if (Platform.IsWindows) {
				steamAPIpath = "steam_api.dll";
				vanillaGoGhash = new byte[] { 185, 47, 36, 43, 200, 116, 221, 219, 240, 143, 112, 86, 97, 179, 19, 169 };
			} 
			else if (Platform.IsOSX) {
				steamAPIpath = "osx/libCSteamworks";
				// hash not yet implemented, need someone with GoG for mac
				vanillaGoGhash = new byte[] { 148, 42, 176, 97, 232, 84, 199, 77, 179, 166, 177, 239, 226, 220, 36, 208 };
			} 
			else if (Platform.IsLinux) {
				steamAPIpath = "lib/libCSteamworks.so";
				vanillaGoGhash = new byte[] { 148, 42, 176, 97, 232, 84, 199, 77, 179, 166, 177, 239, 226, 220, 36, 208 };
			}
			else {
				throw new Exception("Platform??");
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
