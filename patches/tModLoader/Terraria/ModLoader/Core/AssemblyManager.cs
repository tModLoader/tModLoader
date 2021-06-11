#if NETCORE
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Terraria.ModLoader.UI;
using System.Runtime.Loader;
using System.Runtime.CompilerServices;
using Terraria.Localization;

namespace Terraria.ModLoader.Core
{
	//todo: further documentation
	internal static class AssemblyManager
	{
		private class ModLoadContext : AssemblyLoadContext
		{
			public readonly TmodFile modFile;
			public readonly BuildProperties properties;

			public List<ModLoadContext> dependencies = new List<ModLoadContext>();

			public Assembly assembly;
			public IDictionary<string, (Assembly assembly, byte[] bytes)> assemblies = new Dictionary<string, (Assembly assembly, byte[] bytes)>();
			public long bytesLoaded = 0;

			public ModLoadContext(LocalMod mod) : base(mod.Name, true) {
				modFile = mod.modFile;
				properties = mod.properties;
			}

			public void AddDependency(ModLoadContext dep) {
				dependencies.Add(dep);
			}

			public void LoadAssemblies() {
				try {
					using (modFile.Open()) {
						if (Debugger.IsAttached && File.Exists(properties.eacPath)) {//load the unmodified dll and EaC pdb
							assembly = LoadAssembly(modFile.GetModAssembly(), File.ReadAllBytes(properties.eacPath));
							// todo: add path for dll resolution?
							return;
						}

						foreach (var dll in properties.dllReferences) {
							byte[] dllBytes = modFile.GetBytes("lib/" + dll + ".FNA.dll") ??
							                  modFile.GetBytes("lib/" + dll + ".dll");

							LoadAssembly(dllBytes);
						}

						assembly = LoadAssembly(modFile.GetModAssembly(), modFile.GetModPdb());
					}
				}
				catch (Exception e) {
					e.Data["mod"] = Name;
					throw;
				}
			}

			private Assembly LoadAssembly(byte[] code, byte[] pdb = null) {
				using var codeStrm = new MemoryStream(code, false);
				using var pdbStrm = pdb == null ? null : new MemoryStream(pdb, false);
				var asm = LoadFromStream(codeStrm, pdbStrm);

				assemblies[asm.GetName().Name] = (asm, code);
				hostContextForAssembly[asm] = this;

				bytesLoaded += code.LongLength + (pdb?.LongLength ?? 0);

				if (Program.LaunchParameters.ContainsKey("-dumpasm")) {
					var dumpdir = Path.Combine(Main.SavePath, "asmdump");
					Directory.CreateDirectory(dumpdir);
					File.WriteAllBytes(Path.Combine(dumpdir, asm.FullName+".dll"), code);
					if (pdb != null)
						File.WriteAllBytes(Path.Combine(dumpdir, asm.FullName+".pdb"), code);
				}

				return asm;
			}

			protected override Assembly Load(AssemblyName assemblyName) {
				if (assemblies.TryGetValue(assemblyName.Name, out var entry))
					return entry.assembly;

				return dependencies.Select(dep => dep.Load(assemblyName)).FirstOrDefault(a => a != null);
			}
		}


		[MethodImpl(MethodImplOptions.NoInlining)]
		internal static void Unload() 
		{
			foreach (var kv in loadedModContexts)
				kv.Value.Unload();

			hostContextForAssembly.Clear();

			for (int i = 0; loadedModContexts.Any(); i++) {
				if (i > 10) {
					Logging.tML.Warn($"{string.Join(", ", loadedModContexts.Select(kv => kv.Key))} refused to finalize");
					break;
				}
				// Beg the assemblies to finalize and cleanup
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}

			loadedModContexts.Clear();
		}

		private static readonly ConditionalWeakTable<string, ModLoadContext> loadedModContexts = new ConditionalWeakTable<string, ModLoadContext>();
		private static readonly IDictionary<Assembly, ModLoadContext> hostContextForAssembly = new Dictionary<Assembly, ModLoadContext>();

		//private static CecilAssemblyResolver cecilAssemblyResolver = new CecilAssemblyResolver();

		private static bool assemblyResolverAdded;
		internal static void AddAssemblyResolver() {
			if (assemblyResolverAdded)
				return;
			assemblyResolverAdded = true;

			AppDomain.CurrentDomain.AssemblyResolve += (_, args) => {
				string name = new AssemblyName(args.Name).Name;
				if (name == "Terraria" || name == "tModLoader" || name == "tModLoaderServer" || name == "tModLoaderDebug" || name == "tModLoaderServerDebug")
					return Assembly.GetExecutingAssembly();

				return null;
			};
		}

