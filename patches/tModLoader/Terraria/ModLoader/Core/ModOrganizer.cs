using Microsoft.CodeAnalysis;
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

namespace Terraria.ModLoader.Core;

/// <summary>
/// Responsible for sorting, dependency verification and organizing which mods to load
/// </summary>
internal static class ModOrganizer
{

	// Used in Mod Browser for tracking changes
	internal delegate void LocalModsChangedDelegate(HashSet<string> modSlugs, bool isDeletion);
	internal static event LocalModsChangedDelegate OnLocalModsChanged;
	internal static event LocalModsChangedDelegate PostLocalModsChanged;
	internal static void LocalModsChanged(HashSet<string> modSlugs, bool isDeletion)
	{
		// On is intended to be used to update Caches of Installed Items. Such as Workshop LocalMod caches etc.
		OnLocalModsChanged?.Invoke(modSlugs, isDeletion);
		// Post is intended to be used to update anything that depends on caches of installed items. Such as UI in Mod Browser
		PostLocalModsChanged?.Invoke(modSlugs, isDeletion);
	}

	internal static string modPath = Path.Combine(Main.SavePath, "Mods");
	internal static string commandLineModPack;

	private static Dictionary<string, LocalMod> modsDirCache = new Dictionary<string, LocalMod>();
	private static HashSet<string> readFailures = new(); //TODO: Reflect these skipped Mods in the UI somehow.

	internal static string lastLaunchedModsFilePath = Path.Combine(Main.SavePath, "LastLaunchedMods.txt");
	internal static List<(string ModName, Version previousVersion)> modsThatUpdatedSinceLastLaunch = new List<(string ModName, Version previousVersion)>();

	internal static WorkshopHelper.UGCBased.Downloader WorkshopFileFinder = new WorkshopHelper.UGCBased.Downloader();

	private enum SearchFolders { }

	internal static string ModPackActive = null;

	/// <summary>Mods in workshop folders, not in dev folder or modpacks</summary>
	internal static IReadOnlyList<LocalMod> FindWorkshopMods() => SelectVersionsToLoad(FindAllMods().Where(m => m.location == ModLocation.Workshop), quiet: true).ToArray();

	/// <summary>Mods from any location, using the default internal priority logic</summary>
	internal static LocalMod[] FindMods(bool logDuplicates = false) => SelectVersionsToLoad(FindAllMods(), quiet: !logDuplicates).ToArray();

	internal static IEnumerable<LocalMod> RecheckVersionsToLoad() => SelectVersionsToLoad(AllFoundMods, quiet: true);

	internal static Dictionary<string, LocalMod> LoadableModVersions { get; private set; }
	internal static LocalMod[] AllFoundMods { get; private set; }

	internal static LocalMod[] FindAllMods()
	{
		Logging.tML.Info("Finding Mods...");

		Directory.CreateDirectory(ModLoader.ModPath);
		DeleteTemporaryFiles();

		var mods = new List<LocalMod>();

		// Active Modpack
		if (!string.IsNullOrEmpty(ModPackActive)) {
			if (Directory.Exists(ModPackActive)) {
				Logging.tML.Info($"Loading Mods from active modpack: {ModPackActive}");
				mods.AddRange(ReadModFiles(ModLocation.Modpack, Directory.GetFiles(ModPackActive, "*.tmod", SearchOption.AllDirectories)));
			}
			else {
				Logging.tML.Warn($"Active modpack missing, deactivating: {ModPackActive}");
				ModPackActive = null;
			}
		}

		// Local/development folder
		mods.AddRange(ReadModFiles(ModLocation.Local, Directory.GetFiles(modPath, "*.tmod", SearchOption.TopDirectoryOnly)));

		// Workshop folders
		WorkshopFileFinder.Refresh(new WorkshopIssueReporter());
		foreach (string repo in WorkshopFileFinder.ModPaths) {
			mods.AddRange(ReadModFiles(ModLocation.Workshop, Directory.GetFiles(repo, "*.tmod", SearchOption.AllDirectories)));
		}

		return AllFoundMods = mods.ToArray();
	}

