using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Terraria.ModLoader.Setup.Core.Utilities
{
	public static class SteamUtils
	{
		public const int TerrariaAppId = 105600;

		public readonly static string TerrariaManifestFile = $"appmanifest_{TerrariaAppId}.acf";

		private readonly static Regex SteamLibraryFoldersRegex = new(@"""(\d+|path)""[^\S\r\n]+""(.+)""", RegexOptions.Compiled);
		private readonly static Regex SteamManifestInstallDirRegex = new(@"""installdir""[^\S\r\n]+""([^\r\n]+)""", RegexOptions.Compiled);

		public static bool TryFindTerrariaDirectory([NotNullWhen(true)] out string? path) {
			if (TryGetSteamDirectory(out string? steamDirectory) && TryGetTerrariaDirectoryFromSteam(steamDirectory, out path)) {
				return true;
			}

			path = null;

			return false;
		}

		private static bool TryGetTerrariaDirectoryFromSteam(string steamDirectory, [NotNullWhen(true)] out string? path) {
			string steamApps = Path.Combine(steamDirectory, "steamapps");

			var libraries = new List<string>() {
				steamApps
			};

			string libraryFoldersFile = Path.Combine(steamApps, "libraryfolders.vdf");

			if (File.Exists(libraryFoldersFile)) {
				string contents = File.ReadAllText(libraryFoldersFile);

				var matches = SteamLibraryFoldersRegex.Matches(contents);

				foreach (Match match in matches) {
					string directory = Path.Combine(match.Groups[2].Value.Replace(@"\\", @"\"), "steamapps");

					if (Directory.Exists(directory)) {
						libraries.Add(directory);
					}
				}
			}

			for (int i = 0; i < libraries.Count; i++) {
				string directory = libraries[i];
				string manifestPath = Path.Combine(directory, TerrariaManifestFile);

				if (File.Exists(manifestPath)) {
					string contents = File.ReadAllText(manifestPath);
					var match = SteamManifestInstallDirRegex.Match(contents);

					if (match.Success) {
						path = Path.Combine(directory, "common", match.Groups[1].Value);

						if (Directory.Exists(path)) {
							return true;
						}
					}
				}
			}

			path = null;

			return false;
		}

		private static bool TryGetSteamDirectory([NotNullWhen(true)] out string? path) {
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
				path = GetSteamDirectoryWindows();
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
				path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library/Application Support/Steam");
			}
			else { // Some kind of linux?
				path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/Steam");
				if (!Directory.Exists(path))
					path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".steam/steam");
			}

			return path != null && Directory.Exists(path);
		}

		// Isolated to avoid loading Win32 stuff outside Windows.
		[SupportedOSPlatform("windows")]
		private static string? GetSteamDirectoryWindows() {
			string keyPath = Environment.Is64BitOperatingSystem ? @"SOFTWARE\Wow6432Node\Valve\Steam" : @"SOFTWARE\Valve\Steam";

			using RegistryKey key = Registry.LocalMachine.CreateSubKey(keyPath);

			return key.GetValue("InstallPath") as string;
		}
	}
}
