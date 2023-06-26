using System.IO;

namespace Terraria.Social.Steam
{
	public partial class CoreSocialModule : ISocialModule
	{
		public const string WindowsSteamCloudPath = "C:\\Program Files (x86)\\Steam\\userdata";
		public const string LinuxSteamCloudPath = "~/.local/share/Steam/userdata";
		public const string MacSteamCloudPath = "~/Library/Application Support/Steam/userdata";

		private static void PortFilesToCurrentStructure() {
			// Note that if we want Steam to actually acknowledge the changes, we have to use the API calls for IREmoteStorage (the isCloud flag)
			Program.PortFilesMaster(GetCloudSaveLocation(), isCloud: true);
		}

		internal static string GetCloudSaveLocation() {
			// 512 is Windows Path max length
			Steamworks.SteamUser.GetUserDataFolder(out string steamCloudFolder, 512);

			// current steamCloudFolder looks like: C:\\Program Files (x86)\\Steam\\userdata\\SteamID OLD\\1281930\\local
			// So we have to do some path manipulation to get to C:\\Program Files (x86)\\Steam\\userdata\\SteamID OLD\\\\105600\\local
			return Path.Combine(Directory.GetParent(Directory.GetParent(steamCloudFolder).ToString()).ToString(), "105600", "remote");
		}
	}
}
