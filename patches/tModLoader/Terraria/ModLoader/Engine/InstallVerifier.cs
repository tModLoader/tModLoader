using ReLogic.OS;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Terraria.Localization;
using Terraria.Social;

namespace Terraria.ModLoader.Engine
{
	public enum DistributionPlatform
	{
		Unknown,
		Steam,
		GoG
	}

	internal static class InstallVerifier
	{
		private static string VanillaExe = "Terraria.exe";
		private static string CheckExe = $"Terraria_1.4.4.5.exe"; // This should match the hashes. {Main.versionNumber}
		private static string vanillaExePath;

		public static DistributionPlatform DistributionPlatform;

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
				gogHash = ToByteArray("cac2ec1ab87b856714e811197f2c5316"); // Don't forget to update CheckExe above
				steamHash = ToByteArray("827016a1be423c25e239c6da6c9ead16");
			}
			else if (Platform.IsOSX) {
				steamAPIPath = "Libraries/Native/OSX/libsteam_api64.dylib";
				steamAPIHash = ToByteArray("801e9bf5e5899a41c5999811d870b1ca");
				vanillaSteamAPI = "libsteam_api.dylib";
				gogHash = ToByteArray("045e27636950d04d1986307f294aac55");
				steamHash = ToByteArray("6259687c7098c2edf6200db831c02faa");
			}
			else if (Platform.IsLinux) {
				steamAPIPath = "Libraries/Native/Linux/libsteam_api64.so";
				steamAPIHash = ToByteArray("ccdf20f0b2f9abbe1fea8314b9fab096");
				vanillaSteamAPI = "libsteam_api.so";
				gogHash = ToByteArray("54467fc23e6f3122a62e461cf2fb2b18");
				steamHash = ToByteArray("b24b94fec7d20f9dba7329275443a859");
			}
			else {
				ErrorReporting.FatalExit(Language.GetTextValue("tModLoader.UnknownVerificationOS"));
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

		internal static void Startup() {
			DistributionPlatform = DetectPlatform(out string detectionDetails);
			Logging.tML.Info($"Distribution Platform: {DistributionPlatform}. Detection method: {detectionDetails}");

			if (DistributionPlatform == DistributionPlatform.GoG) {
				CheckGoG();
			}
			else {
				CheckSteam();
			}
		}

		private static DistributionPlatform DetectPlatform(out string detectionDetails) {
			if (Program.LaunchParameters.ContainsKey("-steam")) {
				detectionDetails = "-steam launch parameter";
				return DistributionPlatform.Steam;
			}

			if (Directory.GetCurrentDirectory().Contains("steamapps", StringComparison.OrdinalIgnoreCase)) {
				detectionDetails = "CWD is /steamapps/";
				return DistributionPlatform.Steam;
			}

			// If can't find a GoG folder, than assume it must be a Steam launch
			if (!ObtainVanillaExePath(out var vanillaSteamAPIDir, out vanillaExePath)) {
				detectionDetails = $"{VanillaExe} or {CheckExe} not found nearby";
				return DistributionPlatform.Steam;
			}

			// Handle uniqueness of OSX installs. We want Terraria.app/Contents/MacOS/osx as the directory for steam_api file
			if (Platform.IsOSX)
				vanillaSteamAPIDir = Path.Combine(Directory.GetParent(vanillaSteamAPIDir).FullName, "MacOS", "osx");

			// If the steam_api file is present in the vanilla folder, than it is a Steam launch
			if (File.Exists(Path.Combine(vanillaSteamAPIDir, vanillaSteamAPI))) {
				detectionDetails = $"{vanillaSteamAPI} found";
				return DistributionPlatform.Steam;
			}

			detectionDetails = $"{Path.GetFileName(vanillaExePath)} found, no steam files or directories nearby.";
			return DistributionPlatform.GoG;
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
				}
				else if (Directory.Exists("../Terraria.app/")) {
					vanillaPath = "../Terraria.app/Contents/Resources/";
				}
			}

			return CheckForExe(vanillaPath, out exePath);
		}

		private static bool CheckForExe(string vanillaPath, out string exePath) {
			exePath = Path.Combine(vanillaPath, CheckExe);
			if (File.Exists(exePath))
				return true;

			exePath = Path.Combine(vanillaPath, VanillaExe);
			if (File.Exists(exePath))
				return true;

			return false;
		}

		private static void CheckSteam() {
			if (!HashMatchesFile(steamAPIPath, steamAPIHash)) {
				Utils.OpenToURL("https://terraria.org");
				ErrorReporting.FatalExit(Language.GetTextValue("tModLoader.SteamAPIHashMismatch"));
				return;
			}

			if (Main.dedServ)
				return;

			// TODO: This will fake install terraria, leading to errors. We need to check if Terraria is actually installed.
			// We can't check Steamworks.SteamApps.BIsAppInstalled here because steam isn't initailized, we'll need to do that in TerrariaSteamClient.Run later.
			var result = TerrariaSteamClient.Launch();
			switch (result) {
				case TerrariaSteamClient.LaunchResult.Ok:
					break;
				case TerrariaSteamClient.LaunchResult.ErrClientProcDied:
					ErrorReporting.FatalExit("The terraria steam client process exited unexpectedly");
					break;
				case TerrariaSteamClient.LaunchResult.ErrSteamInitFailed:
					ErrorReporting.FatalExit(Language.GetTextValue("tModLoader.SteamInitFailed"));
					break;
				case TerrariaSteamClient.LaunchResult.ErrNotInstalled:
					ErrorReporting.FatalExit(Language.GetTextValue("tModLoader.TerrariaNotInstalled"));
					break;
				case TerrariaSteamClient.LaunchResult.ErrInstallOutOfDate:
					ErrorReporting.FatalExit(Language.GetTextValue("tModLoader.TerrariaOutOfDateMessage"));
					break;
				default:
					throw new Exception("Unsupported result type: " + result);
			}
		}

		// Check if GOG install is correct
		private static void CheckGoG() {
			if (!File.Exists(vanillaExePath)) {
				if (Main.dedServ)
					return;

				ErrorReporting.FatalExit(Language.GetTextValue("tModLoader.VanillaGOGNotFound", vanillaExePath, CheckExe));
			}

			if (!HashMatchesFile(vanillaExePath, gogHash) && !HashMatchesFile(vanillaExePath, steamHash)) {
				ErrorReporting.FatalExit(Language.GetTextValue("tModLoader.GOGHashMismatch", vanillaExePath));
			}

			if (Path.GetFileName(vanillaExePath) != CheckExe) {
				string pathToCheckExe = Path.Combine(Path.GetDirectoryName(vanillaExePath), CheckExe);
				Logging.tML.Info($"Backing up {Path.GetFileName(vanillaExePath)} to {CheckExe}");
				File.Copy(vanillaExePath, pathToCheckExe);
			}
		}
	}
}