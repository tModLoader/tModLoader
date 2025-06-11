using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.DownloadManager;
using Terraria.ModLoader.UI.ModBrowser;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Terraria.Social.Base;

public struct ModPubId_t
{
	public string m_ModPubId;
}

public class SocialBrowserException : Exception
{
	public SocialBrowserException(string message) : base(message)
	{
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
}

public enum QueryType
{
	SearchAll,
	SearchDirect,
	SearchUserPublishedOnly
}
