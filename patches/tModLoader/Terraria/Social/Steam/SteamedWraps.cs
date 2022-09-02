using ReLogic.OS;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.DownloadManager;

namespace Terraria.Social.Steam
{
	public static class SteamedWraps
	{
		internal const uint thisApp = ModLoader.Engine.Steam.TMLAppID;

		public static bool SteamClient { get; set; }
		public static bool FamilyShared { get; set; } = false;
		internal static bool SteamAvailable { get; set; }

		// Used to get the right token for fetching/setting localized descriptions from/to Steam Workshop
		internal static string GetCurrentSteamLangKey() {
			//TODO: Unhardcode this whenever the language roster is unhardcoded for modding.
			return (GameCulture.CultureName)LanguageManager.Instance.ActiveCulture.LegacyId switch {
				GameCulture.CultureName.German => "german",
				GameCulture.CultureName.Italian => "italian",
				GameCulture.CultureName.French => "french",
				GameCulture.CultureName.Spanish => "spanish",
				GameCulture.CultureName.Russian => "russian",
				GameCulture.CultureName.Chinese => "schinese",
				GameCulture.CultureName.Portuguese => "portuguese",
				GameCulture.CultureName.Polish => "polish",
				_ => "english",
			};
		}

		internal static void ReportCheckSteamLogs() {
			string workshopLogLoc = "";
			if (Platform.IsWindows)
				workshopLogLoc = "C:/Program Files (x86)/Steam/logs/workshop_log.txt";
			else if (Platform.IsOSX)
				workshopLogLoc = "~/Library/Application Support/Steam/logs/workshop_log.txt";
			else if (Platform.IsLinux)
				workshopLogLoc = "/home/user/.local/share/Steam/logs/workshop_log.txt";

			Utils.LogAndConsoleInfoMessage(Language.GetTextValue("tModLoader.ConsultSteamLogs", workshopLogLoc));
		}

		internal static void Initialize() {
			if (!FamilyShared && SocialAPI.Mode == SocialMode.Steam) {
				SteamAvailable = true;
				SteamClient = true;
				return;
			}

			// On some systems without steam, the native dependencies required for steam fail to load (eg docker without requisite glibc)
			// Thus, for dedicated servers we delay game-server init until someone tries to use steam features (eg mod browser)

			// Non-steam tModLoader will use the SteamGameServer to perform Browsing & Downloading
			if (!Main.dedServ && !TryInitViaGameServer())
				Logging.tML.Error("Steam Game Server failed to Init. Steam Workshop downloading on GoG is unavailable. Make sure Steam is installed");
		}

		public static bool TryInitViaGameServer() {
			ModLoader.Engine.Steam.SetAppId(ModLoader.Engine.Steam.TMLAppID_t);
			try {
				if (!GameServer.Init(0x7f000001, 7775, 7774, EServerMode.eServerModeNoAuthentication, "0.11.9.0"))
					return false;

				SteamGameServer.SetGameDescription("tModLoader Mod Browser");
				SteamGameServer.SetProduct(thisApp.ToString());
				SteamGameServer.LogOnAnonymous();
			}
			catch (DllNotFoundException e) {
				Logging.tML.Error(e);
				return false;
			}

			SteamAvailable = true;
			return true;
		}

		public static void ReleaseWorkshopHandle(UGCQueryHandle_t handle) {
			if (SteamClient)
				SteamUGC.ReleaseQueryUGCRequest(handle);
			else if (SteamAvailable)
				SteamGameServerUGC.ReleaseQueryUGCRequest(handle);
		}

		public static SteamUGCDetails_t FetchItemDetails(UGCQueryHandle_t handle, uint index) {
			SteamUGCDetails_t pDetails = new();
			if (SteamClient)
				SteamUGC.GetQueryUGCResult(handle, index, out pDetails);
			else if (SteamAvailable)
				SteamGameServerUGC.GetQueryUGCResult(handle, index, out pDetails);
			return pDetails;
		}

		public static PublishedFileId_t[] FetchItemDependencies(UGCQueryHandle_t handle, uint index, uint numChildren) {
			var deps = new PublishedFileId_t[numChildren];
			if (SteamClient)
				SteamUGC.GetQueryUGCChildren(handle, index, deps, numChildren);
			else if (SteamAvailable)
				SteamGameServerUGC.GetQueryUGCChildren(handle, index, deps, numChildren);
			return deps;
		}

