using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.ModLoader.UI.DownloadManager;
using Terraria.Social.Base;

namespace Terraria.Social.Steam
{
	public partial class WorkshopHelper
	{
		public struct ItemInstallInfo
		{
			public string installPath;
			public UInt32 lastUpdatedTime;
		}

		public class ModPublisherInstance : UGCBased.APublisherInstance
		{
			protected override string GetHeaderText() => ModWorkshopEntry.GetHeaderTextFor(_publishedFileID.m_PublishedFileId, _entryData.Tags, _publicity, _entryData.PreviewImagePath);

			protected override void PrepareContentForUpdate() {
			}
		}

		internal class ModManager
		{
			internal static bool SteamUser { get; set; } = true;
			public static AppId_t thisApp = ModLoader.Engine.Steam.TMLAppID_t;

			public static void Initialize() {
				if (!ModLoader.Engine.Steam.IsSteamApp) {
					// Non-steam tModLoader will use the SteamGameServer to perform Browsing & Downloading
					SteamUser = false;
					GameServer.Init(0x7f000001, 7776, 7775, 7774, EServerMode.eServerModeNoAuthentication, "0.11.9.0");
					SteamGameServer.SetGameDescription("tModLoader Mod Browser");
					SteamGameServer.SetProduct(thisApp.ToString());

					SteamGameServer.LogOnAnonymous();
				}
			}

			PublishedFileId_t itemID;
			protected Callback<DownloadItemResult_t> m_DownloadItemResult;

			public ModManager(PublishedFileId_t itemID) {
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
				var uiProgress = new UIWorkshopDownload(Interface.modBrowser);
				Main.MenuUI.SetState(uiProgress);
				int counter = 0;

				Task.Run(() => InnerDownload(counter, uiProgress, items));
			}

			private static void InnerDownload(int counter, UIWorkshopDownload uiProgress, List<UIModDownloadItem> items) {
				var item = items[counter++];
				var mod = new ModManager(new PublishedFileId_t(ulong.Parse(item.PublishId)));
				
				uiProgress.PrepUIForDownload(item.DisplayName);
				mod.Download(uiProgress);

				if (counter == items.Count)
					uiProgress.Leave();
				else
					Task.Run(() => InnerDownload(counter, uiProgress, items));
			}

			public EResult downloadResult;

			/// <summary>
			/// Updates and/or Downloads the Item specified when generating the ModManager Instance.
			/// </summary>
			private bool Download(UIWorkshopDownload uiProgress) {
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
					Logging.tML.Debug("Item was installed at start of session: " + itemID.ToString() + "\nIf attempting to re-install, close current instance and re-launch");
				}

				return downloadResult == EResult.k_EResultOK;
			}

			private void SteamDownload(UIWorkshopDownload uiProgress) {
				if (!SteamUGC.DownloadItem(itemID, true)) {
					throw new ArgumentException("Downloading Workshop Item failed due to unknown reasons");
				}

				ulong dlBytes, totalBytes;
				do {
					SteamUGC.GetItemDownloadInfo(itemID, out dlBytes, out totalBytes);
					uiProgress.UpdateDownloadProgress(dlBytes / Math.Max(totalBytes, 1), (long)dlBytes, (long)totalBytes);

					SteamAPI.RunCallbacks();
				} while (downloadResult == EResult.k_EResultNone);

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

				ulong dlBytes, totalBytes;
				while (!IsInstalled()) {
					SteamGameServerUGC.GetItemDownloadInfo(itemID, out dlBytes, out totalBytes);
					uiProgress.UpdateDownloadProgress(dlBytes / Math.Max(totalBytes, 1), (long)dlBytes, (long)totalBytes);
				}

				// We don't receive a callback, so we manually set the success.
				downloadResult = EResult.k_EResultOK;
			}

