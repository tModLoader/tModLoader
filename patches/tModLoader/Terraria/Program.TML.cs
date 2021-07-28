using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Terraria.ModLoader;

namespace Terraria
{
	public static partial class Program
	{
		public static string SavePath { get; private set; } // moved from Main to avoid triggering the Main static constructor before logging initializes

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

		/// <summary>
		/// The Linker on Linux was discovered to be using the SDL2 on the system, instead of the portable included.
		/// Pointing the Linker to the correct location via LD_LIBRARY_PATH in .sh rectifies this, but also introduces SegFaults from SDL.
		/// A separately run app suggests through cross comparison that SDL2 is not initialized under this setup. 
		/// Thus we initialize it ourselves prior to FNA usage - Solxan
		/// The root issue comes from: https://github.com/dotnet/runtime/issues/34711
		/// </summary>
		private static void SDLFixUnix() {
			if (Platform.IsWindows)
				return; // Not applicable, windows uses the portable correctly.

			if (Platform.IsOSX)
				return; // Not observed as an issue.

			if (SDL2.SDL.SDL_INIT_EVERYTHING != SDL2.SDL.SDL_WasInit(SDL2.SDL.SDL_INIT_EVERYTHING)) {
				Logging.tML.Info("Ensuring SDL2 is initialized prior to FNA usage.");
				SDL2.SDL.SDL_Init(SDL2.SDL.SDL_INIT_EVERYTHING);
			}
		}

		private static void ForceStaticInitializers(Type[] types) {
			foreach (Type type in types) {
				if (!type.IsGenericType)
					RuntimeHelpers.RunClassConstructor(type.TypeHandle);
			}
		}
	}
}
