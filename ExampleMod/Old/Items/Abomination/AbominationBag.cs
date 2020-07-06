using ExampleMod.Items.Armor;
using ExampleMod.Items.Weapons;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;

namespace ExampleMod.Items.Abomination
{
	public class AbominationBag : ModItem
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
			item.rare = ItemRarityID.Cyan;
			item.expert = true;
		}

		public override bool CanRightClick() {
			return true;
		}

		public override void OpenBossBag(Player player) {
			player.TryGettingDevArmor();
			if (Main.rand.NextBool(7)) {
				player.QuickSpawnItem(ItemType<AbominationMask>());
			}
			player.QuickSpawnItem(ItemType<MoltenDrill>());
			player.QuickSpawnItem(ItemType<ElementResidue>());
			player.QuickSpawnItem(ItemType<PurityTotem>());
			player.QuickSpawnItem(ItemType<SixColorShield>());
		}

		public override int BossBagNPC => NPCType<NPCs.Abomination.Abomination>();
	}
}