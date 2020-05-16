using System;
using System.Collections.Generic;
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
		public static bool IsSteamApp = SocialAPI.Mode == SocialMode.Steam && Steamworks.SteamAPI.Init() && SteamApps.BIsAppInstalled(new AppId_t(TMLAppID));
	}
}
