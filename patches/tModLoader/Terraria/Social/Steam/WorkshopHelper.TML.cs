using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
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

		internal class ModDownloader {
			public ModDownloader(PublishedFileId_t itemID) {
				this.itemID = itemID;
				m_DownloadItemResult = Callback<DownloadItemResult_t>.Create(OnItemDownloaded);
			}

			PublishedFileId_t itemID;
			protected Callback<DownloadItemResult_t> m_DownloadItemResult;
			private bool downloadComplete = false;

			public static bool steamUser = true;

			public bool Download() {
				bool downloadStarted = false;
				if (steamUser)
					downloadStarted = SteamUGC.DownloadItem(itemID, false);
				else
					downloadStarted = SteamGameServerUGC.DownloadItem(itemID, false);

				if (downloadStarted) {
					ulong dlBytes = 0, totalBytes = 1;
					do {
						if (steamUser)
							SteamUGC.GetItemDownloadInfo(itemID, out dlBytes, out totalBytes);
						else
							SteamGameServerUGC.GetItemDownloadInfo(itemID, out dlBytes, out totalBytes);
						
						// Do stuff
					} while (dlBytes != totalBytes);
				}
				else {
					string error = "Download Failed";
				}

				return downloadStarted;
			}

			private void OnItemDownloaded(DownloadItemResult_t pCallback) {
				if (pCallback.m_eResult == EResult.k_EResultOK && pCallback.m_unAppID == ModLoader.Engine.Steam.TerrariaAppId_t && pCallback.m_nPublishedFileId == itemID) {
					downloadComplete = true;
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

			public static void Initialize() {
				if (!ModLoader.Engine.Steam.IsSteamApp) {
					steamUser = false;
					GameServer.Init(0x7f000001, 7776, 7775, 7774, EServerMode.eServerModeNoAuthentication, "0.11.9.0");
					SteamGameServer.LogOnAnonymous();
					SetInstallLocation("D:\\Other\\TempFileShare");
				}
			}
			
			public static bool SetInstallLocation(string installLocation) {
				return SteamGameServerUGC.BInitWorkshopForGameServer(new DepotId_t(1000), installLocation);
			}
		}
	}
}
