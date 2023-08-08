using Newtonsoft.Json;
using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
			string newFolderPathTemp = Path.Combine(superSavePath, Legacy143Folder + "-temp");
			string oldFolderPath = Path.Combine(superSavePath, StableFolder);
			string cloudName = isCloud ? "Steam Cloud" : "Local Files";
			// Previous code relied on "143portedLocal Files.txt" and "143portedSteam Cloud.txt" in oldFolderPath, this could potentially cause issues if a user clears out their stable folder in the future. Now we rely on the folder existing as the sole indicator.

			// We need to port if:
			// 1. We haven't already ported -> Check if tModLoader-1.4.3 folder exists.
			// and
			// 2. We have something to port -> Check if tModLoader folder exists and we that have no indication that the files in it are for a future version.

			if (Directory.Exists(newFolderPath) || !Directory.Exists(oldFolderPath))
				return;

			// We need onedrive running if it is on Path
			if (newFolderPath.Contains("OneDrive")) {
				Logging.tML.Info("Ensuring OneDrive is running before starting to Migrate Files");
				try {
					var oneDrivePath1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\OneDrive\\OneDrive.exe");
					var oneDrivePath2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft OneDrive\\OneDrive.exe");
					if (File.Exists(oneDrivePath1))
						Process.Start(oneDrivePath1);
					else if (File.Exists(oneDrivePath2))
						Process.Start(oneDrivePath2);

					Thread.Sleep(3000);
				}
				catch { }
			}

			// Verify that we are moving 2022.9 player data to 1.4.3 folder. Do so by checking for version <= 2022.9
			string defaultSaveFolder = LaunchParameters.ContainsKey("-savedirectory") ? LaunchParameters["-savedirectory"] :
				Platform.Get<IPathService>().GetStoragePath($"Terraria");

			string stableFolderConfig = Path.Combine(defaultSaveFolder, StableFolder, "config.json");
			if (!File.Exists(stableFolderConfig))
				return;

			string lastLaunchedTml = null;
			try {
				var configCollection = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(stableFolderConfig));
				if (configCollection.TryGetValue("LastLaunchedTModLoaderVersion", out object lastLaunchedTmlObject))
					lastLaunchedTml = (string)lastLaunchedTmlObject;
			}
			catch (Exception e){
				e.HelpLink = "https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Usage-FAQ#configjson-corrupted";
				ErrorReporting.FatalExit($"Attempt to Port from \"{oldFolderPath}\" to \"{newFolderPath}\" aborted, the \"{stableFolderConfig}\" file is corrupted.", e);
			}

			if (string.IsNullOrEmpty(lastLaunchedTml)) {
				// It's unclear what we should do in this situation. Leave it up to the user.
				// It is possible the user copied in their Terraria config.json.
				Logging.tML.Info($"Attempt to Port from \"{oldFolderPath}\" to \"{newFolderPath}\" aborted, the \"{stableFolderConfig}\" file is missing the \"LastLaunchedTModLoaderVersion\" entry. If porting is desired, follow the instructions at \"https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Usage-FAQ#manually-port\"");
				return;
			}
			if (new Version(lastLaunchedTml).MajorMinor() > new Version("2022.9")) {
				Logging.tML.Info($"Attempt to Port from \"{oldFolderPath}\" to \"{newFolderPath}\" aborted, \"{lastLaunchedTml}\" is a newer version.");
				return;
			}

			// Copy all current stable player files to 1.4.3-legacy during transition period. Skip ModSources & Workshop shared folders
			Logging.tML.Info($"Cloning current Stable files to 1.4.3 save folder. Porting {cloudName}." +
				$"\nThis may take a few minutes for a large amount of files.");
			try {
				Utilities.FileUtilities.CopyFolderEXT(oldFolderPath, isCloud ? newFolderPath : newFolderPathTemp, isCloud,
					// Exclude the ModSources folder that exists only on Stable, and exclude the temporary 'Workshop' folder created during first time Mod Publishing
					excludeFilter: new System.Text.RegularExpressions.Regex(@"(Workshop|ModSources)($|/|\\)"),
					overwriteAlways: false, overwriteOld: true);
			}
			catch (Exception e) {
				e.HelpLink = "https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Usage-FAQ#migration-failed";
				ErrorReporting.FatalExit($"Migration Failed, please consult the instructions in the \"Migration Failed\" section at \"{e.HelpLink}\" for more information.", e);
			}

			if (!isCloud) { 
				// If everything goes well, rename the folder. Only local files use this atomic approach. This will prevent situations where a user ends the porting process from impatience and the port is half complete.
				Directory.Move(newFolderPathTemp, newFolderPath);
			}
			else {
				// We need a way on the cloud of knowing if the porting has been done, since users might have multiple computers.
				// In case there are no players and worlds, we don't want to keep attempting to port, since eventually that will port future stable files if they appear.
				// We need at least 1 file in the directory, otherwise the directory will not exist.
				if (Social.SocialAPI.Cloud != null) {
					Social.SocialAPI.Cloud.Write(Path.Combine(Legacy143Folder, $"143ported_{cloudName}.txt"), new byte[] { });
				}
			}

			Logging.tML.Info($"Porting {cloudName} finished");
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

			var savePathCopy = SavePath;
			// 1.4.3 legacy custom statement - the legacy 143 folder
			var fileFolder = Legacy143Folder;
			SavePath = Path.Combine(SavePath, fileFolder);

			// Used for ModSources sharing across folders
			SavePathShared = Path.Combine(SavePath, "..", StableFolder);

			// File migration is only attempted for the default save folder
			if (!saveHere && !tmlSaveDirectoryParameterSet) {
				PortFilesMaster(savePathCopy, isCloud: false);
			}

			if (saveHere)
				SavePath = fileFolder; // Fallback for unresolveable antivirus/onedrive issues. Also makes the game portable I guess.

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
