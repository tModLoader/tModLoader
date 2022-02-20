using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
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

			if (Directory.Exists(oldBetas)) {
				var newPath = Path.Combine(SavePath, PreviewFolder);
				Directory.Move(oldBetas, newPath);

				string[] subDirsToMove = { "Mod Reader", "Mod Sources", "Mod Configs" };
				foreach (var subDir in subDirsToMove) {
					if (Directory.Exists(Path.Combine(newPath, subDir)))
						Directory.Move(Path.Combine(newPath, subDir), Path.Combine(newPath, subDir.Replace(" ", "")));
				}

				if (Directory.Exists(Path.Combine(newPath, "Workshop")))
					Directory.Move(Path.Combine(newPath, "Workshop"), Path.Combine(SavePath, "Workshop"));

				FileUtilities.CopyFolder(newPath, Path.Combine(SavePath, DevFolder));
				FileUtilities.CopyFolder(newPath, Path.Combine(SavePath, ReleaseFolder));
			}
		}
	}
}
