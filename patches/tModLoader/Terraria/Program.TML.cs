using Newtonsoft.Json;
using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Terraria.ModLoader;
using Terraria.ModLoader.Engine;

namespace Terraria;

public static partial class Program
{
	public static string SavePathShared { get; private set; } // Points to the Stable tModLoader save folder, used for Mod Sources only currently

	private static IEnumerable<MethodInfo> GetAllMethods(Type type)
	{
		return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
	}

	private static IEnumerable<MethodInfo> CollectMethodsToJIT(IEnumerable<Type> types) =>
		from type in types
		from method in GetAllMethods(type)
		where !method.IsAbstract && !method.ContainsGenericParameters && method.GetMethodBody() != null
		select method;

	private static void ForceJITOnMethod(MethodInfo method)
	{
		RuntimeHelpers.PrepareMethod(method.MethodHandle);

		Interlocked.Increment(ref ThingsLoaded);
	}

	private static void ForceStaticInitializers(Type[] types)
	{
		foreach (Type type in types) {
			if (!type.IsGenericType)
				RuntimeHelpers.RunClassConstructor(type.TypeHandle);
		}
	}

	public const string PreviewFolder = "tModLoader-preview";
	public const string ReleaseFolder = "tModLoader";
	public const string DevFolder = "tModLoader-dev";
	public const string Legacy143Folder = "tModLoader-1.4.3";
	public static string SaveFolderName => BuildInfo.IsStable ? ReleaseFolder : BuildInfo.IsPreview ? PreviewFolder : DevFolder;

