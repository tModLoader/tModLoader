using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	internal class UnloadedInfo
	{
		internal string modName;
		internal string name;
		internal ushort fallbackType;

		public UnloadedInfo(string modName, string name, ushort fallbackType = 0) {
			this.modName = modName;
			this.name = name;
			this.fallbackType = fallbackType;
		}

		public TagCompound Save() {
			var tag = new TagCompound {
				["mod"] = modName,
				["name"] = name,
				["fallbackType"] = fallbackType,
			};
			return tag;
		}
	}
}
