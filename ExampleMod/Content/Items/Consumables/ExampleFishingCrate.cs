using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

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

		public override void RightClick(Player player) {
			// Drop the items here. Drops mirrored from pre-hm crates, with ExampleMod additions to the drop table
			var entitySource = player.GetSource_OpenItem(Type);

			// Drop a special weapon/accessory etc. specific to this crate's theme (i.e. Sky Crate dropping Fledgling Wings or Starfury)
			int guaranteedType = Main.rand.Next(new int[] {
				ModContent.ItemType<Accessories.ExampleBeard>(),
				ModContent.ItemType<Accessories.ExampleStatBonusAccessory>()
			});

			player.QuickSpawnItem(entitySource, guaranteedType);

			// Drop coins
			if (Main.rand.NextBool(4)) {
				player.QuickSpawnItem(entitySource, ItemID.GoldCoin, Main.rand.Next(5, 13));
			}

			// Drop pre-hm ores, with the addition of one from ExampleMod
			if (Main.rand.NextBool(7)) {
				int oreType = Main.rand.Next(new int[] {
					ItemID.CopperOre,
					ItemID.TinOre,
					ItemID.IronOre,
					ItemID.LeadOre,
					ItemID.SilverOre,
					ItemID.TungstenOre,
					ItemID.GoldOre,
					ItemID.PlatinumOre,
					ModContent.ItemType<Placeable.ExampleOre>()
				});

				player.QuickSpawnItem(entitySource, oreType, Main.rand.Next(30, 50));
			}

			// Drop pre-hm bars (except copper/tin), with the addition of one from ExampleMod
			if (Main.rand.NextBool(4)) {
				int barType = Main.rand.Next(new int[] {
					ItemID.IronBar,
					ItemID.LeadBar,
					ItemID.SilverBar,
					ItemID.TungstenBar,
					ItemID.GoldBar,
					ItemID.PlatinumBar,
					ModContent.ItemType<Placeable.ExampleBar>()
				});

				player.QuickSpawnItem(entitySource, barType, Main.rand.Next(10, 21));
			}

			// Drop an "exploration utility" potion, with the addition of one from ExampleMod
			if (Main.rand.NextBool(4)) {
				int potionType = Main.rand.Next(new int[] {
					ItemID.ObsidianSkinPotion,
					ItemID.SpelunkerPotion,
					ItemID.HunterPotion,
					ItemID.GravitationPotion,
					ItemID.MiningPotion,
					ItemID.HeartreachPotion,
					ModContent.ItemType<Consumables.ExampleBuffPotion>()
				});

				player.QuickSpawnItem(entitySource, potionType, Main.rand.Next(2, 5));
			}

			// Drop (pre-hm) resource potion
			if (Main.rand.NextBool(2)) {
				int resourcePotionType = Main.rand.NextBool() ? ItemID.HealingPotion : ItemID.ManaPotion;
				player.QuickSpawnItem(entitySource, resourcePotionType, Main.rand.Next(5, 18));
			}

			// Drop (high-end) bait
			if (Main.rand.NextBool(2)) {
				int baitType = Main.rand.NextBool() ? ItemID.JourneymanBait : ItemID.MasterBait;
				player.QuickSpawnItem(entitySource, baitType, Main.rand.Next(2, 7));
			}
		}
	}
}
