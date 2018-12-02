using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	public class MysteryPlayer : ModPlayer
	{
		internal IList<TagCompound> data;

		public override void Initialize() {
			data = new List<TagCompound>();
		}

		public override TagCompound Save() {
			return new TagCompound { ["list"] = data };
		}

		public override void Load(TagCompound tag) {
			PlayerIO.LoadModData(player, tag.GetList<TagCompound>("list"));
		}

		public override void LoadLegacy(BinaryReader reader) {
			var list = new List<TagCompound>();
			int count = reader.ReadUInt16();
			for (int k = 0; k < count; k++) {
				list.Add(new TagCompound {
					["mod"] = reader.ReadString(),
					["name"] = reader.ReadString(),
					["legacyData"] = reader.ReadBytes(reader.ReadUInt16())
				});
			}
			Load(new TagCompound { ["list"] = list });
		}

		public override void SetupStartInventory(IList<Item> items, bool mediumcoreDeath) {
			if (AprilFools.CheckAprilFools()) {
				Item item = new Item();
				item.SetDefaults(mod.ItemType("AprilFools"));
				items.Add(item);
			}
		}
	}
}
