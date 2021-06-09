using Steamworks;
using System;
using System.IO;
using System.Threading;
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

		internal class ModManager {
			private static bool steamUser = true;
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
				else
					m_DownloadItemResult = Callback<DownloadItemResult_t>.Create(OnItemDownloaded);
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
	}
}
