using ReLogic.OS;
using System.IO;
using Steamworks;
using Terraria.Social;

namespace Terraria.ModLoader.Engine
{
	internal class Steam
	{
		public const uint TMLAppID = 1281930;
		public const uint TerrariaAppID = 105600;

		public static AppId_t TMLAppID_t = new AppId_t(TMLAppID);
		public static AppId_t TerrariaAppId_t = new AppId_t(TerrariaAppID);

		private static bool SteamAPIRegistered = SteamAPI.Init() && SteamApps.BIsAppInstalled(new AppId_t(TMLAppID));
		public static bool IsSteamApp { 
			get => SocialAPI.Mode == SocialMode.Steam && SteamAPIRegistered;
		}

		public static ulong lastAvailableSteamCloudStorage = ulong.MaxValue;

		public static bool CheckSteamCloudStorageSufficient(ulong input) {
			if (SocialAPI.Cloud != null)
				return input < lastAvailableSteamCloudStorage;
			return true;
		}

		// Called in PostSocialInitialize and just before files get sent to cloud
		public static void RecalculateAvailableSteamCloudStorage() {
			if (SocialAPI.Cloud != null)
				SteamRemoteStorage.GetQuota(out _, out lastAvailableSteamCloudStorage);
		}

		public static string GetSteamTerrariaInstallDir() {
			SteamApps.GetAppInstallDir(TerrariaAppId_t, out string terrariaInstallLocation, 1000);
			if (Platform.IsOSX) {
				terrariaInstallLocation = Path.Combine(terrariaInstallLocation, "Terraria.app" + Path.DirectorySeparatorChar + "Contents" + Path.DirectorySeparatorChar + "Resources");
			}

			return terrariaInstallLocation;
		}

		public static string GetSteamTMLInstallDir() {
			SteamApps.GetAppInstallDir(TMLAppID_t, out string tmlInstallDIr, 1000);
#if MAC
			tmlInstallDIr = Path.Combine(tmlInstallDIr, "tModLoader.app/Contents/MacOS");
#endif
			return tmlInstallDIr;
		}

		public static bool EnsureSteamAppIdTMLFile() {
			bool exists = File.Exists("steam_appid.txt");
			File.WriteAllText("steam_appid.txt", TMLAppID.ToString());
			return exists;
		}

		public static bool EnsureSteamAppIdTerrariaFile() {
			bool exists = File.Exists("steam_appid.txt");
			File.WriteAllText("steam_appid.txt", TerrariaAppID.ToString());
			return exists;
		}
	}
}