		private static void ModifyQueryHandle(ref UGCQueryHandle_t qHandle, bool returnChildInfo = false, bool returnLongDesc = false, bool returnKeyValueTags = false, bool returnPlaytimeStats = false) {
			if (SteamClient) {
				SteamUGC.SetAllowCachedResponse(qHandle, 0); // Anything other than 0 may cause Access Denied errors.

				SteamUGC.SetLanguage(qHandle, GetCurrentSteamLangKey());
				SteamUGC.SetReturnLongDescription(qHandle, returnLongDesc);
				SteamUGC.SetReturnChildren(qHandle, returnChildInfo);
				SteamUGC.SetReturnKeyValueTags(qHandle, returnKeyValueTags);
				if (returnPlaytimeStats)
					SteamUGC.SetReturnPlaytimeStats(qHandle, 30); // Last 30 days of playtime statistics
			}
			else if (SteamAvailable) {
				SteamGameServerUGC.SetAllowCachedResponse(qHandle, 0); // Anything other than 0 may cause Access Denied errors.

				SteamGameServerUGC.SetLanguage(qHandle, GetCurrentSteamLangKey());
				SteamGameServerUGC.SetReturnLongDescription(qHandle, returnLongDesc);
				SteamGameServerUGC.SetReturnChildren(qHandle, returnChildInfo);
				SteamGameServerUGC.SetReturnKeyValueTags(qHandle, returnKeyValueTags);
				if (returnPlaytimeStats)
					SteamGameServerUGC.SetReturnPlaytimeStats(qHandle, 30); // Last 30 days of playtime statistics
			}
		}

		public static SteamAPICall_t GenerateSingleItemQuery(ulong publishId) {
			if (SteamClient) {
				UGCQueryHandle_t qHandle = SteamUGC.CreateQueryUGCDetailsRequest(new PublishedFileId_t[1] { new PublishedFileId_t(publishId) }, 1);
				ModifyQueryHandle(ref qHandle, returnChildInfo: true, returnLongDesc: true);
				return SteamUGC.SendQueryUGCRequest(qHandle);
			} else if (SteamAvailable) {
				UGCQueryHandle_t qHandle = SteamGameServerUGC.CreateQueryUGCDetailsRequest(new PublishedFileId_t[1] { new PublishedFileId_t(publishId) }, 1);
				ModifyQueryHandle(ref qHandle, returnChildInfo: true, returnLongDesc: true);
				return SteamGameServerUGC.SendQueryUGCRequest(qHandle);
			}

			return new();
		}

		public static SteamAPICall_t GenerateModBrowserQuery(string queryCursor) {
			if (SteamClient) {
				UGCQueryHandle_t qHandle = SteamUGC.CreateQueryAllUGCRequest(EUGCQuery.k_EUGCQuery_RankedByTotalUniqueSubscriptions, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, new AppId_t(thisApp), new AppId_t(thisApp), queryCursor);
				ModifyQueryHandle(ref qHandle, returnKeyValueTags: true, returnPlaytimeStats: true);
				return SteamUGC.SendQueryUGCRequest(qHandle);
			}
			else if (SteamAvailable) {
				UGCQueryHandle_t qHandle = SteamGameServerUGC.CreateQueryAllUGCRequest(EUGCQuery.k_EUGCQuery_RankedByTotalUniqueSubscriptions, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, new AppId_t(thisApp), new AppId_t(thisApp), queryCursor);
				ModifyQueryHandle(ref qHandle, returnKeyValueTags: true, returnPlaytimeStats: true);
				return SteamGameServerUGC.SendQueryUGCRequest(qHandle);
			}

			return new();
		}

		public static void FetchPlayTimeStats(UGCQueryHandle_t handle, uint index, out ulong hot, out ulong downloads) {
			if (SteamClient) {
				SteamUGC.GetQueryUGCStatistic(handle, index, EItemStatistic.k_EItemStatistic_NumUniqueSubscriptions, out downloads);
				SteamUGC.GetQueryUGCStatistic(handle, index, EItemStatistic.k_EItemStatistic_NumSecondsPlayedDuringTimePeriod, out hot); //Temp: based on how often being played lately?
			}
			else if (SteamAvailable){
				SteamGameServerUGC.GetQueryUGCStatistic(handle, index, EItemStatistic.k_EItemStatistic_NumUniqueSubscriptions, out downloads);
				SteamGameServerUGC.GetQueryUGCStatistic(handle, index, EItemStatistic.k_EItemStatistic_NumSecondsPlayedDuringTimePeriod, out hot); //Temp: based on how often being played lately?
			}
			else {
				hot = 0;
				downloads = 0;
			}
		}

