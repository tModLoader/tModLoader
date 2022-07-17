using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.GameContent.ItemDropRules;

namespace ExampleMod.Content.Items.Consumables
{
	// Basic code for a fishing crate
	// The catch code is in a separate ModPlayer class (ExampleFishingPlayer)
	// The placed tile is in a separate ModTile class
	public class ExampleFishingCrate : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Crate");
			Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}"); // References a language key that says "Right Click To Open" in the language of the game

			// Disclaimer for both of these sets (as per their docs): They are only checked for vanilla item IDs, but for cross-mod purposes it would be helpful to set them for modded crates too
			ItemID.Sets.IsFishingCrate[Type] = true;
			//ItemID.Sets.IsFishingCrateHardmode[Type] = true; // This is a crate that mimics a pre-hardmode biome crate, so this is commented out

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 10;
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.ExampleFishingCrate>());
			Item.width = 12; //The hitbox dimensions are intentionally smaller so that it looks nicer when fished up on a bobber
			Item.height = 12;
			Item.maxStack = 99;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(0, 2);
		}

		// TODO ExampleMod: apply this to all items where necessary (which are not automatically detected)
		public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.Crates;
		}

		public override bool CanRightClick() {
			return true;
		}

		public override void ModifyItemLoot(ItemLoot itemLoot) {
			// Drop a special weapon/accessory etc. specific to this crate's theme (i.e. Sky Crate dropping Fledgling Wings or Starfury)
			itemLoot.Add(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ModContent.ItemType<Accessories.ExampleBeard>(), ModContent.ItemType<Accessories.ExampleStatBonusAccessory>()));

			// Drop coins
			itemLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 4, 5, 13));

			// Drop pre-hm ores, with the addition of one from ExampleMod
			IItemDropRule[] oreTypes = new IItemDropRule[9];
			oreTypes[0] = new CommonDrop(ItemID.CopperOre, 1, 30, 50);
			oreTypes[1] = new CommonDrop(ItemID.TinOre, 1, 30, 50);
			oreTypes[2] = new CommonDrop(ItemID.IronOre, 1, 30, 50);
			oreTypes[3] = new CommonDrop(ItemID.LeadOre, 1, 30, 50);
			oreTypes[4] = new CommonDrop(ItemID.SilverOre, 1, 30, 50);
			oreTypes[5] = new CommonDrop(ItemID.TungstenOre, 1, 30, 50);
			oreTypes[6] = new CommonDrop(ItemID.GoldOre, 1, 30, 50);
			oreTypes[7] = new CommonDrop(ItemID.PlatinumOre, 1, 30, 50);
			oreTypes[8] = new CommonDrop(ModContent.ItemType<Placeable.ExampleOre>(), 1, 30, 50);
			itemLoot.Add(new OneFromRulesRule(7, oreTypes));

			// Drop pre-hm bars (except copper/tin), with the addition of one from ExampleMod
			IItemDropRule[] oreBars = new IItemDropRule[7];
			oreBars[0] = new CommonDrop(ItemID.IronBar, 1, 10, 21);
			oreBars[1] = new CommonDrop(ItemID.LeadBar, 1, 10, 21);
			oreBars[2] = new CommonDrop(ItemID.SilverBar, 1, 10, 21);
			oreBars[3] = new CommonDrop(ItemID.TungstenBar, 1, 10, 21);
			oreBars[4] = new CommonDrop(ItemID.GoldBar, 1, 10, 21);
			oreBars[5] = new CommonDrop(ItemID.PlatinumBar, 1, 10, 21);
			oreBars[6] = new CommonDrop(ModContent.ItemType<Placeable.ExampleBar>(), 1, 10, 21);
			itemLoot.Add(new OneFromRulesRule(4, oreBars));

			// Drop an "exploration utility" potion, with the addition of one from ExampleMod
			IItemDropRule[] explorationPotions = new IItemDropRule[7];
			explorationPotions[0] = new CommonDrop(ItemID.ObsidianSkinPotion, 1, 2, 5);
			explorationPotions[1] = new CommonDrop(ItemID.SpelunkerPotion, 1, 2, 5);
			explorationPotions[2] = new CommonDrop(ItemID.HunterPotion, 1, 2, 5);
			explorationPotions[3] = new CommonDrop(ItemID.GravitationPotion, 1, 2, 5);
			explorationPotions[4] = new CommonDrop(ItemID.MiningPotion, 1, 2, 5);
			explorationPotions[5] = new CommonDrop(ItemID.HeartreachPotion, 1, 2, 5);
			explorationPotions[6] = new CommonDrop(ModContent.ItemType<Consumables.ExampleBuffPotion>(), 1, 2, 5);
			itemLoot.Add(new OneFromRulesRule(4, explorationPotions));

			// Drop (pre-hm) resource potion
			IItemDropRule[] resourcePotions = new IItemDropRule[2];
			resourcePotions[0] = new CommonDrop(ItemID.HealingPotion, 1, 5, 18);
			resourcePotions[1] = new CommonDrop(ItemID.ManaPotion, 1, 5, 18);
			itemLoot.Add(new OneFromRulesRule(2, resourcePotions));

			// Drop (high-end) bait
			IItemDropRule[] highendBait = new IItemDropRule[2];
			highendBait[0] = new CommonDrop(ItemID.JourneymanBait, 1, 2, 7);
			highendBait[1] = new CommonDrop(ItemID.MasterBait, 1, 2, 7);
			itemLoot.Add(new OneFromRulesRule(2, highendBait));
		}
	}
}
