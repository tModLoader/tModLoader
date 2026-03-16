using ReLogic.OS;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Terraria.Localization;

namespace Terraria.ModLoader.Engine;

public enum DistributionPlatform
{
	Unknown,
	Steam,
	GoG
}

internal static class InstallVerifier
{
	private static string VanillaExe = "Terraria.exe";
	private const string TerrariaVersion = "1.4.5.6";
	private static string CheckExe = $"Terraria_v{TerrariaVersion}.exe"; // This should match the hashes. {Main.versionNumber}
	internal static string vanillaExePath; // Only reliable for GOG installs

	public static DistributionPlatform DistributionPlatform;

	private static string steamAPIPath;
	private static string vanillaSteamAPI;
	private static byte[] steamAPIHash;
	private static byte[] gogHash;
	private static byte[] steamHash;

	private static bool IsSteamUnsupported = false;

	static InstallVerifier()
	{
		string portableRid = RuntimeInformation.RuntimeIdentifier;
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
			if (RuntimeInformation.ProcessArchitecture == Architecture.X86) {
				steamAPIPath = "/libsteam_api.dll";
				steamAPIHash = ToByteArray("6750595142dad4552d0f6f04973a7331");
			}
			else {
				steamAPIPath = "/steam_api64.dll";
				steamAPIHash = ToByteArray("3bae3a5ecad22eec751e154f68e09361");
			}

			vanillaSteamAPI = "steam_api.dll";
			gogHash = ToByteArray("18013fe58e2b64be3ed0d7b7161c6800"); // Don't forget to update CheckExe above
			steamHash = ToByteArray("1ea72236140aafb8737e5664557266c3");
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
			portableRid = $"osx";
			steamAPIPath = "libsteam_api.dylib";
			steamAPIHash = ToByteArray("b7736b391a8276faccb4c055d515d531");
			vanillaSteamAPI = "libsteam_api.dylib";
			gogHash = ToByteArray("bac96f0a8b5bf31f64e19dddd89184a7");
			steamHash = ToByteArray("352f3707864771ce2e51fc299dbf6fcb");
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
			if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64) {
				IsSteamUnsupported = true;
				return;
			}