		public static void FetchPreviewImageUrl(UGCQueryHandle_t handle, uint index, out string modIconUrl) {
			if (SteamClient)
				SteamUGC.GetQueryUGCPreviewURL(handle, index, out modIconUrl, 1000);
			else if (SteamAvailable)
				SteamGameServerUGC.GetQueryUGCPreviewURL(handle, index, out modIconUrl, 1000);
			else
				modIconUrl = null;
		}

		public static void FetchMetadata(UGCQueryHandle_t handle, uint index, out NameValueCollection metadata) {
			uint keyCount;
			metadata = new NameValueCollection();

			if (SteamClient)
				keyCount = SteamUGC.GetQueryUGCNumKeyValueTags(handle, index);
			else if (SteamAvailable)
				keyCount = SteamGameServerUGC.GetQueryUGCNumKeyValueTags(handle, index);
			else
				keyCount = 0;

			for (uint j = 0; j < keyCount; j++) {
				string key, val;

				if (SteamClient)
					SteamUGC.GetQueryUGCKeyValueTag(handle, index, j, out key, byte.MaxValue, out val, byte.MaxValue);
				else
					SteamGameServerUGC.GetQueryUGCKeyValueTag(handle, index, j, out key, byte.MaxValue, out val, byte.MaxValue);

				metadata[key] = val;
			}
		}

		public static void ForceCallbacks() {
			Thread.Sleep(5);

			if (SteamClient)
				SteamAPI.RunCallbacks();
			else if (SteamAvailable)
				GameServer.RunCallbacks();
		}

		public static void StopPlaytimeTracking() {
			// Call the appropriate variant
			if (SteamClient)
				SteamUGC.StopPlaytimeTrackingForAllItems();
			else if (SteamAvailable)
				SteamGameServerUGC.StopPlaytimeTrackingForAllItems();
		}

		private const int PlaytimePagingConst = 100; //https://partner.steamgames.com/doc/api/ISteamUGC#StartPlaytimeTracking

		public static void BeginPlaytimeTracking() {
			if (!SteamAvailable)
				return;

			List<PublishedFileId_t> list = new List<PublishedFileId_t>();
			foreach (var mod in ModLoader.ModLoader.Mods) {
				if (WorkshopHelper.GetPublishIdLocal(mod.File, out ulong publishId))
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
				if (SteamClient)
					SteamUGC.StartPlaytimeTracking(pgList.ToArray(), (uint)pgList.Count);
				else if (SteamAvailable)
					SteamGameServerUGC.StartPlaytimeTracking(pgList.ToArray(), (uint)pgList.Count);
			}
		}

		internal static void OnGameExitCleanup() {
			if (!SteamAvailable)
				return;

			if (SteamClient) {
				SteamAPI.Shutdown();
				return;
			}
			
			GameServer.Shutdown();
		}

		public static uint GetWorkshopItemState(PublishedFileId_t publishId) {
			if (SteamClient)
				return SteamUGC.GetItemState(publishId);
			else if (SteamAvailable)
				return SteamGameServerUGC.GetItemState(publishId);
			return 0;
		}

		public struct ItemInstallInfo
		{
			public string installPath;
			public uint lastUpdatedTime;
		}

		public static ItemInstallInfo GetInstallInfo(PublishedFileId_t publishId) {
			string installPath = null;
			uint lastUpdatedTime = 0;

			if (SteamClient)
				SteamUGC.GetItemInstallInfo(publishId, out var installSize, out installPath, 1000, out lastUpdatedTime);
			else if (SteamAvailable)
				SteamGameServerUGC.GetItemInstallInfo(publishId, out var installSize, out installPath, 1000, out lastUpdatedTime);

			return new ItemInstallInfo() { installPath = installPath, lastUpdatedTime = lastUpdatedTime };
		}

		public static void UninstallWorkshopItem(PublishedFileId_t publishId, string installPath = null) {
			if (string.IsNullOrEmpty(installPath))
				installPath = GetInstallInfo(publishId).installPath;

			if (!Directory.Exists(installPath))
				return;

			// Unsubscribe
			if (SteamClient)
				SteamUGC.UnsubscribeItem(publishId);

			// Remove the files
			Directory.Delete(installPath, true);

			if (!SteamClient)
				UninstallACF(publishId);
		}

