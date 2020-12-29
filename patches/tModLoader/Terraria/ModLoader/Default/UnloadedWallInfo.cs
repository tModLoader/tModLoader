using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	internal class UnloadedWallInfo : UnloadedInfo 
	{
		public UnloadedWallInfo(string modName, string name):base(modName, name) {
			this.modName = modName;
			this.name = name;
		}

		public new TagCompound Save() {
			var tag = new TagCompound {
				["mod"] = modName,
				["name"] = name,
			};
			return tag;
		}
	}
}
