using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Terraria.Localization;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.DownloadManager;
using Terraria.Social.Base;
using Terraria.Social.Steam;

namespace Terraria.ModLoader.Core
{
	/// <summary>
	/// Responsible for sorting, dependency verification and organizing which mods to load
	/// </summary>
	internal static class ModOrganizer
	{
		internal static string modPath = Path.Combine(Main.SavePath, "Mods");
		internal static string commandLineModPack;

		private static Dictionary<string, LocalMod> modsDirCache = new Dictionary<string, LocalMod>();
		private static List<string> readFailures = new List<string>(); // TODO: Reflect these skipped Mods in the UI somehow.

		internal static WorkshopHelper.UGCBased.Downloader WorkshopFileFinder = new WorkshopHelper.UGCBased.Downloader();

		internal static LocalMod[] FindMods() {
			Directory.CreateDirectory(ModLoader.ModPath);
			var mods = new List<LocalMod>();
			var names = new HashSet<string>();

			DeleteTemporaryFiles();

			WorkshopFileFinder.Refresh(new WorkshopIssueReporter());

			// Prioritize loading Mods from Mods folder for Dev/Beta simplicitiy.
			foreach (string mod in Directory.GetFiles(ModLoader.ModPath, "*.tmod", SearchOption.TopDirectoryOnly))
				AttemptLoadMod(mod, ref mods, ref names);

			// Load Mods from Workshop downloads
			foreach (string repo in WorkshopFileFinder.ModPaths) {
				var fileName = GetActiveTmodInRepo(repo);
				if (fileName == null)
					continue;

				AttemptLoadMod(fileName, ref mods, ref names);
			}

			return mods.OrderBy(x => x.Name, StringComparer.InvariantCulture).ToArray();
		}

		private static bool AttemptLoadMod(string fileName, ref List<LocalMod> mods, ref HashSet<string> names) {
			var lastModified = File.GetLastWriteTime(fileName);

			if (!modsDirCache.TryGetValue(fileName, out var mod) || mod.lastModified != lastModified) {
				try {
					var modFile = new TmodFile(fileName);

					using (modFile.Open()) {
						mod = new LocalMod(modFile) {
							lastModified = lastModified
						};
					}
				}
				catch (Exception e) {
					if (!readFailures.Contains(fileName)) {
						Logging.tML.Warn("Failed to read " + fileName, e);
					}
					else {
						readFailures.Add(fileName);
					}

					return false;
				}

				modsDirCache[fileName] = mod;
			}

			// Ignore it from Workshop if it appeared in Mods folder/already exists.
			if (names.Add(mod.Name)) {
				mods.Add(mod);
			}
			else {
				Logging.tML.Warn($"Ignoring {mod.Name} found at: {fileName}. A mod with the same name already exists.");
			}
			return true;
		}

		private static void DeleteTemporaryFiles() {
			foreach (string path in GetTemporaryFiles()) {
				Logging.tML.Info($"Cleaning up leftover temporary file {Path.GetFileName(path)}");
				try {
					File.Delete(path);
				}
				catch (Exception e) {
					Logging.tML.Error($"Could not delete leftover temporary file {Path.GetFileName(path)}", e);
				}
			}
		}

		private static IEnumerable<string> GetTemporaryFiles() {
			return Directory.GetFiles(ModLoader.ModPath, $"*{DownloadFile.TEMP_EXTENSION}", SearchOption.TopDirectoryOnly)
				.Union(Directory.GetFiles(ModLoader.ModPath, "temporaryDownload.tmod", SearchOption.TopDirectoryOnly)); // Old tML remnant
		}

		internal static bool LoadSide(ModSide side) => side != (Main.dedServ ? ModSide.Client : ModSide.Server);

		internal static List<Mod> LoadMods(CancellationToken token) {
			CommandLineModPackOverride();

			// Alternate fix for updating enabled mods
			//foreach (string fileName in Directory.GetFiles(ModLoader.ModPath, "*.tmod.update", SearchOption.TopDirectoryOnly)) {
			//	File.Copy(fileName, Path.GetFileNameWithoutExtension(fileName), true);
			//	File.Delete(fileName);
			//}
			Interface.loadMods.SetLoadStage("tModLoader.MSFinding");
			var modsToLoad = FindMods().Where(mod => ModLoader.IsEnabled(mod.Name) && LoadSide(mod.properties.side)).ToList();

			// Press shift while starting up tModLoader or while trapped in a reload cycle to skip loading all mods.
			if (Main.instance.IsActive && Main.oldKeyState.PressingShift() || ModLoader.skipLoad || token.IsCancellationRequested) {
				ModLoader.skipLoad = false;
				modsToLoad.Clear();
				Interface.loadMods.SetLoadStage("Cancelling loading...");
			}

			VerifyNames(modsToLoad);

			try {
				EnsureDependenciesExist(modsToLoad, false);
				EnsureTargetVersionsMet(modsToLoad);
				modsToLoad = Sort(modsToLoad);
			}
			catch (ModSortingException e) {
				e.Data["mods"] = e.errored.Select(m => m.Name).ToArray();
				e.Data["hideStackTrace"] = true;
				throw;
			}

			return AssemblyManager.InstantiateMods(modsToLoad, token);
		}

