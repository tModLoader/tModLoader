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
using Terraria.Localization;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.IO;
using Terraria.UI;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Terraria.ModLoader.Audio;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This serves as the central class which loads mods. It contains many static fields and methods related to mods and their contents.
	/// </summary>
	public static class ModLoader
	{
		//change Terraria.Main.DrawMenu change drawn version number string to include this
		/// <summary>The name and version number of tModLoader.</summary>
		public static readonly Version version = new Version(0, 10, 1);
		public static readonly string versionedName = "tModLoader v" + version;
#if WINDOWS
		public static readonly bool windows = true;
#else
		public static readonly bool windows = false;
#endif
#if LINUX
		public static readonly bool linux = true;
#else
		public static readonly bool linux = false;
#endif
#if MAC
		public static readonly bool mac = true;
#else
		public static readonly bool mac = false;
#endif
#if GOG
		public static readonly bool gog = true;
#else
		public static readonly bool gog = false;
#endif
		public static readonly string compressedPlatformRepresentation = (windows ? "w" : (linux ? "l" : "m")) + (gog ? "g" : "s");
		//change Terraria.Main.SavePath and cloud fields to use "ModLoader" folder
		/// <summary>The file path in which mods are stored.</summary>
		public static string ModPath => modPath;
		internal static string modPath = Main.SavePath + Path.DirectorySeparatorChar + "Mods";
		/// <summary>The file path in which mod sources are stored. Mod sources are the code and images that developers work with.</summary>
		public static readonly string ModSourcePath = Main.SavePath + Path.DirectorySeparatorChar + "Mod Sources";
		private static readonly string ImagePath = "Content" + Path.DirectorySeparatorChar + "Images";
		internal const int earliestRelease = 149;
		internal static string modToBuild;
		internal static bool reloadAfterBuild = false;
		internal static bool buildAll = false;
		private static readonly Stack<string> loadOrder = new Stack<string>();
		private static WeakReference[] loadedModsWeakReferences = new WeakReference[0];
		private static Mod[] loadedMods = new Mod[0];
		internal static readonly IDictionary<string, Mod> mods = new Dictionary<string, Mod>(StringComparer.OrdinalIgnoreCase);
		internal static readonly IDictionary<string, ModHotKey> modHotKeys = new Dictionary<string, ModHotKey>();
		internal static readonly string modBrowserPublicKey = "<RSAKeyValue><Modulus>oCZObovrqLjlgTXY/BKy72dRZhoaA6nWRSGuA+aAIzlvtcxkBK5uKev3DZzIj0X51dE/qgRS3OHkcrukqvrdKdsuluu0JmQXCv+m7sDYjPQ0E6rN4nYQhgfRn2kfSvKYWGefp+kqmMF9xoAq666YNGVoERPm3j99vA+6EIwKaeqLB24MrNMO/TIf9ysb0SSxoV8pC/5P/N6ViIOk3adSnrgGbXnFkNQwD0qsgOWDks8jbYyrxUFMc4rFmZ8lZKhikVR+AisQtPGUs3ruVh4EWbiZGM2NOkhOCOM4k1hsdBOyX2gUliD0yjK5tiU3LBqkxoi2t342hWAkNNb4ZxLotw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
		internal static string modBrowserPassphrase = "";
		internal static bool isModder;
		internal static bool dontRemindModBrowserUpdateReload;
		internal static bool dontRemindModBrowserDownloadEnable;
		internal static byte musicStreamMode;
		internal static string commandLineModPack = "";
		private static string steamID64 = "";
		internal static string SteamID64
		{
			get
			{
#if GOG
				return steamID64;
#else
				return Steamworks.SteamUser.GetSteamID().ToString();
#endif
			}
			set
			{
				steamID64 = value;
			}
		}

		internal static Action PostLoad;

		internal static bool ModLoaded(string name)
		{
			return mods.ContainsKey(name);
		}

		public static int ModCount => loadedMods.Length;

		/// <summary>
		/// Gets the instance of the Mod with the specified name.
		/// </summary>
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

		public static Mod[] LoadedMods => (Mod[])loadedMods.Clone();

		/// <summary>
		/// Returns an array containing the names of all loaded mods. The array entries will be in the reverse order in which the mods were loaded.
		/// </summary>
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
					mod.loading = true;
					mod.Autoload();
					Interface.loadMods.SetSubProgressInit("");
					mod.Load();
					mod.loading = false;
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
			RecipeGroupHelper.FixRecipeGroupLookups();
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
			RefreshModLanguage(Language.ActiveCulture);

			if (Main.dedServ)
			{
				ModNet.AssignNetIDs();
				//Main.player[0] = new Player();
			}
			Main.player[255] = new Player();

			MapLoader.SetupModMap();
			ItemSorting.SetupWhiteLists();

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
			ItemLoader.ResizeArrays(unloading);
			EquipLoader.ResizeAndFillArrays();
			Main.InitializeItemAnimations();
			ModDust.ResizeArrays();
			TileLoader.ResizeArrays(unloading);
			WallLoader.ResizeArrays(unloading);
			ProjectileLoader.ResizeArrays();
			NPCLoader.ResizeArrays(unloading);
			NPCHeadLoader.ResizeAndFillArrays();
			ModGore.ResizeAndFillArrays();
			SoundLoader.ResizeAndFillArrays();
			MountLoader.ResizeArrays();
			BuffLoader.ResizeArrays();
			PlayerHooks.RebuildHooks();
			BackgroundTextureLoader.ResizeAndFillArrays();
			UgBgStyleLoader.ResizeAndFillArrays();
			SurfaceBgStyleLoader.ResizeAndFillArrays();
			GlobalBgStyleLoader.ResizeAndFillArrays(unloading);
			WaterStyleLoader.ResizeArrays();
			WaterfallStyleLoader.ResizeArrays();
			WorldHooks.ResizeArrays();
			foreach (LocalizedText text in LanguageManager.Instance._localizedTexts.Values)
			{
				text.Override = null;
			}
		}

		public static void RefreshModLanguage(GameCulture culture)
		{
			Dictionary<string, LocalizedText> dict = LanguageManager.Instance._localizedTexts;
			foreach (ModItem item in ItemLoader.items)
			{
				LocalizedText text = new LocalizedText(item.DisplayName.Key, item.DisplayName.GetTranslation(culture));
				Lang._itemNameCache[item.item.type] = SetLocalizedText(dict, text);
				text = new LocalizedText(item.Tooltip.Key, item.Tooltip.GetTranslation(culture));
				if (text.Value != null)
				{
					text = SetLocalizedText(dict, text);
					Lang._itemTooltipCache[item.item.type] = ItemTooltip.FromLanguageKey(text.Key);
				}
			}
			foreach (var keyValuePair in MapLoader.tileEntries)
			{
				foreach (MapEntry entry in keyValuePair.Value)
				{
					if (entry.translation != null)
					{
						LocalizedText text = new LocalizedText(entry.translation.Key, entry.translation.GetTranslation(culture));
						SetLocalizedText(dict, text);
					}
				}
			}
			foreach (var keyValuePair in MapLoader.wallEntries)
			{
				foreach (MapEntry entry in keyValuePair.Value)
				{
					if (entry.translation != null)
					{
						LocalizedText text = new LocalizedText(entry.translation.Key, entry.translation.GetTranslation(culture));
						SetLocalizedText(dict, text);
					}
				}
			}
			foreach (ModProjectile proj in ProjectileLoader.projectiles)
			{
				LocalizedText text = new LocalizedText(proj.DisplayName.Key, proj.DisplayName.GetTranslation(culture));
				Lang._projectileNameCache[proj.projectile.type] = SetLocalizedText(dict, text);
			}
			foreach (ModNPC npc in NPCLoader.npcs)
			{
				LocalizedText text = new LocalizedText(npc.DisplayName.Key, npc.DisplayName.GetTranslation(culture));
				Lang._npcNameCache[npc.npc.type] = SetLocalizedText(dict, text);
			}
			foreach (ModBuff buff in BuffLoader.buffs)
			{
				LocalizedText text = new LocalizedText(buff.DisplayName.Key, buff.DisplayName.GetTranslation(culture));
				Lang._buffNameCache[buff.Type] = SetLocalizedText(dict, text);
				text = new LocalizedText(buff.Description.Key, buff.Description.GetTranslation(culture));
				Lang._buffDescriptionCache[buff.Type] = SetLocalizedText(dict, text);
			}
			foreach (Mod mod in loadedMods)
			{
				foreach (ModTranslation translation in mod.translations.Values)
				{
					LocalizedText text = new LocalizedText(translation.Key, translation.GetTranslation(culture));
					SetLocalizedText(dict, text);
				}
			}
			LanguageManager.Instance.ProcessCopyCommandsInTexts();
		}

		private static LocalizedText SetLocalizedText(Dictionary<string, LocalizedText> dict, LocalizedText value)
		{
			if (dict.ContainsKey(value.Key))
			{
				dict[value.Key].SetValue(value.Value);
			}
			else
			{
				dict[value.Key] = value;
			}
			return dict[value.Key];
		}

		// TODO, investigate if this causes memory errors.
		internal static Dictionary<string, Tuple<DateTime, TmodFile>> findModsCache = new Dictionary<string, Tuple<DateTime, TmodFile>>();
		internal static TmodFile[] FindMods()
		{
			Directory.CreateDirectory(ModPath);
			IList<TmodFile> files = new List<TmodFile>();

			foreach (string fileName in Directory.GetFiles(ModPath, "*.tmod", SearchOption.TopDirectoryOnly))
			{
				var lastModified = File.GetLastWriteTime(fileName);
				Tuple<DateTime, TmodFile> cacheMod;
				TmodFile file = null;
				if (findModsCache.TryGetValue(fileName, out cacheMod))
				{
					if (cacheMod.Item1 == lastModified)
						file = cacheMod.Item2;
					else
						findModsCache.Remove(fileName);
				}
				if (file == null)
				{
					file = new TmodFile(fileName);
					file.lastModifiedTime = lastModified;
					file.Read();
					findModsCache.Add(fileName, Tuple.Create(lastModified, file));
				}
				if (file.ValidMod() == null)
					files.Add(file);
			}
			return files.OrderBy(x => x.name, StringComparer.InvariantCulture).ToArray();
		}

		private static bool LoadMods()
		{
			//load all referenced assemblies before mods for compiling
			ModCompile.LoadReferences();

			if (!CommandLineModPackOverride())
				return false;

			Interface.loadMods.SetProgressFinding();
			var modsToLoad = FindMods()
				.Where(IsEnabled)
				.Select(mod => new LoadingMod(mod, BuildProperties.ReadModFile(mod)))
				.Where(mod => LoadSide(mod.properties.side))
				.ToList();

			// Press shift while starting up tModLoader or while trapped in a reload cycle to skip loading all mods.
			if (Main.oldKeyState.PressingShift())
				modsToLoad.Clear();

			if (!VerifyNames(modsToLoad))
				return false;

			try
			{
				EnsureDependenciesExist(modsToLoad, false);
				EnsureTargetVersionsMet(modsToLoad);
				modsToLoad = Sort(modsToLoad);
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
			loadedModsWeakReferences = loadedMods.Skip(1).Select(x => new WeakReference(x)).ToArray();
			foreach (var mod in modInstances)
			{
				loadOrder.Push(mod.Name);
				mods[mod.Name] = mod;
			}

			return true;
		}

		private static bool CommandLineModPackOverride()
		{
			if (commandLineModPack != "")
			{
				try
				{
					string fileName = UI.UIModPacks.ModListSaveDirectory + Path.DirectorySeparatorChar + commandLineModPack + ".json";
					Directory.CreateDirectory(UI.UIModPacks.ModListSaveDirectory);
					if (File.Exists(fileName))
					{
						using (StreamReader r = new StreamReader(fileName))
						{
							Console.WriteLine($"Loading specified modpack: {commandLineModPack}\n");
							string json = r.ReadToEnd();
							string[] modsToEnable = JsonConvert.DeserializeObject<string[]>(json);
							var mods = ModLoader.FindMods();
							foreach (var item in mods)
							{
								DisableMod(item);
							}
							foreach (string modname in modsToEnable)
							{
								foreach (var item in mods)
								{
									if (item.name == modname)
									{
										EnableMod(item);
									}
								}
							}
						}
					}
					else
					{
						if (Main.dedServ)
						{
							Console.ForegroundColor = ConsoleColor.Red;
							Console.WriteLine($"No modpack named {commandLineModPack} was found in {UI.UIModPacks.ModListSaveDirectory}. Make sure not to include the .json extension.\n");
							Console.ResetColor();
						}
						else
						{
							Interface.errorMessage.SetMessage($"No modpack named {commandLineModPack} was found in {UI.UIModPacks.ModListSaveDirectory}. Make sure not to include the .json extension.");
						}
						commandLineModPack = "";
						return false;
					}
				}
				catch
				{
					if (Main.dedServ)
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine($"The {commandLineModPack} modpack failed to be read properly, it might be malformed.\n");
						Console.ResetColor();
					}
					else
					{
						Interface.errorMessage.SetMessage($"No modpack named {commandLineModPack} was found in {UI.UIModPacks.ModListSaveDirectory}. Make sure not to include the .json extension.");
					}
					commandLineModPack = "";
					return false;
				}
			}
			commandLineModPack = "";
			return true;
		}

		// TODO: This doesn't work on mono for some reason. Investigate.
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

					if (mod.Name.IndexOf('.') >= 0)
						throw new ModNameException("A mod's internal name cannot contain a period");

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

		internal static void EnsureDependenciesExist(ICollection<LoadingMod> mods, bool includeWeak)
		{
			var nameMap = mods.ToDictionary(mod => mod.Name);
			var errored = new HashSet<LoadingMod>();
			var errorLog = new StringBuilder();

			foreach (var mod in mods)
				foreach (var depName in mod.properties.RefNames(includeWeak))
					if (!nameMap.ContainsKey(depName))
					{
						errored.Add(mod);
						errorLog.AppendLine("Missing mod: " + depName + " required by " + mod);
					}

			if (errored.Count > 0)
				throw new ModSortingException(errored, errorLog.ToString());
		}

		internal static void EnsureTargetVersionsMet(ICollection<LoadingMod> mods)
		{
			var nameMap = mods.ToDictionary(mod => mod.Name);
			var errored = new HashSet<LoadingMod>();
			var errorLog = new StringBuilder();

			foreach (var mod in mods)
				foreach (var dep in mod.properties.Refs(true))
				{
					LoadingMod inst;
					if (nameMap.TryGetValue(dep.mod, out inst) && inst.properties.version < dep.target)
					{
						errored.Add(mod);
						errorLog.AppendLine(mod + " requires version " + dep.target + "+ of " + dep.mod +
							" but version " + inst.properties.version + " is installed");
					}
				}

			if (errored.Count > 0)
				throw new ModSortingException(errored, errorLog.ToString());
		}

		internal static void EnsureSyncedDependencyStability(TopoSort<LoadingMod> synced, TopoSort<LoadingMod> full)
		{
			var errored = new HashSet<LoadingMod>();
			var errorLog = new StringBuilder();

			foreach (var mod in synced.list)
			{
				var chains = new List<List<LoadingMod>>();
				//define recursive chain finding method
				Action<LoadingMod, Stack<LoadingMod>> FindChains = null;
				FindChains = (search, stack) =>
				{
					stack.Push(search);

					if (search.properties.side == ModSide.Both && stack.Count > 1)
					{
						if (stack.Count > 2)//direct Both -> Both references are ignored
							chains.Add(stack.Reverse().ToList());
					}
					else
					{//recursively build the chain, all entries in stack should be unsynced
						foreach (var dep in full.Dependencies(search))
							FindChains(dep, stack);
					}

					stack.Pop();
				};
				FindChains(mod, new Stack<LoadingMod>());

				if (chains.Count == 0)
					continue;

				var syncedDependencies = synced.AllDependencies(mod);
				foreach (var chain in chains)
					if (!syncedDependencies.Contains(chain.Last()))
					{
						errored.Add(mod);
						errorLog.AppendLine(mod + " indirectly depends on " + chain.Last() + " via " + string.Join(" -> ", chain));
					}
			}

			if (errored.Count > 0)
			{
				errorLog.AppendLine("Some of these mods may not exist on both client and server. Add a direct sort entries or weak references.");
				throw new ModSortingException(errored, errorLog.ToString());
			}
		}

		private static TopoSort<LoadingMod> BuildSort(ICollection<LoadingMod> mods)
		{
			var nameMap = mods.ToDictionary(mod => mod.Name);
			return new TopoSort<LoadingMod>(mods,
				mod => mod.properties.sortAfter.Where(nameMap.ContainsKey).Select(name => nameMap[name]),
				mod => mod.properties.sortBefore.Where(nameMap.ContainsKey).Select(name => nameMap[name]));
		}

		internal static List<LoadingMod> Sort(ICollection<LoadingMod> mods)
		{
			var preSorted = mods.OrderBy(mod => mod.Name).ToList();
			var syncedSort = BuildSort(preSorted.Where(mod => mod.properties.side == ModSide.Both).ToList());
			var fullSort = BuildSort(preSorted);
			EnsureSyncedDependencyStability(syncedSort, fullSort);

			try
			{
				var syncedList = syncedSort.Sort();

				//preserve synced order
				for (int i = 1; i < syncedList.Count; i++)
					fullSort.AddEntry(syncedList[i - 1], syncedList[i]);

				return fullSort.Sort();
			}
			catch (TopoSort<LoadingMod>.SortingException e)
			{
				throw new ModSortingException(e.set, e.Message);
			}
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
			ModTileEntity.Unload();
			WallLoader.Unload();
			ProjectileLoader.Unload();
			NPCLoader.Unload();
			NPCHeadLoader.Unload();
			PlayerHooks.Unload();
			BuffLoader.Unload();
			MountLoader.Unload();
			ModGore.Unload();
			SoundLoader.Unload();
			DisposeMusic();
			BackgroundTextureLoader.Unload();
			UgBgStyleLoader.Unload();
			SurfaceBgStyleLoader.Unload();
			GlobalBgStyleLoader.Unload();
			WaterStyleLoader.Unload();
			WaterfallStyleLoader.Unload();
			mods.Clear();
			WorldHooks.Unload();
			ResizeArrays(true);
			for (int k = 0; k < Recipe.maxRecipes; k++)
			{
				Main.recipe[k] = new Recipe();
			}
			Recipe.numRecipes = 0;
			RecipeGroupHelper.ResetRecipeGroups();
			Recipe.SetupRecipes();
			MapLoader.UnloadModMap();
			ItemSorting.SetupWhiteLists();
			modHotKeys.Clear();
			RecipeHooks.Unload();
			CommandManager.Unload();
			TagSerializer.Reload();
			ModNet.Unload();
			GameContent.UI.CustomCurrencyManager.Initialize();
			CleanupModReferences();

			if (!Main.dedServ && Main.netMode != 1) //disable vanilla client compatiblity restrictions when reloading on a client
				ModNet.AllowVanillaClients = false;
		}

		/// <summary>
		/// Several arrays and other fields hold references to various classes from mods, we need to clean them up to give properly coded mods a chance to be completely free of references
		/// so that they can be collected by the garbage collection. For most things eventually they will be replaced during gameplay, but we want the old instance completely gone quickly.
		/// </summary>
		internal static void CleanupModReferences()
		{
			// Clear references to ModPlayer instances
			for (int i = 0; i < 256; i++)
			{
				Main.player[i] = new Player();
			}
			Main.ActivePlayerFileData = new Terraria.IO.PlayerFileData();
			Main._characterSelectMenu._playerList?.Clear();
			Main.PlayerList.Clear();

			foreach (var npc in Main.npc)
			{
				npc.SetDefaults(0);
			}

			foreach (var item in Main.item)
			{
				item.SetDefaults(0);
			}
			ItemSlot.singleSlotArray[0]?.SetDefaults(0);

			for (int i = 0; i < Main.chest.Length; i++)
			{
				Main.chest[i] = new Chest();
			}

			// TODO: Display this warning to modders
			GC.Collect();
			if (ModLoader.isModder)
			{
				foreach (var weakReference in loadedModsWeakReferences)
				{
					if (weakReference.IsAlive)
						ErrorLogger.Log((weakReference.Target as Mod).Name + " not fully unloaded during unload.");
				}
			}
		}

		private static void DisposeMusic()
		{
			for (int i = 0; i < Main.music.Length; i++)
			{
				MusicStreaming music = Main.music[i] as MusicStreaming;
				if (music != null)
				{
					if (i < Main.maxMusic)
					{
						Main.music[i] = Main.soundBank.GetCue("Music_" + i);
					}
					else
					{
						Main.music[i] = null;
					}
					music.Stop(AudioStopOptions.Immediate);
					music.Dispose();
				}
			}
		}

		internal static void Reload()
		{
			Unload();
			Main.menuMode = Interface.loadModsID;
		}

		internal static bool LoadSide(ModSide side) => side != (Main.dedServ ? ModSide.Client : ModSide.Server);

		/// <summary>A cached list of enabled mods (not necessarily currently loaded or even installed), mirroring the enabled.json file.</summary>
		private static HashSet<string> enabledMods;
		internal static bool IsEnabled(TmodFile mod)
		{
			if (enabledMods == null)
			{
				LoadEnabledModCache();
			}
			return enabledMods.Contains(mod.name);
		}

		private static void LoadEnabledModCache()
		{
			enabledMods = new HashSet<string>();
			string path = ModPath + Path.DirectorySeparatorChar + "enabled.json";
			if (File.Exists(path))
			{
				using (StreamReader r = new StreamReader(path))
				{
					string json = r.ReadToEnd();
					try
					{
						enabledMods = JsonConvert.DeserializeObject<HashSet<string>>(json);
					}
					catch { }
				}
			}
		}

		internal static void SetModActive(TmodFile mod, bool active)
		{
			if (mod == null)
				return;
			if (enabledMods == null)
			{
				LoadEnabledModCache();
			}

			if (active)
			{
				enabledMods.Add(mod.name);
			}
			else
			{
				enabledMods.Remove(mod.name);
			}
			Directory.CreateDirectory(ModPath);
			string path = ModPath + Path.DirectorySeparatorChar + "enabled.json";
			string json = JsonConvert.SerializeObject(enabledMods, Newtonsoft.Json.Formatting.Indented);
			File.WriteAllText(path, json);
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
			return Directory.GetDirectories(ModSourcePath, "*", SearchOption.TopDirectoryOnly).Where(dir => new DirectoryInfo(dir).Name != ".vs").ToArray();
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

		/// <summary>
		/// Gets the byte representation of the file with the specified name. The name is in the format of "ModFolder/OtherFolders/FileNameWithExtension". Throws an ArgumentException if the file does not exist.
		/// </summary>
		/// <exception cref="MissingResourceException">Missing mod: " + name</exception>
		public static byte[] GetFileBytes(string name)
		{
			string modName, subName;
			SplitName(name, out modName, out subName);

			Mod mod = GetMod(modName);
			if (mod == null)
				throw new MissingResourceException("Missing mod: " + name);

			return mod.GetFileBytes(subName);
		}

		/// <summary>
		/// Returns whether or not a file with the specified name exists.
		/// </summary>
		public static bool FileExists(string name)
		{
			if (!name.Contains('/'))
				return false;

			string modName, subName;
			SplitName(name, out modName, out subName);

			Mod mod = GetMod(modName);
			return mod != null && mod.FileExists(subName);
		}

		/// <summary>
		/// Gets the texture with the specified name. The name is in the format of "ModFolder/OtherFolders/FileNameWithoutExtension". Throws an ArgumentException if the texture does not exist. If a vanilla texture is desired, the format "Terraria/FileNameWithoutExtension" will reference an image from the "terraria/Content/Images" folder. Note: Texture2D is in the Microsoft.Xna.Framework.Graphics namespace.
		/// </summary>
		/// <exception cref="MissingResourceException">Missing mod: " + name</exception>
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

		/// <summary>
		/// Returns whether or not a texture with the specified name exists.
		/// </summary>
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

		/// <summary>
		/// Gets the sound with the specified name. The name is in the same format as for texture names. Throws an ArgumentException if the sound does not exist. Note: SoundEffect is in the Microsoft.Xna.Framework.Audio namespace.
		/// </summary>
		/// <exception cref="MissingResourceException">Missing mod: " + name</exception>
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

		/// <summary>
		/// Returns whether or not a sound with the specified name exists.
		/// </summary>
		public static bool SoundExists(string name)
		{
			if (!name.Contains('/'))
				return false;

			string modName, subName;
			SplitName(name, out modName, out subName);

			Mod mod = GetMod(modName);
			return mod != null && mod.SoundExists(subName);
		}

		/// <summary>
		/// Gets the music with the specified name. The name is in the same format as for texture names. Throws an ArgumentException if the music does not exist. Note: SoundMP3 is in the Terraria.ModLoader namespace.
		/// </summary>
		/// <exception cref="MissingResourceException">Missing mod: " + name</exception>
		public static Music GetMusic(string name)
		{
			if (Main.dedServ) { return null; }
			string modName, subName;
			SplitName(name, out modName, out subName);
			Mod mod = GetMod(modName);
			if (mod == null) { throw new MissingResourceException("Missing mod: " + name); }
			return mod.GetMusic(subName);
		}

		/// <summary>
		/// Returns whether or not a sound with the specified name exists.
		/// </summary>
		public static bool MusicExists(string name)
		{
			if (!name.Contains('/')) { return false; }
			string modName, subName;
			SplitName(name, out modName, out subName);
			Mod mod = GetMod(modName);
			return mod != null && mod.MusicExists(subName);
		}

		public static ModHotKey RegisterHotKey(Mod mod, string name, string defaultKey)
		{
			string key = mod.Name + ": " + name;
			modHotKeys[key] = new ModHotKey(mod, name, defaultKey);
			return modHotKeys[key];
		}

		internal static void SaveConfiguration()
		{
			Main.Configuration.Put("ModBrowserPassphrase", ModLoader.modBrowserPassphrase);
			Main.Configuration.Put("SteamID64", ModLoader.steamID64);
			Main.Configuration.Put("DownloadModsFromServers", ModNet.downloadModsFromServers);
			Main.Configuration.Put("OnlyDownloadSignedModsFromServers", ModNet.onlyDownloadSignedMods);
			Main.Configuration.Put("DontRemindModBrowserUpdateReload", ModLoader.dontRemindModBrowserUpdateReload);
			Main.Configuration.Put("DontRemindModBrowserDownloadEnable", ModLoader.dontRemindModBrowserDownloadEnable);
			Main.Configuration.Put("MusicStreamMode", ModLoader.musicStreamMode);
		}

		internal static void LoadConfiguration()
		{
			Main.Configuration.Get<string>("ModBrowserPassphrase", ref ModLoader.modBrowserPassphrase);
			Main.Configuration.Get<string>("SteamID64", ref ModLoader.steamID64);
			Main.Configuration.Get<bool>("DownloadModsFromServers", ref ModNet.downloadModsFromServers);
			Main.Configuration.Get<bool>("OnlyDownloadSignedModsFromServers", ref ModNet.onlyDownloadSignedMods);
			Main.Configuration.Get<bool>("DontRemindModBrowserUpdateReload", ref ModLoader.dontRemindModBrowserUpdateReload);
			Main.Configuration.Get<bool>("DontRemindModBrowserDownloadEnable", ref ModLoader.dontRemindModBrowserDownloadEnable);
			Main.Configuration.Get<byte>("MusicStreamMode", ref ModLoader.musicStreamMode);
		}

		/// <summary>
		/// Allows type inference on T and F
		/// </summary>
		internal static void BuildGlobalHook<T, F>(ref F[] list, IList<T> providers, Expression<Func<T, F>> expr)
		{
			list = BuildGlobalHook(providers, expr).Select(expr.Compile()).ToArray();
		}

		internal static T[] BuildGlobalHook<T, F>(IList<T> providers, Expression<Func<T, F>> expr)
		{
			return BuildGlobalHook(providers, Method(expr));
		}

		internal static T[] BuildGlobalHook<T>(IList<T> providers, MethodInfo method)
		{
			if (!method.IsVirtual) throw new ArgumentException("Cannot build hook for non-virtual method " + method);
			var argTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
			return providers.Where(p => p.GetType().GetMethod(method.Name, argTypes).DeclaringType != typeof(T)).ToArray();
		}

		internal static MethodInfo Method<T, F>(Expression<Func<T, F>> expr)
		{
			MethodInfo method;
			try
			{
				var convert = expr.Body as UnaryExpression;
				var makeDelegate = convert.Operand as MethodCallExpression;
				var methodArg = makeDelegate.Arguments[2] as ConstantExpression;
				method = methodArg.Value as MethodInfo;
				if (method == null) throw new NullReferenceException();
			}
			catch (Exception e)
			{
				throw new ArgumentException("Invalid hook expression " + expr, e);
			}
			return method;
		}

		internal class LoadingMod
		{
			public readonly TmodFile modFile;
			public readonly BuildProperties properties;

			public string Name => modFile.name;

			public override string ToString() => Name;

			public LoadingMod(TmodFile modFile, BuildProperties properties)
			{
				this.modFile = modFile;
				this.properties = properties;
			}
		}
	}
}
