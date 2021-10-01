using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	public class UnloadedPlayer : ModPlayer
	{
		internal IList<TagCompound> data;
		internal IList<TagCompound> unloadedResearch;

		public override void Initialize() {
			data = new List<TagCompound>();
			unloadedResearch = new List<TagCompound>();
		}

		public override void SaveData(TagCompound tag) {
			tag["list"] = data;
			tag["unloadedResearch"] = unloadedResearch;
		}

		public override void LoadData(IReadOnlyTagCompound tag) {
			PlayerIO.LoadModData(Player, tag.GetList<TagCompound>("list"));
			PlayerIO.LoadResearch(Player, tag.GetList<TagCompound>("unloadedResearch"));
		}

		public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath) {
			if (AprilFools.CheckAprilFools()) {
				return new List<Item> { new Item(ModContent.ItemType<AprilFools>()) };
			}

			return Enumerable.Empty<Item>();
		}
	}
}
