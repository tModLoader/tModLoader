/* Unused class for future considerations?
using ReLogic.OS;
using Steamworks;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Terraria.Social;

namespace Terraria.ModLoader.Engine
{
	internal static class GoGVerifier
	{
		private static bool? isGoG;
		public static bool IsGoG => isGoG ?? (isGoG = GoGCheck()).Value;

		private static bool HashMatchesFile(byte[] hash, string path)
		{
			using (var md5 = MD5.Create())
			using (var stream = File.OpenRead(path))
				return hash.SequenceEqual(md5.ComputeHash(stream));
		}

		private static byte[] ToByteArray(string hexString)
		{
			byte[] retval = new byte[hexString.Length / 2];
			for (int i = 0; i < hexString.Length; i += 2)
				retval[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
			return retval;
		}

		private static void GetPlatformCheckInfo(out string steamAPIpath, out byte[] steamAPIhash, out byte[] vanillaGoGhash)
		{
			// I'd like it if you couldn't just open tML in a hex editor and replace the hash
			// but whether I make it byte array or string doesn't change that. I could obfuscate the hash a bit
			// but I can't really make it more effort than just nuking the steam check with dnspy (which could be done before we added GoG support)
			if (Platform.IsWindows) {
				steamAPIpath = "steam_api.dll";
				steamAPIhash = ToByteArray("73688FFCBC2E5F0980B055C1D93B2FB2");
				vanillaGoGhash = ToByteArray("81ef4a9337ae6d7a1698fdeb3137580d");
			}
			else if (Platform.IsOSX) {
				steamAPIpath = "osx/libsteam_api.dylib";
				steamAPIhash = ToByteArray("FB32124B2E07ED2AAE54FE8823D069B3");
				vanillaGoGhash = ToByteArray("e8dfb127721edc4ceb32381f41ece7b8");
			}
			else if (Platform.IsLinux) {
				steamAPIpath = "lib/libsteam_api.so";
				steamAPIhash = ToByteArray("8915306857EEF2A516956A2398845EA2");
				vanillaGoGhash = ToByteArray("942ab061e854c74db3a6b1efe2dc24d0");
			}
			else {
				throw new Exception("Unknown OS platform");
			}
		}

		private static bool GoGCheck()
		{
			GetPlatformCheckInfo(out var steamAPIpath, out var steamAPIhash, out var vanillaGoGhash);

			const string ContentDirectory = "Content";
			const string InstallInstructions = "Please restore your Terraria install, then install tModLoader on Steam or by following the README.txt instructions for manual installation.";

			void Exit(string errorMessage)
			{
				errorMessage += $"\r\n\r\n{InstallInstructions}";
				Logging.tML.Fatal(errorMessage);
				UI.Interface.MessageBoxShow(errorMessage);
				Environment.Exit(1);
			}

			// TODO These steam checks should not be here, but I'm afraid to move stuff around right now -Jof
			bool checkSteam = File.Exists(steamAPIpath);
#if !SERVER
			//if (!Directory.Exists(ContentDirectory)) {
			//	Exit($"{ContentDirectory} directory could not be found.\r\n\r\nDid you forget to extract tModLoader's Content directory into the tModLoader folder?\r\n\r\nEnsure tModLoader is installed in a separate folder from Terraria.");
			//	return false;
			//}
			if (checkSteam) {
				SocialAPI.LoadSteam();
				SteamApps.GetAppInstallDir(Steam.TerrariaAppId_t, out string terrariaInstallLocation, 1000);
				if (!Directory.Exists(Path.Combine(terrariaInstallLocation, ContentDirectory))) {
					Exit($"Terraria Steam installation or Terraria Content directory not found.\r\n\r\nPlease ensure Terraria 1.4 is installed through Steam.");
					return false;
				}
			}
#endif
			if (checkSteam) {
				VerifySteamAPI(steamAPIpath, steamAPIhash);
				return false;
			}
			Logging.tML.Info("GoG detected. Disabled steam check.");

			const string DefaultExe = "Terraria.exe";
			string CheckExe = $"Terraria_{Main.versionNumber}.exe";
			string vanillaPath = File.Exists(CheckExe) ? CheckExe : DefaultExe;

			if (!File.Exists(vanillaPath)) {
#if SERVER
				return false;
#else
				Exit($"{vanillaPath} could not be found.\r\n\r\nGoG installs must have the unmodified Terraria executable to function.");
				return false;
#endif
			}

			if (!HashMatchesFile(vanillaGoGhash, vanillaPath)) {
				Exit($"{vanillaPath} is not the unmodified Terraria executable.\r\n\r\nGoG installs must have the unmodified Terraria executable to function.");
				return false;
			}

			return true;
		}

		private static void VerifySteamAPI(string steamAPIpath, byte[] steamAPIhash)
		{
			if (!HashMatchesFile(steamAPIhash, steamAPIpath)) {
				string message = "Steam API hash mismatch, assumed pirated.\n\ntModLoader requires a legitimate Terraria install to work.";
				Logging.tML.Fatal(message);
				UI.Interface.MessageBoxShow(message);
				Process.Start(@"https://terraria.org");
				Environment.Exit(1);
			}
		}
	}
}
*/