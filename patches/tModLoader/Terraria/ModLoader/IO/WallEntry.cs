using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	internal class WallEntry : ModEntry
	{
		internal WallEntry(ushort id, string modName, string name, ushort fallbackID, string unloadedType) : base(id, modName, name, fallbackID, unloadedType) {
			this.id = id;
			this.modName = modName;
			this.name = name;
			this.fallbackID = fallbackID;
			this.unloadedType = unloadedType;
		}

		internal WallEntry(TagCompound tag) : base(tag) {
			id = tag.Get<ushort>("id");
			modName = tag.Get<string>("mod");
			name = tag.Get<string>("name");
			fallbackID = tag.Get<ushort>("fallbackID");
			unloadedType = tag.Get<string>("uType");
		}

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
