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
		public static string SavePath { get; private set; } // Moved from Main to avoid triggering the Main static constructor before logging initializes

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
			JitForcedMethodCache = method.MethodHandle.GetFunctionPointer();
#endif

			Interlocked.Increment(ref ThingsLoaded);
		}

		private static void ForceStaticInitializers(Type[] types) {
			foreach (Type type in types) {
				if (!type.IsGenericType)
					RuntimeHelpers.RunClassConstructor(type.TypeHandle);
			}
		}
	}
}
