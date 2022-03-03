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
		public const string TmlContentDirectory = "Content";
		public const string SteamAppIDPath = "steam_appid.txt";
		private const string DefaultExe = "Terraria.exe";
		private static string CheckExe = $"Terraria_1.4.3.6.exe"; // This should match the hashes. {Main.versionNumber}
		public const bool RequireContentDirectory = false; // Not currently needed, due to tML matching vanilla's version.

		private static bool? isValid;
		public static bool IsValid => isValid ?? (isValid = InstallCheck()).Value;
		public static bool IsGoG = false;
		public static bool IsSteam = false;

		private static string steamAPIPath;
		private static string vanillaSteamAPI;
		private static byte[] steamAPIHash;
		private static byte[] gogHash;
		private static byte[] steamHash;

		static InstallVerifier() {
			if (Platform.IsWindows) {
				if (IntPtr.Size == 4) {
					steamAPIPath = "Libraries/Native/Windows32/steam_api.dll";
					steamAPIHash = ToByteArray("56d9f94d37cb8f03049a1cc3062bffaf");
				}
				else {
					steamAPIPath = "Libraries/Native/Windows/steam_api64.dll";
					steamAPIHash = ToByteArray("500475b20083ccdc64f12d238cab687a");
				}

				vanillaSteamAPI = "steam_api.dll";
				gogHash = ToByteArray("d05cf700a90fc12d7f9ef40f1d303b3a"); // Don't forget to update CheckExe above
				steamHash = ToByteArray("22e41c9960f3db473a036e93bbaec671");
			}
			else if (Platform.IsOSX) {
				steamAPIPath = "Libraries/Native/OSX/libsteam_api64.dylib";
				steamAPIHash = ToByteArray("801e9bf5e5899a41c5999811d870b1ca");
				vanillaSteamAPI = "libsteam_api.dylib";
				gogHash = ToByteArray("4946b4e30e40c3a238a1aaecf0829bd6");
				steamHash = ToByteArray("6b9b97670cc7cc922db77288d6ff0e88");
			}
			else if (Platform.IsLinux) {
				steamAPIPath = "Libraries/Native/Linux/libsteam_api64.so";
				steamAPIHash = ToByteArray("ccdf20f0b2f9abbe1fea8314b9fab096");
				vanillaSteamAPI = "libsteam_api.so";
				gogHash = ToByteArray("2bb44d560a3799caa34310a4d9ee8f89");
				steamHash = ToByteArray("c8112696dcdf53fe5a1a2810089f992b");
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

		private static byte[] ToByteArray(string hexString, bool forceLowerCase = true) {
			if (forceLowerCase) {
				hexString = hexString.ToLower();
			}

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
			// Check if the content directory is present, if it's required
			if (!Main.dedServ && RequireContentDirectory && !Directory.Exists(TmlContentDirectory)) {
				Exit(Language.GetTextValue("tModLoader.ContentFolderNotFoundInstallCheck", TmlContentDirectory), Language.GetTextValue("tModLoader.DefaultExtraMessage"));
				return false;
			}

			// If its clearly a steam install/launch, use Steam API.
			if (Directory.GetCurrentDirectory().Contains("steamapps", StringComparison.OrdinalIgnoreCase) || Program.LaunchParameters.ContainsKey("-steam"))
				return CheckSteam();

			Logging.tML.Info("Checking if GoG or Steam...");

			// If can't find a GoG folder, than assume it must be a Steam launch
			if (!ObtainVanillaExePath(out var steamApiPath, out var exePath))
				return CheckSteam();

			// Handle uniqueness of OSX installs. We want Terraria.app/Contents/MacOS/osx as the directory for steam_api file
			if (Platform.IsOSX)
				steamApiPath = Path.Combine(Directory.GetParent(steamApiPath).FullName, "MacOS", "osx");

			// If the steam_api file is present in the vanilla folder, than it is a Steam launch
			if (File.Exists(Path.Combine(steamApiPath, vanillaSteamAPI)))
				return CheckSteam();

			return CheckGoG(exePath);
		}

		private static bool ObtainVanillaExePath(out string vanillaPath, out string exePath) {
			// Check if in the same folder somehow.
			vanillaPath = Directory.GetCurrentDirectory();
			if (CheckForExe(vanillaPath, out exePath))
				return true;

			// If .exe not present check parent directory (Nested Manual Install)
			vanillaPath = Directory.GetParent(vanillaPath).FullName;
			if (CheckForExe(vanillaPath, out exePath))
				return true;

			// If .exe not present, check Terraria directory (Side-by-Side Manual Install)
			vanillaPath = Path.Combine(vanillaPath, "Terraria");
			if (Platform.IsOSX) {
				// GOG installs to /Applications/Terraria.app, Steam installs to /Applications/Terraria/Terraria.app
				// Vanilla .exe files are in /Contents/Resources/, not /Contents/MacOS/
				if (Directory.Exists("../Terraria/Terraria.app/")) {
					vanillaPath = "../Terraria/Terraria.app/Contents/Resources/";
					Logging.tML.Info($"Mac installation location found at {vanillaPath}, assuming Steam install");
				}
				else if (Directory.Exists("../Terraria.app/")) {
					vanillaPath = "../Terraria.app/Contents/Resources/";
					Logging.tML.Info($"Mac installation location found at {vanillaPath}, assuming GOG install");
				}
			}

			if (CheckForExe(vanillaPath, out exePath))
				return true;

			Logging.tML.Info($"No nearby installation location found. Presuming Steam");
			return false;
		}

		private static bool CheckForExe(string vanillaPath, out string exePath) {
			exePath = Path.Combine(vanillaPath, CheckExe);
			if (File.Exists(exePath))
				return true;

			exePath = Path.Combine(vanillaPath, DefaultExe);
			if (File.Exists(exePath))
				return true;

			return false;
		}

		// Check if Steam installation is correct
		private static bool CheckSteam() {
			Logging.tML.Info("Checking Steam installation...");
			IsSteam = true;
			if (!Main.dedServ) {
				SocialAPI.LoadSteam();
				string terrariaInstallLocation = Steam.GetSteamTerrariaInstallDir();
				string terrariaContentLocation = Path.Combine(terrariaInstallLocation, TmlContentDirectory);

				if (!Directory.Exists(terrariaContentLocation)) {
					Exit(Language.GetTextValue("tModLoader.VanillaSteamInstallationNotFound"), Language.GetTextValue("tModLoader.DefaultExtraMessage"));
					return false;
				}
			}

			if (!HashMatchesFile(steamAPIPath, steamAPIHash)) {
				Utils.OpenToURL("https://terraria.org");
				Exit(Language.GetTextValue("tModLoader.SteamAPIHashMismatch"), string.Empty);
				return false;
			}

			Logging.tML.Info("Steam installation OK.");
			return true;
		}

		// Check if GOG install is correct
		private static bool CheckGoG(string vanillaExePath) {
			Logging.tML.Info("Checking GOG installation...");
			IsGoG = true;

			if (!File.Exists(vanillaExePath)) {
				if(Main.dedServ)
					return false;

				Exit(Language.GetTextValue("tModLoader.VanillaGOGNotFound", vanillaExePath, CheckExe), string.Empty);
				return false;
			}

			if (!HashMatchesFile(vanillaExePath, gogHash) && !HashMatchesFile(vanillaExePath, steamHash)) {
				Exit(Language.GetTextValue("tModLoader.GOGHashMismatch", vanillaExePath), string.Empty);
				return false;
			}

			if (Path.GetFileName(vanillaExePath) != CheckExe) {
				string pathToCheckExe = Path.Combine(Path.GetDirectoryName(vanillaExePath), CheckExe);
				Logging.tML.Info($"Backing up {Path.GetFileName(vanillaExePath)} to {CheckExe}");
				File.Copy(vanillaExePath, pathToCheckExe);
			}

			Logging.tML.Info("GOG installation OK.");
			return true;
		}
	}
}