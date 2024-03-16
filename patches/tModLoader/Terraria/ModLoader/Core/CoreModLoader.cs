using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using Mono.Cecil;

namespace Terraria.ModLoader.Core;

internal static class CoreModLoader
{
	private class ChildLoadContext : AssemblyLoadContext
	{
		public ChildLoadContext() : base(isCollectible: true) { }

		protected override Assembly Load(AssemblyName assemblyName)
		{
			// If this is part of the standard library or is a System.* DLL loaded in the wider load context, just use that.
			// If it isn't already loaded, tough luck; try to load it in this child ALC instead so it can get transformed.
			if (IsStandard(assemblyName)) {
				if (Default.Assemblies.FirstOrDefault(x => x.GetName().Name == assemblyName.Name) is { } standardAsm)
					return standardAsm;
			}

			if (_transformedAssemblies.TryGetValue(assemblyName.Name!, out Assembly assembly))
				return assembly;

			try {
				return LoadAndTransformAssembly(assemblyName);
			}
			catch(Exception e) {
				Logging.tML.Error("Failed to load (and transform) assembly", e);
				return Default.LoadFromAssemblyName(assemblyName);
			}
		}

		private static Assembly LoadAndTransformAssembly(AssemblyName name)
		{
			if (_transformedAssemblies.TryGetValue(name.Name!, out Assembly assembly))
				return assembly;

			if (!_assemblyNamePathCache.TryGetValue(name.Name, out string assemblyPath)) {
				// TODO
				throw new Exception();
			}

			// TODO: Proper resolution...
			using AssemblyDefinition asmDef = AssemblyDefinition.ReadAssembly(assemblyPath);

			bool transformed = false;
			foreach (ModuleTransformer transformer in _transformers)
				transformed |= transformer.Transform(asmDef.MainModule);

			if (!transformed) {
				// For already-loaded assemblies aside from tModLoader...
				if (name.Name != "tModLoader" && Default.Assemblies.FirstOrDefault(x => x.GetName().Name == name.Name) is { } alreadyLoadedAsm)
					return alreadyLoadedAsm;

				Assembly a = _context.LoadFromAssemblyPath(Path.GetFullPath(assemblyPath));
				return a;
			}

			using MemoryStream ms = new MemoryStream();
			asmDef.Write(ms);
			ms.Position = 0;
			return _transformedAssemblies[name.Name] = _context.LoadFromStream(ms);
		}

		private static bool IsStandard(AssemblyName name)
		{
			return name.Name == "mscorlib" || name.Name.StartsWith("System.");
		}
	}

	private static string _virtualLocation;
	public static string VirtualLocation {
		get => _virtualLocation ??= typeof(CoreModLoader).Assembly.Location;
		private set => _virtualLocation = value;
	}

	public static bool CoreModChild { get; private set; }

	private static readonly Dictionary<string, Assembly> _transformedAssemblies = new();
	private static readonly ChildLoadContext _context = new();
	private static readonly Dictionary<string, string> _assemblyNamePathCache = new();
	private static List<ModuleTransformer> _transformers = new();

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
		Logging.tML.Info("Initializing coremods...");
		foreach (Mod mod in mods) {
			_transformers = AssemblyManager.GetLoadableTypes(mod.Code)
				.OrderBy(type => type.FullName)
				.Where(type => !type.IsAbstract && !type.ContainsGenericParameters)
				.Where(type => type.IsSubclassOf(typeof(ModuleTransformer)))
				.Where(type => type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes) != null)
				.Select(type => (ModuleTransformer)Activator.CreateInstance(type))
				.ToList();
		}

		Logging.tML.Info("Building assembly name <-> assembly path cache...");
		string tmlPath = typeof(CoreModLoader).Assembly.Location;
		string libDir = Path.Combine(Path.GetDirectoryName(tmlPath)!, "Libraries");
		foreach (string candidate in new[] { tmlPath }.Concat(Directory.EnumerateFiles(libDir, "*.dll", SearchOption.AllDirectories))) {
			string fileName = Path.GetFileName(candidate);
			if (candidate.Contains("Native") || candidate.Contains("runtime") || fileName.StartsWith("System."))
				continue;

			try {
				using AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(candidate);
				_assemblyNamePathCache[asm.Name.Name] = fileName;
			}
			catch { }
		}

		Logging.tML.Info("Entering child ALC...");

		Assembly tml = _context.LoadFromAssemblyName(typeof(CoreModLoader).Assembly.GetName());
		Type coreModLoader = GetType(typeof(CoreModLoader));
		SetProperty(coreModLoader, nameof(VirtualLocation), null, tmlPath);
		SetProperty(coreModLoader, nameof(CoreModChild), null, true);

		Type program = GetType(typeof(Program));
		SetField(program, nameof(Program.LaunchParameters), null, Program.LaunchParameters);
		SetField(program, nameof(Program.SavePath), null, Program.SavePath);
		SetProperty(program, nameof(Program.SavePathShared), null, Program.SavePathShared);
		SetProperty(program, nameof(Program.MainThread), null, Program.MainThread);
		CallMethod(program, nameof(Program.LaunchGame_), null, isServer);

		Logging.tML.Info("Exited child ALC!");

		void SetProperty(Type type, string name, object instance, object value)
		{
			PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
			property.SetValue(instance, value);
		}

		void SetField(Type type, string name, object instance, object value)
		{
			FieldInfo field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
			field.SetValue(instance, value);
		}

		Type GetType(Type type)
		{
			return tml.GetType(type.FullName);
		}

		void CallMethod(Type type, string name, object instance, params object[] args)
		{
			type.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).Invoke(instance, args);
		}
	}
}
