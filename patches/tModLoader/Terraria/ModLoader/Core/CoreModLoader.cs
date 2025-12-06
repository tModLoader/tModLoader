//#define LOAD_UNTRANSFORMED_ASSEMBLIES_TO_KEEP_DEBUGGER_HAPPY_TEMPORARILY

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using log4net;
using Microsoft.VisualBasic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;

namespace Terraria.ModLoader.Core;
internal static class CoreModLoader
{
	private record struct AssemblyTransformationCandidate(AssemblyDefinition Definition, bool HasSymbols = true, string ModName = null, bool WasTransformed = false)
	{
		public static implicit operator AssemblyTransformationCandidate(AssemblyDefinition definition) => new(definition);

		public static implicit operator AssemblyDefinition(AssemblyTransformationCandidate candidate) => candidate.Definition;
	}

	// The same dictionary is shared into the child ALC's instance of this field
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

	internal static bool IsAnyCoreMods(out Mod[] allMods, out Mod[] coreMods)
	{
		allMods = [];
		coreMods = [];

		// Don't need to do a full initialization since we aren't going to be loading any "normal" mod content, just CoreMod transformers and mod assemblies
		ModLoader.MinimalEngineInit();

		LocalMod[] availableMods = ModOrganizer.FindMods();
		try {
			List<LocalMod> localMods = ModOrganizer.SelectAndSortMods(availableMods, CancellationToken.None, true);
			if (!localMods.Any(mod => mod.properties.hasCoreModTransformers)) {
				return false;
			}

			allMods = AssemblyManager.InstantiateMods(localMods, CancellationToken.None).ToArray();
			coreMods = allMods.Where(mod => mod.HasCoreModTransformers).ToArray();
			return true;
		}
		catch {
			// TODO: Add error checking
		}

		return false;
	}

