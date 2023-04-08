using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI.DownloadManager;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Terraria.ModLoader.UI.ModBrowser;

public struct ModPubId_t
{
	public string m_ModPubId;
}

public struct ModDownloadItemInstallInfo
{
	//internal LocalMod Installed; // Shoudn't be internal
	public bool IsInstalled;
	// IsEnabled :( should pass LocalMod but visibility is a mess from there
	public bool NeedUpdate;
	public bool AppNeedRestartToReinstall;
}

public interface SocialBrowserModule
{
	/////// Management of Browser Items ///////////////////////////////////////////

#pragma warning disable CS8424 // I know [EnumeratorCancellation] has no effect, but it's placed here to remember to add it to async implementations
	public IAsyncEnumerable<ModDownloadItem> QueryBrowser(QueryParameters queryParams, [EnumeratorCancellation] CancellationToken token = default);
#pragma warning restore CS8424

	/////// Display of Browser Items ///////////////////////////////////////////

	public string GetModWebPage(ModPubId_t item);

	/////// Management of Local Install ///////////////////////////////////////////

	public ModDownloadItemInstallInfo GetInstallInfo(ModDownloadItem item);
	public bool GetModIdFromLocalFiles(TmodFile modFile, out ModPubId_t item);

	/////// Management of Downloads ///////////////////////////////////////////

	/// <summary>
	/// Downloads all UIModDownloadItems provided.
	/// </summary>
	internal Task SetupDownload(List<ModDownloadItem> items, UIWorkshopDownload uiProgress = null)
	{
		// Can't update enabled items due to in-use file access constraints
		var needFreeInUseMods = items.Any(item => item.Installed != null && item.Installed.Enabled);
		if (needFreeInUseMods)
			ModLoader.Unload();

		/*
		if (!Main.dedServ) {
			uiProgress = new UIWorkshopDownload(previousMenuId);
			Main.MenuUI.SetState(uiProgress);
		}
		*/

		return Task.Run(() => InnerDownload(uiProgress, items, needFreeInUseMods));
	}

	private void InnerDownload(UIWorkshopDownload uiProgress, List<ModDownloadItem> items, bool reloadWhenDone)
	{
		var changedModsSlugs = new HashSet<string>();

		foreach (var item in items) {
			DownloadItem(item, uiProgress);

			// Due to issues with Steam moving files from downloading folder to installed folder,
			// there can be some latency in detecting it's installed. - Solxan
			Thread.Sleep(1000);

			// Add installed info to the downloaded item
			changedModsSlugs.Add(item.ModName);
			//var localMod = GetInstalledItems().FirstOrDefault(m => m.Name == item.ModName);
			//FindDownloadItem(item.ModName).Installed = localMod;
		}

		ModOrganizer.LocalModsChanged(changedModsSlugs);
		//Interface.modBrowser.PopulateModBrowser(uiOnly: true);
		//Interface.modBrowser.UpdateNeeded = true;

		uiProgress?.Leave(refreshBrowser: true); // @TODO refreshBrowser is redundant!!!

		if (reloadWhenDone)
			ModLoader.Reload();
	}

	internal void DownloadItem(ModDownloadItem item, UIWorkshopDownload uiProgress);

	/////// Management of Dependencies ///////////////////////////////////////////

	public HashSet<ModDownloadItem> GetDependencies(HashSet<ModPubId_t> modIds);

	public void GetDependenciesRecursive(HashSet<ModPubId_t> modIds, ref HashSet<ModDownloadItem> set)
	{
		//TODO: What if the same mod is a dependency twice, but different versions?

		var toQuery = modIds;

		while (true) {
			var deps = GetDependencies(toQuery);
			deps.ExceptWith(set);
			if (deps.Count <= 0)
				break;
			set.UnionWith(deps);
			toQuery = deps.Select(d => d.PublishId).ToHashSet();
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
	public string[] searchModIds;
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
