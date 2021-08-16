using Steamworks;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.DownloadManager;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.Social.Base;

namespace Terraria.Social.Steam
{
	public partial class WorkshopHelper
	{
		internal static string[] MetadataKeys = new string[7] { "name", "author", "modside", "homepage", "modloaderversion", "version", "modreferences" };

		public struct ItemInstallInfo
		{
			public string installPath;
			public uint lastUpdatedTime;
		}

		public class ModPublisherInstance : UGCBased.APublisherInstance
		{
			protected override string GetHeaderText() => ModWorkshopEntry.GetHeaderTextFor(_publishedFileID.m_PublishedFileId, _entryData.Tags, _publicity, _entryData.PreviewImagePath);

			protected override void PrepareContentForUpdate() { }
		}

		internal static void OnGameExitCleanup() {
			if (ModManager.SteamUser)
				return;

			GameServer.Shutdown();
		}

		internal static void ForceCallbacks() {
			Thread.Sleep(5);

			if (ModManager.SteamUser)
				SteamAPI.RunCallbacks();
			else
				GameServer.RunCallbacks();
		}

		// Used to get the right token for fetching/setting localized descriptions from/to Steam Workshop
		internal static string GetCurrentSteamLangKey() {
			switch (Localization.LanguageManager.Instance.ActiveCulture.LegacyId) {
				default:
					return "english";
				case 2:
					return "german";
				case 3:
					return "italian";
				case 4:
					return "french";
				case 5:
					return "spanish";
				case 6:
					return "russian";
				case 7:
					return "schinese";
				case 8:
					return "portuguese";
				case 9:
					return "polish";
			}
		}

		internal class ModManager
		{
			internal static bool SteamUser { get; set; } = false;
			internal static bool SteamAvailable { get; set; } = true;
			internal const uint thisApp = 1281930;

			protected Callback<DownloadItemResult_t> m_DownloadItemResult;

			private PublishedFileId_t itemID;

			internal static void Initialize() {
				// Non-steam tModLoader will use the SteamGameServer to perform Browsing & Downloading
				if (ModLoader.Engine.InstallVerifier.IsSteam) {
					SteamUser = true;
					return;
				}

				if (GameServer.Init(0x7f000001, 7775, 7774, EServerMode.eServerModeNoAuthentication, "0.11.9.0")) {
					SteamGameServer.SetGameDescription("tModLoader Mod Browser");
					SteamGameServer.SetProduct(thisApp.ToString());
					SteamGameServer.LogOnAnonymous();
				}
				else {
					Logging.tML.Error("Steam Game Server failed to Init. Steam Workshop downloading on GoG is unavailable. Make sure Steam is installed");
					SteamAvailable = false;
				}
			}

			internal static bool GetPublishIdLocal(LocalMod mod, out ulong publishId) {
				publishId = 0;
				if (!AWorkshopEntry.TryReadingManifest(Path.Combine(Directory.GetParent(mod.modFile.path).ToString(), "workshop.json"), out var info))
					return false;

				publishId = info.workshopEntryId;
				return true;
			}

			internal ModManager(PublishedFileId_t itemID) {
				this.itemID = itemID;
			}

			private static List<LocalMod> enabledItems;

			/// <summary>
			/// Downloads all UIModDownloadItems provided.
			/// </summary>
			public static void Download(List<ModDownloadItem> items) {
				//Set UIWorkshopDownload
				UIWorkshopDownload uiProgress = null;

				// Can't update enabled items due to in-use file access constraints
				enabledItems = new List<LocalMod>();
				foreach (var item in items) {
					if (item.Installed != null && item.Installed.Enabled) {
						enabledItems.Add(item.Installed);
						item.Installed.Enabled = false;
					}
				}

				if (enabledItems.Count > 0) {
					ModLoader.ModLoader.Unload();
				}	

				if (!Main.dedServ) {
					uiProgress = new UIWorkshopDownload(Interface.modBrowser);
					Main.MenuUI.SetState(uiProgress);
				}

				int counter = 0;

				Task.Run(() => TaskDownload(counter, uiProgress, items));
			}

