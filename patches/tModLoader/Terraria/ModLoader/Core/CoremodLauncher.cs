using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Terraria.ModLoader.Core
{
	/// <summary>
	///		Handles creating an <see cref="AssemblyLoadContext"/> with modified (coremodded) assemblies in which the game should be played.
	/// </summary>
	internal static class CoremodLauncher
	{
		/// <summary>
		///		An <see cref="AssemblyLoadContext"/> which modifies resolved assemblies before using them.
		/// </summary>
		internal class CoremodLoadContext : AssemblyLoadContext
		{
			protected override Assembly Load(AssemblyName assemblyName) {
				return Default.LoadFromAssemblyName(assemblyName);
			}

			protected override IntPtr LoadUnmanagedDll(string unmanagedDllName) {
				Console.WriteLine("Loading unmanaged DLL: " + unmanagedDllName);
				return base.LoadUnmanagedDll(unmanagedDllName);
			}
		}

		public const string SkipCoremods = "-skip-coremods";
		public const string AppliedCoremods = "-applied-coremods";
		private const string CoremodLockFileName = "coremod.lock";

		internal static CoremodLoadContext ChildLoadContext;

		/// <summary>
		///
		/// </summary>
		/// <param name="args">The program arguments that should be passed.</param>
		internal static void LaunchWithCoremods(string[] args) {
			args = args.Concat(new[] {AppliedCoremods}).ToArray();

			ChildLoadContext = new CoremodLoadContext();

			Assembly tmlAsm = ChildLoadContext.LoadFromAssemblyPath(typeof(CoremodLauncher).Assembly.Location);

			// TODO: Do we need to bother with null checking here and throwing if it's null?

			// Initialize inter-ALC communications.
			Type coremodCommunications = tmlAsm.GetType(typeof(CoremodCommunications).FullName!);
			MethodInfo initializeFromparent = coremodCommunications?.GetMethod(nameof(CoremodCommunications.InitializeFromParent), BindingFlags.NonPublic | BindingFlags.Static);
			initializeFromparent?.Invoke(null, new object[] {typeof(CoremodCommunications)});

			// Launch the game.
			Type monoLaunch = tmlAsm.GetType(nameof(MonoLaunch));
			MethodInfo main = monoLaunch?.GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Static);
			main?.Invoke(null, new object[] {args});
		}

		private static bool CanCoremodsLoad() {
			return !File.Exists(CoremodLockFileName);
		}
	}
}