	private static void PortOldSaveDirectories(string savePath)
	{
		// PortOldSaveDirectories should only run once no matter which branch is run first.

		// Port old file format users
		var oldBetas = Path.Combine(savePath, "ModLoader", "Beta");

		if (!Directory.Exists(oldBetas))
			return;

		Logging.tML.Info($"Old tModLoader alpha folder \"{oldBetas}\" found, attempting folder migration");

		var newPath = Path.Combine(savePath, ReleaseFolder);
		if (Directory.Exists(newPath)) {
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

	private static void PortCommonFilesToStagingBranches(string savePath)
	{
		// Only create and port config files from stable if needed.
		if (BuildInfo.IsStable)
			return;

		var releasePath = Path.Combine(savePath, ReleaseFolder);
		var newPath = Path.Combine(savePath, SaveFolderName);
		if (Directory.Exists(releasePath) && !Directory.Exists(newPath)) {
			Directory.CreateDirectory(newPath);
			Logging.tML.Info("Cloning common files from Stable to preview and dev.");

			if (File.Exists(Path.Combine(releasePath, "config.json")))
				File.Copy(Path.Combine(releasePath, "config.json"), Path.Combine(newPath, "config.json"));
			if (File.Exists(Path.Combine(releasePath, "input profiles.json")))
				File.Copy(Path.Combine(releasePath, "input profiles.json"), Path.Combine(newPath, "input profiles.json"));
		}
	}

	/// <summary>
	/// Super Save Path is the parent directory containing both folders. Usually Program.SavePath or Steam Cloud
	/// Source is of variety StableFolder, PreviewFolder... etc
	/// Destination is of variety StableFolder, PreviewFolder... etc
	/// maxVersionOfSource is used to determine if we even should port the files. Example: 1.4.3-Legacy has maxVersion of 2022.9
	/// isAtomicLockable could be expressed as CopyToNewlyCreatedDestinationFolderViaTempFolder if that makes more sense to the reader.
	/// </summary>
	private static void PortFilesFromXtoY(string superSavePath, string source, string destination, string maxVersionOfSource, bool isCloud, bool isAtomicLockable, DateTime migrationDay)
	{
		string newFolderPath = Path.Combine(superSavePath, destination);
		string newFolderPathTemp = Path.Combine(superSavePath, destination + "-temp");
		string oldFolderPath = Path.Combine(superSavePath, source);
		string cloudName = isCloud ? "Steam Cloud" : "Local Files";
		// Previous code relied on "143portedLocal Files.txt" and "143portedSteam Cloud.txt" in oldFolderPath,
		// this could potentially cause issues if a user clears out their stable folder in the future.
		// Now we rely on the folder existing as the sole indicator.

		// We need to port if:
		// 1. We haven't already ported -> Check if destination folder exists if Atomic Lockable, or Porting File exists if not.
		// and
		// 2. We have something to port -> Check if source folder exists and we that have no indication that the files in it are for a future version.

		// Note that non-atomic lockable file ports carry some risk as the Porting File could be deleted.
		// CopyFolderEXT must thus be called with prudence and any pre-checks we can, such as the version check included

		if (!Directory.Exists(oldFolderPath))
			return;

		// Backwards compat line for 1.4.3-legacy, intended for use when is not Atomic Lockable
		string portFilePath = Path.Combine(superSavePath, destination, maxVersionOfSource == "2022.9" ? $"143ported_{cloudName}.txt" : $"{maxVersionOfSource}{destination}ported_{cloudName}.txt");

		if (isAtomicLockable && Directory.Exists(newFolderPath) || !isAtomicLockable && File.Exists(portFilePath))
			return;

		// We need onedrive running if it is on Path
		if (newFolderPath.Contains("OneDrive")) {
			Logging.tML.Info("Ensuring OneDrive is running before starting to Migrate Files");
			try {
				var oneDrivePath1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\OneDrive\\OneDrive.exe");
				var oneDrivePath2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft OneDrive\\OneDrive.exe");

				// passing /background to onedrive starts it with neither a tooltip popup nor a file explorer window
				System.Diagnostics.ProcessStartInfo oneDriveInfo = new System.Diagnostics.ProcessStartInfo {
					Arguments = "/background",
					UseShellExecute = false
				};

				if (File.Exists(oneDrivePath1)) {
					oneDriveInfo.FileName = oneDrivePath1;
					System.Diagnostics.Process.Start(oneDriveInfo);
				}
				else if (File.Exists(oneDrivePath2)) {
					oneDriveInfo.FileName = oneDrivePath2;
					System.Diagnostics.Process.Start(oneDriveInfo);
				}
				Thread.Sleep(3000);
			}
			catch { }
		}

		// Verify that we are moving maxVersionOfSource player data to destination folder. Do so by checking for version <= maxVersionOfSource
		string defaultSaveFolder = LaunchParameters.ContainsKey("-savedirectory") ? LaunchParameters["-savedirectory"] :
			Platform.Get<IPathService>().GetStoragePath($"Terraria");

		string sourceFolderConfig = Path.Combine(defaultSaveFolder, source, "config.json");
		if (!File.Exists(sourceFolderConfig)) {
			Logging.tML.Info($"No config.json found at {sourceFolderConfig}\nAssuming nothing to port");
			return;
		}

		string lastLaunchedTml = null;
		try {
			var configCollection = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(sourceFolderConfig));
			if (configCollection.TryGetValue("LastLaunchedTModLoaderVersion", out object lastLaunchedTmlObject))
				lastLaunchedTml = (string)lastLaunchedTmlObject;
		}
		catch (Exception e) {
			if (File.GetLastWriteTime(sourceFolderConfig) > migrationDay) {
				// If the file was edited recently, we assume that an updated tModLoader edited it. This should ignore modifications made long ago by pre-migration logic tModLoader releases.
				lastLaunchedTml = BuildInfo.tMLVersion.ToString();
			}
			else {
				PromptUserForNewestTMLVersionLaunched(ref lastLaunchedTml);

				if (string.IsNullOrEmpty(lastLaunchedTml)) {
					e.HelpLink = "https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Usage-FAQ#configjson-corrupted";
					ErrorReporting.FatalExit($"Attempt to Port from \"{oldFolderPath}\" to \"{newFolderPath}\" aborted, the \"{sourceFolderConfig}\" file is corrupted.", e);
				}
			}
		}

		PromptUserForNewestTMLVersionLaunched(ref lastLaunchedTml);

		if (string.IsNullOrEmpty(lastLaunchedTml)) {
			// It's unclear what we should do in this situation. Leave it up to the user.
			// It is possible the user copied in their Terraria config.json.
			ErrorReporting.FatalExit($"Attempt to Port from \"{oldFolderPath}\" to \"{newFolderPath}\" aborted, the \"{sourceFolderConfig}\" file is missing the \"LastLaunchedTModLoaderVersion\" entry. If porting is desired, follow the instructions at \"https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Usage-FAQ#manually-port\"");
			return;
		}
		if (new Version(lastLaunchedTml).MajorMinor() > new Version(maxVersionOfSource)) {
			Logging.tML.Info($"Attempt to Port from \"{oldFolderPath}\" to \"{newFolderPath}\" aborted, \"{lastLaunchedTml}\" is a newer version.");
			return;
		}

		// Copy all current stable player files to 1.4.3-legacy during transition period. Skip ModSources & Workshop shared folders
		Logging.tML.Info($"Cloning current {source} files to {destination} save folder. Porting {cloudName}." +
			$"\nThis may take a few minutes for a large amount of files.");
		try {
			Utilities.FileUtilities.CopyFolderEXT(oldFolderPath, isAtomicLockable ? newFolderPathTemp : newFolderPath, isCloud,
				// Exclude the ModSources folder that exists only on Stable, and exclude the temporary 'Workshop' folder created during first time Mod Publishing
				excludeFilter: new System.Text.RegularExpressions.Regex(@"(Workshop|ModSources)($|/|\\)"),
				overwriteAlways: false, overwriteOld: true);
		}
		catch (Exception e) {
			e.HelpLink = "https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Usage-FAQ#migration-failed";
			ErrorReporting.FatalExit($"Migration Failed, please consult the instructions in the \"Migration Failed\" section at \"{e.HelpLink}\" for more information.", e);
		}

		if (isAtomicLockable) {
			// If everything goes well, rename the folder. Only local files use this atomic approach. This will prevent situations where a user ends the porting process from impatience and the port is half complete.
			Directory.Move(newFolderPathTemp, newFolderPath);
		}
		else {
			if (isCloud) {
				// We need a way on the cloud of knowing if the porting has been done, since users might have multiple computers.
				// In case there are no players and worlds, we don't want to keep attempting to port, since eventually that will port future stable files if they appear.
				// We also need at least 1 file in the directory, otherwise the directory will not exist.
				if (Social.SocialAPI.Cloud != null) {
					Social.SocialAPI.Cloud.Write(Utilities.FileUtilities.ConvertToRelativePath(superSavePath, portFilePath), new byte[] { });
				}
			}
			else {
				File.Create(portFilePath);
			}
		}

		Logging.tML.Info($"Porting {cloudName} finished");

		static void PromptUserForNewestTMLVersionLaunched(ref string lastLaunchedTml)
		{
			if (string.IsNullOrEmpty(lastLaunchedTml)) {
				// If the config.json is missing LastLaunchedTModLoaderVersion entry, we can ask the user. (Most likely the user copied Terraria/config.json over)
				// We can't localized these the normal way because localization isn't loaded at this point.
				int result = ErrorReporting.ShowMessageBoxWithChoices(
					title: "Failed to read config.json configuration file",
					message: "Your config.json file is incomplete.\n\nPlease select one of the following options and the game will resume loading:\n\nWhat is the highest version of tModLoader that you have launched?",
					buttonLabels: new string[] { "1.4.4", "1.4.3", "Cancel" }
				);
				if (result == 0)
					lastLaunchedTml = BuildInfo.tMLVersion.ToString();
				if (result == 1)
					lastLaunchedTml = "2022.09";
				// If the user presses escape or presses cancel, lastLaunchedTml will still be NullOrEmpty.
			}
		}
	}

	internal static void PortFilesMaster(string savePath, bool isCloud)
	{
		// Moving from ModLoader-Beta to 1.4+ file system
		PortOldSaveDirectories(savePath);
		PortCommonFilesToStagingBranches(savePath);

		// Establishing 1.4.3-Legacy branch
		PortFilesFromXtoY(savePath, ReleaseFolder, Legacy143Folder, maxVersionOfSource: "2022.9", isCloud, isAtomicLockable: !isCloud, migrationDay: new DateTime(2023, 9, 1));
		// Local: This is supposed to create a new 1.4.3 folder if it doesn't exist, and ignore it if it does. (already migrated)
		// Steam: Move files if canary file doesn't exist.

		// Moving files from 1.4.4-preview (beta) to 1.4.4-stable - August 1st 2023 steam release
		if (BuildInfo.IsStable)
			PortFilesFromXtoY(savePath, PreviewFolder, ReleaseFolder, maxVersionOfSource: "2023.6", isCloud, isAtomicLockable: false, migrationDay: new DateTime(2023, 9, 1));
			// Local: Files and destination folder likely exist, copying in new files is expected/desired. Rely on already migrated file (canary file) to determine if migration should happen
			// Steam: Move files if canary file doesn't exist.
	}

	private static void SetSavePath()
	{
		if (ControlledFolderAccessSupport.ControlledFolderAccessDetectionPrevented)
			Logging.tML.Info($"Controlled Folder Access detection failed, something is preventing the game from accessing the registry.");
		if (ControlledFolderAccessSupport.ControlledFolderAccessDetected)
			Logging.tML.Info($"Controlled Folder Access feature detected. If game fails to launch make sure to add \"{Environment.ProcessPath}\" to the \"Allow an app through Controlled folder access\" menu found in the \"Ransomware protection\" menu."); // Before language is loaded, no need to localize

		if (LaunchParameters.TryGetValue("-tmlsavedirectory", out var customSavePath)) {
			// With a custom tmlsavedirectory, the shared saves are assumed to be in the same folder
			SavePathShared = customSavePath;
			SavePath = customSavePath;
		}
		else if (File.Exists("savehere.txt")) {
			// Fallback for unresolvable antivirus/onedrive issues. Also makes the game portable I guess.
			SavePathShared = ReleaseFolder;
			SavePath = SaveFolderName;
		}
		else {
			// Needs to run as early as possible, given exception handler depends on ModCompile, and Porting carries exception risk
			SavePathShared = Path.Combine(SavePath, ReleaseFolder);
			var savePathCopy = SavePath;

			SavePath = Path.Combine(SavePath, SaveFolderName);

			// File migration is only attempted for the default save folder
			try {
				PortFilesMaster(savePathCopy, isCloud: false);
			}
			catch (Exception e) {
				bool controlledFolderAccessMightBeRelevant = (e is COMException || e is FileNotFoundException) && ControlledFolderAccessSupport.ControlledFolderAccessDetected;

				ErrorReporting.FatalExit("An error occurred migrating files and folders to the new structure" + (controlledFolderAccessMightBeRelevant ? $"\n\nControlled Folder Access feature detected, this might be the cause of this error.\n\nMake sure to add \"{Environment.ProcessPath}\" to the \"Allow an app through Controlled folder access\" menu found in the \"Ransomware protection\" menu." : ""), e);
			}
		}

		if (Platform.IsWindows) {
			// Fix #4168, for some reason sometimes Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) in PathService.GetStoragePath is returning a path with / and \, causing issues for some interactions. Bad -tmlsavedirectory or -savedirectory arguments could also cause this.
			string SavePathFixed = SavePath.Replace('/', Path.DirectorySeparatorChar);
			string SavePathSharedFixed = SavePathShared.Replace('/', Path.DirectorySeparatorChar);
			if (SavePath != SavePathFixed || SavePathShared != SavePathSharedFixed) {
				Logging.tML.Warn($"Saves paths had incorrect slashes somehow: \"{SavePath}\"=>\"{SavePathFixed}\", \"{SavePathShared}\"=>\"{SavePathSharedFixed}\"");
				SavePath = SavePathFixed;
				SavePathShared = SavePathSharedFixed;
			}
		}

		Logging.tML.Info($"Saves Are Located At: {Path.GetFullPath(SavePath)}");
	}

	private static void StartupSequenceTml(bool isServer)
	{
		try {
			ControlledFolderAccessSupport.CheckFileSystemAccess();
			Logging.Init(isServer ? Logging.LogFile.Server : Logging.LogFile.Client);

			if (Platform.Current.Type == PlatformType.Windows && System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture != System.Runtime.InteropServices.Architecture.X64) {
				if (Program.LaunchParameters.ContainsKey("-build"))
					Console.WriteLine("Warning: Building mods requires the 64 bit dotnet SDK to be installed, but the 32 bit dotnet SDK appears to be running. It is likely that you accidentally installed the 32 bit dotnet SDK and it is taking priority. To fix this, follow the instructions at https://github.com/tModLoader/tModLoader/wiki/tModLoader-guide-for-developers#net-sdk"); // MessageBoxShow called below will also error when attempting to load 32 bit SDL2.
				ErrorReporting.FatalExit("The current Windows Architecture of your System is CURRENTLY unsupported. Aborting...");
			}
			if (Platform.Current.Type == PlatformType.OSX && Environment.OSVersion.Version < new Version(10, 15)) {
				ErrorReporting.FatalExit("tModLoader requires macOS v10.15 (Catalina) or higher to run as of tModLoader v2024.03+. Please update macOS.\nIf updating is not possible, manually downgrading (https://github.com/tModLoader/tModLoader/wiki/tModLoader-guide-for-players#manual-installation) to tModLoader v2024.02.3.0 (https://github.com/tModLoader/tModLoader/releases/tag/v2024.02.3.0) is an option to keep playing.\nAborting...");
			}

			Logging.LogStartup(isServer); // Should run as early as is possible. Want as complete a log file as possible

			SetSavePath();

			AttemptSupportHighDPI(isServer); // Can run anytime

		    if (!isServer) {
		    	NativeLibraries.CheckNativeFAudioDependencies();
		       	FNALogging.RedirectLogs(); // Needs to run after CheckDependencies
		    }
		}
		catch (Exception ex) {
			ErrorReporting.FatalExit("An unexpected error occurred during tML startup", ex);
		}
	}

	private static void ProcessLaunchArgs(string[] args, bool monoArgs, out bool isServer)
	{
		isServer = false;

		try {
			if (monoArgs)
				args = Utils.ConvertMonoArgsToDotNet(args);

			LaunchParameters = Utils.ParseArguements(args);

			if (LaunchParameters.ContainsKey("-terrariasteamclient")) {
				// Launch the Terraria playtime tracker and quit.
				TerrariaSteamClient.Run();
				Environment.Exit(1);
			}

			SavePath = (LaunchParameters.ContainsKey("-savedirectory") ? LaunchParameters["-savedirectory"] : Platform.Get<IPathService>().GetStoragePath("Terraria"));

			// Unify server and client dll via launch param
			isServer = LaunchParameters.ContainsKey("-server");
		}
		catch (Exception e) {
			ErrorReporting.FatalExit("Unhandled Issue with Launch Arguments. Please verify sources such as Steam Launch Options, cli-ArgsConfig, and VS profiles", e);
		}
	}

	private const int HighDpiThreshold = 96; // Rando internet value that Solxan couldn't refind the sauce for.

	// Add Support for High DPI displays, such as Mac M1 laptops. Must run before Game constructor.
	private static void AttemptSupportHighDPI(bool isServer)
	{
		if (isServer)
			return;

		if (Platform.IsWindows) {
			[System.Runtime.InteropServices.DllImport("user32.dll")]
			static extern bool SetProcessDPIAware();

			SetProcessDPIAware();
		}

		SDL2.SDL.SDL_VideoInit(null);
		SDL2.SDL.SDL_GetDisplayDPI(0, out var ddpi, out float hdpi, out float vdpi);
		Logging.tML.Info($"Display DPI: Diagonal DPI is {ddpi}. Vertical DPI is {vdpi}. Horizontal DPI is {hdpi}");
		if (ddpi >= HighDpiThreshold || hdpi >= HighDpiThreshold || vdpi >= HighDpiThreshold) {
			Environment.SetEnvironmentVariable("FNA_GRAPHICS_ENABLE_HIGHDPI", "1");
			Logging.tML.Info($"High DPI Display detected: setting FNA to highdpi mode");
		}
	}
}
