using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Consumables
{
	public class ExampleCratePotion : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 20;

			// Dust that will appear in these colors when the item with ItemUseStyleID.DrinkLiquid is used
			ItemID.Sets.DrinkParticleColors[Type] = [
				new Color(240, 240, 240),
				new Color(200, 200, 200),
				new Color(140, 140, 140)
			];
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 26;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
			Item.useAnimation = 15;
			Item.useTime = 15;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item3;
			Item.maxStack = Item.CommonMaxStack;
			Item.consumable = true;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(silver: 8);
			Item.buffType = ModContent.BuffType<Buffs.ExampleCrateBuff>(); // Specify an existing buff to be applied when used.
			Item.buffTime = 3 * 60 * 60; // The amount of time the buff declared in Item.buffType will last in ticks. Set to 3 minutes, as 60 ticks = 1 second.
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.CratePotion, 4)
				.AddTile(TileID.CrystalBall)
				.Register();
		}
	}
}