			private static void TaskDownload(int counter, UIWorkshopDownload uiProgress, List<ModDownloadItem> items) {
				var item = items[counter++];
				var mod = new ModManager(new PublishedFileId_t(ulong.Parse(item.PublishId)));
				
				uiProgress?.PrepUIForDownload(item.DisplayName);
				Utils.LogAndConsoleInfoMessage("Attempting Download Item: " + item.DisplayName);
				mod.InnerDownload(uiProgress, item.HasUpdate);

				if (counter == items.Count) {
					uiProgress?.Leave();

					// Restore Enabled items.
					if (enabledItems.Count > 0) {
						foreach (var localMod in enabledItems) {
							localMod.Enabled = true;
						}
						ModLoader.ModLoader.Reload();
					}
				}
				else
					Task.Run(() => TaskDownload(counter, uiProgress, items));
			}

			private EResult downloadResult;

			/// <summary>
			/// Updates and/or Downloads the Item specified when generating the ModManager Instance.
			/// </summary>
			private bool InnerDownload(UIWorkshopDownload uiProgress, bool mbHasUpdate) {
				downloadResult = EResult.k_EResultOK;

				if (NeedsUpdate() || mbHasUpdate) {
					downloadResult = EResult.k_EResultNone;
					Utils.LogAndConsoleInfoMessage("Queueing download with Steam download manager...");

					if (SteamUser)
						SteamDownload(uiProgress);
					else
						GoGDownload(uiProgress);

					Utils.LogAndConsoleInfoMessage("Item Download Completed");
				}
				else {
					// A warning here that you will need to restart the game for item to be removed completely from Steam's runtime cache.
					Utils.LogAndConsoleErrorMessage("Item is/was already installed: " + itemID.ToString() + ". If attempting to re-install, close current instance and re-launch");
				}

				return downloadResult == EResult.k_EResultOK;
			}

			private void SteamDownload(UIWorkshopDownload uiProgress) {
				if (!SteamUGC.DownloadItem(itemID, true)) {
					Utils.LogAndConsoleInfoMessage("Consult " + "C:\\Program Files(x86)\\Steam\\logs\\workshop.log" + " for more information.");
					throw new ArgumentException("Downloading Workshop Item failed due to unknown reasons");
				}

				InnerDownloadQueue(uiProgress);
				SteamUGC.SubscribeItem(itemID);
			}

			private void GoGDownload(UIWorkshopDownload uiProgress) {
				if (!SteamGameServerUGC.DownloadItem(itemID, true)) {
					Utils.LogAndConsoleInfoMessage("Consult " + "C:\\Program Files(x86)\\Steam\\logs\\workshop.log" + " for more information.");
					throw new ArgumentException("GoG: Downloading Workshop Item failed due to unknown reasons");
				}

				InnerDownloadQueue(uiProgress);
			}

			private void InnerDownloadQueue(UIWorkshopDownload uiProgress) {
				bool percent25 = false, percent50 = false, percent75 = false;
				ulong dlBytes, totalBytes;

				while (!IsInstalled()) {
					if (SteamUser)
						SteamUGC.GetItemDownloadInfo(itemID, out dlBytes, out totalBytes);
					else
						SteamGameServerUGC.GetItemDownloadInfo(itemID, out dlBytes, out totalBytes);

					if (uiProgress != null)
						uiProgress.UpdateDownloadProgress((float)dlBytes / Math.Max(totalBytes, 1), (long)dlBytes, (long)totalBytes);

					if (!percent25 && ((float)dlBytes/totalBytes) > 0.25) {
						Utils.LogAndConsoleInfoMessage("Download 25% Complete");
						percent25 = true;
					}

					if (!percent50 && ((float)dlBytes / totalBytes) > 0.50) {
						Utils.LogAndConsoleInfoMessage("Download 50% Complete");
						percent50 = true;
					}

					if (!percent75 && ((float)dlBytes / totalBytes) > 0.75) {
						Utils.LogAndConsoleInfoMessage("Download 75% Complete");
						percent75 = true;
					}
				}

				// We don't receive a callback, so we manually set the success.
				downloadResult = EResult.k_EResultOK;
			}