			portableRid = $"linux-{RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant()}";
			steamAPIPath = "libsteam_api.so";
			steamAPIHash = ToByteArray("4b7a8cabaa354fcd25743aabfb4b1366");
			vanillaSteamAPI = "libsteam_api.so";
			gogHash = ToByteArray("0dbf043f8b07e128e124358ff1e32858");
			steamHash = ToByteArray("15c5b36710fa6d5d911c474e60022df2");
		}
		else {
			ErrorReporting.FatalExit(Language.GetTextValue("tModLoader.UnknownVerificationOS"));
		}

		var steamworksFolder = typeof(SteamAPI).Assembly.Location;
		while (!Directory.Exists($"{steamworksFolder}/runtimes"))
			steamworksFolder = Path.GetDirectoryName(steamworksFolder);

		steamAPIPath = $"{steamworksFolder}/runtimes/{portableRid}/native/{steamAPIPath}";
	}

	private static bool HashMatchesFile(string path, byte[] hash)
	{
		using (var md5 = MD5.Create())
		using (var stream = File.OpenRead(path))
			return hash.SequenceEqual(md5.ComputeHash(stream));
	}

	private static byte[] ToByteArray(string hexString, bool forceLowerCase = true)
	{
		if (forceLowerCase) {
			hexString = hexString.ToLower();
		}

		byte[] retval = new byte[hexString.Length / 2];

		for (int i = 0; i < hexString.Length; i += 2)
			retval[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);

		return retval;
	}

	internal static void Startup()
	{
		DistributionPlatform = DetectPlatform(out string detectionDetails);
		Logging.tML.Info($"Distribution Platform: {DistributionPlatform}. Detection method: {detectionDetails}");

		if (DistributionPlatform == DistributionPlatform.GoG) {
			CheckGoG();
		}
		else {
			CheckSteam();
		}
	}

	private static DistributionPlatform DetectPlatform(out string detectionDetails)
	{
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

	private static bool ObtainVanillaExePath(out string vanillaPath, out string exePath)
	{
		foreach (var possibleVanillaInstallFolder in GetPossibleVanillaInstallFolders()) {
			if (CheckForExe(possibleVanillaInstallFolder, out exePath)) {
				vanillaPath = possibleVanillaInstallFolder;
				return true;
			}
		}
		vanillaPath = exePath = null;
		return false;
	}

	private static IEnumerable<string> GetPossibleVanillaInstallFolders()
	{
		// Check if in the same folder somehow.
		string vanillaPath = Directory.GetCurrentDirectory();
		yield return vanillaPath;

		// If .exe not present check parent directory (Nested Manual Install)
		vanillaPath = Directory.GetParent(vanillaPath).FullName;
		yield return vanillaPath;

		// If .exe not present, check Terraria directory (Side-by-Side Manual Install)
		vanillaPath = Path.Combine(vanillaPath, "Terraria");
		if (Platform.IsOSX) {
			// GOG installs to /Applications/Terraria.app, Steam installs to /Library/Application Support/Steam/steamapps/common/Terraria/Terraria.app
			// Vanilla .exe files are in /Contents/Resources/, not /Contents/MacOS/
			if (Directory.Exists("../Terraria/Terraria.app/")) {
				vanillaPath = "../Terraria/Terraria.app/Contents/Resources/";
			}
			else if (Directory.Exists("../Terraria.app/")) {
				vanillaPath = "../Terraria.app/Contents/Resources/";
			}
		}
		yield return vanillaPath;

		if (Platform.IsLinux)
			yield return Path.Combine(vanillaPath, "game"); // GOG+Linux installs exe to Terraria/game/

		// Fallback to default GOG install locations
		if (Platform.IsWindows) {
			yield return Path.Combine(@"c:\", "Program Files (x86)", "GOG Galaxy", "Games", "Terraria");
			yield return Path.Combine(@"c:\", "GOG Games", "Terraria");
		}
		else if (Platform.IsLinux) {
			yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "GOG Games", "Terraria");
		}
		else {
			yield return Path.Combine("/Applications", "Terraria.app", "Contents", "Resources");
		}
	}

	private static bool CheckForExe(string vanillaPath, out string exePath)
	{
		exePath = Path.Combine(vanillaPath, CheckExe);
		if (File.Exists(exePath))
			return true;

		exePath = Path.Combine(vanillaPath, VanillaExe);
		if (File.Exists(exePath))
			return true;

		return false;
	}

	private static void CheckSteam()
	{
		if (IsSteamUnsupported)
			return;

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
				Utils.OpenToURL("https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Usage-FAQ#terraria-is-out-of-date-or-terraria-is-on-a-legacy-version");
				ErrorReporting.FatalExit(Language.GetTextValue("tModLoader.TerrariaOutOfDateMessage"));
				break;
			case TerrariaSteamClient.LaunchResult.ErrInstallLegacyVersion:
				Utils.OpenToURL("https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Usage-FAQ#terraria-is-out-of-date-or-terraria-is-on-a-legacy-version");
				ErrorReporting.FatalExit(Language.GetTextValue("tModLoader.TerrariaLegacyBranchMessage"));
				break;
			default:
				throw new Exception("Unsupported result type: " + result);
		}
	}

	// Check if GOG install is correct
	private static void CheckGoG()
	{
		if (!HashMatchesFile(vanillaExePath, gogHash) && !HashMatchesFile(vanillaExePath, steamHash)) {
			ErrorReporting.FatalExit(Language.GetTextValue("tModLoader.GOGHashMismatch", vanillaExePath, TerrariaVersion, CheckExe));
		}

		if (Path.GetFileName(vanillaExePath) != CheckExe) {
			string pathToCheckExe = Path.Combine(Path.GetDirectoryName(vanillaExePath), CheckExe);
			Logging.tML.Info($"Backing up {Path.GetFileName(vanillaExePath)} to {CheckExe}");
			File.Copy(vanillaExePath, pathToCheckExe);
		}
	}
}
