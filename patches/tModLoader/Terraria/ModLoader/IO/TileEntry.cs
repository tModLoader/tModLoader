using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	internal class TileEntry : ModEntry
	{
		internal bool frameImportant;
		internal string unloadedType;
		
		internal TileEntry(ushort id, string modName, string name, ushort fallbackID, bool frameImportant, string unloadedType) : base (id, modName, name, fallbackID) {
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
			frameImportant = tag.Get<bool>("frameI");
			unloadedType = tag.Get<string>("uType");
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
