using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using log4net;

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
	}

	internal static bool FindCoremods(string[] programArgs, out Mod[] coreMods)
	{
		coreMods = Array.Empty<Mod>();
		// Don't need to do a full initialization since we're looking for just coremod transformers
		ModLoader.MinimalEngineInit();

		LocalMod[] availableMods = ModOrganizer.FindMods(true);
		try {
			List<LocalMod> loadableCoreMods = ModOrganizer.SelectAndSortMods(availableMods, CancellationToken.None, true).Where(mod => mod.properties.hasCoreModTransformers).ToList();
			if (loadableCoreMods.Count <= 0) {
				return false;
			}

			coreMods = AssemblyManager.InstantiateMods(loadableCoreMods, CancellationToken.None).ToArray();
			return true;
		}
		catch {
			// TODO: Add error checking
		}

		return false;
	}

	internal static void LaunchALCWithCoremods(bool isServer, Mod[] coreMods)
	{
		_childALC = new ChildLoadContext();

		// TODO: Actually load transformers
		// For now, just unload the loaded mod ALCs, since after their transformers are applied they are just taking up space
		ModLoader.ClearMods();
		AssemblyManager.Unload();

		Assembly childTMLAssembly = _childALC.LoadFromAssemblyPath(typeof(CoreModLoader).Assembly.Location);

		// Set Launch Params, Save Paths, and Main Thread
		Type childProgramType = childTMLAssembly.GetType(typeof(Program).FullName!)!;
		childProgramType.GetField(nameof(Program.LaunchParameters), BindingFlags.Public | BindingFlags.Static)!.SetValue(null, Program.LaunchParameters);
		childProgramType.GetField(nameof(Program.SavePath), BindingFlags.Static | BindingFlags.Public)!.SetValue(null, Program.SavePath);
		childProgramType.GetProperty(nameof(Program.SavePathShared), BindingFlags.Static | BindingFlags.Public)!.SetValue(null, Program.SavePathShared);
		childProgramType.GetProperty(nameof(Program.MainThread), BindingFlags.Public | BindingFlags.Static)!.SetValue(null, Program.MainThread);

		// Set logging of child to be "tML_Child" for clarity's sake
		Type childLoggingType = childTMLAssembly.GetType(typeof(Logging).FullName!)!;
		childLoggingType.GetProperty(nameof(Logging.tML), BindingFlags.NonPublic | BindingFlags.Static)!.SetValue(null, LogManager.GetLogger("tML_CHILD"));

		// Launch child ALC
		Logging.tML.InfoFormat("Launching Child tML...");
		childProgramType.GetMethod(nameof(Program.LaunchGame_), BindingFlags.Public | BindingFlags.Static)!.Invoke(null, new object?[] { isServer });
	}
}
