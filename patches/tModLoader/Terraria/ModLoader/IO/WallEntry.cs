using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	internal class WallEntry : ModEntry
	{
		public WallEntry() { }

		public override TagCompound Save() {
			return new TagCompound {
				["id"] = id,
				["mod"] = modName,
				["name"] = name,
				["fallbackID"] = fallbackID,
				["uType"] = unloadedType
			};
		}
	}
}
