using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steamworks;
using Terraria.Social;

namespace Terraria.ModLoader.Engine
{
	internal class Steam
	{
		public const uint TMLAppID = 1281930;
		public const uint TerrariaAppID = 105600;

		public static AppId_t TMLAppID_t = new AppId_t(TMLAppID);
		public static AppId_t TerrariaAppID_t = new AppId_t(TerrariaAppID);
		public static bool IsSteamApp => SocialAPI.Mode == SocialMode.Steam && SteamAPI.Init() && SteamApps.BIsAppInstalled(new AppId_t(TMLAppID));

		public static bool EnsureSteamAppIdTMLFile()
		{
			bool exists = File.Exists("steam_appid.txt");
			File.WriteAllText("steam_appid.txt", TMLAppID.ToString());
			return exists;
		}

		public static bool EnsureSteamAppIdTerrariaFile()
		{
			bool exists = File.Exists("steam_appid.txt");
			File.WriteAllText("steam_appid.txt", TerrariaAppID.ToString());
			return exists;
		}
	}
}
