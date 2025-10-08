using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Terraria.Localization;
using Terraria.ModLoader;
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
		OnLocalModsChanged(null, false);
		return true;
	}

	private void OnLocalModsChanged(HashSet<string> modSlugs, bool isDeletion)
	{
		InstalledItems = ModOrganizer.FindWorkshopMods();

		if (SteamedWraps.SteamAvailable)
			CachedInstalledModDownloadItems = (this as SocialBrowserModule).DirectQueryInstalledMDItems();

		if (!isDeletion)
			return;

		foreach (var item in modSlugs) {
			intermediateInstallStateMods.Add(item);
		}
	}

	public IReadOnlyList<LocalMod> GetInstalledMods()
	{
		if (InstalledItems == null)
			InstalledItems = ModOrganizer.FindWorkshopMods();

		return InstalledItems;
	}

	public List<ModDownloadItem> CachedInstalledModDownloadItems { get; set; }
	private HashSet<string> intermediateInstallStateMods = new HashSet<string>();

	// Cache to minimize heavy costs associated with scanning over 50+ mods installed. Test anytime after big optimization to see if can remove
	// last test Jun 23 2023 - Solxan
	private IReadOnlyList<LocalMod> InstalledItems { get; set; }

	// Managing Installs /////////////////////////

	public bool GetModIdFromLocalFiles(TmodFile modFile, out ModPubId_t modId)
	{
		bool success = WorkshopHelper.GetPublishIdLocal(modFile, out ulong publishId);

		modId = new ModPubId_t() { m_ModPubId = publishId.ToString() };
		return success;
	}

	public bool DoesItemNeedUpdate(ModPubId_t modId, LocalMod installed, System.Version webVersion)
	{
		if (installed.Version < webVersion)
			return true;

		if (SteamedWraps.SteamAvailable && SteamedWraps.DoesWorkshopItemNeedUpdate(GetId(modId)))
			return true;

		return false;
	}

	// Assumes SteamAvailable
	public bool DoesAppNeedRestartToReinstallItem(ModPubId_t modId) => SteamedWraps.IsWorkshopItemInstalled(GetId(modId));

	// Downloading Items /////////////////////////

	// assumes SteamAvailable
	public void DownloadItem(ModDownloadItem item, IDownloadProgress uiProgress)
	{
		item.UpdateInstallState();
		if (item.Banned)
			throw new BannedModException($"Attempted to Download a Banned Mod {item.DisplayName} with ID {item.PublishId}. Aborting...", item.DisplayName, item.PublishId.ToString());

		var publishId = new PublishedFileId_t(ulong.Parse(item.PublishId.m_ModPubId));
		bool forceUpdate = item.NeedUpdate || !SteamedWraps.IsWorkshopItemInstalled(publishId);

		uiProgress?.DownloadStarted(item.DisplayName);
		Utils.LogAndConsoleInfoMessage(Language.GetTextValue("tModLoader.BeginDownload", item.DisplayName));
		new SteamedWraps.ModDownloadInstance().Download(publishId, uiProgress, forceUpdate);
	}

	// More Info for Items /////////////////////////
	public string GetModWebPage(ModPubId_t modId) => $"https://steamcommunity.com/sharedfiles/filedetails/?id={modId.m_ModPubId}";

	// Query Items /////////////////////////

	/// <summary>
	/// Assumes Intialize has been run prior to use.
	/// As of Sept 21, 2025, called exclusively by UI Mod Browser
	/// </summary>
	public async IAsyncEnumerable<ModDownloadItem> QueryBrowser(QueryParameters queryParams, [EnumeratorCancellation] CancellationToken token = default)
	{
		if (!SteamedWraps.SteamAvailable)
			yield break;

		// Special Mod Pack Filter. Needs rework.
		if (queryParams.searchModIds != null && queryParams.searchModIds.Any()) {
			foreach (var item in DirectQueryItems(queryParams, out _))
				yield return item;
			yield break;
		}

		// Each filter has independent code for simplicity and readability
		switch (queryParams.updateStatusFilter) {
			case UpdateFilter.All:
				await foreach (var item in WorkshopHelper.QueryHelper.QueryWorkshop(queryParams, token)) {
					if (CachedInstalledModDownloadItems.Contains(item) || intermediateInstallStateMods.Contains(item.ModName))
						item.UpdateInstallState();
					yield return item;
				}
				yield break;
			case UpdateFilter.Available:
				await foreach (var item in WorkshopHelper.QueryHelper.QueryWorkshop(queryParams, token)) {
					if (!CachedInstalledModDownloadItems.Contains(item) && !intermediateInstallStateMods.Contains(item.ModName)) {
						yield return item;
					}
				}
				yield break;
			case UpdateFilter.UpdateOnly:
				foreach (var item in CachedInstalledModDownloadItems.Where(item => item.NeedUpdate)) {
					yield return item;
				}
				yield break;
			case UpdateFilter.InstalledOnly:
				foreach (var item in CachedInstalledModDownloadItems) {
					yield return item;
				}
				yield break;
		}
	}

	public List<ModDownloadItem> DirectQueryItems(QueryParameters queryParams, out List<string> missingMods)
	{
		if (queryParams.searchModIds == null || !SteamedWraps.SteamAvailable)
			throw new Exception("Unexpected Call of DirectQueryItems while either Steam is not initialized or query parameters.searchModIds is null"); // Should only be called if the above is filled in & Steam is Available.

		return new WorkshopHelper.QueryHelper.AQueryInstance(queryParams).QueryItemsSynchronously(out missingMods);
	}

	/// <summary>
	/// This uses the provided modId and retrieves the list of approved mod hashes from ModBrowser.
	/// Uses Steam Web API as a fall back when Steam API isn't available (such as headless environments)
	/// </summary>
	public DeveloperMetadata GetDeveloperMetadataFromModBrowser(ModPubId_t modId)
	{
		// Mod Doesn't Exist, return empty hashes
		if (string.IsNullOrEmpty(modId.m_ModPubId) || string.Equals(modId.m_ModPubId, "0"))
			return new();

		if (!SteamedWraps.SteamAvailable) {
			// If Steam Server and Steam Client are both not available, retrieve it via WebAPI
			var itemDetails = SteamWebWrapper.GetItemMetadata(modId.m_ModPubId);

			return DeveloperMetadata.Deserialize(itemDetails.Metadata);
		}

		// Mod should Exist, check Mod Browser
		var items = DirectQueryItems(new QueryParameters() { searchModIds = [modId], queryType = QueryType.SearchDirect, returnDevMetadata = true }, out _);

		return items.FirstOrDefault()?.DevMetadata ?? new();
	}
}

