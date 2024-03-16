using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using log4net;
using Mono.Cecil;
using Mono.Cecil.Cil;

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

	internal static bool FindCoreMods(string[] programArgs, out Mod[] coreMods)
	{
		coreMods = Array.Empty<Mod>();

		// Don't need to do a full initialization since we aren't going to be loading any "normal" mod content, just CoreMod transformers
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

	internal static void LaunchALCWithCoreMods(bool isServer, Mod[] coreMods)
	{
		_childALC = new ChildLoadContext();

		Logging.tML.InfoFormat("Applying CoreMod transformers...");
		Assembly transformedChildtML = ApplyTransformers(typeof(CoreModLoader).Assembly.Location, coreMods);
		Logging.tML.InfoFormat("Success! Transformed tML Child Assembly Created.");

		// For now, just unload the loaded mod ALCs, since after their transformers are applied they are just taking up space
		ModLoader.ClearMods();
		AssemblyManager.Unload();

		// Set Launch Params, Save Paths, and Main Thread
		Type childProgramType = transformedChildtML.GetType("Terraria.Program")!;
		childProgramType.GetField("LaunchParameters", BindingFlags.Public | BindingFlags.Static)!.SetValue(null, Program.LaunchParameters);
		childProgramType.GetField("SavePath", BindingFlags.Static | BindingFlags.Public)!.SetValue(null, Program.SavePath);
		childProgramType.GetProperty("SavePathShared", BindingFlags.Static | BindingFlags.Public)!.SetValue(null, Program.SavePathShared);
		childProgramType.GetProperty("MainThread", BindingFlags.Public | BindingFlags.Static)!.SetValue(null, Program.MainThread);

		// Set logging of child to be "tML_Child" for clarity's sake
		Type childLoggingType = transformedChildtML.GetType("Terraria.ModLoader.Logging")!;
		childLoggingType.GetProperty("tML", BindingFlags.NonPublic | BindingFlags.Static)!.SetValue(null, LogManager.GetLogger("tML_CHILD"));

		// Launch child ALC
		Logging.tML.InfoFormat("Launching Transformed Child tML...");
		childProgramType.GetMethod("LaunchGame_", BindingFlags.Public | BindingFlags.Static)!.Invoke(null, new object?[] { isServer });
	}

	private static Assembly ApplyTransformers(string assemblyLocation, Mod[] coreMods)
	{
		// TODO: Allow transformation of other mod assemblies
		using AssemblyDefinition childAssemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyLocation);
		childAssemblyDefinition.Name.Name += " (Transformed)";

		foreach (Mod coreMod in coreMods) {
			AssemblyManager.GetLoadableTypes(coreMod.Code)
			               .Where(t => !t.IsAbstract && !t.ContainsGenericParameters)
			               .Where(t => t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes) != null) // has default constructor
			               .Where(t => t.BaseType is { } baseType && baseType == typeof(ModuleTransformer))
			               .OrderBy(t => t.FullName, StringComparer.InvariantCulture)
			               .Select(t => (ModuleTransformer)Activator.CreateInstance(t, true))
			               .ToList().ForEach(transformer => transformer.Transform(childAssemblyDefinition.MainModule));
		}

		using MemoryStream assemblyStream = new MemoryStream();
		using MemoryStream assemblySymbolStream = new MemoryStream();
		childAssemblyDefinition.Write(assemblyStream);

		assemblyStream.Position = 0;
		return _childALC.LoadFromStream(assemblyStream);
	}
}
