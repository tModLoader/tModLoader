using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	internal class TileEntry : ModEntry
	{
		internal bool frameImportant { set; get; }

		public TileEntry() { }

		internal override void LoadData(TagCompound tag) {
			id = tag.Get<ushort>("id");
			modName = tag.Get<string>("mod");
			name = tag.Get<string>("name");
			fallbackID = tag.Get<ushort>("fallbackID");
			unloadedType = tag.Get<string>("uType");
			frameImportant = tag.Get<bool>("frameI");
		}

		public override TagCompound Save() {
			return new TagCompound {
				["id"] = id,
				["mod"] = modName,
				["name"] = name,
				["fallbackID"] = fallbackID,
				["frameI"] = frameImportant,
				["uType"] = unloadedType
			};
		}
	}
}
