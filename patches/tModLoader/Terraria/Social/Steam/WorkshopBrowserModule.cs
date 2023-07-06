using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI.DownloadManager;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.Social.Base;

namespace Terraria.Social.Steam;

internal class WorkshopBrowserModule : SocialBrowserModule
{
	public static WorkshopBrowserModule Instance = new WorkshopBrowserModule();

	private PublishedFileId_t GetId(ModPubId_t modId) => new PublishedFileId_t(ulong.Parse(modId.m_ModPubId));

	// For caching installed mods for performance and thread conflict management of Steam Queries /////////////////////////
	public WorkshopBrowserModule()
	{
		ModOrganizer.OnLocalModsChanged += OnLocalModsChanged;
	}

	public bool Initialize()
	{
		OnLocalModsChanged(null);
		return true;
	}

	private void OnLocalModsChanged(HashSet<string> modSlugs)
	{
		InstalledItems = ModOrganizer.FindWorkshopMods();
		CachedInstalledModDownloadItems = (this as SocialBrowserModule).DirectQueryInstalledMDItems();
	}

	public IReadOnlyList<LocalMod> GetInstalledMods()
	{
		if (InstalledItems == null)
			InstalledItems = ModOrganizer.FindWorkshopMods();

		return InstalledItems;
	}

	public ModDownloadItem[] CachedInstalledModDownloadItems { get; set; }

	// Cache to minimize heavy costs associated with scanning over 50+ mods installed. Test anytime after big optimization to see if can remove
	// last test Jun 23 2023 - Solxan
	public IReadOnlyList<LocalMod> InstalledItems { get; set; }

	// Managing Installs /////////////////////////

	public bool GetModIdFromLocalFiles(TmodFile modFile, out ModPubId_t modId)
	{
		bool success = WorkshopHelper.GetPublishIdLocal(modFile, out ulong publishId);

		modId = new ModPubId_t() { m_ModPubId = publishId.ToString() };
		return success;
	}

	public bool DoesItemNeedUpdate(ModPubId_t modId, LocalMod installed, System.Version webVersion)
	{
		if (installed.properties.version < webVersion)
			return true;

		if (SteamedWraps.DoesWorkshopItemNeedUpdate(GetId(modId)))
			return true;

		return false;
	}

	public bool DoesAppNeedRestartToReinstallItem(ModPubId_t modId) => SteamedWraps.IsWorkshopItemInstalled(GetId(modId));

	// Downloading Items /////////////////////////

	public void DownloadItem(ModDownloadItem item, IDownloadProgress uiProgress)
	{
		item.UpdateInstallState();

		var publishId = new PublishedFileId_t(ulong.Parse(item.PublishId.m_ModPubId));
		bool forceUpdate = item.NeedUpdate || !SteamedWraps.IsWorkshopItemInstalled(publishId);

		uiProgress?.DownloadStarted(item.DisplayName);
		Utils.LogAndConsoleInfoMessage(Language.GetTextValue("tModLoader.BeginDownload", item.DisplayName));
		SteamedWraps.Download(publishId, uiProgress, forceUpdate);
	}

	// More Info for Items /////////////////////////
	public string GetModWebPage(ModPubId_t modId) => $"https://steamcommunity.com/sharedfiles/filedetails/?id={modId.m_ModPubId}";

	// Query Items /////////////////////////

	//TODO: Needs a refactor at some point, not the cleanest, but survivable 
	//START SECTION
	public async IAsyncEnumerable<ModDownloadItem> QueryBrowser(QueryParameters queryParams, [EnumeratorCancellation] CancellationToken token = default)
	{
		// @TODO: "Solxan" was not used???
		InstalledItems = GetInstalledMods();

		// Each filter has independent code for simplicity and readability
		switch (queryParams.updateStatusFilter) {
			case UpdateFilter.All:
				await foreach (var item in WorkshopHelper.QueryHelper.QueryWorkshop(queryParams, token)) {
					item.UpdateInstallState();
					yield return item;
				}
				yield break;
			case UpdateFilter.Available:
				await foreach (var item in WorkshopHelper.QueryHelper.QueryWorkshop(queryParams, token)) {
					if (!CachedInstalledModDownloadItems.Contains(item)) {
						item.UpdateInstallState(); // @TODO: Needed?
						yield return item;
					}
				}
				yield break;
			case UpdateFilter.UpdateOnly:
				// Special code for checking all mods installed. Can't use 'Subscribed' API query because GoG
				// @TODO: Techinically DirectyQuery and derivates are blocking!
				// @TODO: "Solxan" this was written so it WILL run even in case of All or
				// Available filters, is it intended?
				// Should be documented in case this method has interesting side effects
				foreach (var item in (this as SocialBrowserModule).DirectQueryInstalledMDItems()) {
					// @TODO: This too is blocking
					item.UpdateInstallState();
					if (item.NeedUpdate) // Needs update beforehand
						yield return item;
				}
				yield break;
			case UpdateFilter.InstalledOnly:
				// Special code for checking all mods installed. Can't use 'Subscribed' API query because GoG
				foreach (var item in (this as SocialBrowserModule).DirectQueryInstalledMDItems()) {
					item.UpdateInstallState();
					yield return item;
				}
				yield break;
		}
	}
	//END SECTION

	public ModDownloadItem[] DirectQueryItems(QueryParameters queryParams)
	{
		if (queryParams.searchModIds == null)
			return null; // Should only be called if the above is filled in.

		return new WorkshopHelper.QueryHelper.AQueryInstance(queryParams).FastQueryItems();
	}
}

