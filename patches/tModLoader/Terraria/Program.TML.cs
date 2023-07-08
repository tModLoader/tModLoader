using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using Terraria.ModLoader;
using Terraria.ModLoader.Engine;

namespace Terraria
{
	public static partial class Program
	{
		public static string SavePath { get; private set; } // Moved from Main to avoid triggering the Main static constructor before logging initializes
		public static string SavePathShared { get; private set; } // Points to the Stable tModLoader save folder, used for Mod Sources only currently

		private static IEnumerable<MethodInfo> GetAllMethods(Type type) {
			return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
		}

		private static IEnumerable<MethodInfo> CollectMethodsToJIT(IEnumerable<Type> types) =>
			from type in types
			from method in GetAllMethods(type)
			where !method.IsAbstract && !method.ContainsGenericParameters && method.GetMethodBody() != null
			select method;

		private static void ForceJITOnMethod(MethodInfo method) {
			RuntimeHelpers.PrepareMethod(method.MethodHandle);

			Interlocked.Increment(ref ThingsLoaded);
		}

		private static void ForceStaticInitializers(Type[] types) {
			foreach (Type type in types) {
				if (!type.IsGenericType)
					RuntimeHelpers.RunClassConstructor(type.TypeHandle);
			}
		}

		private static void PortOldSaveDirectories(string savePath) {
			// PortOldSaveDirectories should only run once no matter which branch is run first.

			// Port old file format users
			var oldBetas = Path.Combine(savePath, "ModLoader", "Beta");

			if (!Directory.Exists(oldBetas))
				return;

			Logging.tML.Info($"Old tModLoader alpha folder \"{oldBetas}\" found, attempting folder migration");

			var newPath = Path.Combine(savePath, StableFolder);
			if (Directory.Exists(newPath)){
				Logging.tML.Warn($"Both \"{oldBetas}\" and \"{newPath}\" exist, assuming user launched old tModLoader alpha, aborting migration");
				return;
			}
			Logging.tML.Info($"Migrating from \"{oldBetas}\" to \"{newPath}\"");
			Directory.Move(oldBetas, newPath);
			Logging.tML.Info($"Old alpha folder to new location migration success");

			string[] subDirsToMove = { "Mod Reader", "Mod Sources", "Mod Configs" };
			foreach (var subDir in subDirsToMove) {
				string newSaveOriginalSubDirPath = Path.Combine(newPath, subDir);
				if (Directory.Exists(newSaveOriginalSubDirPath)) {
					string newSaveNewSubDirPath = Path.Combine(newPath, subDir.Replace(" ", ""));
					Logging.tML.Info($"Renaming from \"{newSaveOriginalSubDirPath}\" to \"{newSaveNewSubDirPath}\"");
					Directory.Move(newSaveOriginalSubDirPath, newSaveNewSubDirPath);
				}
			}
			Logging.tML.Info($"Folder Renames Success");
		}

		private static void PortCommonFilesToStagingBranches(string savePath) {
			// Only create and port config files from stable if needed.
			if(BuildInfo.IsDev || BuildInfo.IsPreview) {
				var releasePath = Path.Combine(SavePath, StableFolder);
				var newPath = Path.Combine(SavePath, BuildInfo.IsPreview ? PreviewFolder : DevFolder);
				if (Directory.Exists(releasePath) && !Directory.Exists(newPath)) {
					Logging.tML.Info("Cloning common files from Stable to preview and dev.");
					Directory.CreateDirectory(newPath);
					if (File.Exists(Path.Combine(releasePath, "config.json")))
						File.Copy(Path.Combine(releasePath, "config.json"), Path.Combine(newPath, "config.json"));
					if (File.Exists(Path.Combine(releasePath, "input profiles.json")))
						File.Copy(Path.Combine(releasePath, "input profiles.json"), Path.Combine(newPath, "input profiles.json"));
				}
			}
		}

