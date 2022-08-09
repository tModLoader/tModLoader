using Steamworks;
using System;
using Terraria.Localization;
using Terraria.ModLoader;

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
			SteamUGCDetails_t pDetails;
			if (SteamClient)
				SteamUGC.GetQueryUGCResult(handle, index, out pDetails);
			else
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
			else {
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
			} else {
				UGCQueryHandle_t qHandle = SteamGameServerUGC.CreateQueryUGCDetailsRequest(new PublishedFileId_t[1] { new PublishedFileId_t(publishId) }, 1);
				ModifyQueryHandle(ref qHandle, returnChildInfo: true, returnLongDesc: true);
				return SteamGameServerUGC.SendQueryUGCRequest(qHandle);
			}
		}

		public static SteamAPICall_t GenerateFullQuery(string queryCursor) {
			if (SteamClient) {
				UGCQueryHandle_t qHandle = SteamUGC.CreateQueryAllUGCRequest(EUGCQuery.k_EUGCQuery_RankedByTotalUniqueSubscriptions, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, new AppId_t(thisApp), new AppId_t(thisApp), queryCursor);
				ModifyQueryHandle(ref qHandle, returnKeyValueTags: true, returnPlaytimeStats: true);
				return SteamUGC.SendQueryUGCRequest(qHandle);
			}
			else {
				UGCQueryHandle_t qHandle = SteamGameServerUGC.CreateQueryAllUGCRequest(EUGCQuery.k_EUGCQuery_RankedByTotalUniqueSubscriptions, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, new AppId_t(thisApp), new AppId_t(thisApp), queryCursor);
				ModifyQueryHandle(ref qHandle, returnKeyValueTags: true, returnPlaytimeStats: true);
				return SteamGameServerUGC.SendQueryUGCRequest(qHandle);
			}
		}

		public static void FetchPlayTimeStats(UGCQueryHandle_t handle, uint index, out ulong hot, out ulong downloads) {
			if (SteamClient) {
				SteamUGC.GetQueryUGCStatistic(handle, index, EItemStatistic.k_EItemStatistic_NumUniqueSubscriptions, out downloads);
				SteamUGC.GetQueryUGCStatistic(handle, index, EItemStatistic.k_EItemStatistic_NumSecondsPlayedDuringTimePeriod, out hot); //Temp: based on how often being played lately?
			}
			else {
				SteamGameServerUGC.GetQueryUGCStatistic(handle, index, EItemStatistic.k_EItemStatistic_NumUniqueSubscriptions, out downloads);
				SteamGameServerUGC.GetQueryUGCStatistic(handle, index, EItemStatistic.k_EItemStatistic_NumSecondsPlayedDuringTimePeriod, out hot); //Temp: based on how often being played lately?
			}
		}

		public static void FetchPreviewImageUrl(UGCQueryHandle_t handle, uint index, out string modIconUrl) {
			if (SteamClient)
				SteamUGC.GetQueryUGCPreviewURL(handle, index, out modIconUrl, 1000);
			else
				SteamGameServerUGC.GetQueryUGCPreviewURL(handle, index, out modIconUrl, 1000);
		}
	}
}
