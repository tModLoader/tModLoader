using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Linq;
using Terraria.ModLoader;
using Terraria.Utilities;

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

		private static void PortOldSaveDirectories() {
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

		private static void PortCommonFiles() {
			// Only create and port config files from stable if needed.
			if(BuildInfo.IsDev || BuildInfo.IsPreview) {
				var releasePath = Path.Combine(SavePath, ReleaseFolder);
				var newPath = Path.Combine(SavePath, BuildInfo.IsPreview ? PreviewFolder : DevFolder);
				if (Directory.Exists(releasePath) && !Directory.Exists(newPath)) {
					Directory.CreateDirectory(newPath);
					if (File.Exists(Path.Combine(releasePath, "config.json")))
						File.Copy(Path.Combine(releasePath, "config.json"), Path.Combine(newPath, "config.json"));
					if (File.Exists(Path.Combine(releasePath, "input profiles.json")))
						File.Copy(Path.Combine(releasePath, "input profiles.json"), Path.Combine(newPath, "input profiles.json"));
				}
			}
		}

		private static void SetSavePath() {
			SavePath =
				LaunchParameters.ContainsKey("-savedirectory") ? LaunchParameters["-savedirectory"] :
				Platform.Get<IPathService>().GetStoragePath($"Terraria");

			bool saveHere = File.Exists("savehere.txt");
			bool tmlSaveDirectoryParameterSet = LaunchParameters.ContainsKey("-tmlsavedirectory");

			// File migration is only attempted for the default save folder
			if (!saveHere && !tmlSaveDirectoryParameterSet) {
				PortOldSaveDirectories();
				PortCommonFiles();
			}

			var fileFolder =
				BuildInfo.IsStable ? ReleaseFolder :
				BuildInfo.IsPreview ? PreviewFolder :
				DevFolder;

			SavePath = Path.Combine(SavePath, fileFolder);

			if (saveHere)
				SavePath = fileFolder; // Fallback for unresolveable antivirus/onedrive issues. Also makes the game portable I guess.

			SavePathShared = Path.Combine(SavePath, "..", ReleaseFolder);

			// With a custom tmlsavedirectory, the shared saves are assumed to be in the same folder
			if (tmlSaveDirectoryParameterSet) {
				SavePath = LaunchParameters["-tmlsavedirectory"];
				SavePathShared = SavePath;
			}
			
			Logging.tML.Info($"Save Are Located At: {Path.GetFullPath(SavePath)}");
		}
	}
}
