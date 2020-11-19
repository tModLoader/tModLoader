using ReLogic.OS;
using Steamworks;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Terraria.Localization;
using Terraria.Social;

namespace Terraria.ModLoader.Engine
{
	internal static class InstallVerifier
	{
		const string ContentDirectory = "Content";

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
				gogHash = ToByteArray("492dc0ff718d48936aa892e84a045e08");
				steamHash = ToByteArray("6a21906ef7681d7d9c40ef61af620974");
			}
			else if (Platform.IsOSX) {
				steamAPIPath = "osx/libsteam_api.dylib";
				steamAPIHash = ToByteArray("4EECD26A0CDF89F90D4FF26ECAD37BE0");
				gogHash = ToByteArray("77aa5e6bba5276a012a4b5be87229d92");
				steamHash = ToByteArray("4784728e5a73e930ad49f9eb7da45ca1");
			}
			else if (Platform.IsLinux) {
				steamAPIPath = "lib/libsteam_api.so";
				steamAPIHash = ToByteArray("7B74FD4C207D22DB91B4B649A44467F6");
				gogHash = ToByteArray("535a6f1705f145efc237711ffe458481");
				steamHash = ToByteArray("758de120bc80c26bdb5e5a78134a652d");
			}
			else {
				string message = Language.GetTextValue("tModLoader.UnknownVerificationOS");
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
		private static void Exit(string errorMessage, string extraMessage) {
			errorMessage += $"\r\n\r\n{extraMessage}";
			Logging.tML.Fatal(errorMessage);
			UI.Interface.MessageBoxShow(errorMessage);
			Environment.Exit(1);
		}

		private static bool InstallCheck() {
#if CLIENT
			// Check if the content directory is present which is required
			if (!Directory.Exists(ContentDirectory)) {
				Exit(Language.GetTextValue("tModLoader.ContentFolderNotFoundInstallCheck", ContentDirectory), Language.GetTextValue("tModLoader.DefaultExtraMessage"));
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
			string terrariaContentLocation = Path.Combine(terrariaInstallLocation, ContentDirectory);
#if MAC
			terrariaContentLocation = Path.Combine(terrariaInstallLocation, "../Resources/Content");
#endif

			if (!Directory.Exists(terrariaContentLocation)) {
				Exit(Language.GetTextValue("tModLoader.VanillaSteamInstallationNotFound"), Language.GetTextValue("tModLoader.DefaultExtraMessage"));
				return false;
			}
#endif
			if (!HashMatchesFile(steamAPIPath, steamAPIHash)) {
				Process.Start(@"https://terraria.org");
				Exit(Language.GetTextValue("tModLoader.SteamAPIHashMismatch"), string.Empty);
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
			string CheckExe = $"Terraria_1.4.1.2.exe"; // {Main.versionNumber}
			string vanillaPath = File.Exists(CheckExe) ? CheckExe : DefaultExe;

			// If .exe not present, check Terraria directory (Side-by-Side Manual Install)
			if (!File.Exists(vanillaPath)) {
				vanillaPath = Path.Combine("..", "Terraria");
#if MAC
				vanillaPath = "../../../../Terraria/Terraria.app/Contents/MacOS/";
#endif
				string defaultExe = Path.Combine(vanillaPath, DefaultExe);
				string checkExe = Path.Combine(vanillaPath, CheckExe);
				vanillaPath = File.Exists(defaultExe) ? defaultExe : checkExe;
			}
			// If .exe not present check parent directory (Nested Manual Install)
			if (!File.Exists(vanillaPath)) {
				string defaultExe = Path.Combine("..", DefaultExe);
				string checkExe = Path.Combine("..", CheckExe);
				vanillaPath = File.Exists(defaultExe) ? defaultExe : checkExe;
			}

			if (!File.Exists(vanillaPath)) {
#if SERVER
				return false;
#else
				Exit(Language.GetTextValue("tModLoader.VanillaGOGNotFound", vanillaPath, CheckExe), string.Empty);
				return false;
#endif
			}

			if (!HashMatchesFile(vanillaPath, gogHash) && !HashMatchesFile(vanillaPath, steamHash)) {
				Exit(Language.GetTextValue("tModLoader.GOGHashMismatch", vanillaPath), string.Empty);
				return false;
			}

			Logging.tML.Info("GOG or manual installation OK.");
			return true;
		}
	}
}