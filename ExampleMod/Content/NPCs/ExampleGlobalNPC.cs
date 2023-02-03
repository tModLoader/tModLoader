using ExampleMod.Content.Items;
using ReLogic.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExampleMod.Content.NPCs
{
	public class ExampleGlobalNPC : GlobalNPC
	{
		// TODO, npc.netUpdate when this changes, and GlobalNPC gets a SendExtraAI hook
		public bool HasBeenHitByPlayer;

		public override bool InstancePerEntity => true;

		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) {
			// after ModNPC has run (lateInstantiation), check if the entity is a townNPC
			return lateInstantiation && entity.townNPC;
		}

		public override void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit) {
			if (projectile.owner != 255) {
				HasBeenHitByPlayer = true;
			}
		}

		public override void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit) {
			HasBeenHitByPlayer = true;
		}

		//If the merchant has been hit by a player, they will double their sell price
		public override void ModifyActiveShop(NPC npc, string shopId, Item[] items) {
			if (!npc.GetGlobalNPC<ExampleGlobalNPC>().HasBeenHitByPlayer) {
				return;
			}

			foreach (Item item in items) {
				int value = item.shopCustomPrice ?? item.value;
				item.shopCustomPrice = value * 2;
			}
		}

		// Example of adding new items with complex conditions in the Merchant shop.
		public override void SetupShop(string shopId, ChestLoot shopContents) {
			// Style 1 check for application
			if (shopId != TMLLootDatabase.GetNPCShopName(NPCID.Merchant, "Shop"))
				return;

			// Style 2 check for application
			if (shopId != "Terraria/Merchant/Shop")
				return;

			// Creaate a condition for later use.
			var redPotCondition = new ChestLoot.Condition(NetworkText.FromLiteral("Describe what your condition is!"), () => !ChestLoot.Condition.HappyWindyDay.IsAvailable() || !ChestLoot.Condition.HappyEnough.IsAvailable());

			// Let's add an item that appears just during Windy day and when NPC is happy enough (can sell pylons)
			// If condition is fulfilled, add an item to the shop.
			shopContents.Add(ModContent.ItemType<ExampleItem>(), ChestLoot.Condition.HappyWindyDay, ChestLoot.Condition.HappyEnough);

			// Otherwise, if condition is not fulfilled, then let's check if its For The Worthy world and then sell Red Potion.
			shopContents.Add(ItemID.RedPotion, redPotCondition, ChestLoot.Condition.ForTheWorthy);
		}

		// PostSetupShop hook is best when it comes to modifying existing items.
		public override void PostSetupShop(string shopId, ChestLoot shopContents) {
			if (shopId != TMLLootDatabase.GetNPCShopName(NPCID.Merchant, "Shop"))
				return;

			// Adding ExampleTorch to Merchant, with condition being sold only during daytime. Have it appear just after Torch
			shopContents.InsertAfter(ItemID.Torch, ModContent.ItemType<Items.Placeable.ExampleTorch>(), ChestLoot.Condition.TimeDay);

			// Hiding Copper Pickaxe and Copper Axe. They will never appear in Merchant shop anymore
			shopContents.Hide(ItemID.CopperPickaxe);
			shopContents.Hide(ItemID.CopperAxe);

			// Adding new Condition to Blue Flare. Now it appers just if player carries a Flare Gun in their inventory AND is in Snow biome
			shopContents[ItemID.BlueFlare].AddCondition(ChestLoot.Condition.InSnowBiome);
		}

		public override void SaveData(NPC npc, TagCompound tag) {
			if (HasBeenHitByPlayer) {
				tag.Add("HasBeenHitByPlayer", true);
			}
		}

		public override void LoadData(NPC npc, TagCompound tag) {
			HasBeenHitByPlayer = tag.ContainsKey("HasBeenHitByPlayer");
		}
	}
}