	internal static bool LaunchALCWithCoreMods(bool isServer, Mod[] allMods, Mod[] coreMods)
	{
		ForceTypeConvertersToLookupConvertersInTheSameAssembly();

		_childALC = new ChildLoadContext();

		Logging.tML.InfoFormat("Getting assemblies that can be transformed...");
		List<string> tModLoaderDependencyAssemblyLocations = GetAllDependentAssemblyLocations();

		Logging.tML.InfoFormat("Applying Core Mod transformers...");

		if (!AddTransformedAssemblies(tModLoaderDependencyAssemblyLocations, allMods, coreMods)) {
			return false;
		};

		Logging.tML.InfoFormat("Success! Transformed Assemblies created.");

		Assembly transformedChildtML = _transformedAssemblies[typeof(CoreModLoader).Assembly.GetName().Name!];

		// For now, just unload the loaded mod ALCs, since after their transformers are applied they are just taking up space
		Logging.tML.InfoFormat("Clearing & unloading all loaded CoreMods...");
		ModLoader.ClearMods();
		AssemblyManager.Unload();

		transformedChildtML.GetType(typeof(CoreModLoader).FullName!)!.GetField(nameof(transformedAssemblyBytes), BindingFlags.Static | BindingFlags.NonPublic)!.SetValue(null, transformedAssemblyBytes);

		// Set Launch Params, Save Paths, Main Thread, tML Directory
		Type childProgramType = transformedChildtML.GetType(typeof(Program).FullName!)!;

		Logging.tML.InfoFormat("Initializing necessary child tML fields...");
		childProgramType.GetField(nameof(Program.LaunchParameters), BindingFlags.Public | BindingFlags.Static)!.SetValue(null, Program.LaunchParameters);
		childProgramType.GetField(nameof(Program.SavePath), BindingFlags.Static | BindingFlags.Public)!.SetValue(null, Program.SavePath);
		childProgramType.GetProperty(nameof(Program.SavePathShared), BindingFlags.Static | BindingFlags.Public)!.SetValue(null, Program.SavePathShared);
		childProgramType.GetProperty(nameof(Program.MainThread), BindingFlags.Public | BindingFlags.Static)!.SetValue(null, Program.MainThread);
		childProgramType.GetProperty(nameof(Program.tMLAssemblyLocation), BindingFlags.Static | BindingFlags.Public)!.SetValue(null, Program.tMLAssemblyLocation);

		// Set logging of child to be "tML_Child" for clarity's sake
		Type childLoggingType = transformedChildtML.GetType(typeof(Logging).FullName!)!;
		Logging.tML.InfoFormat("Initializing child tML Logging...");
		childLoggingType.GetProperty(nameof(Logging.tML), BindingFlags.NonPublic | BindingFlags.Static)!.SetValue(null, LogManager.GetLogger("tML_CHILD"));

		// Launch child ALC
		_childALC.ResolvingUnmanagedDll += MonoLaunch.ResolveNativeLibrary;

		Logging.tML.InfoFormat("----====---- LAUNCHING TRANSFORMED CHILD TML ----====----");
		childProgramType.GetMethod(nameof(Program.LaunchGame_), BindingFlags.Public | BindingFlags.Static)!.Invoke(null, [ isServer ]);
		return true;
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

	private static bool AddTransformedAssemblies(List<string> dependentAssemblyLocations, Mod[] allMods, Mod[] coreMods)
	{
		List<AssemblyTransformationCandidate> allAssemblyCandidates = [];
		// Load from file directly
		foreach (string assemblyLocation in dependentAssemblyLocations) {
			bool hasSymbols = File.Exists(Path.ChangeExtension(assemblyLocation, ".pdb"));
			// AssemblyDefinition internally handles streams/byte data, so no needing to persist anything
			allAssemblyCandidates.Add(AssemblyDefinition.ReadAssembly(assemblyLocation, new ReaderParameters { ReadSymbols = hasSymbols }));
		}

		// Load mod assemblies into streams, which then can be put into assembly definitions
		foreach (Mod mod in allMods) {
			using (mod.File.Open()) {
				TmodFile modFile = mod.File;

				// Cecil holds onto these streams by placing them into the definitions and lazy reading/writing, so they CANNOT be cleaned up until we're done with the definitions
				var assemblyStream = new MemoryStream(modFile.GetModAssembly(), true);

				bool hasSymbols = modFile.HasFile(modFile.GetModPdbFileName());
				var readerParameters = new ReaderParameters { ReadSymbols = hasSymbols};
				if (hasSymbols) {
					readerParameters.SymbolStream = new MemoryStream(modFile.GetModPdb(), true);
				}

				allAssemblyCandidates.Add(
					new AssemblyTransformationCandidate(
							AssemblyDefinition.ReadAssembly(assemblyStream, readerParameters),
							hasSymbols,
							mod.Name
						)
					);
			}
		}

		foreach (AssemblyDefinition assemblyDefinition in allAssemblyCandidates) {
			// May or may not be required. Haven't got line numbers in stack traces or VS debugging to work yet -- CB
			assemblyDefinition.MainModule.Mvid = Guid.NewGuid();
		}

		bool anyModLoaderDependencyAssemblyTransformed = false;
		foreach (Mod coreMod in coreMods) {
			List<ModuleTransformer> transformers =
				AssemblyManager.GetLoadableTypes(coreMod.Code)
				               .Where(t => !t.IsAbstract && !t.ContainsGenericParameters)
				               .Where(t => t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes) != null) // has default constructor
				               .Where(t => t.BaseType is { } baseType && baseType == typeof(ModuleTransformer))
				               .OrderBy(t => t.FullName, StringComparer.InvariantCulture)
				               .Select(t => (ModuleTransformer)Activator.CreateInstance(t, true))
				               .ToList();


			Logging.tML.InfoFormat("Starting \"{0}\"'s transformation process.", coreMod.Name);
			for (int i = 0; i < allAssemblyCandidates.Count; i++) {
				(AssemblyDefinition definition, bool _, string assemblyModName, bool _) = allAssemblyCandidates[i];

				// Core Mods cannot modify themselves
				// tML dependency candidates have their ModName field set to null
				if (assemblyModName is not null && assemblyModName == coreMod.Name) {
					continue;
				}

				// Apply transformers
				foreach (ModuleTransformer transformer in transformers) {
					if (!transformer.Transform(definition.MainModule)) {
						continue;
					}

					allAssemblyCandidates[i] = allAssemblyCandidates[i] with { WasTransformed = true };

					if (assemblyModName is null) {
						anyModLoaderDependencyAssemblyTransformed = true;
					}

					Logging.tML.InfoFormat("{0} successfully applied transformer on {1}.", coreMod.Name, definition.Name);
				}
			}
		}

		// Generate assemblies from all candidates that were successfully transformed
		// If any tML dependency assemblies were transformed, ALL others will be psuedo-transformed and added to the transformed dictionary above.
		// TODO: Figure out why the above line/explanation ^ is necessary
		bool anyAssembliesTransformed = false;
		foreach ((AssemblyDefinition definition, bool hasSymbols, string modName, bool wasTransformed) in allAssemblyCandidates) {
			if ((modName is null && !wasTransformed && !anyModLoaderDependencyAssemblyTransformed) || (modName is not null && !wasTransformed)) {
				definition.Dispose();
				continue;
			}

			// Write to stream, which is then loaded to actual assembly. Skips the intermediary step of writing to a file instead, then immediately loading said file
			using var assemblyStream = new MemoryStream();
			using var symbolStream = new MemoryStream();

			definition.Write(assemblyStream, new WriterParameters { WriteSymbols = hasSymbols, SymbolStream = symbolStream, SymbolWriterProvider = new PortablePdbWriterProvider() });

			assemblyStream.Position = symbolStream.Position = 0;

			#if LOAD_UNTRANSFORMED_ASSEMBLIES_TO_KEEP_DEBUGGER_HAPPY_TEMPORARILY
				assemblyStream.SetLength(0);
				symbolStream.SetLength(0);
				assemblyStream.Write(File.ReadAllBytes(assemblyLocation));
				if (hasSymbols)
					symbolStream.Write(File.ReadAllBytes(Path.ChangeExtension(assemblyLocation, ".pdb")));
				assemblyStream.Position = 0;
				symbolStream.Position = 0;
			#endif

			// TODO: Persist transformed mod assemblies to the child tML ALC
			Assembly transformedAssembly = _childALC.LoadFromStream(assemblyStream, symbolStream);
			_transformedAssemblies[transformedAssembly.GetName().Name!] = transformedAssembly;

			transformedAssemblyBytes[transformedAssembly] = assemblyStream.ToArray();

			anyAssembliesTransformed = true;

			definition.Dispose();
		}

		return anyAssembliesTransformed;
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
