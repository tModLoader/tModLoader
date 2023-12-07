using ExampleMod.Content.Items.Armor.Vanity;
using ExampleMod.Content.NPCs.MinionBoss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Consumables
{
	// This file showcases how to use the CanOpen hook using a simple bag/item mechanic similar to the Lock Boxes and Golden Keys in the vanilla game.
	public class ExampleKeyBag : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 3;
		}

		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.consumable = true;
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.Purple;
		}

		public override bool CanRightClick() {
			return true;
		}

		public override bool CanOpen(Player player) {
			// Finds if this player has the example golden key in its inventory.
			return player.HasItemInAnyInventory(ModContent.ItemType<ExampleGoldenKey>());
		}

		public override void OnOpen(Player player) {
			player.ConsumeItem(ModContent.ItemType<ExampleGoldenKey>(), true, true);
		}

		public override void ModifyItemLoot(ItemLoot itemLoot) {
			// We have to replicate the expert drops from MinionBossBody here

			itemLoot.Add(ItemDropRule.NotScalingWithLuck(ItemID.GoldBar, 4));
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<ExampleItem>(), 1, 12, 16));
			itemLoot.Add(ItemDropRule.Coins(5000, true));
		}
	}
}
