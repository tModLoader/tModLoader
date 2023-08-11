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

	public ModDownloadItem[] DirectQueryItems(QueryParameters queryParams);

	/////// Display of Browser Items ///////////////////////////////////////////

	public string GetModWebPage(ModPubId_t item);

	/////// Management of Local Install ///////////////////////////////////////////
	public bool GetModIdFromLocalFiles(TmodFile modFile, out ModPubId_t item);

	// Needed for ensuring that the 'Update All' button works correctly. Without caching the mod browser locks out on the update all button
	internal ModDownloadItem[] CachedInstalledModDownloadItems { get; set; }

	public ModDownloadItem[] DirectQueryInstalledMDItems(QueryParameters qParams = new QueryParameters()) {
		var mods = GetInstalledMods();
		var listIds = new List<ModPubId_t>();

		foreach (var mod in mods) {
			if (GetModIdFromLocalFiles(mod.modFile, out var id))
				listIds.Add(id);
		}

		qParams.searchModIds = listIds.ToArray();

		return DirectQueryItems(qParams);
	}

	public ModDownloadItem[] GetInstalledModDownloadItems()
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
		return GetInstalledMods().Where(t => t.Name == slug).FirstOrDefault();
	}

	/////// Management of Downloads ///////////////////////////////////////////

	/// <summary>
	/// Downloads all UIModDownloadItems provided.
	/// </summary>
	internal Task SetupDownload(List<ModDownloadItem> items, int previousMenuId)
	{
		// Can't update enabled items due to in-use file access constraints
		// Mod unloading needs to occur off the main thread o avoid UI impacts/soft-freeze
		var needFreeInUseMods = items.Where(item => { item.UpdateInstallState(); return item.IsEnabled; });

		var needsEnableFormerInUseMods = needFreeInUseMods.Select(item => item.Installed.Name);
		Task downloadRunner = DownloadRunner(items, previousMenuId, needsEnableFormerInUseMods);

		if (needFreeInUseMods.Count() > 0) {
			foreach (var item in needFreeInUseMods) {
				item.Installed.Enabled = false;
				// We must clear the Installed reference in ModDownloadItem to facilitate downloading, in addition to disabling - Solxan
				item.Installed = null;
			}

			ModLoader.ModLoader.OnSuccessfulLoad += delegate { downloadRunner.Start(); };
			return new Task(() => { Main.menuMode = Interface.loadModsID; } );
		}

		return downloadRunner;
	}

	private Task DownloadRunner(List<ModDownloadItem> items, int previousMenuId, IEnumerable<string> needsEnableFormerInUseMods)
	{
		IDownloadProgress progress = null;

		if (!Main.dedServ) {
			// Create UIWorkshopDownload
			var ui = new UIWorkshopDownload(previousMenuId);
			Main.menuMode = 888;
			Main.MenuUI.SetState(ui);
			progress = ui;
		}

		return Task.Run(() => InnerDownload(progress, items, needsEnableFormerInUseMods));
	}

	private void InnerDownload(IDownloadProgress uiProgress, List<ModDownloadItem> items, IEnumerable<string> needsEnableFormerInUseMods)
	{
		var changedModsSlugs = new HashSet<string>();

		foreach (var item in items) {
			DownloadItem(item, uiProgress);

			// Add installed info to the downloaded item
			changedModsSlugs.Add(item.ModName);
		}

		ModOrganizer.LocalModsChanged(changedModsSlugs);

		uiProgress?.DownloadCompleted();

		// Set all mods that were previously enabled to be enabled, but let the player choose when to reload them.
		if (needsEnableFormerInUseMods.Count() > 0) {
			var installedMods = GetInstalledMods();
			foreach (var slug in needsEnableFormerInUseMods) {
				var mod = installedMods.Where(item => item.Name == slug).FirstOrDefault();
				if (mod != null)
					mod.Enabled = true;
			}
		}	
	}

	internal void DownloadItem(ModDownloadItem item, IDownloadProgress uiProgress);

	/////// Management of Dependencies ///////////////////////////////////////////

	public void GetDependenciesRecursive(ref HashSet<ModDownloadItem> set)
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
			iterationSet = DirectQueryItems(new QueryParameters() { searchModIds = iterationList.ToArray() }).ToHashSet();

			// Add the net-new publish IDs & ModDownLoadItems to the full list
			fullList.UnionWith(iterationList);
			set.UnionWith(iterationSet);
		}
	}

	public static string GetBrowserVersionNumber(Version tmlVersion)
	{
		if (tmlVersion < new Version(0, 12)) // Versions 0 to 0.11.8.9
			return "1.3";

		if (tmlVersion < new Version(2022, 10)) // Versions 0.12 to 2022.9
			return "1.4";

		return "1.4.4";
	}
}

public struct QueryParameters
{
	public string[] searchTags;
	public ModPubId_t[] searchModIds;
	public string[] searchModSlugs;
	public string searchGeneric;
	public string searchAuthor;

	public ModBrowserSortMode sortingParamater;
	public UpdateFilter updateStatusFilter;
	public ModSideFilter modSideFilter;

	public QueryType queryType;
}

public enum QueryType
{
	SearchAll,
	SearchDirect
}
