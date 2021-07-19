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

		internal class ModManager
		{
			internal static bool SteamUser { get; set; } = false;
			internal static AppId_t thisApp = ModLoader.Engine.Steam.TMLAppID_t;

			protected Callback<DownloadItemResult_t> m_DownloadItemResult;

			private PublishedFileId_t itemID;

			internal static void Initialize() {
				if (!ModLoader.Engine.Steam.IsSteamApp) {
					// Non-steam tModLoader will use the SteamGameServer to perform Browsing & Downloading
					GameServer.Init(0x7f000001, 7776, 7775, 7774, EServerMode.eServerModeNoAuthentication, "0.11.9.0");

					SteamGameServer.SetGameDescription("tModLoader Mod Browser");
					SteamGameServer.SetProduct(thisApp.ToString());
					SteamGameServer.LogOnAnonymous();
				}
				else
					SteamUser = true;
			}

			internal ModManager(PublishedFileId_t itemID) {
				this.itemID = itemID;

				if (SteamUser)
					m_DownloadItemResult = Callback<DownloadItemResult_t>.Create(OnItemDownloaded);
			}

			public static void Download(UIModDownloadItem item) => Download(new List<UIModDownloadItem>() { item });

			/// <summary>
			/// Downloads all UIModDownloadItems provided.
			/// </summary>
			public static void Download(List<UIModDownloadItem> items) {
				//Set UIWorkshopDownload
				UIWorkshopDownload uiProgress = null;
				
				if (!Main.dedServ) {
					uiProgress = new UIWorkshopDownload(Interface.modBrowser);
					Main.MenuUI.SetState(uiProgress);
				}

				int counter = 0;

				Task.Run(() => TaskDownload(counter, uiProgress, items));
			}

			private static void TaskDownload(int counter, UIWorkshopDownload uiProgress, List<UIModDownloadItem> items) {
				var item = items[counter++];
				var mod = new ModManager(new PublishedFileId_t(ulong.Parse(item.PublishId)));
				
				uiProgress?.PrepUIForDownload(item.DisplayName);
				mod.InnerDownload(uiProgress);

				if (counter == items.Count)
					uiProgress?.Leave();
				else
					Task.Run(() => TaskDownload(counter, uiProgress, items));
			}

			private EResult downloadResult;

			/// <summary>
			/// Updates and/or Downloads the Item specified when generating the ModManager Instance.
			/// </summary>
			private bool InnerDownload(UIWorkshopDownload uiProgress) {
				downloadResult = EResult.k_EResultOK;

				if (NeedsUpdate()) {
					downloadResult = EResult.k_EResultNone;

					if (SteamUser)
						SteamDownload(uiProgress);
					else
						GoGDownload(uiProgress);
				}
				else {
					// A warning here that you will need to restart the game for item to be removed completely from Steam's runtime cache.
					Logging.tML.Warn("Item was installed at start of session: " + itemID.ToString() + ". If attempting to re-install, close current instance and re-launch");
				}

				return downloadResult == EResult.k_EResultOK;
			}

			private void SteamDownload(UIWorkshopDownload uiProgress) {
				if (!SteamUGC.DownloadItem(itemID, true)) {
					throw new ArgumentException("Downloading Workshop Item failed due to unknown reasons");
				}

				do {
					SteamUGC.GetItemDownloadInfo(itemID, out ulong dlBytes, out ulong totalBytes);
					if (uiProgress != null)
						uiProgress.UpdateDownloadProgress(dlBytes / Math.Max(totalBytes, 1), (long)dlBytes, (long)totalBytes);

					SteamAPI.RunCallbacks();
				}
				while (downloadResult == EResult.k_EResultNone);

				SteamUGC.SubscribeItem(itemID);
			}

			private void OnItemDownloaded(DownloadItemResult_t pCallback) {
				if (pCallback.m_nPublishedFileId == itemID) {
					downloadResult = pCallback.m_eResult;
				}
			}

			private void GoGDownload(UIWorkshopDownload uiProgress) {
				if (!SteamGameServerUGC.DownloadItem(itemID, true)) {
					throw new ArgumentException("GoG: Downloading Workshop Item failed due to unknown reasons");
				}

				while (!IsInstalled()) {
					SteamGameServerUGC.GetItemDownloadInfo(itemID, out ulong dlBytes, out ulong totalBytes);

					if (uiProgress != null)
						uiProgress.UpdateDownloadProgress(dlBytes / Math.Max(totalBytes, 1), (long)dlBytes, (long)totalBytes);
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

			public bool IsInstalled() => (GetState() & (uint)EItemState.k_EItemStateInstalled) != 0;

			public bool NeedsUpdate() {
				var currState = GetState();

				return (currState & (uint)EItemState.k_EItemStateNeedsUpdate) != 0 ||
					(currState == (uint)EItemState.k_EItemStateNone) ||
					(currState & (uint)EItemState.k_EItemStateDownloadPending) != 0;
			}

			public static bool BeginPlaytimeTracking(PublishedFileId_t[] modsById) {
				uint count = (uint)modsById.Length;

				if (count == 0)
					return true;
				//TODO: Improve. You can't begin tracking more than 100 items within one call.
				else if (count >= 100)
					return false;

				// Call the appropriate variant
				if (SteamUser)
					SteamUGC.StartPlaytimeTracking(modsById, (uint)modsById.Length);
				else
					SteamGameServerUGC.StartPlaytimeTracking(modsById, (uint)modsById.Length);

				return true;
			}

			public static bool StopPlaytimeTracking(PublishedFileId_t[] modsById) {
				uint count = (uint)modsById.Length;

				if (count == 0)
					return true;
				//TODO: Improve. You can't begin tracking more than 100 items within one call.
				else if (count >= 100)
					return false;

				// Call the appropriate variant
				if (SteamUser)
					SteamUGC.StopPlaytimeTracking(modsById, (uint)modsById.Length);
				else
					SteamGameServerUGC.StopPlaytimeTracking(modsById, (uint)modsById.Length);

				return true;
			}
		}

		internal class QueryHelper
		{
			private CallResult<SteamUGCQueryCompleted_t> _queryHook;
			protected UGCQueryHandle_t _primaryUGCHandle;
			protected EResult _primaryQueryResult;
			protected uint _queryReturnCount;

			internal QueryHelper() {
				_queryHook = CallResult<SteamUGCQueryCompleted_t>.Create(OnWorkshopQueryCompleted);
			}

			internal List<UIModDownloadItem> QueryWorkshop() {
				uint queryPage = 0;
				List<UIModDownloadItem> items = new List<UIModDownloadItem>();
				LocalMod[] installedMods = ModOrganizer.FindMods();

				do {
					QueryForPage(++queryPage);

					if (_primaryQueryResult == EResult.k_EResultAccessDenied) {
						Utils.ShowFancyErrorMessage("Error: Access to Steam Workshop was denied.", 0);
						return items;
					}
					else if (_primaryQueryResult != EResult.k_EResultOK) {
						Utils.ShowFancyErrorMessage("Error: Unable to access Steam Workshop. " + _primaryQueryResult, 0);
						return items;
					}


					for (uint i = 0; i < _queryReturnCount; i++) {
						// Item Result call data
						SteamUGCDetails_t pDetails;

						if (ModManager.SteamUser)
							SteamUGC.GetQueryUGCResult(_primaryUGCHandle, i, out pDetails);
						else
							SteamGameServerUGC.GetQueryUGCResult(_primaryUGCHandle, i, out pDetails);

						PublishedFileId_t id = pDetails.m_nPublishedFileId;

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

						if (keyCount != MetadataKeys.Length) {
							Logging.tML.Warn("Mod is missing required metadata: " + displayname);
							continue;
						}

						var metadata = new NameValueCollection();

						for (uint j = 0; j < keyCount; j++) {
							string key, val;

							if (ModManager.SteamUser)
								SteamUGC.GetQueryUGCKeyValueTag(_primaryUGCHandle, i, j, out key, 100, out val, 100);
							else
								SteamGameServerUGC.GetQueryUGCKeyValueTag(_primaryUGCHandle, i, j, out key, 100, out val, 100);

							metadata[MetadataKeys[j]] = val;
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
							SteamUGC.GetQueryUGCStatistic(_primaryUGCHandle, i, EItemStatistic.k_EItemStatistic_NumPlaytimeSessionsDuringTimePeriod, out hot); //Temp: based on how often being played lately?
						}
						else {
							SteamGameServerUGC.GetQueryUGCStatistic(_primaryUGCHandle, i, EItemStatistic.k_EItemStatistic_NumUniqueSubscriptions, out downloads);
							SteamGameServerUGC.GetQueryUGCStatistic(_primaryUGCHandle, i, EItemStatistic.k_EItemStatistic_NumPlaytimeSessionsDuringTimePeriod, out hot); //Temp: based on how often being played lately?
						}

						// Calculate the Mod Browser Version
						System.Version cVersion = new System.Version(metadata["version"].Substring(1));

						// Prioritize version information found in the display name, for automation purposes.
						int findVersion = displayname.LastIndexOf("v") + 1;
						if (findVersion > 0) {
							string possibleVersion = displayname.Substring(findVersion);
							if (possibleVersion.Contains(".")) {
								cVersion = new System.Version(possibleVersion);
							}
						}

						// Check against installed mods
						bool update = false;
						bool updateIsDowngrade = false;
						var installed = installedMods.FirstOrDefault(m => m.Name == metadata["name"]);

						if (installed != null) {
							//exists = true;
							if (cVersion > installed.modFile.Version)
								update = true;
							else if (cVersion < installed.modFile.Version)
								update = updateIsDowngrade = true;
						}

						items.Add(new UIModDownloadItem(displayname, metadata["name"], cVersion.ToString(), metadata["author"], metadata["modreferences"], modside, modIconURL, id.m_PublishedFileId.ToString(), (int)downloads, (int)hot, lastUpdate.ToString(), update, updateIsDowngrade, installed, metadata["modloaderversion"], metadata["homepage"], i));
					}
					ReleaseWorkshopQuery();
				} while (_queryReturnCount == Steamworks.Constants.kNumUGCResultsPerPage); // 50 is based on kNumUGCResultsPerPage constant in ISteamUGC. Can't find constant itself? - Solxan

				return items;
			}

			private void QueryForPage(uint page) {
				_primaryQueryResult = EResult.k_EResultNone;

				SteamAPICall_t call;

				if (ModManager.SteamUser) {
					UGCQueryHandle_t qHandle = SteamUGC.CreateQueryAllUGCRequest(EUGCQuery.k_EUGCQuery_RankedByTotalUniqueSubscriptions, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, ModManager.thisApp, ModManager.thisApp, page);

					SteamUGC.SetReturnKeyValueTags(qHandle, true);
					SteamUGC.SetReturnLongDescription(qHandle, true);
					SteamUGC.SetAllowCachedResponse(qHandle, 30); // Prevents spamming refreshes from overloading Steam

					call = SteamUGC.SendQueryUGCRequest(qHandle);
				}
				else {
					UGCQueryHandle_t qHandle = SteamGameServerUGC.CreateQueryAllUGCRequest(EUGCQuery.k_EUGCQuery_RankedByTotalUniqueSubscriptions, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, ModManager.thisApp, ModManager.thisApp, page);

					SteamGameServerUGC.SetReturnKeyValueTags(qHandle, true);
					SteamGameServerUGC.SetReturnLongDescription(qHandle, true);
					SteamGameServerUGC.SetAllowCachedResponse(qHandle, 30); // Prevents spamming refreshes from overloading Steam

					call = SteamGameServerUGC.SendQueryUGCRequest(qHandle);
				}
				
				_queryHook.Set(call);

				do {
					// Do Pretty Stuff if want here
					Thread.Sleep(5);

					if (ModManager.SteamUser) 
						SteamAPI.RunCallbacks();
					else
						GameServer.RunCallbacks();
				}
				while (_primaryQueryResult == EResult.k_EResultNone);
			}

			private void OnWorkshopQueryCompleted(SteamUGCQueryCompleted_t pCallback, bool bIOFailure) {
				_primaryUGCHandle = pCallback.m_handle;
				_primaryQueryResult = pCallback.m_eResult;
				_queryReturnCount = pCallback.m_unNumResultsReturned;
			}

			/// <summary>
			/// Ought be called to release the existing query when we are done with it. Frees memory associated with the handle.
			/// </summary>
			internal void ReleaseWorkshopQuery() {
				if (ModManager.SteamUser)
					SteamUGC.ReleaseQueryUGCRequest(_primaryUGCHandle);
				else
					SteamGameServerUGC.ReleaseQueryUGCRequest(_primaryUGCHandle);
			}

			internal string GetDescription(uint queryIndex) {
				SteamUGCDetails_t pDetails;

				uint pg = queryIndex / 50 + 1;
				uint index = queryIndex % 50;

				QueryForPage(pg);

				if (ModManager.SteamUser)
					SteamUGC.GetQueryUGCResult(_primaryUGCHandle, index, out pDetails);
				else
					SteamGameServerUGC.GetQueryUGCResult(_primaryUGCHandle, index, out pDetails);

				ReleaseWorkshopQuery();
				return pDetails.m_rgchDescription;
			}

			internal ulong GetSteamOwner(uint queryIndex) {
				uint pg = queryIndex / 50 + 1;
				uint index = queryIndex % 50;

				QueryForPage(pg);

				SteamUGC.GetQueryUGCResult(_primaryUGCHandle, index, out var pDetails);

				ReleaseWorkshopQuery();
				return pDetails.m_ulSteamIDOwner;
			}

			internal static bool CheckWorkshopConnection() {
				if (Interface.modBrowser.Items.Count != 0)
					return true;

				Interface.modBrowser.InnerPopulateModBrowser();

				if (Interface.modBrowser.Items.Count != 0)
					return true;

				return false;
			}
		}
	}
}
