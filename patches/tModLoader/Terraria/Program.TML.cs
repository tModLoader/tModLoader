using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
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

		/// <summary>
		/// Ensure sufficient stack size (4MB) on MacOS and Windows secondary threads, doesn't hurt for Linux. 16^5 = 1MB, value in hex 
		/// </summary>
		private static void EnsureMinimumStackSizeOnThreads() {
			// Doesn't work on .NET5, as the COMPlus_DefaultStackSize env var is used during runtime initialization (ie prior to reaching tML code)
			Environment.SetEnvironmentVariable("COMPlus_DefaultStackSize", "400000");
		}

		/*
		internal static void TestNewThreadSizeWindows() {
			[System.Runtime.InteropServices.DllImport("kernel32.dll")]
			static extern void GetCurrentThreadStackLimits(out uint lowLimit, out uint highLimit);

			uint low;
			uint high;

			GetCurrentThreadStackLimits(out low, out high);
			var size = (high - low) / 1024; // in kB
			ModLoader.Logging.tML.Debug("Thread Size: " + size + " kB");
		}
		*/
	}
}
