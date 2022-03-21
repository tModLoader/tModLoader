using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Terraria
{
	public static partial class Program
	{
		public static string SavePath { get; private set; } // Moved from Main to avoid triggering the Main static constructor before logging initializes

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
			// Port old file format users
			var oldBetas = Path.Combine(SavePath, "ModLoader/Beta");

			if (!Directory.Exists(oldBetas))
				return;

			var newPath = Path.Combine(SavePath, PreviewFolder);
			Directory.Move(oldBetas, newPath);

			string[] subDirsToMove = { "Mod Reader", "Mod Sources", "Mod Configs" };
			foreach (var subDir in subDirsToMove) {
				if (Directory.Exists(Path.Combine(newPath, subDir)))
					Directory.Move(Path.Combine(newPath, subDir), Path.Combine(newPath, subDir.Replace(" ", "")));
			}

			if (Directory.Exists(Path.Combine(newPath, "Workshop"))) {
				string workshopPath = Path.Combine(newPath, "Workshop", Steamworks.SteamUser.GetSteamID().m_SteamID.ToString());
				foreach (var repo in Directory.GetDirectories(workshopPath)) {
					var msNew = Path.Combine(ModLoader.Core.ModCompile.ModSourcePath, Path.GetDirectoryName(repo), "Workshop");

					foreach (var file in Directory.GetFiles(repo)) {
						File.Copy(file, Path.Combine(msNew, Path.GetFileName(file)));
					}
				}
			}
				
			FileUtilities.CopyFolder(newPath, Path.Combine(SavePath, DevFolder));
			FileUtilities.CopyFolder(newPath, Path.Combine(SavePath, ReleaseFolder));
		}

		private static void SetSavePath() {
			SavePath =
				LaunchParameters.ContainsKey("-savedirectory") ? LaunchParameters["-savedirectory"] :
				Platform.Get<IPathService>().GetStoragePath($"Terraria");

			PortOldSaveDirectories();

			var fileFolder =
				BuildInfo.IsStable ? ReleaseFolder :
				BuildInfo.IsPreview ? PreviewFolder :
				DevFolder;

			SavePath = Path.Combine(SavePath, fileFolder);

			if (File.Exists("savehere.txt"))
				SavePath = fileFolder; // Fallback for unresolveable antivirus/onedrive issues. Also makes the game portable I guess.

			if (LaunchParameters.ContainsKey("-tmlsavedirectory"))
				SavePath = LaunchParameters["-tmlsavedirectory"];

			Logging.tML.Info($"Save Are Located At: {SavePath}");
		}
	}
}
