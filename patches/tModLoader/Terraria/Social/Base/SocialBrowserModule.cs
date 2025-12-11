using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI.DownloadManager;
using Terraria.ModLoader.UI.ModBrowser;
using System.Threading;
using System.Runtime.CompilerServices;
using Terraria.Localization;
using Terraria.Social.Steam;

namespace Terraria.Social.Base;

public struct ModPubId_t
{
	public string m_ModPubId;

	public override string ToString() => m_ModPubId;
}

public class SocialBrowserException : Exception
{
	public SocialBrowserException(string message) : base(message)
	{
	}
}

public class BannedModException : SocialBrowserException
{
	internal string displayName;
	internal string modPubId;

	public BannedModException(string message, string displayName, string modPubId) : base(message)
	{
		this.displayName = displayName;
		this.modPubId = modPubId;
	}
}

public interface SocialBrowserModule
{
	public bool Initialize();

	/////// Management of Browser Items ///////////////////////////////////////////

#pragma warning disable CS8424 // I know [EnumeratorCancellation] has no effect, but it's placed here to remember to add it to async implementations
	public IAsyncEnumerable<ModDownloadItem> QueryBrowser(QueryParameters queryParams, [EnumeratorCancellation] CancellationToken token = default);
#pragma warning restore CS8424

	public List<ModDownloadItem> DirectQueryItems(QueryParameters queryParams, out List<string> missingMods);

	public DeveloperMetadata GetDeveloperMetadataFromModBrowser(ModPubId_t modId);

	/////// Display of Browser Items ///////////////////////////////////////////

	public string GetModWebPage(ModPubId_t item);

	/////// Management of Local Install ///////////////////////////////////////////
	public bool GetModIdFromLocalFiles(TmodFile modFile, out ModPubId_t item);

	// Needed for ensuring that the 'Update All' button works correctly. Without caching the mod browser locks out on the update all button
	internal List<ModDownloadItem> CachedInstalledModDownloadItems { get; set; }

	public List<ModDownloadItem> DirectQueryInstalledMDItems(QueryParameters qParams = new QueryParameters()) {
		var mods = GetInstalledMods();
		var listIds = new List<ModPubId_t>();

		foreach (var mod in mods) {
			if (GetModIdFromLocalFiles(mod.modFile, out var id))
				listIds.Add(id);
		}

		qParams.searchModIds = listIds.ToArray();

		return DirectQueryItems(qParams, out _);
	}

	public List<ModDownloadItem> GetInstalledModDownloadItems()
	{
		if (CachedInstalledModDownloadItems == null) {
			CachedInstalledModDownloadItems = DirectQueryInstalledMDItems();
		}

		return CachedInstalledModDownloadItems;
	}

	/////// Specialty Internal LocalMod related Methods ///////////////////////////////////////////
	public bool DoesAppNeedRestartToReinstallItem(ModPubId_t modId);

	internal bool DoesItemNeedUpdate(ModPubId_t modId, LocalMod installed, Version webVersion);

	internal IReadOnlyList<LocalMod> GetInstalledMods();

