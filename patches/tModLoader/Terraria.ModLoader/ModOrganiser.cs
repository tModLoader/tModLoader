using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Terraria.Localization;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class serves as a central place for the sorting and handling dependencies of mods
	/// </summary>
	public static class ModOrganiser
	{
		private static readonly Stack<string> loadOrder = new Stack<string>();
		private static WeakReference[] loadedModsWeakReferences = new WeakReference[0];
		private static Mod[] loadedMods = new Mod[0];
		internal static readonly IDictionary<string, Mod> mods = new Dictionary<string, Mod>(StringComparer.OrdinalIgnoreCase);
		internal static string commandLineModPack = "";
		
		public static int ModCount => loadedMods.Length;

		/// <summary>
		/// Gets the instance of the Mod with the specified name.
		/// </summary>
		public static Mod GetMod(string name)
		{
			mods.TryGetValue(name, out Mod m);
			return m;
		}

		public static Mod GetMod(int index)
		{
			return index >= 0 && index < loadedMods.Length ? loadedMods[index] : null;
		}

		public static Mod[] LoadedMods => (Mod[])loadedMods.Clone();
		public static string[] LoadedModsNames => (string[]) loadedMods.Select(x => x.Name).ToArray().Clone();

		internal static void UnloadMods()
		{
			while (loadOrder.Count > 0)
				GetMod(loadOrder.Pop()).UnloadContent();

			loadOrder.Clear();
			loadedMods = new Mod[0];
		}
		
		internal static Dictionary<string, LocalMod> modsDirCache = new Dictionary<string, LocalMod>();
		internal static LocalMod[] FindMods()
		{
			Directory.CreateDirectory(ModLoader.ModPath);
			var mods = new List<LocalMod>();

			foreach (string fileName in Directory.GetFiles(ModLoader.ModPath, "*.tmod", SearchOption.TopDirectoryOnly))
			{
				if (Path.GetFileName(fileName) == "temporaryDownload.tmod")
					continue;
				var lastModified = File.GetLastWriteTime(fileName);
				if (!modsDirCache.TryGetValue(fileName, out var mod) || mod.lastModified != lastModified)
				{
					var modFile = new TmodFile(fileName);
					try
					{
						modFile.Read(TmodFile.LoadedState.Info);
					}
					catch (Exception e) //this will probably spam, given the number of calls to FindMods
					{
						// TODO: Reflect these skipped Mods in the UI somehow.
						//ErrorLogger.LogException(e, Language.GetTextValue("tModLoader.LoadErrorErrorReadingModFile", modFile.path));
						continue;
					}

					mod = new LocalMod(modFile) { lastModified = lastModified };
					modsDirCache[fileName] = mod;
				}
				mods.Add(mod);
			}
			return mods.OrderBy(x => x.Name, StringComparer.InvariantCulture).ToArray();
		}
		
		private static bool CommandLineModPackOverride()
		{
			if (commandLineModPack == "")
				return true;

			if (!commandLineModPack.EndsWith(".json"))
				commandLineModPack += ".json";

			string filePath = Path.Combine(UI.UIModPacks.ModListSaveDirectory, commandLineModPack);

			try
			{
				Directory.CreateDirectory(UI.UIModPacks.ModListSaveDirectory);

				Console.WriteLine(Language.GetTextValue("tModLoader.LoadingSpecifiedModPack", commandLineModPack) + "\n");
				var modSet = JsonConvert.DeserializeObject<HashSet<string>>(File.ReadAllText(filePath));
				foreach (var mod in FindMods())
				{
					SetModEnabled(mod.Name, modSet.Contains(mod.Name));
				}

				return true;
			}
			catch (Exception e)
			{
				string err;
				if (e is FileNotFoundException)
					err = Language.GetTextValue("tModLoader.ModPackDoesNotExist", filePath) + "\n";
				else
					err = Language.GetTextValue("tModLoader.ModPackDoesNotExist", commandLineModPack, e.Message) + "\n";

				if (Main.dedServ)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine(err);
					Console.ResetColor();
				}
				else
				{
					Interface.errorMessage.SetMessage(err);
				}

				return false;
			}
			finally
			{
				commandLineModPack = "";
			}
		}
		
		internal static IDictionary<string, Mod> LoadInstances()
		{
			//load all referenced assemblies before mods for compiling
			ModCompile.LoadReferences();

			if (!CommandLineModPackOverride())
				return null;

			Interface.loadMods.SetProgressFinding();
			var modsToLoad = FindMods().Where(mod => IsEnabled(mod.Name) && LoadSide(mod.properties.side)).ToList();

			// Press shift while starting up tModLoader or while trapped in a reload cycle to skip loading all mods.
			if (Main.oldKeyState.PressingShift())
				modsToLoad.Clear();

			if (!VerifyNames(modsToLoad))
				return null;

			try
			{
				EnsureDependenciesExist(modsToLoad, false);
				EnsureTargetVersionsMet(modsToLoad);
				modsToLoad = Sort(modsToLoad);
			}
			catch (ModSortingException e)
			{
				foreach (var mod in e.errored)
					mod.Enabled = false;

				ErrorLogger.LogDependencyError(e.Message);
				return null;
			}

			var modInstances = AssemblyManager.InstantiateMods(modsToLoad);
			if (modInstances == null)
				return null;

			modInstances.Insert(0, new ModLoaderMod());
			loadedMods = modInstances.ToArray();
			loadedModsWeakReferences = loadedMods.Skip(1).Select(x => new WeakReference(x)).ToArray();
			foreach (var mod in modInstances)
			{
				loadOrder.Push(mod.Name);
				mods[mod.Name] = mod;
			}

			return new Dictionary<string, Mod>(mods);
		}
		
		private static bool VerifyNames(List<LocalMod> mods)
		{
			var names = new HashSet<string>();
			foreach (var mod in mods)
			{
				try
				{
					if (mod.Name.Length == 0)
						throw new ModNameException(Language.GetTextValue("tModLoader.BuildErrorModNameEmpty"));

					if (mod.Name.Equals("Terraria", StringComparison.InvariantCultureIgnoreCase))
						throw new ModNameException(Language.GetTextValue("tModLoader.BuildErrorModNamedTerraria"));

					if (names.Contains(mod.Name))
						throw new ModNameException(Language.GetTextValue("tModLoader.BuildErrorTwoModsSameName", mod.Name));

					if (mod.Name.IndexOf('.') >= 0)
						throw new ModNameException(Language.GetTextValue("tModLoader.BuildErrorModNameHasPeriod"));

					names.Add(mod.Name);
				}
				catch (Exception e)
				{
					mod.Enabled = false;
					ErrorLogger.LogLoadingError(mod.Name, mod.modFile.tModLoaderVersion, e);
					return false;
				}
			}

			return true;
		}

		internal static void EnsureDependenciesExist(ICollection<LocalMod> mods, bool includeWeak)
		{
			var nameMap = mods.ToDictionary(mod => mod.Name);
			var errored = new HashSet<LocalMod>();
			var errorLog = new StringBuilder();

			foreach (var mod in mods)
				foreach (var depName in mod.properties.RefNames(includeWeak))
					if (!nameMap.ContainsKey(depName))
					{
						errored.Add(mod);
						errorLog.AppendLine(Language.GetTextValue("tModLoader.LoadErrorDependencyMissing", depName, mod));
					}

			if (errored.Count > 0)
				throw new ModSortingException(errored, errorLog.ToString());
		}

		internal static void EnsureTargetVersionsMet(ICollection<LocalMod> mods)
		{
			var nameMap = mods.ToDictionary(mod => mod.Name);
			var errored = new HashSet<LocalMod>();
			var errorLog = new StringBuilder();

			foreach (var mod in mods)
				foreach (var dep in mod.properties.Refs(true))
					if (nameMap.TryGetValue(dep.mod, out var inst) && inst.properties.version < dep.target)
					{
						errored.Add(mod);
						errorLog.AppendLine(Language.GetTextValue("tModLoader.LoadErrorDependencyVersionTooLow", mod, dep.target, dep.mod, inst.properties.version));
					}

			if (errored.Count > 0)
				throw new ModSortingException(errored, errorLog.ToString());
		}

		internal static void EnsureSyncedDependencyStability(TopoSort<LocalMod> synced, TopoSort<LocalMod> full)
		{
			var errored = new HashSet<LocalMod>();
			var errorLog = new StringBuilder();

			foreach (var mod in synced.list)
			{
				var chains = new List<List<LocalMod>>();
				//define recursive chain finding method
				Action<LocalMod, Stack<LocalMod>> FindChains = null;
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
				FindChains(mod, new Stack<LocalMod>());

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
		
		private static TopoSort<LocalMod> BuildSort(ICollection<LocalMod> mods)
		{
			var nameMap = mods.ToDictionary(mod => mod.Name);
			return new TopoSort<LocalMod>(mods,
				mod => mod.properties.sortAfter.Where(nameMap.ContainsKey).Select(name => nameMap[name]),
				mod => mod.properties.sortBefore.Where(nameMap.ContainsKey).Select(name => nameMap[name]));
		}

		internal static List<LocalMod> Sort(ICollection<LocalMod> mods)
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
			catch (TopoSort<LocalMod>.SortingException e)
			{
				throw new ModSortingException(e.set, e.Message);
			}
		}
		
		internal static bool LoadSide(ModSide side) => side != (Main.dedServ ? ModSide.Client : ModSide.Server);

		/// <summary>A cached list of enabled mods (not necessarily currently loaded or even installed), mirroring the enabled.json file.</summary>
		private static HashSet<string> _enabledMods;

		internal static HashSet<string> EnabledMods
		{
			get
			{
				if (_enabledMods == null)
				{
					try
					{
						string path = ModLoader.ModPath + Path.DirectorySeparatorChar + "enabled.json";
						_enabledMods = JsonConvert.DeserializeObject<HashSet<string>>(File.ReadAllText(path));
					}
					catch
					{
						_enabledMods = new HashSet<string>();
					}
				}
				return _enabledMods;
			}
		}

		internal static bool IsEnabled(string modName) => EnabledMods.Contains(modName);

		internal static void EnableMod(string modName) => SetModEnabled(modName, true);
		internal static void DisableMod(string modName) => SetModEnabled(modName, false);

		internal static void SetModEnabled(string modName, bool active)
		{
			if (active)
				EnabledMods.Add(modName);
			else
				EnabledMods.Remove(modName);

			//save
			Directory.CreateDirectory(ModLoader.ModPath);
			string path = ModLoader.ModPath + Path.DirectorySeparatorChar + "enabled.json";
			_enabledMods.IntersectWith(FindMods().Select(x => x.Name)); // Clear out mods that no longer exist.
			string json = JsonConvert.SerializeObject(EnabledMods, Formatting.Indented);
			File.WriteAllText(path, json);
		}
		
		/// <summary>
		/// Several arrays and other fields hold references to various classes from mods, we need to clean them up to give properly coded mods a chance to be completely free of references so that they can be collected by the garbage collection. For most things eventually they will be replaced during gameplay, but we want the old instance completely gone quickly.
		/// </summary>
		internal static void CleanupModReferences()
		{
			// Clear references to ModPlayer instances
			for (int i = 0; i < 256; i++)
			{
				Main.player[i] = new Player();
			}
			// TODO: This breaks net reload. Restore this cleanup step later?
			// Main.ActivePlayerFileData = new Terraria.IO.PlayerFileData();
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
						ErrorLogger.Log((weakReference.Target as Mod)?.Name ?? "null" + " not fully unloaded during unload.");
				}
			}
		}
	}
}