	private static IEnumerable<LocalMod> SelectVersionsToLoad(IEnumerable<LocalMod> mods, bool quiet = false) => mods.GroupBy(m => m.Name).OrderBy(g => g.Key).Select(m => SelectVersionToLoad(m, quiet)).Where(m => m != null);

	private static LocalMod SelectVersionToLoad(IEnumerable<LocalMod> versions, bool quiet = false)
	{
		var list = versions.ToList();
		void FilterOut(Func<LocalMod, bool> condition, string reason)
		{
			// Hopefully temporary
			if (list.All(condition)) {
				if (!quiet)
					Logging.tML.Warn($"All remaining versions of {list.First().Name} would be ignored, continuing anyway so that at least one can be shown. Reason: {reason}");

				return;
			}

			for (int i = list.Count - 1; i >= 0; i--) {
				var m = list[i];
				if (condition(m)) {
					if (!quiet)
						Logging.tML.Debug($"Skipped {m.DetailedInfo}. Reason: {reason}.");

					list.RemoveAt(i);
				}
			}
		}

		void OrderByDescending<T>(Func<LocalMod, T> prop, string msg) where T : IComparable<T>
		{
			var top = list.Select(prop).Max(); // note Max returns null on empty
			FilterOut(m => prop(m).CompareTo(top) != 0, msg);
		}

		// FilterOut currently does not _enforce_ the condition, because we must currently leave at least one mod version. Thus the order of these calls matters :(
		FilterOut(ModNet.ServerRequiresDifferentVersion, "connecting to a server which requires a different version");
		FilterOut(m => SocialBrowserModule.GetBrowserVersionNumber(m.tModLoaderVersion) != SocialBrowserModule.GetBrowserVersionNumber(BuildInfo.tMLVersion), "mod is for a different Terraria version/LTS release stream");
		FilterOut(m => SocialBrowserModule.GetBrowserVersionNumber(m.tModLoaderVersion).Contains("Transitive"), "The tML version is transitional with no distribution or mod browser support");
		FilterOut(SkipModForPreviewNotPlayable, "preview early-access disabled");
		FilterOut(m => BuildInfo.tMLVersion < m.tModLoaderVersion.MajorMinor(), "mod is for a newer tML monthly release"); // condition ignored if it applies to all versions of the mod. Ordering logic below will choose the newest version of the mod. This may be misleading, showing the maximum supported tML version rather than the minimum.
		OrderByDescending(m => m.location == ModLocation.Modpack, "a frozen copy is present in the active modpack"); // the condition above means that we may ignore items in the local modpack if they are for a newer tML version, preferring a loadable version in the local or workshop directories
		OrderByDescending(m => m.Version, "a newer version exists");
		OrderByDescending(m => m.tModLoaderVersion, "a matching version for a newer tModLoader exists");
		FilterOut(m => m.location != ModLocation.Workshop && list.Any(m2 => m2.location == ModLocation.Workshop && m2.modFile.Hash == m.modFile.Hash), "an identical copy exists in the workshop folder");
		OrderByDescending(m => m.location == ModLocation.Local, "a local copy with the same version (but different hash) exists");
		OrderByDescending(m => Path.GetFileNameWithoutExtension(m.modFile.path) == m.Name, "this .tmod has been renamed");

		var selected = list.FirstOrDefault();

		// should never be called, but could help diagnose a logic error in the future. Better than calling SingleOrDefault and throwing.
		FilterOut(v => v != selected, "Logic Error, multiple versions remain. One was randomly selected");

		if (!quiet)
			Logging.tML.Debug($"Selected {selected.DetailedInfo}.");

		return selected;
	}

	private static IEnumerable<LocalMod> ReadModFiles(ModLocation location, IEnumerable<string> fileNames)
	{
		foreach (var fileName in fileNames)
			if (TryReadLocalMod(location, fileName, out var mod))
				yield return mod;
	}

