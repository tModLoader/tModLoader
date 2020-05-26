using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Social;
using Terraria.Utilities;

namespace Terraria.ModLoader.x64bit
{
	static class WorldIO64
	{
		internal static void MoveToCloud(string localPath, string cloudPath) {
			localPath = Path.ChangeExtension(localPath, ".twldexchest");
			cloudPath = Path.ChangeExtension(cloudPath, ".twldexchest");
			if (File.Exists(localPath)) {
				FileUtilities.MoveToCloud(localPath, cloudPath);
			}
		}
		//add to end of Terraria.IO.WorldFileData.MoveToLocal
		internal static void MoveToLocal(string cloudPath, string localPath) {
			cloudPath = Path.ChangeExtension(cloudPath, ".twldexchest");
			localPath = Path.ChangeExtension(localPath, ".twldexchest");
			if (FileUtilities.Exists(cloudPath, true)) {
				FileUtilities.MoveToLocal(cloudPath, localPath);
			}
		}

		internal static void EraseWorld(string path, bool cloudSave) {
			path = Path.ChangeExtension(path, ".twldexchest");
			if (!cloudSave) {
				if (PlatformUtilities.IsWindows) {
					FileOperationAPIWrapper.MoveToRecycleBin(path);
				}
				else {
					File.Delete(path);
				}


			}
			else if (SocialAPI.Cloud != null) {
				SocialAPI.Cloud.Delete(path);
			}
		}
	}
}
