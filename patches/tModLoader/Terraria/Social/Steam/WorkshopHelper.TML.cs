using Newtonsoft.Json;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Terraria.IO;
using Terraria.ModLoader.Engine;
using Terraria.Social.Base;
using Terraria.Utilities;

namespace Terraria.Social.Steam
{
	public partial class WorkshopHelper
	{
		public struct ItemInstallInfo
		{
			public string installPath;
			public UInt32 lastUpdatedTime;
		}

		internal class ModManager {
			public static bool steamUser = true;
			public static AppId_t thisApp = ModLoader.Engine.Steam.TerrariaAppId_t;

			public static void Initialize() {
				if (!ModLoader.Engine.Steam.IsSteamApp) {
					steamUser = false;
					GameServer.Init(0x7f000001, 7776, 7775, 7774, EServerMode.eServerModeNoAuthentication, "0.11.9.0");
					string currDir = Directory.GetCurrentDirectory();
					SteamGameServer.SetModDir("D:\\Other");
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
				var currState = GetState();
				if ((currState & (uint)EItemState.k_EItemStateNeedsUpdate) != 0 ||
					(currState == (uint)EItemState.k_EItemStateNone) ||
					(currState & (uint)EItemState.k_EItemStateDownloadPending) != 0 ) {
					if (steamUser)
						SteamDownload(); 
					else
						GoGDownload();
				}
				else if ((currState & (uint)EItemState.k_EItemStateInstalled) != 0) {
					downloadResult = EResult.k_EResultOK;
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

			public void Remove() {
				if (steamUser)
					SteamUGC.UnsubscribeItem(itemID);
				else
					RemoveGoG();
			}

			private void RemoveGoG() {
				// Remove the files
				Directory.Delete(GetInstallInfo().installPath, true);
				// Cleanup acf file
				string acfPath = Path.Combine(Directory.GetCurrentDirectory(), "steamapps", "workshop", "appworkshop_" + thisApp.ToString() + ".acf");
			}

			private uint GetState() {
				if (steamUser)
					return SteamUGC.GetItemState(itemID);
				else
					return SteamGameServerUGC.GetItemState(itemID);
			}
		}
	}
}
