using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;

namespace Terraria.ModLoader.Core;

internal static class CoreModLoader
{
	private class ChildLoadContext : AssemblyLoadContext
	{
		public ChildLoadContext() : base(isCollectible: true) { }

		protected override Assembly Load(AssemblyName assemblyName)
		{
			return _transformedAssemblies.TryGetValue(assemblyName.Name!, out Assembly assembly) ? assembly : Default.LoadFromAssemblyName(assemblyName);
		}
	}

	private static Dictionary<string, Assembly> _transformedAssemblies = new();

	internal static bool FindCoreMods(string[] args, out List<Mod> mods)
	{
		LocalMod[] available = ModOrganizer.FindMods(true);
		try {
			List<LocalMod> loadable = ModOrganizer.SelectAndSortMods(available, CancellationToken.None).Where(mod => mod.properties.coreMod).ToList();
			if (loadable.Count <= 0) {
				mods = new List<Mod>();
				return false;
			}

			mods = AssemblyManager.InstantiateMods(loadable, CancellationToken.None);
			return true;
		}
		catch {
			// TODO
		}

		mods = new List<Mod>();
		return false;
	}

	internal static void StartChildAlc(bool isServer, List<Mod> mods)
	{

	}
}
