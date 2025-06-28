using ExampleMod.Content.Buffs;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Consumables
{
	/// <summary>
	/// A potion that applies the ExampleWeaponImbue buff to the player.
	/// See also ExampleWeaponImbue and ExampleWeaponEnchantmentPlayer.
	/// </summary>
	public class ExampleFlask : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 20;

			ItemID.Sets.DrinkParticleColors[Type] = [
				new Color(240, 240, 240),
				new Color(200, 200, 200),
				new Color(140, 140, 140)
			];
		}

		public override void SetDefaults() {
			Item.UseSound = SoundID.Item3;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
			Item.useTurn = true;
			Item.useAnimation = 17;
			Item.useTime = 17;
			Item.maxStack = Item.CommonMaxStack;
			Item.consumable = true;
			Item.width = 14;
			Item.height = 24;
			Item.buffType = ModContent.BuffType<ExampleWeaponImbue>();
			Item.buffTime = Item.flaskTime;
			Item.value = Item.sellPrice(0, 0, 5);
			Item.rare = ItemRarityID.LightRed;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.BottledWater)
				.AddIngredient<ExampleItem>(2)
				.AddTile(TileID.ImbuingStation)
				.Register();
		}
	}
}