		private static void UninstallACF(PublishedFileId_t publishId) {
			// Cleanup acf file by removing info on this itemID
			string acfPath = Path.Combine(Directory.GetCurrentDirectory(), "steamapps", "workshop", "appworkshop_" + SteamedWraps.thisApp.ToString() + ".acf");

			string[] acf = File.ReadAllLines(acfPath);
			using StreamWriter w = new StreamWriter(acfPath);

			int blockLines = 5;
			int skip = 0;

			for (int i = 0; i < acf.Length; i++) {
				if (acf[i].Contains(publishId.ToString())) {
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

		public static bool IsWorkshopItemInstalled(PublishedFileId_t publishId) {
			var currState = GetWorkshopItemState(publishId);

			bool installed = (currState & (uint)(EItemState.k_EItemStateInstalled)) != 0;
			bool downloading = (currState & ((uint)EItemState.k_EItemStateDownloading + (uint)EItemState.k_EItemStateDownloadPending)) != 0;
			return installed && !downloading;
		}

		public static bool DoesWorkshopItemNeedUpdate(PublishedFileId_t publishId) {
			var currState = SteamedWraps.GetWorkshopItemState(publishId);

			return (currState & (uint)EItemState.k_EItemStateNeedsUpdate) != 0 ||
				(currState == (uint)EItemState.k_EItemStateNone) ||
				(currState & (uint)EItemState.k_EItemStateDownloadPending) != 0;
		}

		/// <summary>
		/// Updates and/or Downloads the Item specified by publishId
		/// </summary>
		internal static void Download(PublishedFileId_t publishId, UIWorkshopDownload uiProgress = null, bool forceUpdate = false) {
			if (!SteamAvailable)
				return;

			if (SteamClient)
				SteamUGC.SubscribeItem(publishId);

			if (DoesWorkshopItemNeedUpdate(publishId) || forceUpdate) {
				Utils.LogAndConsoleInfoMessage(Language.GetTextValue("tModLoader.SteamDownloader"));

				bool downloadStarted;
				if (SteamClient)
					downloadStarted = SteamUGC.DownloadItem(publishId, true);
				else 
					downloadStarted = SteamGameServerUGC.DownloadItem(publishId, true);

				if (!downloadStarted) {
					ReportCheckSteamLogs();
					throw new ArgumentException("Downloading Workshop Item failed due to unknown reasons");
				}

				InnerDownloadHandler(uiProgress, publishId);

				Utils.LogAndConsoleInfoMessage(Language.GetTextValue("tModLoader.EndDownload"));
			}
			else {
				// A warning here that you will need to restart the game for item to be removed completely from Steam's runtime cache.
				Utils.LogAndConsoleErrorMessage(Language.GetTextValue("tModLoader.SteamRejectUpdate"));
			}
		}

		private static void InnerDownloadHandler(UIWorkshopDownload uiProgress, PublishedFileId_t publishId) {
			ulong dlBytes, totalBytes;

			const int LogEveryXPercent = 10;

			int nextPercentageToLog = LogEveryXPercent;

			while (!IsWorkshopItemInstalled(publishId)) {
				if (SteamClient)
					SteamUGC.GetItemDownloadInfo(publishId, out dlBytes, out totalBytes);
				else
					SteamGameServerUGC.GetItemDownloadInfo(publishId, out dlBytes, out totalBytes);

				if (uiProgress != null)
					uiProgress.UpdateDownloadProgress((float)dlBytes / Math.Max(totalBytes, 1), (long)dlBytes, (long)totalBytes);

				int percentage = (int)MathF.Round(dlBytes / (float)totalBytes * 100f);

				if (percentage >= nextPercentageToLog) {
					string str = Language.GetTextValue("tModLoader.DownloadProgress", percentage);

					Utils.LogAndConsoleInfoMessage(str);

					nextPercentageToLog = percentage + LogEveryXPercent;

					if (nextPercentageToLog > 100 && nextPercentageToLog != 100 + LogEveryXPercent) {
						nextPercentageToLog = 100;
					}
				}

				// This is a hack for #2887 in case IsWorkshopItemInstalled() fails for some odd reason?
				float progressRaw = dlBytes / totalBytes;
				if (progressRaw == 1) {
					break;
				}
			}
		}
	}
}
