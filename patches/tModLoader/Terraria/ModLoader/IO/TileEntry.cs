using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	internal class TileEntry : ModEntry
	{
		internal bool frameImportant;
		
		internal TileEntry(ushort id, string modName, string name, ushort fallbackID, bool frameImportant, string unloadedType) : base (id, modName, name, fallbackID, unloadedType) {
			this.id = id;
			this.modName = modName;
			this.name = name;
			this.fallbackID = fallbackID;
			this.frameImportant = frameImportant;
			this.unloadedType = unloadedType;
		}

		internal TileEntry(TagCompound tag) : base(tag) {
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
