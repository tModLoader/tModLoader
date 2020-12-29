using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	internal class UnloadedInfo
	{
		public string modName;
		public string name;

		public UnloadedInfo(string modName, string name) {
			this.modName = modName;
			this.name = name;
		}

		public TagCompound Save() {
			var tag = new TagCompound {
				["mod"] = modName,
				["name"] = name
			};
			return tag;
		}
	}
}
