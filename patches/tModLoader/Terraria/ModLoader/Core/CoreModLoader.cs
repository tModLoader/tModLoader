using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;

namespace Terraria.ModLoader.Core;
internal static class CoreModLoader
{

	private static ChildLoadContext _childALC;

	internal class ChildLoadContext : AssemblyLoadContext
	{
		public ChildLoadContext() : base(isCollectible: true) { }

		protected override Assembly Load(AssemblyName assemblyName) {
			return Default.LoadFromAssemblyName(assemblyName);
		}

		protected override IntPtr LoadUnmanagedDll(string unmanagedDllName) {
			Console.WriteLine("Loading unmanaged DLL: " + unmanagedDllName);
			return base.LoadUnmanagedDll(unmanagedDllName);
		}
	}

	internal static bool FindCoremods(string[] programArgs, out Mod[] coreMods)
	{
		coreMods = Array.Empty<Mod>();
		// Don't need to do a full initialization since we're looking for just coremod transformers
		Program.PreLaunchGame(programArgs, true, out _);
		ModLoader.MinimalEngineInit();

		LocalMod[] availableMods = ModOrganizer.FindMods(true);
		try {
			List<LocalMod> loadableMods = ModOrganizer.SelectAndSortMods(availableMods, CancellationToken.None, true);
			List<Mod> modInstances = AssemblyManager.InstantiateMods(loadableMods, CancellationToken.None);

			// COREMOD TESTING, ExampleMod will just be hardcoded check until transformers and actual coremod functionality is added
			coreMods = modInstances.Where(mod => mod.Name == "ExampleMod").ToArray();
			//
			return true;
		}
		catch {
			// TODO: Add error checking
		}

		return false;
	}

	internal static void LaunchALCWithCoremods(string[] programArgs, Mod[] coreMods)
	{
		_childALC = new ChildLoadContext();

		// TODO: Actually load transformers
		// For now, just unload the loaded mod ALCs, since after their transformers are applied they are just taking up space
		ModLoader.ClearMods();
		AssemblyManager.Unload();

		Assembly childTMLAssembly = _childALC.LoadFromAssemblyPath(typeof(CoreModLoader).Assembly.Location);

		Type childMonoLaunch = childTMLAssembly.GetType(typeof(MonoLaunch).FullName ?? "")!;
		childMonoLaunch.GetMethod(nameof(MonoLaunch.BeginEntrySequence), BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, new object?[] { programArgs });
	}
}