			public void Uninstall() {
				var installPath = GetInstallInfo().installPath;
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
				string acfPath = Path.Combine(Directory.GetCurrentDirectory(), "steamapps", "workshop", "appworkshop_" + thisApp.ToString() + ".acf");

				var acf = File.ReadAllLines(acfPath);
				using (StreamWriter w = new StreamWriter(acfPath)) {
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

				// You can't begin tracking more than 100 items within one call.
				//TODO: Improve.
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

				// You can't begin tracking more than 100 items within one call.
				//TODO: Improve.
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
						throw new AccessViolationException("Error: Access to Steam Workshop was denied.");
					}
					else if (_primaryQueryResult != EResult.k_EResultOK) {
						Logging.tML.Error("Unable to access Steam Workshop");
						return items;
					}


					for (uint i = 0; i < _queryReturnCount; i++) {
						// Item Result call data
						SteamUGCDetails_t pDetails;
						if (ModManager.SteamUser)
							SteamUGC.GetQueryUGCResult(_primaryUGCHandle, i, out pDetails);
						else
							SteamGameServerUGC.GetQueryUGCResult(_primaryUGCHandle, i, out pDetails);
						
						if (pDetails.m_eResult != EResult.k_EResultOK) {
							Logging.tML.Debug("Unable to fetch mod: " + (queryPage - 1) * 50 + i);
							continue;
						}	

						string displayname = pDetails.m_rgchTitle;
						PublishedFileId_t id = pDetails.m_nPublishedFileId;
						DateTime lastUpdate = Utils.UnixTimeStampToDateTime((long)pDetails.m_rtimeUpdated);

						// Dependencies data
						PublishedFileId_t[] depsArr = new PublishedFileId_t[pDetails.m_unNumChildren];
						if (ModManager.SteamUser)
							SteamUGC.GetQueryUGCChildren(_primaryUGCHandle, i, depsArr, (uint)depsArr.Length);
						else
							SteamGameServerUGC.GetQueryUGCChildren(_primaryUGCHandle, i, depsArr, (uint)depsArr.Length);

						string modreferences = "";
						foreach (var item in depsArr) {
							modreferences += item.ToString() + ",";
						}

						// Item Tagged data
						uint keyCount;
						if (ModManager.SteamUser)
							keyCount = SteamUGC.GetQueryUGCNumKeyValueTags(_primaryUGCHandle, i);
						else
							keyCount = SteamGameServerUGC.GetQueryUGCNumKeyValueTags(_primaryUGCHandle, i);

						string author = null, modloaderversion = null, modsideString = null, homepage = null, version = null, name = null;
						for (uint j = 0; j < keyCount; j++) {
							string key, val;
							if (ModManager.SteamUser)
								SteamUGC.GetQueryUGCKeyValueTag(_primaryUGCHandle, i, j, out key, 100, out val, 100);
							else
								SteamGameServerUGC.GetQueryUGCKeyValueTag(_primaryUGCHandle, i, j, out key, 100, out val, 100);

							switch (key) {
								case "internalname": // index 0
									name = val;
									continue;
								case "author": // index 1
									author = val;
									continue;
								case "modside": // index 2
									modsideString = val;
									continue;
								case "homepage": // index 3
									homepage = val;
									continue;
								case "modloaderversion": // index 4
									modloaderversion = val;
									continue;
								case "version": // index 5
									version = val;
									continue;
							}
						}

						ModSide modside = ModSide.Both;
						if (modsideString == "Client")
							modside = ModSide.Client;
						if (modsideString == "Server")
							modside = ModSide.Server;
						if (modsideString == "NoSync")
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
						

						// Check against installed mods
						bool update = false;
						bool updateIsDowngrade = false;
						var installed = installedMods.FirstOrDefault(m => m.Name == name);
						if (installed != null) {
							//exists = true;
							var cVersion = new System.Version(version.Substring(1));
							if (cVersion > installed.modFile.Version)
								update = true;
							else if (cVersion < installed.modFile.Version)
								update = updateIsDowngrade = true;
						}

						items.Add(new UIModDownloadItem(displayname, name, version, author, modreferences, modside, modIconURL, id.m_PublishedFileId.ToString(), (int)downloads, (int)hot, lastUpdate.ToString(), update, updateIsDowngrade, installed, modloaderversion, homepage, i));
					}

				} while (_queryReturnCount == 50); // 50 is based on kNumUGCResultsPerPage constant in ISteamUGC. Can't find constant itself?

				
				return items;
			}

			private void QueryForPage(uint page) {
				_primaryQueryResult = EResult.k_EResultNone;

				SteamAPICall_t call;
				if (ModManager.SteamUser) {
					UGCQueryHandle_t qHandle = SteamUGC.CreateQueryAllUGCRequest(EUGCQuery.k_EUGCQuery_RankedByTotalUniqueSubscriptions, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, ModManager.thisApp, ModManager.thisApp, page);
					SteamUGC.SetReturnKeyValueTags(qHandle, true);
					SteamUGC.SetReturnChildren(qHandle, true);

					call = SteamUGC.SendQueryUGCRequest(qHandle);
				}
				else {
					UGCQueryHandle_t qHandle = SteamGameServerUGC.CreateQueryAllUGCRequest(EUGCQuery.k_EUGCQuery_RankedByTotalUniqueSubscriptions, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, ModManager.thisApp, ModManager.thisApp, page);
					SteamGameServerUGC.SetReturnKeyValueTags(qHandle, true);
					SteamGameServerUGC.SetReturnChildren(qHandle, true);

					call = SteamGameServerUGC.SendQueryUGCRequest(qHandle);
				}
				
				_queryHook.Set(call);

				do {
					// Do Pretty Stuff

					Thread.Sleep(5);
					if (ModManager.SteamUser) 
						SteamAPI.RunCallbacks();
					else
						GameServer.RunCallbacks();
				} while (_primaryQueryResult == EResult.k_EResultNone);
			}

			private void OnWorkshopQueryCompleted(SteamUGCQueryCompleted_t pCallback, bool bIOFailure) {
				_primaryUGCHandle = pCallback.m_handle;
				_primaryQueryResult = pCallback.m_eResult;
				_queryReturnCount = pCallback.m_unNumResultsReturned;
			}

			/// <summary>
			/// Ought be called to release the existing query when we are done with it.
			/// </summary>
			internal void ReleaseWorkshopQuery() {
				if (ModManager.SteamUser)
					SteamUGC.ReleaseQueryUGCRequest(_primaryUGCHandle);
				else
					SteamGameServerUGC.ReleaseQueryUGCRequest(_primaryUGCHandle);
			}

			internal string GetDescription(uint queryIndex) {
				SteamUGCDetails_t pDetails;
				if (ModManager.SteamUser)
					SteamUGC.GetQueryUGCResult(_primaryUGCHandle, queryIndex, out pDetails);
				else
					SteamGameServerUGC.GetQueryUGCResult(_primaryUGCHandle, queryIndex, out pDetails);

				return pDetails.m_rgchDescription;
			}

			internal ulong GetSteamOwner(uint queryIndex) {
				SteamUGC.GetQueryUGCResult(_primaryUGCHandle, queryIndex, out var pDetails);
				return pDetails.m_ulSteamIDOwner;
			}
		}
	}
}
