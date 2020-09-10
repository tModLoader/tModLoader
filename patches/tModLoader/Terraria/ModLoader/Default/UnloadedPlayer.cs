using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	public class UnloadedPlayer : ModPlayer
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

		public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath) {
			if (AprilFools.CheckAprilFools()) {
				Item item = new Item();
				item.SetDefaults(mod.ItemType("AprilFools"));
				return new List<Item> { item };
			}

			return Enumerable.Empty<Item>();
		}
	}
}
