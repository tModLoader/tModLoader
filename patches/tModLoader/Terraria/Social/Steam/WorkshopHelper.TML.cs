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
			public static bool steamUser = true;
			public static AppId_t thisApp = ModLoader.Engine.Steam.TMLAppID_t;

			public static void Initialize() {
				if (!ModLoader.Engine.Steam.IsSteamApp) {
					steamUser = false;
					GameServer.Init(0x7f000001, 7776, 7775, 7774, EServerMode.eServerModeNoAuthentication, "0.11.9.0");
					string currDir = Directory.GetCurrentDirectory();
					SteamGameServer.SetModDir(currDir);
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

				ulong dlBytes = 0, totalBytes = 1;
				do {
					SteamUGC.GetItemDownloadInfo(itemID, out dlBytes, out totalBytes);

					// Do Pretty Stuff
				} while (dlBytes != totalBytes);

				do {
					Thread.Sleep(10);
					SteamAPI.RunCallbacks();
					// Say installing stuff, there is a delay here it seems.
				} while (downloadResult == EResult.k_EResultNone);
				
				SteamUGC.SubscribeItem(itemID);
			}

			private void GoGDownload() {
				if (!SteamGameServerUGC.DownloadItem(itemID, true)) {
					throw new ArgumentException("GoG: Downloading Workshop Item failed due to unknown reasons");
				}

				ulong dlBytes = 0, totalBytes = 1;
				do {
					// Handle the download not yet starting, there is a delay for GameServer
					SteamGameServerUGC.GetItemDownloadInfo(itemID, out dlBytes, out totalBytes);
				} while (totalBytes == 0);

				while (dlBytes != totalBytes) { 
					SteamGameServerUGC.GetItemDownloadInfo(itemID, out dlBytes, out totalBytes);
					// Do Stuff
				} 

				// We don't receive a callback, so we have to manually set the success.
				downloadResult = EResult.k_EResultOK;
			}

			private void OnItemDownloaded(DownloadItemResult_t pCallback) {
				if (pCallback.m_nPublishedFileId == itemID) {
					downloadResult = pCallback.m_eResult;
				}
			}

			public ItemInstallInfo GetInstallInfo() {
				string installPath; uint lastUpdatedTime;
				if (steamUser)
					SteamUGC.GetItemInstallInfo(itemID, out var installSize, out installPath, 1000, out lastUpdatedTime);
				else
					SteamGameServerUGC.GetItemInstallInfo(itemID, out var installSize, out installPath, 1000, out lastUpdatedTime);
				
				return new ItemInstallInfo() { installPath = installPath, lastUpdatedTime = lastUpdatedTime };
			}

			public void Uninstall() {
				//TODO: Add a warning here that you will need to restart the game for item to be removed completely from Steam's runtime cache.
				var installPath = GetInstallInfo().installPath;
				if (!Directory.Exists(installPath))
					return;

				// Remove the files
				Directory.Delete(installPath, true);

				// Unsubsribe
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
		}
	}
}
