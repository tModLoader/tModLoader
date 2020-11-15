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
	internal static class InstallVerifier
	{
		const string ContentDirectory = "Content";
		const string InstallInstructions = "Please restore your Terraria install, then install tModLoader on Steam or by following the README.txt instructions for manual installation.";

		private static bool? isValid;
		public static bool IsValid => isValid ?? (isValid = InstallCheck()).Value;
		public static bool IsGoG = false;
		public static bool IsSteam = false;

		private static string steamAPIPath;
		private static byte[] steamAPIHash;
		private static byte[] gogHash;
		private static byte[] steamHash;

		static InstallVerifier() {
			if (Platform.IsWindows) {
				steamAPIPath = "steam_api.dll";
				steamAPIHash = ToByteArray("7B857C897BC69313E4936DC3DCCE5193");
				gogHash = ToByteArray("329296a600b71a4684e5e40a1899124b");
				steamHash = ToByteArray("17c42ca2c951a7a8a53ddf3a0ca00290");
			}
			else if (Platform.IsOSX) {
				steamAPIPath = "osx/libsteam_api.dylib";
				steamAPIHash = ToByteArray("4EECD26A0CDF89F90D4FF26ECAD37BE0");
				gogHash = ToByteArray("c9ac52c4b6deaf3aba63d6878ff1a38b");
				steamHash = ToByteArray("6d256ff00b438884d79bab2a68d19937");
			}
			else if (Platform.IsLinux) {
				steamAPIPath = "lib/libsteam_api.so";
				steamAPIHash = ToByteArray("7B74FD4C207D22DB91B4B649A44467F6");
				gogHash = ToByteArray("61bcb380ba3408ab664bc6fac3364faf");
				steamHash = ToByteArray("d5a43f1fd4823af91fdf7b2745248c61");
			}
			else {
				string message = "Unknown OS platform: unable to verify installation.";
				Logging.tML.Fatal(message);
				Exit(message, string.Empty);
			}
		}

		private static bool HashMatchesFile(string path, byte[] hash) {
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
		private static void Exit(string errorMessage, string extraMessage = InstallInstructions) {
			errorMessage += $"\r\n\r\n{extraMessage}";
			Logging.tML.Fatal(errorMessage);
			UI.Interface.MessageBoxShow(errorMessage);
			Environment.Exit(1);
		}

		private static bool InstallCheck() {
#if CLIENT
			// Check if the content directory is present which is required
			if (!Directory.Exists(ContentDirectory)) {
				Exit($"{ContentDirectory} directory could not be found.\r\n\r\nDid you forget to extract tModLoader's Content directory into the tModLoader folder?\r\n\r\nEnsure tModLoader is installed in a separate folder from Terraria.");
				return false;
			}
#endif
			// Whether the steam_api file exists, indicating we'd have to check steam installation
			if (File.Exists(steamAPIPath))
				return CheckSteam();

			return CheckGoG();
		}

		// Check if steam installation is correct
		private static bool CheckSteam() {
			Logging.tML.Info("Checking Steam installation...");
			IsSteam = true;
#if CLIENT
			SocialAPI.LoadSteam();
			string terrariaInstallLocation = Steam.GetSteamTerrariaInstallDir();

			if (!Directory.Exists(Path.Combine(terrariaInstallLocation, ContentDirectory))) {
				Exit($"Terraria Steam installation or Terraria Content directory not found.\r\n\r\nPlease ensure Terraria 1.4 is installed through Steam.");
				return false;
			}
#endif
			if (!HashMatchesFile(steamAPIPath, steamAPIHash)) {
				Process.Start(@"https://terraria.org");
				Exit("Steam API hash mismatch, assumed pirated.\n\ntModLoader requires a legitimate Terraria install to work.", "");
				return false;
			}

			Logging.tML.Info("Steam installation OK.");
			return true;
		}

		// Check if GOG install or manual install is correct
		private static bool CheckGoG() {
			Logging.tML.Info("Checking GOG or manual installation...");
			IsGoG = true;

			const string DefaultExe = "Terraria.exe";
			string CheckExe = $"Terraria_{Main.versionNumber}.exe";
			string vanillaPath = File.Exists(CheckExe) ? CheckExe : DefaultExe;

			// If .exe not present check Terraria dir in root
			if (!File.Exists(vanillaPath)) {
				vanillaPath = Path.Combine("..", "Terraria");
				string defaultExe = Path.Combine(vanillaPath, DefaultExe);
				string checkExe = Path.Combine(vanillaPath, CheckExe);
				vanillaPath = File.Exists(defaultExe) ? defaultExe : checkExe;
			}

			if (!File.Exists(vanillaPath)) {
#if SERVER
				return false;
#else
				Exit($"{vanillaPath} could not be found.\r\n\r\nGOG and manual installs must have the unmodified Terraria executable to function.", string.Empty);
				return false;
#endif
			}

			if (!HashMatchesFile(vanillaPath, gogHash) && !HashMatchesFile(vanillaPath, steamHash)) {
				Exit($"{vanillaPath} is not the unmodified Terraria executable.\r\n\r\nGOG and manual installs must have the unmodified Terraria executable to function.\r\n\r\nIf you patched the .exe, you can create a copy of the original exe and name it \"Terraria_v<VERSION>.exe\"", string.Empty);
				return false;
			}

			Logging.tML.Info("GOG or manual installation OK.");
			return true;
		}
	}
}
