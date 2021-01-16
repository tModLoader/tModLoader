using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	internal class UnloadedWallInfo : UnloadedInfo 
	{
		
		public UnloadedWallInfo(string modName, string name, ushort fallbackType = 0) :base(modName, name,fallbackType) {
			this.modName = modName;
			this.name = name;
			FallbackType = fallbackType;
		}

		public new TagCompound Save() {
			var tag = new TagCompound {
				["mod"] = modName,
				["name"] = name,
				["fallbackType"] = FallbackType,
			};
			return tag;
		}
	}
}