	internal LocalMod IsItemInstalled(string slug)
	{
		return GetInstalledMods().Where(t => string.Equals(t.Name, slug, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
	}

	/////// Management of Downloads ///////////////////////////////////////////

	internal void DownloadItem(ModDownloadItem item, IDownloadProgress uiProgress);

	/////// Management of Dependencies ///////////////////////////////////////////

	public void GetDependenciesRecursive(HashSet<ModDownloadItem> set)
	{
		//NOTE: What if the same mod is a dependency twice, but different versions?
		// In The Steam Workshop implementation, this is not tracked. Dependencies are by slug/ID only
		// If we change backends, and we re-add the capability to have concurrent versions in a folder, may be significant work involved
		var fullList = set.Select(x => x.PublishId).ToHashSet();
		var iterationList = new HashSet<ModPubId_t>();

		var iterationSet = set;

		while (true) {
			// Get the list of all Publish IDs labelled as dependencies
			foreach (var item in iterationSet) {
				iterationList.UnionWith(item.ModReferenceByModId);
			}

			// Remove Publish IDs already captured
			iterationList.ExceptWith(fullList);

			// If No New Publish IDs, then we have all the download Items already. Let's end this loop
			if (iterationList.Count <= 0)
				return;

			// Get the ModDownloadItems for the new IDs
			iterationSet = DirectQueryItems(new QueryParameters() { searchModIds = iterationList.ToArray() }, out var notFoundMods).ToHashSet();

			if (notFoundMods.Any())
				notFoundMods = notFoundMods; //TODO: Do we care here if a dependency isn't found?

			// Add the net-new publish IDs & ModDownLoadItems to the full list
			fullList.UnionWith(iterationList);
			set.UnionWith(iterationSet);
		}
	}

	public static string GetBrowserVersionNumber(Version tmlVersion)
	{
		if (tmlVersion < new Version(0, 12)) // Versions 0 to 0.11.8.9
			return "1.3"; // Long Term Service Version 1.3

		if (tmlVersion < new Version(2022, 10)) // Versions 0.12 to 2022.9
			return "1.4.3"; // Long Term Service version 1.4.3

		// We treat tModLoader versions between 2022.10.0.0 and 2023.3.85.0 as 'dead' versions.
		// Any mods built against these are not expected to actually work with tModLoader, and should be excluded in any ModBrowser or Mods Menu usage
		// The core reasonsing is due to systemic changes that broke nearly all mods during the 1.4.4 port (Localization rework)
		// It is recommended, given the timing of it, to ignore all tMods in publish folder with this.
		// NOTE: This does cause this tag to be added on Steam in the 'unsorted tags' category, for better or worse - Solxan
		if (tmlVersion < new Version(2023, 3, 85)) // Introduction of 1.4.4 tag and end of major 1.4.4 breaking changes
			return "1.4.4-Transitive";

		return "1.4.4"; // Long Term Service Version 1.4.4 (Current)
	}

	public static (string browserVersion, int keepCount)[] keepRequirements =
			{ ("1.4.3", 1), ("1.4.4", 3), ("1.3", 1), ("1.4.4-Transitive", 0) };

	internal static List<(WorkshopTagOption tag, bool setState, bool degraded)> GetModLocalizationProgress(TmodFile tModFile, List<WorkshopTagOption> existingActiveTagsList)
	{
		var localizationCounts = ModLoader.LocalizationLoader.GetLocalizationCounts(tModFile);
		int countMaxEntries = localizationCounts.DefaultIfEmpty().Max(x => x.Value);

		ModLoader.Logging.tML.Info($"Determining localization progress for {tModFile.Name}:");

		List<(WorkshopTagOption tag, bool setState, bool degraded)> autoLangTags = new List<(WorkshopTagOption tag, bool setState, bool degraded)>();

		foreach (var tag in SteamedWraps.ModTags) {
			if (tag.NameKey.StartsWith("tModLoader.TagsLanguage_")) {
				// I couldn't see any other way to convert this.
				var culture = tag.NameKey.Split('_')[1] switch {
					"English" => GameCulture.FromName("en-US"),
					"Spanish" => GameCulture.FromName("es-ES"),
					"French" => GameCulture.FromName("fr-FR"),
					"Italian" => GameCulture.FromName("it-IT"),
					"Russian" => GameCulture.FromName("ru-RU"),
					"Chinese" => GameCulture.FromName("zh-Hans"),
					"Portuguese" => GameCulture.FromName("pt-BR"),
					"German" => GameCulture.FromName("de-DE"),
					"Polish" => GameCulture.FromName("pl-PL"),
					_ => throw new NotImplementedException(),
				};

				int countOtherEntries;
				localizationCounts.TryGetValue(culture, out countOtherEntries);
				float localizationProgress = (float)countOtherEntries / countMaxEntries;

				ModLoader.Logging.tML.Info($"{culture.Name}, {countOtherEntries}/{countMaxEntries}, {localizationProgress:P0}, missing {countMaxEntries - countOtherEntries}");

				bool languageMostlyLocalized = localizationProgress > 0.75f; // 75% Threshold to be localized.
				bool languagePreviouslyLocalizedAndStillEnough = existingActiveTagsList.Contains(tag) && localizationProgress > 0.5f; // If mod previously tagged as localized, persist selection as long as above 50%

				// Override existing selection. Existing selection will persist if still above 50% to accommodate temporarily falling below threshold.
				autoLangTags.Add((tag, languageMostlyLocalized || languagePreviouslyLocalizedAndStillEnough, !languageMostlyLocalized));
			}
		}

		return autoLangTags;
	}
}

public struct QueryParameters
{
	public string[] searchTags;
	public ModPubId_t[] searchModIds;
	public string[] searchModSlugs;
	public string searchGeneric;
	public string searchAuthor;
	public uint days;

	public ModBrowserSortMode sortingParamater;
	public UpdateFilter updateStatusFilter;
	public ModSideFilter modSideFilter;

	public QueryType queryType;
	public bool returnDevMetadata;
}

public enum QueryType
{
	SearchAll,
	SearchDirect,
	SearchUserPublishedOnly
}
