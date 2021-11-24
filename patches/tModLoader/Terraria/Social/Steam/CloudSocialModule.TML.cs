using System.IO;
using System.Linq;
using Terraria.Utilities;

namespace Terraria.Social.Steam
{
	public partial class CloudSocialModule : Base.CloudSocialModule
	{
		/// <summary>
		/// This method is for migrating cloud files from 105600 to 1281930 in Steam.
		/// This method was introduced in Nov 2021.
		/// All original 105600 files not transitioned using this method can be found at:
		/// https://store.steampowered.com/account/remotestorageapp/?appid=105600
		/// </summary>
		/// <returns></returns>
		public bool MigrateFrom105600() {
			var files = GetFiles().Where(file => file.Contains("ModLoader")).ToList();

			if (files.Count == 0)
				return true;

			foreach (var item in files) {
				var bytes = Read(item);

				string target = Path.Combine(Main.SavePath, item.Split("ModLoader/")[1]);
				string dir = Path.GetDirectoryName(target);
				if(!Directory.Exists(dir))
					Directory.CreateDirectory(dir);

				if (!File.Exists(target)) 
					File.WriteAllBytes(target, bytes);

				Delete(item);
			}

			Utils.LogAndConsoleInfoMessage("Cloud files migrated from 105600 to Save File Directory");
			return true;
		}
	}
}
