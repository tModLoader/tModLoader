using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.IO;
using System.Security.Cryptography;

namespace Terraria.ModLoader
{
	public static class ModLoader
	{
		//change Terraria.Main.DrawMenu change drawn version number string to include this
		public static readonly Version version = new Version(0, 8, 2, 2);
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
		internal const int earliestRelease = 149;
		internal static string modToBuild;
		internal static bool reloadAfterBuild = false;
		internal static bool buildAll = false;
		private static readonly Stack<string> loadOrder = new Stack<string>();
		private static Mod[] loadedMods;
		internal static readonly IDictionary<string, Mod> mods = new Dictionary<string, Mod>();
		internal static readonly IDictionary<string, ModHotkey> modHotKeys = new Dictionary<string, ModHotkey>();
		internal static readonly string modBrowserPublicKey = "<RSAKeyValue><Modulus>oCZObovrqLjlgTXY/BKy72dRZhoaA6nWRSGuA+aAIzlvtcxkBK5uKev3DZzIj0X51dE/qgRS3OHkcrukqvrdKdsuluu0JmQXCv+m7sDYjPQ0E6rN4nYQhgfRn2kfSvKYWGefp+kqmMF9xoAq666YNGVoERPm3j99vA+6EIwKaeqLB24MrNMO/TIf9ysb0SSxoV8pC/5P/N6ViIOk3adSnrgGbXnFkNQwD0qsgOWDks8jbYyrxUFMc4rFmZ8lZKhikVR+AisQtPGUs3ruVh4EWbiZGM2NOkhOCOM4k1hsdBOyX2gUliD0yjK5tiU3LBqkxoi2t342hWAkNNb4ZxLotw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
		internal static Action PostLoad;

		internal static bool ModLoaded(string name)
		{
			return mods.ContainsKey(name);
		}

		public static int ModCount => loadedMods.Length;

		public static Mod GetMod(string name)
		{
			Mod m;
			mods.TryGetValue(name, out m);
			return m;
		}

		public static Mod GetMod(int index)
		{
			return index >= 0 && index < loadedMods.Length ? loadedMods[index] : null;
		}

		public static Mod[] LoadedMods => (Mod[]) loadedMods.Clone();

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

			if (Main.dedServ)
				ModNet.AssignNetIDs();

			MapLoader.SetupModMap();
			Interface.loadMods.SetProgressRecipes();
			for (int k = 0; k < Recipe.maxRecipes; k++)
			{
				Main.recipe[k] = new Recipe();
			}
			Recipe.numRecipes = 0;
			RecipeGroupHelper.ResetRecipeGroups();
			try
			{
				Recipe.SetupRecipes();
			}
			catch (AddRecipesException e)
			{
				ErrorLogger.LogLoadingError(e.modName, version, e.InnerException, true);
				Main.menuMode = Interface.errorMessageID;
				return;
			}
			if (PostLoad != null)
			{
				PostLoad();
				PostLoad = null;
			}
			else
			{
				Main.menuMode = 0;
			}
			GameInput.PlayerInput.ReInitialize();
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
			BackgroundTextureLoader.ResizeAndFillArrays();
			UgBgStyleLoader.ResizeAndFillArrays();
			SurfaceBgStyleLoader.ResizeAndFillArrays();
			GlobalBgStyleLoader.ResizeAndFillArrays(unloading);
			WaterStyleLoader.ResizeArrays();
			WaterfallStyleLoader.ResizeArrays();
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

		private static bool LoadMods()
		{
			//load all referenced assemblies before mods for compiling
			ModCompile.LoadReferences();

			Interface.loadMods.SetProgressFinding();
			var modsToLoad = FindMods()
				.Where(IsEnabled)
				.Select(mod => new LoadingMod(mod, BuildProperties.ReadModFile(mod)))
				.Where(mod => LoadSide(mod.properties.side))
				.ToList();

			if (!VerifyNames(modsToLoad))
				return false;

			try
			{
				modsToLoad = TopoSort(modsToLoad, false);
			}
			catch (ModSortingException e)
			{
				foreach (var mod in e.errored)
					DisableMod(mod.modFile);

				ErrorLogger.LogDependencyError(e.Message);
				return false;
			}

			var modInstances = AssemblyManager.InstantiateMods(modsToLoad);
			if (modInstances == null)
				return false;

			modInstances.Insert(0, new ModLoaderMod());
			loadedMods = modInstances.ToArray();
			foreach (var mod in modInstances)
			{
				loadOrder.Push(mod.Name);
				mods[mod.Name] = mod;
			}

			return true;
		}

