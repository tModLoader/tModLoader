using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Terraria.Localization;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader
{
	/// <summary>
	/// Responsible for sorting, dependency verification and organizing which mods to load
	/// </summary>
	internal class ModOrganizer
	{
		internal static Dictionary<string, LocalMod> modsDirCache = new Dictionary<string, LocalMod>();

		internal static List<string> readFailures = new List<string>(); // TODO: Reflect these skipped Mods in the UI somehow.

		internal static Dictionary<string, List<string>> dependencyCache; // for some internal features that need to lookup dependencies after load

		internal static LocalMod[] FindMods() {
			Directory.CreateDirectory(ModLoader.ModPath);
			var mods = new List<LocalMod>();

			foreach (string fileName in Directory.GetFiles(ModLoader.ModPath, "*.tmod", SearchOption.TopDirectoryOnly)) {
				if (Path.GetFileName(fileName) == "temporaryDownload.tmod")
					continue;
				var lastModified = File.GetLastWriteTime(fileName);
				if (!modsDirCache.TryGetValue(fileName, out var mod) || mod.lastModified != lastModified) {
					try {
						var modFile = new TmodFile(fileName);
						modFile.Read();
						mod = new LocalMod(modFile) { lastModified = lastModified };
						modFile.Close();
					}
					catch (Exception e) {
						if (!readFailures.Contains(fileName)) {
							Logging.tML.Warn("Failed to read " + fileName, e);
						}
						else {
							readFailures.Add(fileName);
						}
						continue;
					}
					modsDirCache[fileName] = mod;
				}
				mods.Add(mod);
			}
			return mods.OrderBy(x => x.Name, StringComparer.InvariantCulture).ToArray();
		}

		private static bool LoadSide(ModSide side) => side != (Main.dedServ ? ModSide.Client : ModSide.Server);

		public static List<Mod> LoadMods() {
			CommandLineModPackOverride();

			// Alternate fix for updating enabled mods
			//foreach (string fileName in Directory.GetFiles(ModLoader.ModPath, "*.tmod.update", SearchOption.TopDirectoryOnly)) {
			//	File.Copy(fileName, Path.GetFileNameWithoutExtension(fileName), true);
			//	File.Delete(fileName);
			//}
			Interface.loadMods.SetLoadStage("tModLoader.MSFinding");
			var modsToLoad = FindMods().Where(mod => ModLoader.IsEnabled(mod.Name) && LoadSide(mod.properties.side)).ToList();

			// Press shift while starting up tModLoader or while trapped in a reload cycle to skip loading all mods.
			if (Main.oldKeyState.PressingShift() || ModLoader.skipLoad) {
				ModLoader.skipLoad = false;
				modsToLoad.Clear();
				Interface.loadMods.SetLoadStage("Loading Cancelled");
			}

			VerifyNames(modsToLoad);

			try {
				EnsureDependenciesExist(modsToLoad, false);
				EnsureTargetVersionsMet(modsToLoad);
				modsToLoad = Sort(modsToLoad);
				dependencyCache = modsToLoad.ToDictionary(m => m.Name, m => m.properties.RefNames(false).ToList());
			}
			catch (ModSortingException e) {
				e.Data["mods"] = e.errored.Select(m => m.Name).ToArray();
				e.Data["hideStackTrace"] = true;
				throw;
			}

			return AssemblyManager.InstantiateMods(modsToLoad);
		}

		internal static string commandLineModPack = "";
		private static void CommandLineModPackOverride() {
			if (commandLineModPack == "")
				return;

			if (!commandLineModPack.EndsWith(".json"))
				commandLineModPack += ".json";

			string filePath = Path.Combine(UIModPacks.ModListSaveDirectory, commandLineModPack);

			try {
				Directory.CreateDirectory(UIModPacks.ModListSaveDirectory);
				Logging.ServerConsoleLine(Language.GetTextValue("tModLoader.LoadingSpecifiedModPack", commandLineModPack));
				var modSet = JsonConvert.DeserializeObject<HashSet<string>>(File.ReadAllText(filePath));
				foreach (var mod in FindMods()) {
					ModLoader.SetModEnabled(mod.Name, modSet.Contains(mod.Name));
				}
			}
			catch (Exception e) {
				var msg = (e is FileNotFoundException) ? Language.GetTextValue("tModLoader.ModPackDoesNotExist", filePath) : Language.GetTextValue("tModLoader.ModPackMalformed", commandLineModPack);
				throw new Exception(msg, e);
			}
			finally {
				commandLineModPack = "";
			}
		}

		private static void VerifyNames(List<LocalMod> mods) {
			var names = new HashSet<string>();
			var errors = new List<string>();
			var erroredMods = new List<LocalMod>();
			foreach (var mod in mods) {
				if (mod.Name.Length == 0)
					errors.Add(Language.GetTextValue("tModLoader.BuildErrorModNameEmpty"));
				else if (mod.Name.Equals("Terraria", StringComparison.InvariantCultureIgnoreCase))
					errors.Add(Language.GetTextValue("tModLoader.BuildErrorModNamedTerraria"));
				else if (mod.Name.IndexOf('.') >= 0)
					errors.Add(Language.GetTextValue("tModLoader.BuildErrorModNameHasPeriod"));
				else if (!names.Add(mod.Name))
					errors.Add(Language.GetTextValue("tModLoader.BuildErrorTwoModsSameName", mod.Name));
				else
					continue;

				erroredMods.Add(mod);
			}

			if (erroredMods.Count > 0) {
				var e = new Exception(string.Join("\n", errors));
				e.Data["mods"] = erroredMods.Select(m => m.Name).ToArray();
				throw e;
			}
		}

		internal static void EnsureDependenciesExist(ICollection<LocalMod> mods, bool includeWeak) {
			var nameMap = mods.ToDictionary(mod => mod.Name);
			var errored = new HashSet<LocalMod>();
			var errorLog = new StringBuilder();

			foreach (var mod in mods)
				foreach (var depName in mod.properties.RefNames(includeWeak))
					if (!nameMap.ContainsKey(depName)) {
						errored.Add(mod);
						errorLog.AppendLine(Language.GetTextValue("tModLoader.LoadErrorDependencyMissing", depName, mod));
					}

			if (errored.Count > 0)
				throw new ModSortingException(errored, errorLog.ToString());
		}

		internal static void EnsureTargetVersionsMet(ICollection<LocalMod> mods) {
			var nameMap = mods.ToDictionary(mod => mod.Name);
			var errored = new HashSet<LocalMod>();
			var errorLog = new StringBuilder();

			foreach (var mod in mods)
				foreach (var dep in mod.properties.Refs(true)) {
					if (dep.target == null || !nameMap.TryGetValue(dep.mod, out var inst))
						continue;
					
					if (inst.properties.version < dep.target) {
						errored.Add(mod);
						errorLog.AppendLine(Language.GetTextValue("tModLoader.LoadErrorDependencyVersionTooLow", mod, dep.target, dep.mod, inst.properties.version));
					} else if (inst.properties.version.Major != dep.target.Major) {
						errored.Add(mod);
						errorLog.AppendLine(Language.GetTextValue("tModLoader.LoadErrorMajorVersionMismatch", mod, dep.target, dep.mod, inst.properties.version));
					}
				}

			if (errored.Count > 0)
				throw new ModSortingException(errored, errorLog.ToString());
		}

		internal static void EnsureSyncedDependencyStability(TopoSort<LocalMod> synced, TopoSort<LocalMod> full) {
			var errored = new HashSet<LocalMod>();
			var errorLog = new StringBuilder();

			foreach (var mod in synced.list) {
				var chains = new List<List<LocalMod>>();
				//define recursive chain finding method
				Action<LocalMod, Stack<LocalMod>> FindChains = null;
				FindChains = (search, stack) => {
					stack.Push(search);

					if (search.properties.side == ModSide.Both && stack.Count > 1) {
						if (stack.Count > 2)//direct Both -> Both references are ignored
							chains.Add(stack.Reverse().ToList());
					}
					else {//recursively build the chain, all entries in stack should be unsynced
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
					if (!syncedDependencies.Contains(chain.Last())) {
						errored.Add(mod);
						errorLog.AppendLine(mod + " indirectly depends on " + chain.Last() + " via " + String.Join(" -> ", chain));
					}
			}

			if (errored.Count > 0) {
				errorLog.AppendLine("Some of these mods may not exist on both client and server. Add a direct sort entries or weak references.");
				throw new ModSortingException(errored, errorLog.ToString());
			}
		}

		private static TopoSort<LocalMod> BuildSort(ICollection<LocalMod> mods) {
			var nameMap = mods.ToDictionary(mod => mod.Name);
			return new TopoSort<LocalMod>(mods,
				mod => mod.properties.sortAfter.Where(nameMap.ContainsKey).Select(name => nameMap[name]),
				mod => mod.properties.sortBefore.Where(nameMap.ContainsKey).Select(name => nameMap[name]));
		}

		internal static List<LocalMod> Sort(ICollection<LocalMod> mods) {
			var preSorted = mods.OrderBy(mod => mod.Name).ToList();
			var syncedSort = BuildSort(preSorted.Where(mod => mod.properties.side == ModSide.Both).ToList());
			var fullSort = BuildSort(preSorted);
			EnsureSyncedDependencyStability(syncedSort, fullSort);

			try {
				var syncedList = syncedSort.Sort();

				//preserve synced order
				for (int i = 1; i < syncedList.Count; i++)
					fullSort.AddEntry(syncedList[i - 1], syncedList[i]);

				return fullSort.Sort();
			}
			catch (TopoSort<LocalMod>.SortingException e) {
				throw new ModSortingException(e.set, e.Message);
			}
		}

		internal static void SaveEnabledMods() {
			Directory.CreateDirectory(ModLoader.ModPath);
			ModLoader.EnabledMods.IntersectWith(FindMods().Select(x => x.Name)); // Clear out mods that no longer exist.
			string json = JsonConvert.SerializeObject(ModLoader.EnabledMods, Formatting.Indented);
			var path = Path.Combine(ModLoader.ModPath, "enabled.json");
			File.WriteAllText(path, json);
		}

		internal static HashSet<string> LoadEnabledMods() {
			try {
				var path = Path.Combine(ModLoader.ModPath, "enabled.json");
				return JsonConvert.DeserializeObject<HashSet<string>>(File.ReadAllText(path));
			}
			catch {
				return new HashSet<string>();
			}
		}
	}
}