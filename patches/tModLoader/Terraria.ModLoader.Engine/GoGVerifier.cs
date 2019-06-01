using ReLogic.OS;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace Terraria.ModLoader.Engine
{
	internal static class GoGVerifier
	{
		private static byte[] GoGHash = new byte[] { 185, 47, 36, 43, 200, 116, 221, 219, 240, 143, 112, 86, 97, 179, 19, 169 };

		private static bool? isGoG;
		public static bool IsGoG => isGoG ?? (isGoG = GoGCheck()).Value;

		private static bool GoGCheck() {
			if (!Platform.IsWindows)
				return false;

			if (File.Exists("steam_api.dll"))
				return false;

			var vanillaPath = Path.GetFileName(Assembly.GetExecutingAssembly().Location) != "Terraria.exe" ? "Terraria.exe" : "Terraria_v1.3.5.3.exe";
			if (!File.Exists(vanillaPath))
				return false;

			bool match;
			using (var md5 = MD5.Create())
			using (var stream = File.OpenRead(vanillaPath))
				match = GoGHash.SequenceEqual(md5.ComputeHash(stream));

			if (match)
				Logging.tML.Info("GoG detected. Disabled steam check.");

			return match;
		}
	}
}
