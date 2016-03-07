using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
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
		public static readonly string DllPath = Main.SavePath + Path.DirectorySeparatorChar + "dllReferences";
		private static readonly string ImagePath = "Content" + Path.DirectorySeparatorChar + "Images";
		private static bool assemblyResolverAdded = false;
		internal const int earliestRelease = 149;
		internal static string modToBuild;
		internal static bool reloadAfterBuild = false;
		internal static bool buildAll = false;
		internal static int numLoads = 0;
        private static readonly Stack<string> loadOrder = new Stack<string>();
        internal static readonly IDictionary<string, Mod> mods = new Dictionary<string, Mod>();
		internal static readonly IDictionary<string, Tuple<Mod, string, string>> modHotKeys = new Dictionary<string, Tuple<Mod, string, string>>();

		private static void AddAssemblyResolver()
		{
			if (assemblyResolverAdded)
			{
				return;
			}
			AppDomain.CurrentDomain.AssemblyResolve += ResolveTerrariaReference;
			AppDomain.CurrentDomain.AssemblyResolve += ResolveDllReference;
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

		private static Assembly ResolveDllReference(object sender, ResolveEventArgs args)
		{
			Directory.CreateDirectory(DllPath);
			string name = args.Name;
			if (name.IndexOf(',') >= 0)
			{
				name = name.Substring(0, name.IndexOf(','));
			}
			try
			{
				return Assembly.LoadFrom(DllPath + Path.DirectorySeparatorChar + name + ".dll");
			}
			catch
			{
				return null;
			}
		}

		private static Assembly ResolveModReference(object sender, ResolveEventArgs args)
		{
			string name = args.Name;
			if (name.IndexOf(',') >= 0)
			{
				name = name.Substring(0, name.IndexOf(','));
			}
			return GetMod(name)?.Code;
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

		private static bool LoadMods()
		{
			//load all referenced assemblies before mods for compiling
			ModCompile.LoadReferences();
			Interface.loadMods.SetProgressFinding();
			var modsToLoad = FindMods().ToList();
            modsToLoad = modsToLoad.Where(IsEnabled).ToList();
            var modNameMap = modsToLoad.ToDictionary(mod => mod.name);
            var properties = modsToLoad.ToDictionary(mod => mod.name, BuildProperties.ReadModFile);
            try {
		        var sorted = TopoSort(modNameMap.Keys, properties);
		        modsToLoad = sorted.Select(name => modNameMap[name]).ToList();
		    }
		    catch (ModSortingException e) {
                foreach (var mod in e.errored)
                    DisableMod(modNameMap[mod]);

                ErrorLogger.LogDependencyError(e.Message);
		        return false;
		    }

			Mod defaultMod = new ModLoaderMod();
			AddMod(defaultMod);

		    int i = 0;
		    foreach (var mod in modsToLoad) {
                Interface.loadMods.SetProgressCompatibility(mod.name, i++, modsToLoad.Count);
                try {
                    LoadMod(mod, properties[mod.name]);
                }
                catch (Exception e) {
                    DisableMod(mod);
                    ErrorLogger.LogLoadingError(mod.name, mod.tModLoaderVersion, e);
                    return false;
                }
            }
			return true;
		}

	    internal static List<string> TopoSort(ICollection<string> mods, IDictionary<string, BuildProperties> properties) {
	        var visiting = new Stack<string>();
            var sorted = new List<string>();
            var errored = new HashSet<string>();
            var errorLog = new StringBuilder();

            Action<string> Visit = null;
	        Visit = mod => {
	            if (sorted.Contains(mod) || errored.Contains(mod))
	                return;

	            visiting.Push(mod);
	            foreach (var dep in properties[mod].modReferences) {
	                if (!mods.Contains(dep)) {
                        errored.Add(mod);
                        errorLog.AppendLine("Missing mod: " + dep + " required by " + mod);
	                    continue;
	                }

	                if (visiting.Contains(dep)) {
	                    var cycle = dep;
	                    var stack = new Stack<string>(visiting);
	                    string entry;
	                    do {
	                        entry = stack.Pop();
	                        errored.Add(entry);
	                        cycle = entry + " -> " + cycle;
	                    } while (entry != dep);
	                    errorLog.AppendLine("Dependency Cycle: " + cycle);
	                    continue;
	                }

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

	    private static void LoadMod(TmodFile modFile, BuildProperties properties)
		{
			AddAssemblyResolver();

            if (modFile.name.Equals("Terraria", StringComparison.InvariantCultureIgnoreCase))
                throw new DuplicateNameException("Mods cannot be named Terraria");

            if (mods.ContainsKey(modFile.name))
                throw new DuplicateNameException("Two mods share the internal name " + modFile.name);

            Interface.loadMods.SetProgressReading(modFile.name, 0, 2);

			Assembly modCode;
			var dllFileName = modFile.HasFile("All.dll") ? "All" : windows ? "Windows" : "Other";
			if (properties.includePDB && modFile.HasFile(dllFileName + ".pdb"))
				modCode = Assembly.Load(modFile.GetFile(dllFileName + ".dll"), modFile.GetFile(dllFileName + ".pdb"));
			else
				modCode = Assembly.Load(modFile.GetFile(dllFileName + ".dll"));

			Interface.loadMods.SetProgressReading(modFile.name, 1, 2);

	        var modType = modCode.GetTypes().Single(t => t.IsSubclassOf(typeof (Mod)));
			Mod mod = (Mod)Activator.CreateInstance(modType);
			mod.File = modFile;
			mod.Code = modCode;
					
			AddMod(mod);
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
	}

    internal class ModSortingException : Exception
    {
        public ICollection<string> errored;

        public ModSortingException(ICollection<string> errored, string message) : base(message) {
            this.errored = errored;
        }
    }
}