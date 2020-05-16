using ExampleMod.Items.Armor;
using ExampleMod.NPCs.PuritySpirit;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

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
				player.QuickSpawnItem(ItemType<PuritySpiritMask>());
			}
			else if (choice == 1) {
				player.QuickSpawnItem(ItemType<BunnyMask>());
			}
			if (choice != 1) {
				player.QuickSpawnItem(ItemID.Bunny);
			}
			player.QuickSpawnItem(ItemType<PurityShield>());
		}

		public override int BossBagNPC => NPCType<PuritySpirit>();
	}
}