		private static void Port143FilesFromStable(string superSavePath, bool isCloud) {
			string newFolderPath = Path.Combine(superSavePath, Legacy143Folder);
			string oldFolderPath = Path.Combine(superSavePath, StableFolder);

			if (!Directory.Exists(oldFolderPath))
				return;

			// Verify that we are moving 2022.9 player data to 1.4.3 folder. Do so by checking for version <= 2022.9
			string defaultSaveFolder = LaunchParameters.ContainsKey("-savedirectory") ? LaunchParameters["-savedirectory"] :
				Platform.Get<IPathService>().GetStoragePath($"Terraria");

			string stableFolderConfig = Path.Combine(defaultSaveFolder, StableFolder, "config.json");
			if (!File.Exists(stableFolderConfig))
				return;

			var configCollection = JsonNode.Parse(File.ReadAllText(stableFolderConfig));
			string lastLaunchedTml = (string)configCollection!["LastLaunchedTModLoaderVersion"];
			if (string.IsNullOrEmpty(lastLaunchedTml) || new Version(lastLaunchedTml).MajorMinor() > new Version("2022.9")) {
				return;
			}

			// Copy all current stable player files to 1.4.3-legacy during transition period. Skip ModSources & Workshop shared folders
			Logging.tML.Info($"Cloning current Stable files to 1.4.3 save folder. Save Folder is Cloud? {isCloud}");
			Utilities.FileUtilities.CopyFolderEXT(oldFolderPath, newFolderPath, isCloud,
				// Exclude the ModSources folder that exists only on Stable, and exclude the temporary 'Workshop' folder created during first time Mod Publishing
				excludeFilter: new System.Text.RegularExpressions.Regex(@"(Workshop|ModSources)($|/|\\)"),
				overwriteAlways: false, overwriteOld: true);
		}

		internal static void PortFilesMaster(string savePath, bool isCloud) {
			PortOldSaveDirectories(savePath);
			PortCommonFilesToStagingBranches(savePath);
			Port143FilesFromStable(savePath, isCloud);
		}

		public const string Legacy143Folder = "tModLoader-1.4.3";
		public const string StableFolder = "tModLoader";
		public const string DevFolder = "tModLoader-dev";
		public const string PreviewFolder = "tModLoader-preview";

		private static void SetSavePath() {
			SavePath =
				LaunchParameters.ContainsKey("-savedirectory") ? LaunchParameters["-savedirectory"] :
				Platform.Get<IPathService>().GetStoragePath($"Terraria");

			bool saveHere = File.Exists("savehere.txt");
			bool tmlSaveDirectoryParameterSet = LaunchParameters.ContainsKey("-tmlsavedirectory");

			// File migration is only attempted for the default save folder
			if (!saveHere && !tmlSaveDirectoryParameterSet) {
				PortFilesMaster(SavePath, isCloud: false);
			}

			// 1.4.3 legacy custom statement - the legacy 143 folder
			var fileFolder = Legacy143Folder;

			SavePath = Path.Combine(SavePath, fileFolder);

			if (saveHere)
				SavePath = fileFolder; // Fallback for unresolveable antivirus/onedrive issues. Also makes the game portable I guess.

			// Used for ModSources sharing across folders
			SavePathShared = Path.Combine(SavePath, "..", StableFolder);

			// With a custom tmlsavedirectory, the shared saves are assumed to be in the same folder
			if (tmlSaveDirectoryParameterSet) {
				SavePath = LaunchParameters["-tmlsavedirectory"];
				SavePathShared = SavePath;
			}
			
			Logging.tML.Info($"Save Are Located At: {Path.GetFullPath(SavePath)}");

			if (ControlledFolderAccessSupport.ControlledFolderAccessDetectionPrevented)
				Logging.tML.Info($"Controlled Folder Access detection failed, something is preventing the game from accessing the registry.");
			if (ControlledFolderAccessSupport.ControlledFolderAccessDetected)
				Logging.tML.Info($"Controlled Folder Access feature detected. If game fails to launch make sure to add \"{Environment.ProcessPath}\" to the \"Allow an app through Controlled folder access\" menu found in the \"Ransomware protection\" menu."); // Before language is loaded, no need to localize
		}

		private const int HighDpiThreshold = 96; // Rando internet value that Solxan couldn't refind the sauce for.

		// Add Support for High DPI displays, such as Mac M1 laptops. Must run before Game constructor.
		private static void AttemptSupportHighDPI(bool isServer) {
			if (isServer)
				return;

			if (Platform.IsWindows) {
				[System.Runtime.InteropServices.DllImport("user32.dll")]
				static extern bool SetProcessDPIAware();

				SetProcessDPIAware();
			}

			SDL2.SDL.SDL_VideoInit(null);
			SDL2.SDL.SDL_GetDisplayDPI(0, out var ddpi, out float hdpi, out float vdpi);
			if (ddpi >= HighDpiThreshold || hdpi >= HighDpiThreshold || vdpi >= HighDpiThreshold)
				Environment.SetEnvironmentVariable("FNA_GRAPHICS_ENABLE_HIGHDPI", "1");
		}
	}
}
