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
		private static HashSet<string> readFailures = new(); // TODO: Reflect these skipped Mods in the UI somehow.

		internal static string lastLaunchedModsFilePath = Path.Combine(Main.SavePath, "LastLaunchedMods.txt");
		internal static List<(string ModName, Version previousVersion)> modsThatUpdatedSinceLastLaunch = new List<(string ModName, Version previousVersion)>();

		internal static WorkshopHelper.UGCBased.Downloader WorkshopFileFinder = new WorkshopHelper.UGCBased.Downloader();

		internal static string ModPackActive = null;

		/// <summary>Mods from any location, using the default internal priority logic</summary>
		internal static LocalMod[] FindMods() => FoundMods = FindAllMods().GroupBy(m => m.Name).Select(SelectModVersion).Where(m => m != null).ToArray();

		internal static LocalMod[] FoundMods { get; private set; }
		internal static LocalMod[] AllFoundMods { get; private set; }

		internal static IReadOnlyList<LocalMod> FindAllMods() {
			Directory.CreateDirectory(ModLoader.ModPath);
			var mods = new List<LocalMod>();

			DeleteTemporaryFiles();
			WorkshopFileFinder.Refresh(new WorkshopIssueReporter());

			mods.AddRange(ParseMods(ModLocation.Dev, Directory.GetFiles(modPath, "*.tmod", SearchOption.TopDirectoryOnly)));

			foreach (string repo in WorkshopFileFinder.ModPaths) {
				mods.AddRange(ParseMods(ModLocation.Workshop, Directory.GetFiles(repo, "*.tmod", SearchOption.AllDirectories)));
			}

			if (!string.IsNullOrEmpty(ModPackActive)) {
				if (Directory.Exists(ModPackActive)) {
					mods.AddRange(ParseMods(ModLocation.ActiveModpack, Directory.GetFiles(ModPackActive, "*.tmod", SearchOption.AllDirectories)));
				}
				else {
					Logging.tML.Warn($"Active modpack missing, deactivating: {ModPackActive}");
					ModPackActive = null;
				}
			}

			return AllFoundMods = mods.ToArray();
		}

		private static IEnumerable<LocalMod> ParseMods(ModLocation location, IEnumerable<string> paths) {
			foreach (var path in paths)
				if (ParseMod(location, path, out var mod))
					yield return mod;
		}

		private static bool ParseMod(ModLocation location, string path, out LocalMod mod) {
			var lastModified = File.GetLastWriteTime(path);

			if (!modsDirCache.TryGetValue(path, out mod) || mod.lastModified != lastModified) {
				try {
					var modFile = new TmodFile(path);

					using (modFile.Open()) {
						modsDirCache[path] = mod = new LocalMod(location, modFile) {
							lastModified = lastModified
						};
					}

					if (mod.tModLoaderVersion > BuildInfo.tMLVersion)
						throw new Exception($"Ignored {mod.Name} {mod.Version}, built for a newer tML version ({mod.tModLoaderVersion})");
				}
				catch (Exception e) {
					if (readFailures.Add(path + "!" + e.Message))
						Logging.tML.Warn("Failed to read " + path, e);

					return false;
				}
			}

			return true;
		}

		private static LocalMod SelectModVersion(IEnumerable<LocalMod> versions) {
			var list = versions.ToList();
			void FilterOut(Predicate<LocalMod> condition, string msg) {
				for (int i = list.Count-1; i >= 0; i--) {
					var v = list[i];
					if (condition(v)) {
						Logging.tML.Warn($"Ignored {v.Name} {v.Version}, {msg}. {v.modFile.path}");
						list.RemoveAt(i);
					}
				}
			}

			if (BuildInfo.IsPreview)
				FilterOut(v => !v.properties.playableOnPreview && v.tModLoaderVersion.MajorMinor() == BuildInfo.tMLVersion.MajorMinor(),
					"preview early-access disabled");

			if (list.Any(v => v.location == ModLocation.ActiveModpack))
				FilterOut(v => v.location != ModLocation.ActiveModpack, "the active modpack has an overriding copy");

			var newestVersion = versions.Select(v => v.Version).Max();
			FilterOut(v => v.Version != newestVersion, $"a newer version exists (newestVersion)");

			if (list.Any(v => v.location == ModLocation.Workshop))
				FilterOut(v => v.location != ModLocation.ActiveModpack, "steam workshop is preferred when versions match");

			var selected = list.SingleOrDefault();

			// should never be called, but could help diagnose a logic error in the future. Better than calling SingleOrDefault and throwing.
			FilterOut(v => v != selected, "Logic Error, multiple versions remain. One was randomly selected");
			return selected;
		}

		/// <summary>
		/// Returns changes based on last time <see cref="SaveLastLaunchedMods"/> was called. Can be null if no changes.
		/// </summary>
		internal static string DetectModChangesForInfoMessage() {
			// Only display if enabled and file exists
			if (!ModLoader.showNewUpdatedModsInfo || !File.Exists(lastLaunchedModsFilePath)) {
				return null;
			}

			// For convenience, convert to dict
			var currMods = FindAllMods().Where(m => m.location == ModLocation.Workshop).ToDictionary(mod => mod.Name, mod => mod);


			// trycatch the read in case users manually modify the file
			try {
				// Construct dict of last mods
				var lines = File.ReadLines(lastLaunchedModsFilePath);
				var lastMods = new Dictionary<string, Version>();
				foreach (var line in lines) {
					string[] parts = line.Split(' ');
					if (parts.Length != 2) {
						continue;
					}

					string name = parts[0];
					string versionString = parts[1];
					lastMods.Add(name, new Version(versionString));
				}

				// Generate diff and display if exists
				// Only track new and updated, not deleted, maybe TODO? Would require saving the display name for deletion info
				var newMods = new List<LocalMod>();
				var updatedMods = new List<string>();
				var messages = new StringBuilder();
				foreach (var item in currMods) {
					string name = item.Key;
					var localMod = item.Value;
					Version version = localMod.Version;

					if (!lastMods.ContainsKey(name)) {
						newMods.Add(localMod);
						modsThatUpdatedSinceLastLaunch.Add((name, null));
					}
					else if (lastMods.TryGetValue(name, out var lastVersion) && lastVersion < version) {
						updatedMods.Add(name);
						modsThatUpdatedSinceLastLaunch.Add((name, lastVersion));
					}
				}

				if (newMods.Count > 0) {
					messages.Append(Language.GetTextValue("tModLoader.ShowNewUpdatedModsInfoMessageNewMods"));
					foreach (var newMod in newMods) {
						messages.Append($"\n  {newMod.Name} ({newMod.DisplayName})");
					}
				}

				if (updatedMods.Count > 0) {
					messages.Append(Language.GetTextValue("tModLoader.ShowNewUpdatedModsInfoMessageUpdatedMods"));
					foreach (var updatedMod in updatedMods) {
						string name = updatedMod;
						string displayName = currMods[name].DisplayName;
						Version lastVersion = lastMods[name];
						Version currVersion = currMods[name].Version;
						messages.Append($"\n  {name} ({displayName}) v{lastVersion} -> v{currVersion}");
					}
				}

				// If any info is accumulated, return it
				return messages.Length > 0 ? messages.ToString() : null;
			}
			catch {
				return null;
			}
		}

		/// <summary>
		/// Collects local mod status and saves it to a file.
		/// </summary>
		internal static void SaveLastLaunchedMods() {
			if (Main.dedServ) // Not relevant for the server yet, all features using this data are clientside
				return;

			if (!ModLoader.showNewUpdatedModsInfo) // Not needed if feature that uses the file is disabled
				return;

			var fileText = new StringBuilder();
			foreach (var mod in AllFoundMods.Where(m => m.location == ModLocation.Workshop)) {
				fileText.Append($"{mod.Name} {mod.Version}\n");
			}
			File.WriteAllText(lastLaunchedModsFilePath, fileText.ToString());
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
			return Directory.GetFiles(modPath, $"*{DownloadFile.TEMP_EXTENSION}", SearchOption.TopDirectoryOnly)
				.Union(Directory.GetFiles(modPath, "temporaryDownload.tmod", SearchOption.TopDirectoryOnly)); // Old tML remnant
		}

		internal static bool LoadSide(ModSide side) => side != (Main.dedServ ? ModSide.Client : ModSide.Server);

		internal static List<LocalMod> SelectAndSortMods(IEnumerable<LocalMod> mods, CancellationToken token) {
			var missing = ModLoader.EnabledMods.Except(mods.Select(mod => mod.Name)).ToList();
			if (missing.Any()) {
				Logging.tML.Info("Missing previously enabled mods: " + string.Join(", ", missing));
				foreach (var name in missing)
					ModLoader.EnabledMods.Remove(name);

				SaveEnabledMods();
			}

			// Press shift while starting up tModLoader or while trapped in a reload cycle to skip loading all mods.
			if (Main.instance.IsActive && Main.oldKeyState.PressingShift() || ModLoader.skipLoad || token.IsCancellationRequested) {
				ModLoader.skipLoad = false;
				Interface.loadMods.SetLoadStage("Cancelling loading...");
				return new();
			}

			CommandLineModPackOverride(mods);

			// Alternate fix for updating enabled mods
			//foreach (string fileName in Directory.GetFiles(modPath, "*.tmod.update", SearchOption.TopDirectoryOnly)) {
			//	File.Copy(fileName, Path.GetFileNameWithoutExtension(fileName), true);
			//	File.Delete(fileName);
			//}
			Interface.loadMods.SetLoadStage("tModLoader.MSFinding");
			var modsToLoad = mods.Where(mod => mod.Enabled && LoadSide(mod.properties.side)).ToList();

			VerifyNames(modsToLoad);

			try {
				EnsureDependenciesExist(modsToLoad, false);
				EnsureTargetVersionsMet(modsToLoad);
				return Sort(modsToLoad);
			}
			catch (ModSortingException e) {
				e.Data["mods"] = e.errored.Select(m => m.Name).ToArray();
				e.Data["hideStackTrace"] = true;
				throw;
			}
		}

		private static void CommandLineModPackOverride(IEnumerable<LocalMod> mods) {
			if (string.IsNullOrWhiteSpace(commandLineModPack))
				return;

			if (!commandLineModPack.EndsWith(".json"))
				commandLineModPack += ".json";

			string filePath = Path.Combine(UIModPacks.ModPacksDirectory, commandLineModPack);

			try {
				Directory.CreateDirectory(UIModPacks.ModPacksDirectory);
				Logging.ServerConsoleLine(Language.GetTextValue("tModLoader.LoadingSpecifiedModPack", commandLineModPack));
				var modSet = JsonConvert.DeserializeObject<HashSet<string>>(File.ReadAllText(filePath));
				foreach (var mod in mods) {
					mod.Enabled = modSet.Contains(mod.Name);
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

					if (inst.Version < dep.target) {
						errored.Add(mod);
						errorLog.AppendLine(Language.GetTextValue("tModLoader.LoadErrorDependencyVersionTooLow", mod, dep.target, dep.mod, inst.Version));
					}
					else if (inst.Version.Major != dep.target.Major) {
						errored.Add(mod);
						errorLog.AppendLine(Language.GetTextValue("tModLoader.LoadErrorMajorVersionMismatch", mod, dep.target, dep.mod, inst.Version));
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
			string json = JsonConvert.SerializeObject(ModLoader.EnabledMods, Formatting.Indented);
			var path = Path.Combine(modPath, "enabled.json");
			File.WriteAllText(path, json);
		}

		internal static HashSet<string> LoadEnabledMods() {
			try {
				var path = Path.Combine(modPath, "enabled.json");
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

		internal static LocalMod GetStableModInWorkshopRepo(string repo)
			=> ParseMods(ModLocation.Workshop, Directory.GetFiles(repo, "*.tmod", SearchOption.AllDirectories))
				.OrderByDescending(mod => mod.tModLoaderVersion)
				.Where(mod => StableTMLVersion >= mod.tModLoaderVersion.MajorMinor())
				.FirstOrDefault();

		private static readonly Regex PublishFolderNameRegex = new(@"[0-9]{4}[.][0-9]{1,2}", RegexOptions.Compiled);
		private static bool IsVersionedPublishFolder(string folderPath) => PublishFolderNameRegex.IsMatch(Path.GetFileName(folderPath));

		internal static void CleanupOldPublish(string repo) {

			var folders = new List<(Version version, string path)>();
			foreach (var modPath in Directory.GetFiles(repo, "*.tmod", SearchOption.AllDirectories)) {
				var dir = Path.GetDirectoryName(modPath);

				// delete old tmods which weren't in versioned publish folders
				if (!IsVersionedPublishFolder(dir))
					File.Delete(modPath);

				folders.Add((new Version(Path.GetFileName(dir)), dir));
			}

			var toDelete = folders.OrderByDescending(e => e.version).Skip(2).ToList();
			foreach (var (_, dir) in toDelete)
				Directory.Delete(dir, true);
		}

		internal static Version PreviousMonth(Version version) {
			return new Version(version.Minor > 1 ? version.Major : version.Major - 1, version.Minor > 1 ? version.Minor - 1 : 12);
		}

		private static Version _stableTMLVersion;
		public static Version StableTMLVersion => _stableTMLVersion ??= ComputeStableTMLVersion();

		private static Version ComputeStableTMLVersion() {
			if (BuildInfo.IsDev)
				throw new Exception("Cannot determine stable version from a dev build");

			int year = BuildInfo.tMLVersion.Major;
			int month = BuildInfo.tMLVersion.Minor;
			if (BuildInfo.IsPreview) {
				if (month == 1) { year--; month = 12; }
				else { month --; }
			}
			return new Version(year, month);
		}

		internal static void DeleteMod(LocalMod tmod) {
			if (TryReadManifest(tmod.modFile, out var info, out var workshopDir)) {
				// Is a mod on Steam Workshop
				var modManager = new WorkshopHelper.ModManager(new Steamworks.PublishedFileId_t(info.workshopEntryId));
				modManager.Uninstall(workshopDir);
			}
			else {
				// Is a Mod in Mods Folder
				File.Delete(tmod.modFile.path);
			}
		}

		// TODO, does this need to be merged with WorkshopSocialModule.TryGetInfoForMod? There's some extra checks there. Are they necessary?
		internal static bool TryReadManifest(TmodFile mod, out FoundWorkshopEntryInfo info, out string workshopDir) {
			info = null;

			workshopDir = Path.GetDirectoryName(mod.path);
			if (IsVersionedPublishFolder(workshopDir))
				workshopDir = Path.GetDirectoryName(workshopDir);

			if (!workshopDir.Contains(Path.Combine("steamapps", "workshop"))) // contains path separator, not the most reliable detection method
				return false;

			return AWorkshopEntry.TryReadingManifest(Path.Combine(workshopDir, "workshop.json"), out info);
		}
	}
}