using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExampleMod.Content.NPCs.MinionBoss;
using ExampleMod.Content.Items.Armor.Vanity;

namespace ExampleMod.Content.Items.Consumables
{
	//Basic code for a boss treasure bag
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
			Item.expert = true; //This makes sure that "Expert" displays in the tooltip and the item name color changes
		}

		public override bool CanRightClick() {
			return true;
		}

		public override void OpenBossBag(Player player) {
			if (Main.tenthAnniversaryWorld) { //Because this bag belongs to a pre-HM boss, we have to include this check
				//Using a particular secret seed world grants doubled chance on dev sets (handled inside TryGettingDevArmor) even for pre-HM bosses
				player.TryGettingDevArmor();
			}

			//We have to replicate the expert drops from MinionBossBody here via QuickSpawnItem
			if (Main.rand.NextBool(7)) {
				player.QuickSpawnItem(ModContent.ItemType<MinionBossMask>());
			}

			player.QuickSpawnItem(ModContent.ItemType<ExampleItem>(), Main.rand.Next(12, 16));
		}
	}
}