		private static Mod Instantiate(ModLoadContext mod) {
			try {
				VerifyMod(mod.Name, mod.assembly, out var modType);
				var m = (Mod)Activator.CreateInstance(modType);
				m.File = mod.modFile;
				m.Code = mod.assembly;
				m.Logger = LogManager.GetLogger(m.Name);
				m.Side = mod.properties.side;
				m.DisplayName = mod.properties.displayName;
				m.TModLoaderVersion = mod.properties.buildVersion;
				return m;
			}
			catch (Exception e) {
				e.Data["mod"] = mod.Name;
				throw;
			}
			finally {
				MemoryTracking.Update(mod.Name).code += mod.bytesLoaded;
			}
		}

		private static void VerifyMod(string modName, Assembly assembly, out Type modType) {
			var asmName = new AssemblyName(assembly.FullName).Name;
			if (asmName != modName)
				throw new Exception(Language.GetTextValue("tModLoader.BuildErrorModNameDoesntMatchAssemblyName", modName, asmName));

			// at least one of the types must be in a namespace that starts with the mod name
			if (!assembly.GetTypes().Any(t => t.Namespace?.StartsWith(modName) == true)) 
				throw new Exception(Language.GetTextValue("tModLoader.BuildErrorNamespaceFolderDontMatch"));

			var modTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Mod))).ToArray();
			if (modTypes.Length > 2)
				throw new Exception(modName + " has multiple classes extending Mod. Only one Mod per mod is supported at the moment");

			modType = modTypes.SingleOrDefault() ?? typeof(Mod); // Mods don't really need a class extending Mod, we can always just make one for them
		}

		internal static List<Mod> InstantiateMods(List<LocalMod> modsToLoad, CancellationToken token) {
			AddAssemblyResolver();

			var modList = modsToLoad.Select(m => new ModLoadContext(m)).ToList();
			foreach (var mod in modList)
				loadedModContexts.Add(mod.Name, mod);

			foreach (var mod in modList)
				foreach (var depName in mod.properties.RefNames(true))
					if (loadedModContexts.TryGetValue(depName, out var dep))
						mod.AddDependency(dep);

			// todo, assign an AssemblyLoadContext to each group of mods

			if (Debugger.IsAttached)
				ModCompile.activelyModding = true;

			try {
				// can no longer load assemblies in parallel due to cecil assembly resolver during ModuleDefinition.Write requiring dependencies
				// could use a topological parallel load but I doubt the performance is worth the development effort - Chicken Bones
				Interface.loadMods.SetLoadStage("tModLoader.MSSandboxing", modsToLoad.Count);
				int i = 0;
				foreach (var mod in modList) {
					token.ThrowIfCancellationRequested();
					Interface.loadMods.SetCurrentMod(i++, mod.Name);
					mod.LoadAssemblies();
				}

				//Assemblies must be loaded before any instantiation occurs to satisfy dependencies
				Interface.loadMods.SetLoadStage("tModLoader.MSInstantiating");
				MemoryTracking.Checkpoint();
				return modList.Select(mod => {
					token.ThrowIfCancellationRequested();
					return Instantiate(mod);
				}).ToList();
			}
			catch (AggregateException ae) {
				ae.Data["mods"] = ae.InnerExceptions.Select(e => (string)e.Data["mod"]).ToArray();
				throw;
			}
		}

		private static string GetModAssemblyFileName(this TmodFile modFile) {
			// legacy
			if (modFile.HasFile($"{modFile.Name}.FNA.dll"))
				return $"{modFile.Name}.FNA.dll";
			
			return $"{modFile.Name}.dll";
		}

		internal static byte[] GetModAssembly(this TmodFile modFile) => modFile.GetBytes(modFile.GetModAssemblyFileName());

		internal static byte[] GetModPdb(this TmodFile modFile) => modFile.GetBytes(Path.ChangeExtension(modFile.GetModAssemblyFileName(), "pdb"));

		private static ModLoadContext GetLoadContext(string name) => loadedModContexts.TryGetValue(name, out var value) ? value : throw new KeyNotFoundException(name);

		internal static IEnumerable<Assembly> GetModAssemblies(string name) => GetLoadContext(name).assemblies.Values.Select(v => v.assembly);

		internal static bool GetAssemblyOwner(Assembly assembly, out string modName) {
			if (hostContextForAssembly.TryGetValue(assembly, out var mod)) {
				modName = mod.Name;
				return true;
			}

			modName = null;
			return false;
		}

		internal static bool FirstModInStackTrace(StackTrace stack, out string modName) {
			for (int i = 0; i < stack.FrameCount; i++) {
				StackFrame frame = stack.GetFrame(i);
				var assembly = frame.GetMethod()?.DeclaringType?.Assembly;
				if (assembly != null && GetAssemblyOwner(assembly, out modName))
					return true;
			}

			modName = null;
			return false;
		}

		internal static IEnumerable<Mod> GetDependencies(Mod mod) => GetLoadContext(mod.Name).dependencies.Select(m => ModLoader.GetMod(mod.Name));
	}
}
#endif
