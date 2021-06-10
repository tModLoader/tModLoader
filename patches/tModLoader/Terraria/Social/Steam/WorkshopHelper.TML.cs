using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI.ModBrowser;
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
			internal static bool steamUser = true;
			public static AppId_t thisApp = ModLoader.Engine.Steam.TMLAppID_t;

			public static void Initialize() {
				if (!ModLoader.Engine.Steam.IsSteamApp) {
					steamUser = false;
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
				if (steamUser)
					m_DownloadItemResult = Callback<DownloadItemResult_t>.Create(OnItemDownloaded);
				// GameServer callback isn't working for this?
				//else
				//m_DownloadItemResult = Callback<DownloadItemResult_t>.Create(OnItemDownloaded);
			}

			public EResult downloadResult;

			/// <summary>
			/// Updates and/or Downloads the Item specified when generating the ModManager Instance.
			/// </summary>
			public bool Download() {
				downloadResult = EResult.k_EResultOK;
				if (NeedsUpdate()) {
					downloadResult = EResult.k_EResultNone;
					if (steamUser)
						SteamDownload();
					else
						GoGDownload();
				}
				return downloadResult == EResult.k_EResultOK;
			}

			private void SteamDownload() {
				if (!SteamUGC.DownloadItem(itemID, true)) {
					throw new ArgumentException("Downloading Workshop Item failed due to unknown reasons");
				}

				ulong dlBytes, totalBytes;
				do {
					SteamUGC.GetItemDownloadInfo(itemID, out dlBytes, out totalBytes);
					// Do Pretty Stuff

					Thread.Sleep(5);
					SteamAPI.RunCallbacks();
				} while (downloadResult == EResult.k_EResultNone);

				SteamUGC.SubscribeItem(itemID);
			}

			private void OnItemDownloaded(DownloadItemResult_t pCallback) {
				if (pCallback.m_nPublishedFileId == itemID) {
					downloadResult = pCallback.m_eResult;
				}
			}

			private void GoGDownload() {
				if (!SteamGameServerUGC.DownloadItem(itemID, true)) {
					throw new ArgumentException("GoG: Downloading Workshop Item failed due to unknown reasons");
				}

				ulong dlBytes, totalBytes;
				while (!IsInstalled()) {
					SteamGameServerUGC.GetItemDownloadInfo(itemID, out dlBytes, out totalBytes);
					// Do Pretty Stuff
				}

				// We don't receive a callback, so we have to manually set the success.
				downloadResult = EResult.k_EResultOK;
			}

			public void Uninstall() {
				//TODO: Add a warning here that you will need to restart the game for item to be removed completely from Steam's runtime cache.
				var installPath = GetInstallInfo().installPath;
				if (!Directory.Exists(installPath))
					return;

				// Remove the files
				Directory.Delete(installPath, true);

				// Unsubscribe
				if (steamUser)
					SteamUGC.UnsubscribeItem(itemID);
				else
					RemoveGoG();
			}

			private void RemoveGoG() {
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
				if (steamUser)
					SteamUGC.GetItemInstallInfo(itemID, out var installSize, out installPath, 1000, out lastUpdatedTime);
				else
					SteamGameServerUGC.GetItemInstallInfo(itemID, out var installSize, out installPath, 1000, out lastUpdatedTime);

				return new ItemInstallInfo() { installPath = installPath, lastUpdatedTime = lastUpdatedTime };
			}

			private uint GetState() {
				if (steamUser)
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
				if (steamUser)
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
				if (steamUser)
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

					for (uint i = 0; i < _queryReturnCount; i++) {
						// Item Result call data
						SteamUGC.GetQueryUGCResult(_primaryUGCHandle, i, out var pDetails);
						string displayname = pDetails.m_rgchTitle;
						PublishedFileId_t id = pDetails.m_nPublishedFileId;
						uint timeStamp = pDetails.m_rtimeUpdated; //TODO: this is unix time, probably doesn't match the old time system so need to update it

						// Dependencies data
						PublishedFileId_t[] depsArr = new PublishedFileId_t[pDetails.m_unNumChildren];
						SteamUGC.GetQueryUGCChildren(_primaryUGCHandle, i, depsArr, (uint)depsArr.Length);
						string modreferences = "";
						foreach (var item in depsArr) {
							modreferences += item.ToString() + ",";
						}

						// Mod internal name
						SteamUGC.GetQueryUGCMetadata(_primaryUGCHandle, i, out string name, 100);

						// Item Tagged data
						uint keyCount = SteamUGC.GetQueryUGCNumKeyValueTags(_primaryUGCHandle, i);
						string author = null, modloaderversion = null, modsideString = null, homepage = null, version = null;
						for (uint j = 0; j < keyCount; j++) {
							SteamUGC.GetQueryUGCKeyValueTag(_primaryUGCHandle, i, j, out string key, 100, out string val, 100);
							switch (key) {
								case "author":
									author = val;
									continue;
								case "modside":
									modsideString = val;
									continue;
								case "homepage":
									homepage = val;
									continue;
								case "modloaderversion":
									modloaderversion = val;
									continue;
								case "version":
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
						SteamUGC.GetQueryUGCPreviewURL(_primaryUGCHandle, i, out string modIconURL, 1000);

						// Item Statistics
						SteamUGC.GetQueryUGCStatistic(_primaryUGCHandle, i, EItemStatistic.k_EItemStatistic_NumUniqueSubscriptions, out ulong downloads);
						SteamUGC.GetQueryUGCStatistic(_primaryUGCHandle, i, EItemStatistic.k_EItemStatistic_NumPlaytimeSessionsDuringTimePeriod, out ulong hot); //Temp: based on how often being played lately?


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

						items.Add(new UIModDownloadItem(displayname, name, version, author, modreferences, modside, modIconURL, id.m_PublishedFileId.ToString(), (int)downloads, (int)hot, timeStamp.ToString(), update, updateIsDowngrade, installed, modloaderversion));
					}

				} while (_queryReturnCount == 50); // 50 is based on kNumUGCResultsPerPage constant in ISteamUGC. Can't find constant itself?

				// We ought to release the handle formally before exiting
				SteamUGC.ReleaseQueryUGCRequest(_primaryUGCHandle);
				return items;
			}

			private void QueryForPage(uint page) {
				_primaryQueryResult = EResult.k_EResultNone;

				UGCQueryHandle_t qHandle = SteamUGC.CreateQueryAllUGCRequest(EUGCQuery.k_EUGCQuery_RankedByTotalUniqueSubscriptions, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, ModManager.thisApp, ModManager.thisApp, page);
				SteamUGC.SetReturnKeyValueTags(qHandle, true);
				SteamUGC.SetReturnChildren(qHandle, true);
				
				SteamAPICall_t call = SteamUGC.SendQueryUGCRequest(qHandle);
				_queryHook.Set(call);

				do {
					// Do Pretty Stuff

					Thread.Sleep(5);
					SteamAPI.RunCallbacks();
				} while (_primaryQueryResult == EResult.k_EResultNone);
			}

			private void OnWorkshopQueryCompleted(SteamUGCQueryCompleted_t pCallback, bool bIOFailure) {
				_primaryUGCHandle = pCallback.m_handle;
				_primaryQueryResult = pCallback.m_eResult;
				_queryReturnCount = pCallback.m_unNumResultsReturned;
			}
		}
	}
}