	private static bool TryReadLocalMod(ModLocation location, string fileName, out LocalMod mod)
	{
		var lastModified = File.GetLastWriteTime(fileName);
		if (modsDirCache.TryGetValue(fileName, out mod) && mod.lastModified == lastModified)
			return true;

		try {
			var modFile = new TmodFile(fileName);
			using (modFile.Open()) {
				mod = new LocalMod(location, modFile) {
					lastModified = lastModified
				};
			}
		}
		catch (Exception e) {
			if (readFailures.Add(fileName))
				Logging.tML.Warn("Failed to read " + fileName, e);

			return false;
		}

		modsDirCache[fileName] = mod;
		return true;
	}

	internal static bool ApplyPreviewChecks(LocalMod mod)
	{
		return BuildInfo.IsPreview && mod.location == ModLocation.Workshop;
	}

	// Used to Warn Players that the mod was built on Stable or earlier, and may not work on Preview.
	internal static bool CheckStableBuildOnPreview(LocalMod mod)
	{
		return ApplyPreviewChecks(mod) && mod.properties.buildVersion.MajorMinor() <= BuildInfo.stableVersion.MajorMinor();
	}

	// Used to Hide Preview built versions of Mods from being displayed as the available mod in 1.4-Preview tModLoader.
	// Primarily intended to allow for publishing your mod ahead of a monthly breaking change build
	internal static bool SkipModForPreviewNotPlayable(LocalMod mod)
	{
		return ApplyPreviewChecks(mod) && !mod.properties.playableOnPreview && mod.properties.buildVersion.MajorMinor() > BuildInfo.stableVersion;
	}

	internal static bool IsModFromSteam(string modPath)
	{
		return modPath.Contains(Path.Combine("workshop"), StringComparison.InvariantCultureIgnoreCase);
	}

	internal static HashSet<string> IdentifyMissingWorkshopDependencies()
	{
		var mods = FindWorkshopMods();
		var installedSlugs = mods.Select(s => s.Name).ToArray();

		HashSet<string> missingModSlugs = new HashSet<string>();

		// This won't look recursively for missing deps. Because any recursive missing deps implies a missing dep elsewhere
		foreach (var mod in mods.Where(m => m.properties.modReferences.Length > 0)) {
			var missingDeps = mod.properties.modReferences.Select(dep => dep.mod).Where(slug => !installedSlugs.Contains(slug));

			missingModSlugs.UnionWith(missingDeps);
		}

		return missingModSlugs;
	}

