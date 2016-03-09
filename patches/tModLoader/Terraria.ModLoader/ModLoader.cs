using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader
{
	public static class ModLoader
	{
		//change Terraria.Main.DrawMenu change drawn version number string to include this
		public static readonly Version version = new Version(0, 7, 1, 2);
	    public static readonly string versionedName = "tModLoader v" + version;
#if WINDOWS
        public const bool windows = true;

#else
        public const bool windows = false;
		#endif
		//change Terraria.Main.SavePath and cloud fields to use "ModLoader" folder
		public static readonly string ModPath = Main.SavePath + Path.DirectorySeparatorChar + "Mods";
		public static readonly string ModSourcePath = Main.SavePath + Path.DirectorySeparatorChar + "Mod Sources";
		private static readonly string ImagePath = "Content" + Path.DirectorySeparatorChar + "Images";
		private static bool assemblyResolverAdded = false;
		internal const int earliestRelease = 149;
		internal static string modToBuild;
		internal static bool reloadAfterBuild = false;
		internal static bool buildAll = false;
		internal static int numLoads;
        private static readonly Stack<string> loadOrder = new Stack<string>();
        internal static readonly IDictionary<string, Mod> mods = new Dictionary<string, Mod>();
        private static readonly IDictionary<string, Assembly> loadedAssemblies = new Dictionary<string, Assembly>();
		internal static readonly IDictionary<string, Tuple<Mod, string, string>> modHotKeys = new Dictionary<string, Tuple<Mod, string, string>>();

		private static void AddAssemblyResolver()
		{
			if (assemblyResolverAdded)
			{
				return;
			}
			AppDomain.CurrentDomain.AssemblyResolve += ResolveTerrariaReference;
			AppDomain.CurrentDomain.AssemblyResolve += ResolveModReference;
			assemblyResolverAdded = true;
		}

		private static Assembly ResolveTerrariaReference(object sender, ResolveEventArgs args)
		{
			string name = args.Name;
			if (name.IndexOf(',') >= 0)
			{
				name = name.Substring(0, name.IndexOf(','));
			}
			if (name.StartsWith("Terraria"))
			{
				return Assembly.GetExecutingAssembly();
			}
			return null;
		}

		private static Assembly ResolveModReference(object sender, ResolveEventArgs args)
		{
			string name = args.Name;
			if (name.IndexOf(',') >= 0)
			{
				name = name.Substring(0, name.IndexOf(','));
			}
			Assembly a;
		    loadedAssemblies.TryGetValue(name, out a);
		    return a;
		}

		internal static bool ModLoaded(string name)
		{
			return mods.ContainsKey(name);
		}

		public static Mod GetMod(string name) {
		    Mod m;
		    mods.TryGetValue(name, out m);
		    return m;
		}

		public static string[] GetLoadedMods()
		{
			return loadOrder.ToArray();
		}

		internal static void Load()
		{
			ThreadPool.QueueUserWorkItem(new WaitCallback(do_Load), 1);
		}

		internal static void do_Load(object threadContext)
		{
			if (!LoadMods())
			{
				Main.menuMode = Interface.errorMessageID;
				return;
			}
			if (Main.dedServ)
			{
				Console.WriteLine("Adding mod content...");
			}
			int num = 0;
			foreach (Mod mod in mods.Values)
			{
				Interface.loadMods.SetProgressInit(mod.Name, num, mods.Count);
				try
				{
					mod.Autoload();
					mod.Load();
				}
				catch (Exception e)
				{
					DisableMod(mod.File);
					ErrorLogger.LogLoadingError(mod.Name, mod.tModLoaderVersion, e);
					Main.menuMode = Interface.errorMessageID;
					return;
				}
				num++;
			}
			Interface.loadMods.SetProgressSetup(0f);
			ResizeArrays();
			num = 0;
			foreach (Mod mod in mods.Values)
			{
				Interface.loadMods.SetProgressLoad(mod.Name, num, mods.Count);
				try
				{
					mod.SetupContent();
					mod.PostSetupContent();
				}
				catch (Exception e)
				{
					DisableMod(mod.File);
					ErrorLogger.LogLoadingError(mod.Name, mod.tModLoaderVersion, e);
					Main.menuMode = Interface.errorMessageID;
					return;
				}
				num++;
			}
			MapLoader.SetupModMap();
			Interface.loadMods.SetProgressRecipes();
			for (int k = 0; k < Recipe.maxRecipes; k++)
			{
				Main.recipe[k] = new Recipe();
			}
			Recipe.numRecipes = 0;
			try
			{
				CraftGroup.ResetVanillaGroups();
				AddCraftGroups();
				Recipe.SetupRecipes();
			}
			catch (Exception e)
			{
				ErrorLogger.LogLoadingError("recipes", version, e);
				Main.menuMode = Interface.errorMessageID;
				return;
			}
			Main.menuMode = 0;
			numLoads++;
		}

		private static void ResizeArrays(bool unloading = false)
		{
			ItemLoader.ResizeArrays();
			EquipLoader.ResizeAndFillArrays();
			Main.InitializeItemAnimations();
			ModDust.ResizeArrays();
			TileLoader.ResizeArrays(unloading);
			WallLoader.ResizeArrays(unloading);
			ProjectileLoader.ResizeArrays();
			NPCLoader.ResizeArrays();
			NPCHeadLoader.ResizeAndFillArrays();
			ModGore.ResizeAndFillArrays();
			SoundLoader.ResizeAndFillArrays();
			MountLoader.ResizeArrays();
			BuffLoader.ResizeArrays();
		}

		internal static TmodFile[] FindMods()
		{
			Directory.CreateDirectory(ModPath);
			IList<TmodFile> files = new List<TmodFile>();
			foreach (string fileName in Directory.GetFiles(ModPath, "*.tmod", SearchOption.TopDirectoryOnly))
			{
				TmodFile file = new TmodFile(fileName);
				file.Read();
				if (file.ValidMod() == null)
				{
					files.Add(file);
				}
			}
			return files.ToArray();
        }

        private static void AddMod(Mod mod) {
            loadOrder.Push(mod.Name);
            mods[mod.Name] = mod;
        }

        private static Assembly LoadAssembly(byte[] code, byte[] pdb = null) {
            var asm = Assembly.Load(code, pdb);
            loadedAssemblies[asm.GetName().Name] = asm;
            return asm;
        }

        private static bool LoadMods()
		{
			//load all referenced assemblies before mods for compiling
			ModCompile.LoadReferences();

			Interface.loadMods.SetProgressFinding();
		    var modsToLoad = FindMods()
                .Where(IsEnabled)
		        .Select(mod => new LoadingMod(mod, BuildProperties.ReadModFile(mod)))
                .ToList();

		    if (!VerifyNames(modsToLoad))
		        return false;
            
            try {
                modsToLoad = TopoSort(modsToLoad);
		    }
		    catch (ModSortingException e) {
                foreach (var mod in e.errored)
                    DisableMod(mod.modFile);

                ErrorLogger.LogDependencyError(e.Message);
		        return false;
		    }

		    EncapsulateReferences(modsToLoad);
            
			Mod defaultMod = new ModLoaderMod();
			AddMod(defaultMod);

            AddAssemblyResolver();
            int i = 0;
		    foreach (var mod in modsToLoad) {
                Interface.loadMods.SetProgressCompatibility(mod.Name, i++, modsToLoad.Count);
                try {
                    LoadMod(mod);
                }
                catch (Exception e) {
                    DisableMod(mod.modFile);
                    ErrorLogger.LogLoadingError(mod.Name, mod.modFile.tModLoaderVersion, e);
                    return false;
                }
            }
			return true;
		}

	    private static bool VerifyNames(List<LoadingMod> mods) {
            var names = new HashSet<string>();
	        foreach (var mod in mods) {
                try {
                    if (mod.Name.Equals("Terraria", StringComparison.InvariantCultureIgnoreCase))
                        throw new DuplicateNameException("Mods cannot be named Terraria");

                    if (names.Contains(mod.Name))
                        throw new DuplicateNameException("Two mods share the internal name " + mod.Name);

                    names.Add(mod.Name);
                }
                catch (Exception e) {
                    DisableMod(mod.modFile);
                    ErrorLogger.LogLoadingError(mod.Name, mod.modFile.tModLoaderVersion, e);
                    return false;
                }
	        }

	        return true;
	    }

        private static void EncapsulateReferences(List<LoadingMod> mods) {
            foreach (var mod in mods) {
                foreach (var dll in mod.properties.dllReferences)
                    mod.dlls[dll] = mod.modFile.GetFile("lib/" + dll + ".dll");

                mod.code = mod.modFile.GetMainAssembly();
            }

            foreach (var mod in mods)
                mod.EncapsulateReferences();
	    }

	    internal static List<LoadingMod> TopoSort(ICollection<LoadingMod> mods) {
	        var nameMap = mods.ToDictionary(mod => mod.Name);

	        var visiting = new Stack<LoadingMod>();
            var sorted = new List<LoadingMod>();
            var errored = new HashSet<LoadingMod>();
            var errorLog = new StringBuilder();

            Action<LoadingMod> Visit = null;
	        Visit = mod => {
	            if (sorted.Contains(mod) || errored.Contains(mod))
	                return;

	            visiting.Push(mod);
	            foreach (var depName in mod.properties.modReferences) {
	                if (!nameMap.ContainsKey(depName)) {
                        errored.Add(mod);
                        errorLog.AppendLine("Missing mod: " + depName + " required by " + mod.Name);
	                    continue;
	                }

	                var dep = nameMap[depName];
	                if (visiting.Contains(dep)) {
	                    var cycle = dep.Name;
	                    foreach (var entry in visiting) {
	                        errored.Add(entry);
	                        cycle = entry.Name + " -> " + cycle;
	                        if (entry == dep) break;
	                    }
	                    errorLog.AppendLine("Dependency Cycle: " + cycle);
	                    continue;
	                }

                    mod.modReferences.Add(dep);
	                Visit(dep);
	            }
	            visiting.Pop();
	            sorted.Add(mod);
	        };

	        foreach (var mod in mods)
	            Visit(mod);

	        if (errored.Count > 0)
                throw new ModSortingException(errored, errorLog.ToString());

	        return sorted;
	    }

	    private static void LoadMod(LoadingMod mod) {
            Interface.loadMods.SetProgressReading(mod.Name, 0, 1);
            foreach (var dll in mod.dlls.Values)
	            LoadAssembly(dll);

	        Assembly modCode = LoadAssembly(mod.code, mod.modFile.GetMainPDB());

            Interface.loadMods.SetProgressReading(mod.Name, 1, 2);
	        var modType = modCode.GetTypes().Single(t => t.IsSubclassOf(typeof (Mod)));
			Mod m = (Mod)Activator.CreateInstance(modType);
			m.File = mod.modFile;
			m.Code = modCode;
			AddMod(m);
		}

		internal static void Unload()
		{
			while (loadOrder.Count > 0)
				GetMod(loadOrder.Pop()).UnloadContent();

            loadOrder.Clear();

			ItemLoader.Unload();
			EquipLoader.Unload();
			ModDust.Unload();
			TileLoader.Unload();
			WallLoader.Unload();
			ProjectileLoader.Unload();
			NPCLoader.Unload();
			NPCHeadLoader.Unload();
			PlayerHooks.Unload();
			BuffLoader.Unload();
			MountLoader.Unload();
			ModGore.Unload();
			SoundLoader.Unload();
            mods.Clear();
            ResizeArrays(true);
			MapLoader.UnloadModMap();
			modHotKeys.Clear();
			WorldHooks.Unload();
			RecipeHooks.Unload();

			loadedAssemblies.Clear();
        }

		internal static void Reload()
		{
			Unload();
			Main.menuMode = Interface.loadModsID;
		}

		internal static bool IsEnabled(TmodFile mod)
		{
			string enablePath = Path.ChangeExtension(mod.path, "enabled");
			return !File.Exists(enablePath) || File.ReadAllText(enablePath) != "false";
		}

		internal static void SetModActive(TmodFile mod, bool active) {
		    if (mod == null)
		        return;

			string path = Path.ChangeExtension(mod.path, "enabled");
			using (StreamWriter writer = File.CreateText(path))
			{
				writer.Write(active ? "true" : "false");
			}
		}

		internal static void EnableMod(TmodFile mod)
		{
			SetModActive(mod, true);
		}

		internal static void DisableMod(TmodFile mod)
		{
			SetModActive(mod, false);
		}

		internal static string[] FindModSources()
		{
			Directory.CreateDirectory(ModSourcePath);
			return Directory.GetDirectories(ModSourcePath, "*", SearchOption.TopDirectoryOnly).Where(dir => dir != ".vs").ToArray();
		}

		internal static void BuildAllMods()
		{
			ThreadPool.QueueUserWorkItem(_ =>
				{
					PostBuildMenu(ModCompile.BuildAll(FindModSources(), Interface.buildMod));
				});
		}

		internal static void BuildMod()
		{
			Interface.buildMod.SetProgress(0, 1);
			ThreadPool.QueueUserWorkItem(_ =>
				{
					try
					{
						PostBuildMenu(ModCompile.Build(modToBuild, Interface.buildMod));
					}
					catch (Exception e)
					{
						ErrorLogger.LogException(e);
					}
				}, 1);
		}

		private static void PostBuildMenu(bool success)
		{
			Main.menuMode = success ? (reloadAfterBuild ? Interface.reloadModsID : 0) : Interface.errorMessageID;
        }

	    private static void SplitName(string name, out string domain, out string subName) {
            int slash = name.IndexOf('/');
            if (slash < 0)
                throw new MissingResourceException("Missing mod qualifier: " + name);

            domain = name.Substring(0, slash);
            subName = name.Substring(slash + 1);
        }

        public static Texture2D GetTexture(string name) {
            if (Main.dedServ)
                return null;

            string modName, subName;
            SplitName(name, out modName, out subName);
            if (modName == "Terraria")
                return Main.instance.Content.Load<Texture2D>("Images" + Path.DirectorySeparatorChar + subName);

            Mod mod = GetMod(modName);
            if (mod == null)
                throw new MissingResourceException("Missing mod: " + name);

            return mod.GetTexture(subName);
        }

	    public static bool TextureExists(string name) {
	        if (!name.Contains('/'))
                return false;

            string modName, subName;
            SplitName(name, out modName, out subName);

	        if (modName == "Terraria")
	            return File.Exists(ImagePath + Path.DirectorySeparatorChar + name + ".xnb");

	        Mod mod = GetMod(modName);
	        return mod != null && mod.TextureExists(subName);
        }

        public static SoundEffect GetSound(string name) {
            if (Main.dedServ)
                return null;

            string modName, subName;
            SplitName(name, out modName, out subName);

            Mod mod = GetMod(modName);
            if (mod == null)
                throw new MissingResourceException("Missing mod: " + name);

            return mod.GetSound(subName);
        }

        public static bool SoundExists(string name) {
            if (!name.Contains('/'))
                return false;

            string modName, subName;
            SplitName(name, out modName, out subName);

            Mod mod = GetMod(modName);
            return mod != null && mod.SoundExists(subName);
        }

        public static void RegisterHotKey(Mod mod, string name, string defaultKey)
		{
			string configurationString = mod.Name + "_" + "HotKey" + "_" + name.Replace(' ', '_');
			string keyFromConfigutation = Main.Configuration.Get<string>(configurationString, defaultKey);
			modHotKeys[name] = new Tuple<Mod, string, string>(mod, keyFromConfigutation, defaultKey);
		}
		// example: ExampleMod_HotKey_Random_Buff="P"
		internal static void SaveConfiguration()
		{
			foreach (KeyValuePair<string, Tuple<Mod, string, string>> hotKey in modHotKeys)
			{
				string name = hotKey.Value.Item1.Name + "_" + "HotKey" + "_" + hotKey.Key.Replace(' ', '_');
				Main.Configuration.Put(name, hotKey.Value.Item2);
			}
		}

		private static void AddCraftGroups()
		{
			foreach (Mod mod in mods.Values)
			{
				try
				{
					mod.AddCraftGroups();
				}
				catch
				{
					DisableMod(mod.File);
					throw;
				}
			}
		}
		//place near end of Terraria.Recipe.SetupRecipes before material checks
		internal static void AddRecipes()
		{
			foreach (Mod mod in mods.Values)
			{
				try
				{
					mod.AddRecipes();
					foreach (ModItem item in mod.items.Values)
					{
						item.AddRecipes();
					}
				}
				catch
				{
					DisableMod(mod.File);
					throw;
				}
			}
        }

        internal class LoadingMod
        {
            public readonly TmodFile modFile;
            public readonly BuildProperties properties;
            public readonly List<LoadingMod> modReferences = new List<LoadingMod>();
             
            public byte[] code;
            public readonly IDictionary<string, byte[]> dlls = new Dictionary<string, byte[]>();

            public string Name => modFile.name;

            public LoadingMod(TmodFile modFile, BuildProperties properties) {
                this.modFile = modFile;
                this.properties = properties;
            }

            public void EncapsulateReferences() {
                code = EncapsulateReferences(code);

                foreach (var dllName in dlls.Keys.ToArray())
                    dlls[dllName] = EncapsulateReferences(dlls[dllName]);
            }

            private byte[] EncapsulateReferences(byte[] code) {
                var asm = AssemblyDefinition.ReadAssembly(new MemoryStream(code));
                asm.Name.Name = EncapsulateName(asm.Name.Name);

                //randomise the module version id so that the debugger can detect it as a different module (even if it has the same content)
                asm.MainModule.Mvid = Guid.NewGuid();

                foreach (var mod in asm.Modules)
                    foreach (var asmRef in mod.AssemblyReferences)
                        asmRef.Name = EncapsulateName(asmRef.Name);

                var ret = new MemoryStream();
                asm.Write(ret);
                return ret.ToArray();
            }

            private string EncapsulateName(string name) {
                if (Name == name)
                    return name + '_' + numLoads;

                if (dlls.ContainsKey(name))
                    return Name + '_' + name + '_' + numLoads;

                foreach (var modRef in modReferences) {
                    var _name = modRef.EncapsulateName(name);
                    if (_name != name)
                        return _name;
                }

                return name;
            }
        }
	}
}