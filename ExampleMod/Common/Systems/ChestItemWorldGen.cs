using ExampleMod.Content.Items.Mounts;
using ExampleMod.Content.Pets.ExampleLightPet;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExampleMod.Common.Systems
{
	// This class showcases adding additional items to vanilla chests.
	public class ChestItemWorldGen : ModSystem
	{
		private bool _bossChestRewardAdded;

		public override void ClearWorld() {
			_bossChestRewardAdded = false;
		}

		public override void SaveWorldData(TagCompound tag) {
			if (_bossChestRewardAdded)
				tag["bossChestRewardAdded"] = true;
		}

		public override void LoadWorldData(TagCompound tag) {
			_bossChestRewardAdded = tag.ContainsKey("bossChestRewardAdded");
		}

		// We use PostWorldGen for this because we want to ensure that all chests have been placed before adding items.
		public override void PostWorldGen() {
			for (int chestIndex = 0; chestIndex < Main.maxChests; chestIndex++) {
				Chest chest = Main.chest[chestIndex];
				if (chest == null)
					continue;

				if (chest.type == ChestID.Frozen) {
					// Add an item while keeping the chest's original contents.
					chest.AddItem(ModContent.ItemType<ExampleMountItem>(), condition: () => WorldGen.genRand.NextBool(3));
					// Replace all Ice Torches and remove up to five Snowballs if those items are present.
					chest.ReplaceItem(ItemID.IceTorch, ItemID.PinkJellyfishJar);
					chest.RemoveItem(ItemID.Snowball, 5);
				}
				else if (chest.type == ChestID.LockedShadow) {
					// Completely replace this chest's generated loot with custom contents.
					chest.ClearItems();
					chest.AddItem(ModContent.ItemType<ExampleMountItem>());
					chest.AddItem(ModContent.ItemType<ExampleLightPetItem>());
					chest.AddItem(ItemID.PinkJellyfishJar, 2, chanceNumerator: 1, chanceDenominator: 3);
				}
			}
		}

		// PostWorldGen runs before any boss has been defeated, so boss-gated chest loot belongs in a world-update hook.
		public override void PostUpdateWorld() {
			if (_bossChestRewardAdded || !NPC.downedBoss1)
				return;

			for (int chestIndex = 0; chestIndex < Main.maxChests; chestIndex++) {
				Chest chest = Main.chest[chestIndex];
				if (chest == null || chest.type != ChestID.LockedShadow)
					continue;

				// The condition is checked when AddItem runs; the saved flag prevents granting it again after reload.
				if (chest.AddItem(ModContent.ItemType<ExampleLightPetItem>(), condition: () => NPC.downedBoss1)) {
					_bossChestRewardAdded = true;
					break;
				}
			}
		}
	}
}
