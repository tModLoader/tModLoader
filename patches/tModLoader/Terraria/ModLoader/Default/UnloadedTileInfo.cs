using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	internal class UnloadedTileInfo : UnloadedInfo 
	{
		public UnloadedTileInfo(string modName, string name, ushort fallbackType=0):base(modName, name,fallbackType) {
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
