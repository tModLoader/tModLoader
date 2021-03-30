using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using ExampleMod.Content.ToolTypes;

namespace ExampleMod.Content.Items.Tools
{
	public class ExampleCustomTool : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a custom tool.");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults() {
			Item.damage = 30;
			Item.DamageType = DamageClass.Melee;
			Item.width = 40;
			Item.height = 40;
			Item.useTime = 13;
			Item.useAnimation = 13;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 6f;
			Item.value = Item.buyPrice(gold: 25);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			//This tool acts as an axe, with a tool power of 20. Note that axes will display power * 5 (100 in this case) on their tooltip.
			Item.ToolPower[ToolType.Axe] = 20;
			//This tool uses custom behavior for tiles that axes can't mine. Check ExampleToolType for more info.
			Item.ToolPower[ModContent.GetInstance<ExampleToolType>()] = 45;
		}

		public override void MeleeEffects(Player player, Rectangle hitbox) {
			if (Main.rand.NextBool(10)) {
				Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, ModContent.DustType<Sparkle>());
			}
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
