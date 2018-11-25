using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader
{
	//todo: further documentation
	internal class AssemblyManager
	{
		private class LoadedMod
		{
			public TmodFile modFile;
			public BuildProperties properties;
			public string Name => modFile.name;

			public readonly List<LoadedMod> dependencies = new List<LoadedMod>();
			public readonly List<LoadedMod> dependents = new List<LoadedMod>();
			//list of weak dependencies that are not currently loaded
			//weak dependencies assume loadIndex 0 when they come into being
			public readonly ISet<string> weakDependencies = new HashSet<string>();

			public Assembly assembly;
			public List<Assembly> assemblies = new List<Assembly>();

			private int loadIndex;
			private bool eacEnabled;

			private bool _needsReload = true;

			private bool NeedsReload
			{
				get { return _needsReload; }
				set
				{
					if (value && !_needsReload) loadIndex++;
					_needsReload = value;
				}
			}

			private string AssemblyName => eacEnabled ? Name : Name + '_' + loadIndex;
			private string DllName(string dll) => eacEnabled ? dll : Name + '_' + dll + '_' + loadIndex;
			private string WeakDepName(string depName) => eacEnabled ? depName : depName + "_0";

			public void SetMod(LocalMod mod)
			{
				if (modFile == null ||
					modFile.version != mod.modFile.version ||
					!modFile.hash.SequenceEqual(mod.modFile.hash))
					SetNeedsReload();

				modFile = mod.modFile;
				properties = mod.properties;
			}

			private void SetNeedsReload()
			{
				NeedsReload = true;
				eacEnabled = false;

				foreach (var dep in dependents)
					dep.SetNeedsReload();
			}

			public void AddDependency(LoadedMod dep)
			{
				dependencies.Add(dep);
				dep.dependents.Add(this);
			}

			public bool CanEaC => eacEnabled ||
				!loadedAssemblies.ContainsKey(modFile.name) && dependencies.All(dep => dep.CanEaC);

			public void EnableEaC()
			{
				if (eacEnabled)
					return;

				SetNeedsReloadUnlessEaC();
				eacEnabled = true;

				//all dependencies need to have unmodified names
				foreach (var dep in dependencies)
					dep.EnableEaC();
			}

			private void SetNeedsReloadUnlessEaC()
			{
				if (!eacEnabled)
					NeedsReload = true;

				foreach (var dep in dependents)
					dep.SetNeedsReloadUnlessEaC();
			}

			public void UpdateWeakRefs()
			{
				foreach (var loaded in dependencies.Where(dep => weakDependencies.Remove(dep.Name)))
				{
					if (eacEnabled && !loaded.eacEnabled)
						loaded.EnableEaC();
					else if (loaded.AssemblyName != WeakDepName(loaded.Name))
						SetNeedsReload();
				}
			}

			public void LoadAssemblies()
			{
				if (!NeedsReload)
					return;

				try
				{
					using (modFile.EnsureOpen()) {
						foreach (var dll in properties.dllReferences)
							LoadAssembly(EncapsulateReferences(modFile.GetBytes("lib/" + dll + ".dll")));

						assembly = LoadAssembly(EncapsulateReferences(modFile.GetMainAssembly()), modFile.GetMainPDB());
						NeedsReload = false;
					}
				}
				catch (Exception e)
				{
					e.Data["mod"] = Name;
					throw;
				}
			}

			private byte[] EncapsulateReferences(byte[] code)
			{
				if (eacEnabled)
					return code;

				var asm = AssemblyDefinition.ReadAssembly(new MemoryStream(code), new ReaderParameters { AssemblyResolver = TerrariaCecilAssemblyResolver.instance });
				asm.Name.Name = EncapsulateName(asm.Name.Name);

				//randomize the module version id so that the debugger can detect it as a different module (even if it has the same content)
				asm.MainModule.Mvid = Guid.NewGuid();

				foreach (var mod in asm.Modules)
					foreach (var asmRef in mod.AssemblyReferences)
						asmRef.Name = EncapsulateName(asmRef.Name);

				var ret = new MemoryStream();
				asm.Write(ret, new WriterParameters { SymbolWriterProvider = SymbolWriterProvider.instance });
				return ret.ToArray();
			}

			private string EncapsulateName(string name)
			{
				if (name == Name)
					return AssemblyName;

				if (properties.dllReferences.Contains(name))
					return DllName(name);

				if (weakDependencies.Contains(name))
					return WeakDepName(name);

				foreach (var dep in dependencies)
				{
					var _name = dep.EncapsulateName(name);
					if (_name != name)
						return _name;
				}

				return name;
			}

			private Assembly LoadAssembly(byte[] code, byte[] pdb = null)
			{
				var asm = Assembly.Load(code, pdb);
				assemblies.Add(asm);
				loadedAssemblies[asm.GetName().Name] = asm;
				assemblyBinaries[asm.GetName().Name] = code;
				hostModForAssembly[asm] = this;
				return asm;
			}
		}

		internal static void AssemblyResolveEarly(ResolveEventHandler handler)
		{
			var f = typeof(AppDomain).GetField(ModLoader.windows ? "_AssemblyResolve" : "AssemblyResolve", BindingFlags.Instance | BindingFlags.NonPublic);
			var a = (ResolveEventHandler)f.GetValue(AppDomain.CurrentDomain);
			f.SetValue(AppDomain.CurrentDomain, null);

			AppDomain.CurrentDomain.AssemblyResolve += handler;
			AppDomain.CurrentDomain.AssemblyResolve += a;
		}

		private static readonly IDictionary<string, LoadedMod> loadedMods = new Dictionary<string, LoadedMod>();
		private static readonly IDictionary<string, Assembly> loadedAssemblies = new ConcurrentDictionary<string, Assembly>();

		private static readonly IDictionary<string, byte[]> assemblyBinaries = new ConcurrentDictionary<string, byte[]>();
		private static readonly IDictionary<Assembly, LoadedMod> hostModForAssembly = new ConcurrentDictionary<Assembly, LoadedMod>();

		static AssemblyManager()
		{
			AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
			{
				string name = new AssemblyName(args.Name).Name;

				if (name == "Terraria")
					return Assembly.GetExecutingAssembly();

				Assembly a;
				loadedAssemblies.TryGetValue(name, out a);
				return a;
			};

			// allow mods which reference embedded assemblies to reference a different version and be safely upgraded
			AssemblyResolveEarly((sender, args) => {
				var name = new AssemblyName(args.Name);
				if (Array.Find(typeof(Program).Assembly.GetManifestResourceNames(), s => s.EndsWith(name.Name + ".dll")) == null)
					return null;

				var existing = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.GetName().Name == name.Name);
				if (existing != null) {
					Logging.tML.Warn($"Upgraded Reference {name.Name} -> Version={name.Version} -> {existing.GetName().Version}");
					return existing;
				}

				return null;
			});
		}

		private static void RecalculateReferences()
		{
			foreach (var mod in loadedMods.Values)
			{
				mod.dependencies.Clear();
				mod.dependents.Clear();
			}

			foreach (var mod in loadedMods.Values)
				foreach (var depName in mod.properties.RefNames(true))
					if (loadedMods.ContainsKey(depName))
						mod.AddDependency(loadedMods[depName]);
					else
						mod.weakDependencies.Add(depName);

			foreach (var mod in loadedMods.Values)
				mod.UpdateWeakRefs();
		}

		private static Mod Instantiate(LoadedMod mod)
		{
			try
			{
				Type modType = mod.assembly.GetTypes().SingleOrDefault(t => t.IsSubclassOf(typeof(Mod)));
				if (modType == null)
					throw new Exception(mod.Name + " does not have a class extending Mod. Mods need a Mod class to function.") {
						HelpLink = "https://github.com/blushiemagic/tModLoader/wiki/Basic-tModLoader-Modding-FAQ#sequence-contains-no-matching-element-error"
					};

				var m = (Mod)Activator.CreateInstance(modType);
				m.File = mod.modFile;
				m.Code = mod.assembly;
				m.Logger = LogManager.GetLogger(m.Name);
				m.Side = mod.properties.side;
				m.DisplayName = mod.properties.displayName;
				return m;
			}
			catch (Exception e)
			{
				e.Data["mod"] = mod.Name;
				throw;
			}
		}

		internal static List<Mod> InstantiateMods(List<LocalMod> modsToLoad)
		{
			var modList = new List<LoadedMod>();
			foreach (var loading in modsToLoad)
			{
				if (!loadedMods.TryGetValue(loading.Name, out LoadedMod mod))
					mod = loadedMods[loading.Name] = new LoadedMod();

				mod.SetMod(loading);
				modList.Add(mod);
			}

			RecalculateReferences();

			if (Debugger.IsAttached)
			{
				ModCompile.DeveloperMode = true;
				foreach (var mod in modList.Where(mod => mod.properties.editAndContinue && mod.CanEaC))
					mod.EnableEaC();
			}

			try
			{
				//load all the assemblies in parallel.
				Interface.loadMods.SetLoadStage("tModLoader.MSSandboxing", modsToLoad.Count);
				int i = 0;
				Parallel.ForEach(modList, mod =>
				{
					Interface.loadMods.SetCurrentMod(i++, mod.Name);
					mod.LoadAssemblies();
				});
				
				Interface.loadMods.SetLoadStage("tModLoader.MSInstantiating");
				//Assemblies must be loaded before any instantiation occurs to satisfy dependencies
				return modList.Select(Instantiate).ToList();
			}
			catch (AggregateException ae)
			{
				ae.Data["mods"] = ae.InnerExceptions.Select(e => (string)e.Data["mod"]);
				throw;
			}
		}

		internal static IEnumerable<Assembly> GetModAssemblies(string name) => loadedMods[name].assemblies;

		internal static byte[] GetAssemblyBytes(string name)
		{
			assemblyBinaries.TryGetValue(name, out var code);
			return code;
		}

		internal static bool FirstModInStackTrace(StackTrace stack, out string modName)
		{
			for (int i = 0; i < stack.FrameCount; i++)
			{
				StackFrame frame = stack.GetFrame(i);
				MethodBase caller = frame.GetMethod();
				var assembly = caller?.DeclaringType?.Assembly;
				if (assembly == null || !hostModForAssembly.TryGetValue(assembly, out var mod))
					continue;

				modName = mod.Name;
				return true;
			}

			modName = null;
			return false;
		}

		internal class TerrariaCecilAssemblyResolver : DefaultAssemblyResolver
		{
			public static readonly TerrariaCecilAssemblyResolver instance = new TerrariaCecilAssemblyResolver();

			private TerrariaCecilAssemblyResolver()
			{
				RegisterAssembly(ModuleDefinition.ReadModule(Assembly.GetExecutingAssembly().Location).Assembly);
			}
		}

		internal class SymbolWriterProvider : ISymbolWriterProvider
		{
			public static readonly SymbolWriterProvider instance = new SymbolWriterProvider();

			private class HeaderCopyWriter : ISymbolWriter
			{
				private ModuleDefinition module;

				public HeaderCopyWriter(ModuleDefinition module) {
					this.module = module;
				}

				public ImageDebugHeader GetDebugHeader() => module.GetDebugHeader();

				public ISymbolReaderProvider GetReaderProvider() => throw new NotImplementedException();
				public void Write(MethodDebugInformation info) => throw new NotImplementedException();

				public void Dispose() { }
			}

			public ISymbolWriter GetSymbolWriter(ModuleDefinition module, string fileName) => new HeaderCopyWriter(module);
			public ISymbolWriter GetSymbolWriter(ModuleDefinition module, Stream symbolStream) => new HeaderCopyWriter(module);
		}
	}
}
