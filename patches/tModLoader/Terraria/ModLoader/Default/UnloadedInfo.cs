using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	internal class UnloadedInfo
	{
		public string modName;
		public string name;
		internal ushort FallbackType;

		public UnloadedInfo(string modName, string name, ushort fallbackType = 0) {
			this.modName = modName;
			this.name = name;
			FallbackType = fallbackType;
		}

		public TagCompound Save() {
			var tag = new TagCompound {
				["mod"] = modName,
				["name"] = name,
				["fallbackType"] = FallbackType,
			};
			return tag;
		}
	}
}
