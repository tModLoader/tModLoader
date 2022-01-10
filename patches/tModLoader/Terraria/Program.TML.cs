using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Terraria
{
	public static partial class Program
	{
		const int HighDpiThreshold = 96;

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

		// Add Support for High DPI displays, such as Mac M1 laptops. Must run before Game constructor.
		private static void AttemptSupportHighDPI(bool isServer) {
			if (isServer)
				return;

			SDL2.SDL.SDL_VideoInit(null);
			SDL2.SDL.SDL_GetDisplayDPI(0, out var ddpi, out float hdpi, out float vdpi);
			if (ddpi >= HighDpiThreshold || hdpi >= HighDpiThreshold || vdpi >= HighDpiThreshold)
				Environment.SetEnvironmentVariable("FNA_GRAPHICS_ENABLE_HIGHDPI", "1");
		}
	}
}