			internal void Uninstall(string installPath = null) {
				if (String.IsNullOrEmpty(installPath))
					installPath = GetInstallInfo().installPath;

				if (!Directory.Exists(installPath))
					return;

				// Remove the files
				Directory.Delete(installPath, true);

				// Unsubscribe
				if (SteamUser)
					SteamUGC.UnsubscribeItem(itemID);

				UninstallACF();
			}

			private void UninstallACF() {
				// Cleanup acf file by removing info on this itemID
				string acfPath;

				if (!SteamUser)
					acfPath = Path.Combine(Directory.GetCurrentDirectory(), "steamapps", "workshop", "appworkshop_" + thisApp.ToString() + ".acf");
				else
					acfPath = Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(GetInstallInfo().installPath).ToString()).ToString()).ToString(), "appworkshop_" + thisApp.ToString() + ".acf");

				string[] acf = File.ReadAllLines(acfPath);
				using StreamWriter w = new StreamWriter(acfPath);

				int blockLines = 5;
				int skip = 0;

				for (int i = 0; i < acf.Length; i++) {
					if (acf[i].Contains(itemID.ToString())) {
						skip = blockLines;
						continue;
					}
					else if (skip > 0) {
						skip--;
						continue;
					}

					w.WriteLine(acf[i]);
				}
			}

			public ItemInstallInfo GetInstallInfo() {
				string installPath;
				uint lastUpdatedTime;

				if (SteamUser)
					SteamUGC.GetItemInstallInfo(itemID, out var installSize, out installPath, 1000, out lastUpdatedTime);
				else
					SteamGameServerUGC.GetItemInstallInfo(itemID, out var installSize, out installPath, 1000, out lastUpdatedTime);

				return new ItemInstallInfo() { installPath = installPath, lastUpdatedTime = lastUpdatedTime };
			}

			private uint GetState() {
				if (SteamUser)
					return SteamUGC.GetItemState(itemID);
				else
					return SteamGameServerUGC.GetItemState(itemID);
			}

			public bool IsInstalled() {
				var currState = GetState();
				
				bool installed = (currState & (uint)(EItemState.k_EItemStateInstalled)) != 0;
				bool downloading = (currState & ((uint)EItemState.k_EItemStateDownloading + (uint)EItemState.k_EItemStateDownloadPending)) != 0;
				return installed && !downloading;
			}

			public bool NeedsUpdate() {
				var currState = GetState();

				return (currState & (uint)EItemState.k_EItemStateNeedsUpdate) != 0 ||
					(currState == (uint)EItemState.k_EItemStateNone) ||
					(currState & (uint)EItemState.k_EItemStateDownloadPending) != 0;
			}

			private const int PlaytimePagingConst = 100; //https://partner.steamgames.com/doc/api/ISteamUGC#StartPlaytimeTracking

			public static void BeginPlaytimeTracking(LocalMod[] localMods) {
				if (localMods.Length == 0)
					return;

				List<PublishedFileId_t> list = new List<PublishedFileId_t>();
				foreach (var item in localMods) {
					if (item.Enabled && GetPublishIdLocal(item, out ulong publishId))
						list.Add(new PublishedFileId_t(publishId));
				}
				
				int count = list.Count;
				if (count == 0)
					return;

				int pg = count / PlaytimePagingConst;
				int rem = count % PlaytimePagingConst;

				for (int i = 0; i < pg + 1; i++) {
					var pgList = list.GetRange(i * PlaytimePagingConst, (i == pg) ? rem : PlaytimePagingConst);

					// Call the appropriate variant, may need performance optimization.
					if (SteamUser)
						SteamUGC.StartPlaytimeTracking(pgList.ToArray(), (uint)pgList.Count);
					else
						SteamGameServerUGC.StartPlaytimeTracking(pgList.ToArray(), (uint)pgList.Count);
				}
			}

			public static void StopPlaytimeTracking() {
				// Call the appropriate variant
				if (SteamUser)
					SteamUGC.StopPlaytimeTrackingForAllItems();
				else
					SteamGameServerUGC.StopPlaytimeTrackingForAllItems();
			}
		}

		internal static class QueryHelper
		{
			internal const int QueryPagingConst = Steamworks.Constants.kNumUGCResultsPerPage;
			internal static int IncompleteModCount;
			internal static uint TotalItemsQueried;
			internal static EResult ErrorState = EResult.k_EResultNone;

			internal static List<ModDownloadItem> Items = new List<ModDownloadItem>();

			internal static bool FetchDownloadItems() {
				if (!QueryWorkshop())
					return false;

				return true;
			}

			internal static ModDownloadItem FindModDownloadItem(string modName)
			=> Items.FirstOrDefault(x => x.ModName.Equals(modName, StringComparison.OrdinalIgnoreCase));

			internal static bool QueryWorkshop() {
				IncompleteModCount = 0;
				TotalItemsQueried = 0;
				Items.Clear();
				ErrorState = EResult.k_EResultNone;

				AQueryInstance.InstalledMods = ModOrganizer.FindMods();

				if (!ModManager.SteamAvailable)
					return false;

				if (!new AQueryInstance().GetPageCountFast())
					return false;

				//TODO: Threaded query of multiple pages at a time appears un-supported within Steam.
				// Concerned for future speed of querying the Steam Workshop (ie at 1000+ items).
				// At 2-4 seconds for each 50 item set, it will add up (~ 1 minute per 1000 items to load).
				// Some sort of long term solution will be needed; for now I've re-designed it to be able to fill pages independently from our end
				int pageCount = (int)TotalItemsQueried / QueryPagingConst + (TotalItemsQueried % 50 == 0 ? 0 : 1);
				for (uint i = 1; i < pageCount + 1; i++) {
					/*Task.Run(() => */ new AQueryInstance().QueryForPage(i, pageCount == i);
				}

				do {
					if (!HandleError(ErrorState)) {
						return false;
					}
				} while (ErrorState != EResult.k_EResultOK);

				AQueryInstance.InstalledMods = null;
				return true;
			}

			internal static bool HandleError(EResult eResult) {
				if (eResult == EResult.k_EResultOK)
					return true;
				if (eResult == EResult.k_EResultNone)
					return true;

				if (eResult == EResult.k_EResultAccessDenied) {
					Utils.ShowFancyErrorMessage("Error: Access to Steam Workshop was denied.", 0);
				}
				else {
					Utils.ShowFancyErrorMessage("Error: Unable to access Steam Workshop. " + eResult, 0);
					Utils.LogAndConsoleInfoMessage("Consult " + "C:\\Program Files(x86)\\Steam\\logs\\workshop.log" + " for more information.");
				}
				return false;
			}

			internal class AQueryInstance
			{
				private CallResult<SteamUGCQueryCompleted_t> _queryHook;
				protected UGCQueryHandle_t _primaryUGCHandle;
				protected EResult _primaryQueryResult;
				protected uint _queryReturnCount;
				protected uint _totalReturnCount;

				internal static LocalMod[] InstalledMods;

				internal AQueryInstance() {
					_queryHook = CallResult<SteamUGCQueryCompleted_t>.Create(OnWorkshopQueryCompleted);
				}

				internal void QueryForPage(uint page, bool final) {
					_primaryQueryResult = EResult.k_EResultNone;

					SteamAPICall_t call;
					if (ModManager.SteamUser) {
						UGCQueryHandle_t qHandle = SteamUGC.CreateQueryAllUGCRequest(EUGCQuery.k_EUGCQuery_RankedByTotalUniqueSubscriptions, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, new AppId_t(ModManager.thisApp), new AppId_t(ModManager.thisApp), page);

						SteamUGC.SetLanguage(qHandle, GetCurrentSteamLangKey());
						SteamUGC.SetReturnKeyValueTags(qHandle, true);
						SteamUGC.SetReturnLongDescription(qHandle, true);
						SteamUGC.SetReturnPlaytimeStats(qHandle, 30); // Last 30 days of playtime statistics
						SteamUGC.SetAllowCachedResponse(qHandle, 0); // Anything other than 0 may cause Access Denied errors.

						call = SteamUGC.SendQueryUGCRequest(qHandle);
					}
					else {
						UGCQueryHandle_t qHandle = SteamGameServerUGC.CreateQueryAllUGCRequest(EUGCQuery.k_EUGCQuery_RankedByTotalUniqueSubscriptions, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, new AppId_t(ModManager.thisApp), new AppId_t(ModManager.thisApp), page);

						SteamGameServerUGC.SetLanguage(qHandle, GetCurrentSteamLangKey());
						SteamGameServerUGC.SetReturnKeyValueTags(qHandle, true);
						SteamGameServerUGC.SetReturnLongDescription(qHandle, true);
						SteamUGC.SetReturnPlaytimeStats(qHandle, 30); // Last 30 days of playtime statistics
						SteamGameServerUGC.SetAllowCachedResponse(qHandle, 0); // Anything other than 0 may cause Access Denied errors.

						call = SteamGameServerUGC.SendQueryUGCRequest(qHandle);
					}

					_queryHook.Set(call);

					do {
						// Do Pretty Stuff if want here
						ForceCallbacks();
					}
					while (_primaryQueryResult == EResult.k_EResultNone);

					if (_primaryQueryResult != EResult.k_EResultOK) {
						ErrorState = _primaryQueryResult;
						return;
					}

					QueryPageResult();

					if (final && ErrorState == EResult.k_EResultNone)
						ErrorState = EResult.k_EResultOK;
				}

				private void QueryPageResult() {
					for (uint i = 0; i < _queryReturnCount; i++) {
						// Item Result call data
						SteamUGCDetails_t pDetails;

						if (ModManager.SteamUser)
							SteamUGC.GetQueryUGCResult(_primaryUGCHandle, i, out pDetails);
						else
							SteamGameServerUGC.GetQueryUGCResult(_primaryUGCHandle, i, out pDetails);

						PublishedFileId_t id = pDetails.m_nPublishedFileId;

						if (pDetails.m_eVisibility != ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic)
							continue;

						if (pDetails.m_eResult != EResult.k_EResultOK) {
							Logging.tML.Warn("Unable to fetch mod PublishId#" + id + " information. " + pDetails.m_eResult);
							continue;
						}

						DateTime lastUpdate = Utils.UnixTimeStampToDateTime((long)pDetails.m_rtimeUpdated);
						string displayname = pDetails.m_rgchTitle;

						// Item Tagged data
						uint keyCount;

						if (ModManager.SteamUser)
							keyCount = SteamUGC.GetQueryUGCNumKeyValueTags(_primaryUGCHandle, i);
						else
							keyCount = SteamGameServerUGC.GetQueryUGCNumKeyValueTags(_primaryUGCHandle, i);

						if (keyCount < MetadataKeys.Length) {
							Logging.tML.Warn("Mod is missing required metadata: " + displayname);
							continue;
						}

						var metadata = new NameValueCollection();

						for (uint j = 0; j < keyCount; j++) {
							string key, val;

							if (ModManager.SteamUser)
								SteamUGC.GetQueryUGCKeyValueTag(_primaryUGCHandle, i, j, out key, byte.MaxValue, out val, byte.MaxValue);
							else
								SteamGameServerUGC.GetQueryUGCKeyValueTag(_primaryUGCHandle, i, j, out key, byte.MaxValue, out val, byte.MaxValue);

							metadata[key] = val;
						}

						string[] missingKeys = MetadataKeys.Where(k => metadata.Get(k) == null).ToArray();

						if (missingKeys.Length != 0) {
							Logging.tML.Warn($"Mod '{displayname}' is missing required metadata: {string.Join(',', missingKeys.Select(k => $"'{k}'"))}.");
							IncompleteModCount++;
							continue;
						}

						ModSide modside = ModSide.Both;

						if (metadata["modside"] == "Client")
							modside = ModSide.Client;

						if (metadata["modside"] == "Server")
							modside = ModSide.Server;

						if (metadata["modside"] == "NoSync")
							modside = ModSide.NoSync;

						// Preview Image url
						string modIconURL;

						if (ModManager.SteamUser)
							SteamUGC.GetQueryUGCPreviewURL(_primaryUGCHandle, i, out modIconURL, 1000);
						else
							SteamGameServerUGC.GetQueryUGCPreviewURL(_primaryUGCHandle, i, out modIconURL, 1000);

						// Item Statistics
						ulong hot, downloads;

						if (ModManager.SteamUser) {
							SteamUGC.GetQueryUGCStatistic(_primaryUGCHandle, i, EItemStatistic.k_EItemStatistic_NumUniqueSubscriptions, out downloads);
							SteamUGC.GetQueryUGCStatistic(_primaryUGCHandle, i, EItemStatistic.k_EItemStatistic_NumSecondsPlayedDuringTimePeriod, out hot); //Temp: based on how often being played lately?
						}
						else {
							SteamGameServerUGC.GetQueryUGCStatistic(_primaryUGCHandle, i, EItemStatistic.k_EItemStatistic_NumUniqueSubscriptions, out downloads);
							SteamGameServerUGC.GetQueryUGCStatistic(_primaryUGCHandle, i, EItemStatistic.k_EItemStatistic_NumSecondsPlayedDuringTimePeriod, out hot); //Temp: based on how often being played lately?
						}

						// Calculate the Mod Browser Version
						System.Version cVersion = new System.Version(metadata["version"].Replace("v", ""));

						// Check against installed mods
						bool update = false;
						bool updateIsDowngrade = false;
						var installed = InstalledMods.FirstOrDefault(m => m.Name == metadata["name"]);

						if (installed != null) {
							//exists = true;
							if (cVersion > installed.modFile.Version)
								update = true;
							else if (cVersion < installed.modFile.Version)
								update = updateIsDowngrade = true;
						}

						Items.Add(new ModDownloadItem(displayname, metadata["name"], cVersion.ToString(), metadata["author"], metadata["modreferences"], modside, modIconURL, id.m_PublishedFileId.ToString(), (int)downloads, (int)hot, lastUpdate, update, updateIsDowngrade, installed, metadata["modloaderversion"], metadata["homepage"]));
					}
					ReleaseWorkshopQuery();
				}

				internal bool GetPageCountFast() {
					_primaryQueryResult = EResult.k_EResultNone;

					SteamAPICall_t call;
					if (ModManager.SteamUser) {
						UGCQueryHandle_t qHandle = SteamUGC.CreateQueryAllUGCRequest(EUGCQuery.k_EUGCQuery_RankedByTotalUniqueSubscriptions, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, new AppId_t(ModManager.thisApp), new AppId_t(ModManager.thisApp), 1);
						SteamUGC.SetReturnTotalOnly(qHandle, true);
						SteamUGC.SetAllowCachedResponse(qHandle, 0); // Anything other than 0 may cause Access Denied errors.
						call = SteamUGC.SendQueryUGCRequest(qHandle);
					}
					else {
						UGCQueryHandle_t qHandle = SteamGameServerUGC.CreateQueryAllUGCRequest(EUGCQuery.k_EUGCQuery_RankedByTotalUniqueSubscriptions, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, new AppId_t(ModManager.thisApp), new AppId_t(ModManager.thisApp), 1);
						SteamGameServerUGC.SetReturnTotalOnly(qHandle, true);
						SteamGameServerUGC.SetAllowCachedResponse(qHandle, 0); // Anything other than 0 may cause Access Denied errors.
						call = SteamGameServerUGC.SendQueryUGCRequest(qHandle);
					}

					_queryHook.Set(call);

					do {
						// Do Pretty Stuff if want here
						ForceCallbacks();
					}
					while (_primaryQueryResult == EResult.k_EResultNone);
					ReleaseWorkshopQuery();

					if (_primaryQueryResult != EResult.k_EResultOK)
						return HandleError(_primaryQueryResult);

					TotalItemsQueried = _totalReturnCount;
					return true;
				}

				internal SteamUGCDetails_t FastQueryItem(ulong publishedId) {
					_primaryQueryResult = EResult.k_EResultNone;

					SteamAPICall_t call;
					if (ModManager.SteamUser) {
						UGCQueryHandle_t qHandle = SteamUGC.CreateQueryUGCDetailsRequest(new PublishedFileId_t[1] { new PublishedFileId_t(publishedId) }, 1);

						SteamUGC.SetLanguage(qHandle, GetCurrentSteamLangKey());
						SteamUGC.SetReturnLongDescription(qHandle, true);
						SteamUGC.SetAllowCachedResponse(qHandle, 0); // Anything other than 0 may cause Access Denied errors.

						call = SteamUGC.SendQueryUGCRequest(qHandle);
					}
					else {
						UGCQueryHandle_t qHandle = SteamGameServerUGC.CreateQueryUGCDetailsRequest(new PublishedFileId_t[1] { new PublishedFileId_t(publishedId) }, 1);

						SteamGameServerUGC.SetLanguage(qHandle, GetCurrentSteamLangKey());
						SteamGameServerUGC.SetReturnLongDescription(qHandle, true);
						SteamGameServerUGC.SetAllowCachedResponse(qHandle, 0); // Anything other than 0 may cause Access Denied errors.

						call = SteamGameServerUGC.SendQueryUGCRequest(qHandle);
					}

					_queryHook.Set(call);

					do {
						// Do Pretty Stuff if want here
						ForceCallbacks();
					}
					while (_primaryQueryResult == EResult.k_EResultNone);

					SteamUGCDetails_t pDetails;
					if (ModManager.SteamUser)
						SteamUGC.GetQueryUGCResult(_primaryUGCHandle, 0, out pDetails);
					else
						SteamGameServerUGC.GetQueryUGCResult(_primaryUGCHandle, 0, out pDetails);

					ReleaseWorkshopQuery();
					return pDetails;
				}

				private void OnWorkshopQueryCompleted(SteamUGCQueryCompleted_t pCallback, bool bIOFailure) {
					_primaryUGCHandle = pCallback.m_handle;
					_primaryQueryResult = pCallback.m_eResult;
					_queryReturnCount = pCallback.m_unNumResultsReturned;
					_totalReturnCount = pCallback.m_unTotalMatchingResults;
				}

				/// <summary>
				/// Ought be called to release the existing query when we are done with it. Frees memory associated with the handle.
				/// </summary>
				private void ReleaseWorkshopQuery() {
					if (ModManager.SteamUser)
						SteamUGC.ReleaseQueryUGCRequest(_primaryUGCHandle);
					else
						SteamGameServerUGC.ReleaseQueryUGCRequest(_primaryUGCHandle);
				}
			}

			internal static string GetDescription(ulong publishedId) {
				var pDetails = new AQueryInstance().FastQueryItem(publishedId);
				return pDetails.m_rgchDescription;
			}

			internal static ulong GetSteamOwner(ulong publishedId) {
				var pDetails = new AQueryInstance().FastQueryItem(publishedId);
				return pDetails.m_ulSteamIDOwner;
			}

			internal static bool CheckWorkshopConnection() {
				// If populating fails during query, than no connection. Attempt connection if not yet attempted.
				if (ErrorState != EResult.k_EResultOK && !FetchDownloadItems())
					return false;

				// If there are zero items on workshop, than return true.
				if (Items.Count + TotalItemsQueried == 0)
					return true;

				// Otherwise, return the original condition. 
				return Items.Count + IncompleteModCount != 0;
			}
		}
	}
}
