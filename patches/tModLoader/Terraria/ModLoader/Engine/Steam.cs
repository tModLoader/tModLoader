using ReLogic.OS;
using System.IO;
using Steamworks;
using Terraria.Social;
using System;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader.Engine
{
	internal class Steam
	{
		public const uint TMLAppID = 1281930;
		public const uint TerrariaAppID = 105600;

		public static AppId_t TMLAppID_t => new AppId_t(TMLAppID);
		public static AppId_t TerrariaAppId_t => new AppId_t(TerrariaAppID);

		public static ulong lastAvailableSteamCloudStorage = ulong.MaxValue;

		public static bool CheckSteamCloudStorageSufficient(ulong input) {
			if (SocialAPI.Cloud != null)
				return input < lastAvailableSteamCloudStorage;

			return true;
		}

		public static void RecalculateAvailableSteamCloudStorage() {
			if (SocialAPI.Cloud != null)
				SteamRemoteStorage.GetQuota(out _, out lastAvailableSteamCloudStorage);
		}

		internal static void PostInit() {
			RecalculateAvailableSteamCloudStorage();
			Logging.Terraria.Info($"Steam Cloud Quota: {UIMemoryBar.SizeSuffix((long)lastAvailableSteamCloudStorage)} available");
		}

		public static string GetSteamTerrariaInstallDir() {
			SteamApps.GetAppInstallDir(TerrariaAppId_t, out string terrariaInstallLocation, 1000);
			if (terrariaInstallLocation == null) {
				terrariaInstallLocation = "../Terraria"; // fallback for #2491
				Logging.Terraria.Warn($"Steam reports no terraria install directory. Falling back to {terrariaInstallLocation}");
			}
			if (Platform.IsOSX) {
				terrariaInstallLocation = Path.Combine(terrariaInstallLocation, "Terraria.app", "Contents", "Resources");
			}
			Logging.tML.Info("Terraria Steam Install Location assumed to be: " + terrariaInstallLocation);

			return terrariaInstallLocation;
		}

		internal static void SetAppId(AppId_t appId) {
			if (Environment.GetEnvironmentVariable("SteamClientLaunch") != "1") {
				File.WriteAllText("steam_appid.txt", appId.ToString());
			}
			else if (Environment.GetEnvironmentVariable("SteamAppId") != appId.ToString()) {
				throw new Exception("Cannot overwrite steam env. SteamAppId=" + Environment.GetEnvironmentVariable("SteamAppId"));
			}
		}
	}
}
