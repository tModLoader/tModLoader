using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExampleMod.Content.NPCs.MinionBoss;

namespace ExampleMod.Content.Items.Consumables
{
	public class MinionBossBag : ModItem
	{
		//Sets the associated NPC this treasure bag is dropped from
		public override int BossBagNPC => ModContent.NPCType<MinionBossBody>();

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Treasure Bag");
			Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}"); //References a language key that says "Right Click To Open" in the language of the game
		}

		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.consumable = true;
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.Purple;
			Item.expert = true;
		}

		public override bool CanRightClick() {
			return true;
		}

		public override void OpenBossBag(Player player) {
			player.TryGettingDevArmor();

			//We have to replicate the expert drops from MinionBossBody here via QuickSpawnItem
			player.QuickSpawnItem(ModContent.ItemType<ExampleItem>(), Main.rand.Next(12, 16));
		}
	}
}
