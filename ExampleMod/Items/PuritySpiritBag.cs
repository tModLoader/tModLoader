using ExampleMod.Items.Armor;
using ExampleMod.NPCs.PuritySpirit;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class PuritySpiritBag : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Treasure Bag");
			Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}");
		}

		public override void SetDefaults() {
			item.maxStack = 999;
			item.consumable = true;
			item.width = 24;
			item.height = 24;
			item.rare = ItemRarityID.Purple;
			item.expert = true;
		}

		public override bool CanRightClick() {
			return true;
		}

		public override void OpenBossBag(Player player) {
			player.TryGettingDevArmor();
			player.TryGettingDevArmor();
			int choice = Main.rand.Next(7);
			if (choice == 0) {
				player.QuickSpawnItem(ModContent.ItemType<PuritySpiritMask>());
			}
			else if (choice == 1) {
				player.QuickSpawnItem(ModContent.ItemType<BunnyMask>());
			}
			if (choice != 1) {
				player.QuickSpawnItem(ItemID.Bunny);
			}
			player.QuickSpawnItem(ModContent.ItemType<PurityShield>());
		}

		public override int BossBagNPC => ModContent.NPCType<PuritySpirit>();
	}
}