		public static bool IsSignedBy(TmodFile mod, string xmlPublicKey)
		{
			var f = new RSAPKCS1SignatureDeformatter();
			var v = AsymmetricAlgorithm.Create("RSA");
			f.SetHashAlgorithm("SHA1");
			v.FromXmlString(xmlPublicKey);
			f.SetKey(v);
			return f.VerifySignature(mod.hash, mod.signature);
		}

		private static bool VerifyNames(List<LoadingMod> mods)
		{
			var names = new HashSet<string>();
			foreach (var mod in mods)
			{
				try
				{
					if (mod.Name.Length == 0)
						throw new ModNameException("Mods must actually have stuff in their names");

					if (mod.Name.Equals("Terraria", StringComparison.InvariantCultureIgnoreCase))
						throw new ModNameException("Mods names cannot be named Terraria");

					if (names.Contains(mod.Name))
						throw new ModNameException("Two mods share the internal name " + mod.Name);

					names.Add(mod.Name);
				}
				catch (Exception e)
				{
					DisableMod(mod.modFile);
					ErrorLogger.LogLoadingError(mod.Name, mod.modFile.tModLoaderVersion, e);
					return false;
				}
			}

			return true;
		}

		internal static List<LoadingMod> TopoSort(ICollection<LoadingMod> mods, bool building)
		{
			var nameMap = mods.ToDictionary(mod => mod.Name);
			var errored = new HashSet<LoadingMod>();
			var errorLog = new StringBuilder();

			//ensure dependencies exist
			foreach (var mod in mods)
				foreach (var depName in mod.properties.RefNames(building))
					if (!nameMap.ContainsKey(depName))
					{
						errored.Add(mod);
						errorLog.AppendLine("Missing mod: " + depName + " required by " + mod.Name);
					}

			if (errored.Count > 0)
				throw new ModSortingException(errored, errorLog.ToString());

			//ensure target versions are met
			foreach (var mod in mods)
				foreach (var dep in mod.properties.Refs(true))
				{
					LoadingMod inst;
					if (nameMap.TryGetValue(dep.mod, out inst) && inst.properties.version < dep.target)
					{
						errored.Add(mod);
						errorLog.AppendLine(mod.Name + " requires version " + dep.target + "+ of " + dep.target + 
							" but version " + inst.properties.version + " is installed");
					}
				}

			if (errored.Count > 0)
				throw new ModSortingException(errored, errorLog.ToString());

			//build graph
			var modsBefore = mods.ToDictionary(mod => mod.Name, mod => mod.properties.sortAfter.ToList());
			foreach (var mod in mods)
				foreach (var before in mod.properties.sortBefore)
					modsBefore[before].Add(mod.Name);


			var visiting = new Stack<LoadingMod>();
			var sorted = new List<LoadingMod>();

			Action<LoadingMod> Visit = null;
			Visit = mod =>
			{
				if (sorted.Contains(mod) || errored.Contains(mod))
					return;

				visiting.Push(mod);
				foreach (var depName in modsBefore[mod.Name].Where(nameMap.ContainsKey))
				{
					var dep = nameMap[depName];
					if (visiting.Contains(dep))
					{
						var cycle = dep.Name;
						foreach (var entry in visiting)
						{
							errored.Add(entry);
							cycle = entry.Name + " -> " + cycle;
							if (entry == dep) break;
						}
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

		internal static void Unload()
		{
			while (loadOrder.Count > 0)
				GetMod(loadOrder.Pop()).UnloadContent();

			loadOrder.Clear();
			loadedMods = new Mod[0];

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
			BackgroundTextureLoader.Unload();
			UgBgStyleLoader.Unload();
			SurfaceBgStyleLoader.Unload();
			GlobalBgStyleLoader.Unload();
			WaterStyleLoader.Unload();
			WaterfallStyleLoader.Unload();
			mods.Clear();
			ResizeArrays(true);
			MapLoader.UnloadModMap();
			modHotKeys.Clear();
			WorldHooks.Unload();
			RecipeHooks.Unload();

			if (!Main.dedServ && Main.netMode != 1) //disable vanilla client compatiblity restrictions when reloading on a client
				ModNet.AllowVanillaClients = false;
		}

		internal static void Reload()
		{
			Unload();
			Main.menuMode = Interface.loadModsID;
		}

		internal static bool LoadSide(ModSide side) => side != (Main.dedServ ? ModSide.Client : ModSide.Server);

		internal static bool IsEnabled(TmodFile mod)
		{
			string enablePath = Path.ChangeExtension(mod.path, "enabled");
			return !File.Exists(enablePath) || File.ReadAllText(enablePath) != "false";
		}

		internal static void SetModActive(TmodFile mod, bool active)
		{
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

		private static void SplitName(string name, out string domain, out string subName)
		{
			int slash = name.IndexOf('/');
			if (slash < 0)
				throw new MissingResourceException("Missing mod qualifier: " + name);

			domain = name.Substring(0, slash);
			subName = name.Substring(slash + 1);
		}

		public static byte[] GetFileBytes(string name)
		{
			string modName, subName;
			SplitName(name, out modName, out subName);

			Mod mod = GetMod(modName);
			if (mod == null)
				throw new MissingResourceException("Missing mod: " + name);

			return mod.GetFileBytes(subName);
		}

		public static bool FileExists(string name)
		{
			if (!name.Contains('/'))
				return false;

			string modName, subName;
			SplitName(name, out modName, out subName);

			Mod mod = GetMod(modName);
			return mod != null && mod.FileExists(subName);
		}

		public static Texture2D GetTexture(string name)
		{
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

		public static bool TextureExists(string name)
		{
			if (!name.Contains('/'))
				return false;

			string modName, subName;
			SplitName(name, out modName, out subName);

			if (modName == "Terraria")
				return File.Exists(ImagePath + Path.DirectorySeparatorChar + name + ".xnb");

			Mod mod = GetMod(modName);
			return mod != null && mod.TextureExists(subName);
		}

		public static SoundEffect GetSound(string name)
		{
			if (Main.dedServ)
				return null;

			string modName, subName;
			SplitName(name, out modName, out subName);

			Mod mod = GetMod(modName);
			if (mod == null)
				throw new MissingResourceException("Missing mod: " + name);

			return mod.GetSound(subName);
		}

		public static bool SoundExists(string name)
		{
			if (!name.Contains('/'))
				return false;

			string modName, subName;
			SplitName(name, out modName, out subName);

			Mod mod = GetMod(modName);
			return mod != null && mod.SoundExists(subName);
		}

		public static void RegisterHotKey(Mod mod, string name, string defaultKey)
		{
			//string configurationString = mod.Name + "_" + "HotKey" + "_" + name.Replace(' ', '_');
			//string keyFromConfigutation = Main.Configuration.Get<string>(configurationString, defaultKey);
			modHotKeys[name] = new ModHotkey(name, mod, defaultKey);
		}
		// example: ExampleMod_HotKey_Random_Buff="P"
		//internal static void SaveConfiguration()
		//{
		//	foreach (KeyValuePair<string, Tuple<Mod, string, string>> hotKey in modHotKeys)
		//	{
		//		string name = hotKey.Value.Item1.Name + "_" + "HotKey" + "_" + hotKey.Key.Replace(' ', '_');
		//		Main.Configuration.Put(name, hotKey.Value.Item2);
		//	}
		//}

		/// <summary>
		/// Allows type inference on T and F
		/// </summary>
		internal static void BuildGlobalHook<T, F>(ref F[] list, IList<T> providers, Expression<Func<T, F>> expr) {
			list = BuildGlobalHook(providers, expr).Select(expr.Compile()).ToArray();
		}

		internal static T[] BuildGlobalHook<T, F>(IList<T> providers, Expression<Func<T, F>> expr) {
			MethodInfo method;
			try {
				var convert = expr.Body as UnaryExpression;
				var makeDelegate = convert.Operand as MethodCallExpression;
				var methodArg = makeDelegate.Arguments[2] as ConstantExpression;
				method = methodArg.Value as MethodInfo;
				if (method == null) throw new NullReferenceException();
			}
			catch (Exception e) {
				throw new ArgumentException("Invalid hook expression " + expr, e);
			}

			if (!method.IsVirtual) throw new ArgumentException("Cannot build hook for non-virtual method " + method);
			var argTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
			return providers.Where(p => p.GetType().GetMethod(method.Name, argTypes).DeclaringType != typeof(T)).ToArray();
		}

		internal class LoadingMod
		{
			public readonly TmodFile modFile;
			public readonly BuildProperties properties;

			public string Name => modFile.name;

			public LoadingMod(TmodFile modFile, BuildProperties properties)
			{
				this.modFile = modFile;
				this.properties = properties;
			}
		}
	}
}