		private static void CommandLineModPackOverride() {
			if (string.IsNullOrWhiteSpace(commandLineModPack))
				return;

			if (!commandLineModPack.EndsWith(".json"))
				commandLineModPack += ".json";

			string filePath = Path.Combine(UIModPacks.ModPacksDirectory, commandLineModPack);

			try {
				Directory.CreateDirectory(UIModPacks.ModPacksDirectory);
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
				commandLineModPack = null;
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
					}
					else if (inst.properties.version.Major != dep.target.Major) {
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
						errorLog.AppendLine(mod + " indirectly depends on " + chain.Last() + " via " + string.Join(" -> ", chain));
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
				if (!File.Exists(path)) {
					Logging.tML.Warn("Did not find enabled.json file");
					return new HashSet<string>();
				}
				return JsonConvert.DeserializeObject<HashSet<string>>(File.ReadAllText(path)) ?? new HashSet<string>();
			}
			catch (Exception e) {
				Logging.tML.Warn("Unknown error occurred when trying to read enabled.json", e);
				return new HashSet<string>();
			}
		}

		private static readonly Regex PublishFolderMetadata = new Regex(@"[/|\\]([0-9]{4}[.][0-9]{1,2})[/|\\]");

		internal static string GetActiveTmodInRepo(string repo) {
			Version tmodVersion = new Version(BuildInfo.tMLVersion.Major, BuildInfo.tMLVersion.Minor);
			string[] tmods = Directory.GetFiles(repo, "*.tmod", SearchOption.AllDirectories);
			if (tmods.Length == 1)
				return tmods[0];

			string val = null;
			Version currVersion = null;
			foreach (string fileName in tmods) {
				var match = PublishFolderMetadata.Match(fileName);

				if (match.Success) {
					Version testVers = new Version(match.Groups[1].Value);
					if (testVers > tmodVersion) {
						continue;
					}
					else if (testVers == currVersion) {
						val = fileName;
						break;
					}
					else if (testVers > currVersion) {
						currVersion = testVers;
						val = fileName;
					}
				}
				else if (val == null) {
					val = fileName;
					currVersion = new Version(0, 12);
				}
			}
			return val;
		}

		internal static void CleanupOldPublish(string repo) {
			string[] tmods = Directory.GetFiles(repo, "*.tmod", SearchOption.AllDirectories);
			if (tmods.Length <= 2)
				return;

			File.Delete(FindOldest(repo));
		}

		internal static string FindOldest(string repo) {
			string[] tmods = Directory.GetFiles(repo, "*.tmod", SearchOption.AllDirectories);
			if (tmods.Length == 1)
				return tmods[0];

			string val = null;
			Version currVersion = new Version(BuildInfo.tMLVersion.Major, BuildInfo.tMLVersion.Minor);
			foreach (string fileName in tmods) {
				var match = PublishFolderMetadata.Match(fileName);

				if (match.Success) {
					Version testVers = new Version(match.Groups[1].Value);
					if (testVers > currVersion) {
						continue;
					}
					else {
						val = Directory.GetParent(fileName).ToString();
						currVersion = testVers;
					}
				}
				else {
					val = fileName;
					break;
				}
			}

			return val;
		}

		internal static void DeleteMod(string tmodPath) {
			string parentDir = GetParentDir(tmodPath);

			if (TryReadManifest(parentDir, out var info)) {
				var modManager = new WorkshopHelper.ModManager(new Steamworks.PublishedFileId_t(info.workshopEntryId));
				modManager.Uninstall(parentDir);
			}
			else {
				File.Delete(tmodPath);
			}
		}

		internal static bool TryReadManifest(string parentDir, out FoundWorkshopEntryInfo info) {
			info = null;
			if (!parentDir.Contains(Path.Combine("steamapps", "workshop")))
				return false;

			string manifest = parentDir + Path.DirectorySeparatorChar + "workshop.json";

			return AWorkshopEntry.TryReadingManifest(manifest, out info);
		}

		internal static string GetParentDir(string tmodPath) {
			string parentDir = Directory.GetParent(tmodPath).ToString();
			if (!tmodPath.Contains(Path.Combine("steamapps", "workshop")))
				return parentDir;

			var match = PublishFolderMetadata.Match(parentDir + Path.DirectorySeparatorChar);
			if (match.Success)
				parentDir = Directory.GetParent(parentDir).ToString();

			return parentDir;
		}
	}
}