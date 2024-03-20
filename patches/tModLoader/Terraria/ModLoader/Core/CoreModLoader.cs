using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using log4net;
using Mono.Cecil;
using MonoMod.RuntimeDetour;

namespace Terraria.ModLoader.Core;
internal static class CoreModLoader
{
	// The same dictioanry is shared into the child ALC's instance of this field
	internal static Dictionary<Assembly, byte[]> transformedAssemblyBytes = new();

	private static Dictionary<string, Assembly> _transformedAssemblies = new();

	private static ChildLoadContext _childALC;

	private class ChildLoadContext : AssemblyLoadContext
	{
		public ChildLoadContext() : base(isCollectible: true) { }

		protected override Assembly Load(AssemblyName assemblyName)
		{
			return _transformedAssemblies.TryGetValue(assemblyName.Name!, out Assembly transformedAssembly) ? transformedAssembly : Default.LoadFromAssemblyName(assemblyName);
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
		ForceTypeConvertersToLookupConvertersInTheSameAssembly();

		_childALC = new ChildLoadContext();

		Logging.tML.InfoFormat("Applying CoreMod transformers...");
		AddTransformedAssemblies(GetAllDependentAssemblyLocations(), coreMods);
		Logging.tML.InfoFormat("Success! Transformed Assemblies created.");
		Assembly transformedChildtML = _transformedAssemblies[typeof(CoreModLoader).Assembly.GetName().Name!];

		// For now, just unload the loaded mod ALCs, since after their transformers are applied they are just taking up space
		ModLoader.ClearMods();
		AssemblyManager.Unload();

		transformedChildtML.GetType(typeof(CoreModLoader).FullName).GetField(nameof(CoreModLoader.transformedAssemblyBytes), BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, transformedAssemblyBytes);

		// Set Launch Params, Save Paths, Main Thread, tML Directory
		Type childProgramType = transformedChildtML.GetType(typeof(Program).FullName!)!;
		childProgramType.GetField(nameof(Program.LaunchParameters), BindingFlags.Public | BindingFlags.Static)!.SetValue(null, Program.LaunchParameters);
		childProgramType.GetField(nameof(Program.SavePath), BindingFlags.Static | BindingFlags.Public)!.SetValue(null, Program.SavePath);
		childProgramType.GetProperty(nameof(Program.SavePathShared), BindingFlags.Static | BindingFlags.Public)!.SetValue(null, Program.SavePathShared);
		childProgramType.GetProperty(nameof(Program.MainThread), BindingFlags.Public | BindingFlags.Static)!.SetValue(null, Program.MainThread);
		childProgramType.GetProperty(nameof(Program.tMLAssemblyLocation), BindingFlags.Static | BindingFlags.Public)!.SetValue(null, Program.tMLAssemblyLocation);

		// Set logging of child to be "tML_Child" for clarity's sake
		Type childLoggingType = transformedChildtML.GetType(typeof(Logging).FullName!)!;
		childLoggingType.GetProperty(nameof(Logging.tML), BindingFlags.NonPublic | BindingFlags.Static)!.SetValue(null, LogManager.GetLogger("tML_CHILD"));

		// Launch child ALC
		Logging.tML.InfoFormat("Launching Transformed Child tML...");
		childProgramType.GetMethod(nameof(Program.LaunchGame_), BindingFlags.Public | BindingFlags.Static)!.Invoke(null, new object?[] { isServer });
	}

	private static List<string> GetAllDependentAssemblyLocations()
	{
		string tmlAssemblyLocation = typeof(CoreModLoader).Assembly.Location;
		string libsDir = Path.Combine(Path.GetDirectoryName(tmlAssemblyLocation)!, "Libraries");

		// Load all dependent dlls, returning FNA & ReLogic dlls
		// TODO: Do we allow more dlls for CoreMod transformation?
		return new List<string>() { tmlAssemblyLocation }.Concat(Directory.EnumerateFiles(libsDir, "*.dll", SearchOption.AllDirectories).Where(path =>
		{
			string fileName = Path.GetFileName(path);

			return fileName is "ReLogic.dll" or "FNA.dll" or "TerrariaHooks.dll";
			/*
			 return !(path.EndsWith(".resources.dll")
			            || path.Contains(@"\Native\")
			            || path.Contains("\\runtime")
					    || fileName.StartsWith("system.", true, null)
			            || fileName.StartsWith("basic.", true, null)
			            || fileName.StartsWith("microsoft.", true, null)
			    );
			 */
		})).ToList();
	}

	private static void AddTransformedAssemblies(List<string> assemblyLocations, Mod[] coreMods)
	{
		// TODO: Allow transformation of other mod assemblies
		foreach (Mod coreMod in coreMods) {
			List<ModuleTransformer> transformers =
				AssemblyManager.GetLoadableTypes(coreMod.Code)
				               .Where(t => !t.IsAbstract && !t.ContainsGenericParameters)
				               .Where(t => t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes) != null) // has default constructor
				               .Where(t => t.BaseType is { } baseType && baseType == typeof(ModuleTransformer))
				               .OrderBy(t => t.FullName, StringComparer.InvariantCulture)
				               .Select(t => (ModuleTransformer)Activator.CreateInstance(t, true))
				               .ToList();

			foreach (string assemblyLocation in assemblyLocations) {
				using AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyLocation);

				// Apply transformers
				transformers.ForEach(transformer => transformer.Transform(assemblyDefinition.MainModule));

				// Write to stream, which is then loaded to actual assembly. Skips the intermediary step of writing to a file instead, then immediately loading said file
				using MemoryStream assemblyStream = new MemoryStream();
				assemblyDefinition.Write(assemblyStream);

				assemblyStream.Position = 0;
				Assembly transformedAssembly = _childALC.LoadFromStream(assemblyStream);
				_transformedAssemblies[transformedAssembly.GetName().Name!] = transformedAssembly;

				transformedAssemblyBytes[transformedAssembly] = assemblyStream.ToArray();
			}
		}
	}

	private static Hook _typeConverterAttrHook;
	private static void ForceTypeConvertersToLookupConvertersInTheSameAssembly()
	{
		// Fixes issue where the TypeConverter on classes like FNA's Color [TypeConverter(typeof(ColorConverter))] is loaded from the root ALC and is thus incompatible with the type containing the attribute
		//
		// TypeConverterAttribute only stores the AssemblyQualifiedName of the converter type
		// This name is then resolved via Type.GetType which resolves the assembly in the root ALC
		// If we instead use the FullName of the type, System.ComponentModel has a fallback resolver using the assembly the attribute is defined on (which is what we want)
		//
		// See https://github.com/dotnet/runtime/blob/main/src/libraries/System.ComponentModel.TypeConverter/src/System/ComponentModel/ReflectTypeDescriptionProvider.ReflectedTypeData.cs#L507

		_typeConverterAttrHook = new Hook(typeof(TypeConverterAttribute).GetConstructor(new Type[] { typeof(Type) }),
			new Action<Action<TypeConverterAttribute, Type>, TypeConverterAttribute, Type>((orig, target, type) => {
				typeof(TypeConverterAttribute).GetConstructor(new Type[] { typeof(string) }).Invoke(target, new object[] { type.FullName });
			}));
	}
}
