using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

	private static void PortOldSaveDirectories()
	{
		// PortOldSaveDirectories should only run once no matter which branch is run first.

		// Port old file format users
		var oldBetas = Path.Combine(SavePath, "ModLoader", "Beta");

		if (!Directory.Exists(oldBetas))
			return;

		Logging.tML.Info($"Old tModLoader alpha folder \"{oldBetas}\" found, attempting folder migration");

		var newPath = Path.Combine(SavePath, ReleaseFolder);
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

	private static void PortCommonFiles()
	{
		// Only create and port config files from stable if needed.
		if (BuildInfo.IsStable)
			return;
		
		var releasePath = Path.Combine(SavePath, ReleaseFolder);
		var newPath = Path.Combine(SavePath, SaveFolderName);
		if (Directory.Exists(releasePath) && !Directory.Exists(newPath)) {
			Directory.CreateDirectory(newPath);
			if (File.Exists(Path.Combine(releasePath, "config.json")))
				File.Copy(Path.Combine(releasePath, "config.json"), Path.Combine(newPath, "config.json"));
			if (File.Exists(Path.Combine(releasePath, "input profiles.json")))
				File.Copy(Path.Combine(releasePath, "input profiles.json"), Path.Combine(newPath, "input profiles.json"));
		}
	}

	private static void SetSavePath()
	{
		if (LaunchParameters.TryGetValue("-tmlsavedirectory", out var customSavePath)) {
			// With a custom tmlsavedirectory, the shared saves are assumed to be in the same folder
			SavePathShared = customSavePath;
			SavePath = customSavePath;
		}
		else if (File.Exists("savehere.txt")) {
			// Fallback for unresolveable antivirus/onedrive issues. Also makes the game portable I guess.
			SavePathShared = ReleaseFolder;
			SavePath = SaveFolderName;
		}
		else {
			// File migration is only attempted for the default save folder
			PortOldSaveDirectories();
			PortCommonFiles();

			SavePathShared = Path.Combine(SavePath, ReleaseFolder);
			SavePath = Path.Combine(SavePath, SaveFolderName);
		}
		
		Logging.tML.Info($"Save Are Located At: {Path.GetFullPath(SavePath)}");

		if (ControlledFolderAccessSupport.ControlledFolderAccessDetectionPrevented)
			Logging.tML.Info($"Controlled Folder Access detection failed, something is preventing the game from accessing the registry.");
		if (ControlledFolderAccessSupport.ControlledFolderAccessDetected)
			Logging.tML.Info($"Controlled Folder Access feature detected. If game fails to launch make sure to add \"{Environment.ProcessPath}\" to the \"Allow an app through Controlled folder access\" menu found in the \"Ransomware protection\" menu."); // Before language is loaded, no need to localize
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
		if (ddpi >= HighDpiThreshold || hdpi >= HighDpiThreshold || vdpi >= HighDpiThreshold)
			Environment.SetEnvironmentVariable("FNA_GRAPHICS_ENABLE_HIGHDPI", "1");
	}
}