	/// <summary>
	/// Returns changes based on last time <see cref="SaveLastLaunchedMods"/> was called. Can be null if no changes.
	/// </summary>
	internal static string DetectModChangesForInfoMessage(out IEnumerable<string> removedMods)
	{
		// Only display if enabled and file exists
		if (!ModLoader.showNewUpdatedModsInfo || !File.Exists(lastLaunchedModsFilePath)) {
			removedMods = [];
			return null;
		}

		// For convenience, convert to dict
		var currMods = FindWorkshopMods().ToDictionary(mod => mod.Name, mod => mod);

		Logging.tML.Info("3 most recently changed workshop mods: " + string.Join(", ", currMods.OrderByDescending(x => x.Value.lastModified).Take(3).Select(x => $"{x.Value.Name} v{x.Value.Version} {x.Value.lastModified:d}")));

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
			var newMods = new List<string>();
			var updatedMods = new List<string>();
			var messages = new StringBuilder();
			removedMods = lastMods.Keys.Where(name => !currMods.ContainsKey(name));
			foreach (var item in currMods) {
				string name = item.Key;
				var localMod = item.Value;
				Version version = localMod.Version;

				if (!lastMods.ContainsKey(name)) {
					newMods.Add(name);
					modsThatUpdatedSinceLastLaunch.Add((name, null));
				}
				else if (lastMods.TryGetValue(name, out var lastVersion) && lastVersion < version) {
					updatedMods.Add(name);
					modsThatUpdatedSinceLastLaunch.Add((name, lastVersion));
				}
			}

			//TODO: This code was added hastily in response to a popular mod being transferred ownership by reuploading it.
			// Revisit this code at a later date, as its not optimized for UX
			if (removedMods.Count() > 0) {
				messages.Append(Language.GetTextValue("tModLoader.ShowRemovedModsInfoMessageUpdatedMods"));
				foreach (var removedMod in removedMods) {
					string name = removedMod;
					messages.Append($"\n  {name} was removed.");
				}
			}

			if (newMods.Count > 0) {
				messages.Append(Language.GetTextValue("tModLoader.ShowNewUpdatedModsInfoMessageNewMods"));
				foreach (var newMod in newMods) {
					messages.Append($"\n  {newMod} ({currMods[newMod].DisplayName})");
				}
			}

			if (updatedMods.Count > 0) {
				messages.Append(Language.GetTextValue("tModLoader.ShowNewUpdatedModsInfoMessageUpdatedMods"));
				foreach (var updatedMod in updatedMods) {
					string name = updatedMod;
					string displayName = currMods[name].DisplayName;
					Version lastVersion = lastMods[name];
					Version currVersion = currMods[name].properties.version;
					messages.Append($"\n  {name} ({displayName}) v{lastVersion} -> v{currVersion}");
				}
			}

			// If any info is accumulated, return it
			return messages.Length > 0 ? messages.ToString() : null;
		}
		catch {
			removedMods = [];
			return null;
		}
	}

	/// <summary>
	/// Collects local mod status and saves it to a file.
	/// </summary>
	internal static void SaveLastLaunchedMods()
	{
		if (Main.dedServ) // Not relevant for the server yet, all features using this data are clientside
			return;

		if (!ModLoader.showNewUpdatedModsInfo) // Not needed if feature that uses the file is disabled
			return;

		var currMods = FindWorkshopMods();
		var fileText = new StringBuilder();
		foreach (var mod in currMods) {
			fileText.Append($"{mod.Name} {mod.Version}\n");
		}
		File.WriteAllText(lastLaunchedModsFilePath, fileText.ToString());
	}

	private static void DeleteTemporaryFiles()
	{
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

	private static IEnumerable<string> GetTemporaryFiles()
	{
		return Directory.GetFiles(modPath, $"*{DownloadFile.TEMP_EXTENSION}", SearchOption.TopDirectoryOnly)
			.Union(Directory.GetFiles(modPath, "temporaryDownload.tmod", SearchOption.TopDirectoryOnly)); // Old tML remnant
	}

	internal static bool LoadSide(ModSide side) => side != (Main.dedServ ? ModSide.Client : ModSide.Server);

	internal static List<LocalMod> SelectAndSortMods(IEnumerable<LocalMod> availableMods, CancellationToken token)
	{
		var missing = ModLoader.EnabledMods.Except(availableMods.Select(mod => mod.Name)).ToList();
		if (missing.Any()) {
			Logging.tML.Info("Missing previously enabled mods: " + string.Join(", ", missing));
			foreach (var name in missing)
				ModLoader.EnabledMods.Remove(name);

			SaveEnabledMods();
		}

		// Press shift while starting up tModLoader or while trapped in a reload cycle to skip loading all mods.
		if (Main.instance.IsActive && Main.oldKeyState.PressingShift() || ModLoader.skipLoad || token.IsCancellationRequested) {
			ModLoader.skipLoad = false;
			Interface.loadMods.SetLoadStage("tModLoader.CancellingLoading");
			return new();
		}

		CommandLineModPackOverride(availableMods);

		// Alternate fix for updating enabled mods
		//foreach (string fileName in Directory.GetFiles(modPath, "*.tmod.update", SearchOption.TopDirectoryOnly)) {
		//	File.Copy(fileName, Path.GetFileNameWithoutExtension(fileName), true);
		//	File.Delete(fileName);
		//}
		Interface.loadMods.SetLoadStage("tModLoader.MSFinding");

		foreach (var mod in GetModsToLoad(availableMods)) {
			EnableWithDeps(mod, availableMods);
		}
		SaveEnabledMods();

		var modsToLoad = GetModsToLoad(availableMods);
		try {
			EnsureRecentlyBuildModsAreLoading(modsToLoad);
			EnsureDependenciesExist(modsToLoad, false);
			EnsureTargetVersionsMet(modsToLoad);
			EnsureHashesAreValid(modsToLoad);
			return Sort(modsToLoad);
		}
		catch (ModSortingException e) {
			e.Data["mods"] = e.errored.Select(m => m.Name).ToArray();
			e.Data["hideStackTrace"] = true;
			throw;
		}
	}

	private static List<LocalMod> GetModsToLoad(IEnumerable<LocalMod> availableMods)
	{
		var modsToLoad = availableMods.Where(mod => mod.Enabled && LoadSide(mod.properties.side)).ToList();
		VerifyNames(modsToLoad);
		return modsToLoad;
	}

	private static void CommandLineModPackOverride(IEnumerable<LocalMod> mods)
	{
		if (string.IsNullOrWhiteSpace(commandLineModPack))
			return;

		if (!commandLineModPack.EndsWith(".json"))
			commandLineModPack += ".json";

		string filePath = Path.Combine(UIModPacks.ModPacksDirectory, commandLineModPack);

		try {
			Directory.CreateDirectory(UIModPacks.ModPacksDirectory);
			Logging.ServerConsoleLine(Language.GetTextValue("tModLoader.ModPackLoadingSpecifiedModPack", commandLineModPack));
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

	//TODO: This duplicates some of the logic in UIModItem
	internal static void EnableWithDeps(LocalMod mod, IEnumerable<LocalMod> availableMods)
	{
		mod.Enabled = true;

		foreach (var depName in mod.properties.RefNames(includeWeak: false)) {
			if (availableMods.SingleOrDefault(m => m.Name == depName) is LocalMod { Enabled: false } dep)
				EnableWithDeps(dep, availableMods);
		}
	}

	private static void VerifyNames(List<LocalMod> mods)
	{
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

	private static void EnsureRecentlyBuildModsAreLoading(List<LocalMod> mods)
	{
		// If a mod maker attempts to debug a mod with a lower version, it won't be selected so we catch that here. We throw an error because this is definitely not desired.
		foreach (var mod in mods) {
			var localMod = AllFoundMods.SingleOrDefault(x => x.Name == mod.Name && x.location == ModLocation.Local && Path.GetFileNameWithoutExtension(x.modFile.path) == mod.Name);

			// If Local mod is newer than selected Workshop/Modpack mod...
			if (localMod == null || localMod == mod || localMod.lastModified.CompareTo(mod.lastModified) <= 0) {
				continue;
			}

			// and is newer than directly before game launch and last time a mod was synced, it is assumed to be a mod built by this modder.
			if (localMod.lastModified.CompareTo(ModCompile.recentlyBuiltModCheckTimeCutoff) > 0) {
				var e = new Exception(Language.GetTextValue("tModLoader.LoadErrorRecentlyBuiltLocalModWithLowerVersion" + mod.location.ToString(), localMod.Name, localMod.Version, mod.Version));
				e.Data["mod"] = mod.Name;
				e.Data["hideStackTrace"] = true;
				throw e;
			}

			continue;
		}
	}

	internal static void EnsureDependenciesExist(ICollection<LocalMod> mods, bool includeWeak)
	{
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

	internal static void EnsureTargetVersionsMet(ICollection<LocalMod> mods)
	{
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

	internal static void EnsureHashesAreValid(ICollection<LocalMod> mods)
	{
		var errored = new HashSet<LocalMod>();
		var errorLog = new StringBuilder();
		foreach (var mod in mods) {
			if (!mod.modFile.VerifyHash()) {
				errored.Add(mod);
				errorLog.AppendLine(Language.GetTextValue("tModLoader.LoadErrorHashMismatchCorruptedWithModName", mod));
			}
		}
		if (errored.Count > 0)
			throw new ModSortingException(errored, errorLog.ToString());
	}

	internal static void EnsureSyncedDependencyStability(TopoSort<LocalMod> synced, TopoSort<LocalMod> full)
	{
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

	private static TopoSort<LocalMod> BuildSort(ICollection<LocalMod> mods)
	{
		var nameMap = mods.ToDictionary(mod => mod.Name);
		return new TopoSort<LocalMod>(mods,
			mod => mod.properties.sortAfter.Where(nameMap.ContainsKey).Select(name => nameMap[name]),
			mod => mod.properties.sortBefore.Where(nameMap.ContainsKey).Select(name => nameMap[name]));
	}

	internal static List<LocalMod> Sort(ICollection<LocalMod> mods)
	{
		var preSorted = mods.OrderBy(mod => mod.Name, StringComparer.InvariantCulture).ToList();
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

	internal static void SaveEnabledMods()
	{
		Directory.CreateDirectory(ModLoader.ModPath);
		string json = JsonConvert.SerializeObject(ModLoader.EnabledMods, Formatting.Indented);
		var path = Path.Combine(modPath, "enabled.json");
		File.WriteAllText(path, json);
	}

	internal static HashSet<string> LoadEnabledMods()
	{
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

	private static readonly Regex PublishFolderMetadata = new Regex(@"[/|\\]([0-9]{4}[.][0-9]{1,2})[/|\\]");

	internal static string GetActiveTmodInRepo(string repo)
	{
		var information = AnalyzeWorkshopTmods(repo).Where(t =>
			// Ignore Transitive versions of tModLoader, such as 1.4.4-transitive. See 'GetBrowserVersionNumber' for why
			!SocialBrowserModule.GetBrowserVersionNumber(t.tModVersion).Contains("Transitive")
		);
		if (information == null || information.Count() == 0) {
			Logging.tML.Warn($"Unexpectedly missing .tMods in Workshop Folder {repo}");
			return null;
		}

		var recommendedTmod = information.Where(t => t.tModVersion <= BuildInfo.tMLVersion).OrderByDescending(t => t.tModVersion).FirstOrDefault();
		if (recommendedTmod == default) {
			Logging.tML.Warn($"No .tMods found for this version in Workshop Folder {repo}. Defaulting to show newest");
			return information.OrderByDescending(t => t.tModVersion).First().file;
		}

		return recommendedTmod.file;
	}

	/// <summary>
	/// Must Be called AFTER the new files are added to the publishing repo.
	/// Assumes one .tmod per YYYY.XX folder in the publishing repo
	/// </summary>
	/// <param name="repo"></param>
	internal static void CleanupOldPublish(string repo)
	{
		if (BuildInfo.IsPreview)
			RemoveSkippablePreview(repo);

		string[] tmods = Directory.GetFiles(repo, "*.tmod", SearchOption.AllDirectories);
		if (tmods.Length <= 3)
			return;

		// Solxan: We want to keep 4 copies of the mod. A Preview version, a Stable Version, and a Legacy version in case
		// we need to rollback to the last stable due to a significant bug.
		// We also keep a 1.4.3 version from version 2022.9 prior

		var information = AnalyzeWorkshopTmods(repo);
		if (information == null || information.Count() <= 3)
			return;

		foreach (var requirement in SocialBrowserModule.keepRequirements) {
			var mods = GetOrderedTmodWorkshopInfoForVersion(information, requirement.browserVersion).Skip(requirement.keepCount);

			foreach (var item in mods) {
				if (item.isInFolder)
					Directory.Delete(Path.GetDirectoryName(item.file), recursive: true);
				else
					File.Delete(item.file);
			}
		}
	}

	internal static IOrderedEnumerable<(string file, Version tModVersion, bool isInFolder)>
			GetOrderedTmodWorkshopInfoForVersion(List<(string file, Version tModVersion, bool isInFolder)> information, string tmlVersion)
	{
		return information.Where(t => SocialBrowserModule.GetBrowserVersionNumber(t.tModVersion) == tmlVersion).OrderByDescending(t => t.tModVersion);
	}

	internal static List<(string file, Version tModVersion, bool isInFolder)> AnalyzeWorkshopTmods(string repo)
	{
		string[] tmods = Directory.GetFiles(repo, "*.tmod", SearchOption.AllDirectories);

		// Get the list of all tMod files on Workshop
		List<(string file, Version tModVersion, bool isInFolder)> information = new();
		foreach (var filename in tmods) {
			var match = PublishFolderMetadata.Match(filename);
			if (match.Success) {
				information.Add((filename, new Version(match.Groups[1].Value), isInFolder: true));
			}
			else {
				// Version 0.12 was the pre-Alpha 1.4 builds where .tMod was placed directly in the Workshop.
				// Was prior to the preview system introduced, but also just above the 0.11.9.X for 1.3 tML
				information.Add((filename, new Version(0, 12), isInFolder: false));
			}
		}
		return information;
	}

	// Remove skippable preview builds from extended version (ie 2022.5 if stable is 2022.4 & Preview is 2022.6
	private static void RemoveSkippablePreview(string repo)
	{
		string[] tmods = Directory.GetFiles(repo, "*.tmod", SearchOption.AllDirectories);

		for (int i = 0; i < tmods.Length; i++) {
			var filename = tmods[i];

			var match = PublishFolderMetadata.Match(filename);
			if (!match.Success)
				continue;

			var checkVersion = new Version(match.Groups[1].Value);

			if (checkVersion > BuildInfo.stableVersion && checkVersion < BuildInfo.tMLVersion.MajorMinor())
				Directory.Delete(Path.GetDirectoryName(filename), true);
		}
	}

	internal static bool CanDeleteFrom(ModLocation location)
	{
		return location == ModLocation.Local || location == ModLocation.Workshop;
	}

	internal static void DeleteMod(LocalMod tmod)
	{
		if (tmod.location == ModLocation.Local) {
			File.Delete(tmod.modFile.path);
		}
		else if (tmod.location == ModLocation.Workshop) {
			string parentDir = GetParentDir(tmod.modFile.path);
			if (TryReadManifest(parentDir, out var info)) {
				// Is a mod on Steam Workshop
				SteamedWraps.UninstallWorkshopItem(new Steamworks.PublishedFileId_t(info.workshopEntryId), parentDir);
			}
			else {
				Logging.tML.Error($"Failed to read manifest for workshop mod {tmod.Name} @ {parentDir}");
				return;
			}
		}
		else {
			throw new InvalidOperationException("Cannot delete mod from " + tmod.location);
		}

		// TODO, notify the mod workshop module directly instead. Only for workshop mods.
		LocalModsChanged(new HashSet<string> { tmod.Name }, isDeletion: true);
	}

	internal static bool TryReadManifest(string parentDir, out FoundWorkshopEntryInfo info)
	{
		info = null;
		if (!IsModFromSteam(parentDir))
			return false;

		string manifest = parentDir + Path.DirectorySeparatorChar + "workshop.json";

		return AWorkshopEntry.TryReadingManifest(manifest, out info);
	}

	internal static string GetParentDir(string tmodPath)
	{
		string parentDir = Directory.GetParent(tmodPath).ToString();
		if (!IsModFromSteam(parentDir))
			return parentDir;

		var match = PublishFolderMetadata.Match(parentDir + Path.DirectorySeparatorChar);
		if (match.Success)
			parentDir = Directory.GetParent(parentDir).ToString();

		return parentDir;
	}

	// NOTE: This does not search the workshop, only checks locally available mods
	internal static string GetDisplayNameCleanFromLocalModsOrDefaultToModName(string modname)
	{
		var localMods = AllFoundMods.Where(m => string.Equals(modname, m.Name));
		if (!localMods.Any())
			return modname;

		return localMods.FirstOrDefault().DisplayNameClean;
	}
}