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

		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone) {
			if (projectile.owner != 255) {
				HasBeenHitByPlayer = true;
			}
		}

		public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone) {
			HasBeenHitByPlayer = true;
		}

		//If the merchant has been hit by a player, they will double their sell price
		public override void ModifyActiveShop(NPC npc, string shopName, Item[] items) {
			if (!npc.GetGlobalNPC<ExampleGlobalNPC>().HasBeenHitByPlayer) {
				return;
			}

			foreach (Item item in items) {
				int value = item.shopCustomPrice ?? item.value;
				item.shopCustomPrice = value * 2;
			}
		}

		// Example of adding new items with complex conditions in the Merchant shop.
		public override void ModifyShop(NPCShop shop) {
			// Style 1 check for application
			if (shop.FullName != NPCShopDatabase.GetShopName(NPCID.Merchant, "Shop"))
				return;

			// Style 2 check for application
			if (shop.NpcType != NPCID.Merchant || shop.Name != "Shop")
				return;

			// Style 3 check for application (works just if NPC has one shop)
			if (shop.NpcType != NPCID.Merchant)
				return;

			// Adding ExampleTorch to Merchant, with condition being sold only during daytime. Have it appear just after Torch
			shop.InsertAfter(ItemID.Torch, ModContent.ItemType<Items.Placeable.ExampleTorch>(), NPCShop.Condition.TimeDay);

			// Hiding Copper Pickaxe and Copper Axe. They will never appear in Merchant shop anymore
			// However, this approach may fail if item doesn't exists in shop.
			shop.GetEntry(ItemID.CopperAxe).Disable();

			// Safer approach for disabling item
			if (shop.TryGetEntry(ItemID.CopperPickaxe, out NPCShop.Entry entry)) {
				entry.Disable();
			}

			// Adding new Condition to Blue Flare. Now it will appear just if player carries a Flare Gun in their inventory AND is in Snow biome
			shop.GetEntry(ItemID.BlueFlare).AddCondition(NPCShop.Condition.InSnowBiome);

			// Custom condition, opposite of conditions for ExampleItem above.
			var redPotCondition = new NPCShop.Condition(NetworkText.FromKey("Mods.ExampleMod.ShopConditions.RedPotCondition"), () => !NPCShop.Condition.HappyWindyDay.IsAvailable() || !NPCShop.Condition.HappyEnough.IsAvailable());

			// Let's add an item that appears just during Windy day and when NPC is happy enough (can sell pylons)
			// If condition is fulfilled, add an item to the shop.
			shop.Add(ModContent.ItemType<ExampleItem>(), NPCShop.Condition.HappyWindyDay, NPCShop.Condition.HappyEnough);

			// Otherwise, if condition is not fulfilled, then let's check if its For The Worthy world and then sell Red Potion.
			shop.Add(ItemID.RedPotion, redPotCondition, NPCShop.Condition.ForTheWorthy);
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