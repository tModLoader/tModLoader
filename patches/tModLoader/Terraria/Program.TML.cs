using ReLogic.IO;
using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Terraria.Initializers;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Exceptions;
using Terraria.Social;
using Terraria.Utilities;

namespace Terraria
{
	public static partial class Program
	{
		public static string SavePath { get; private set; } // moved from Main to avoid triggering the Main static constructor before logging initializes

		private static IEnumerable<MethodInfo> GetMethodsCrossPlatform(Type type) {
#if WINDOWS
			return type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
#else
			return type.GetMethods();
#endif
		}

		private static IEnumerable<MethodInfo> CollectMethodsToJIT(IEnumerable<Type> types) =>
			from type in types
			from method in GetMethodsCrossPlatform(type)
			where !method.IsAbstract && !method.ContainsGenericParameters && method.GetMethodBody() != null
			select method;

		private static void ForceJITOnMethod(MethodInfo method) {
#if WINDOWS
			RuntimeHelpers.PrepareMethod(method.MethodHandle);
#else
			// We don't synchronize access to JitForcedMethodCache because no one ever needs to read its value.
			var methodPointer = method.MethodHandle.GetFunctionPointer();
			JitForcedMethodCache = methodPointer;
#endif

			Interlocked.Increment(ref ThingsLoaded);
		}

		private static void ForceStaticInitializers(Assembly assembly) {
			Type[] types = assembly.GetTypes();

			foreach (Type type in types) {
				if (!type.IsGenericType)
					RuntimeHelpers.RunClassConstructor(type.TypeHandle);
			}
		}
	}
}
