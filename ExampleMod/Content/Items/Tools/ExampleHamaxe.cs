using ExampleMod.Content.Dusts;
using ExampleMod.Content.Tiles.Furniture;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content.Items.Tools
{
	public class ExampleHamaxe : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded hamaxe.");
		}

		public override void SetDefaults() {
			item.damage = 25;
			item.melee = true;
			item.width = 40;
			item.height = 40;
			item.useTime = 15;
			item.useAnimation = 15;
			item.useStyle = ItemUseStyleID.Swing;
			item.knockBack = 6;
			item.value = 10000;
			item.rare = ItemRarityID.Green;
			item.UseSound = SoundID.Item1;
			item.autoReuse = true;

			item.axe = 30; //How much axe power the weapon has, note that the axe power displayed in-game is this value multiplied by 5
			item.hammer = 100; //How much hammer power the weapon has
		}

		public override void MeleeEffects(Player player, Rectangle hitbox) {
			if (Main.rand.NextBool(10)) {
				Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustType<Sparkle>());
			}
		}
		
		//Please see ExampleItem.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>(10)
				.AddTile<ExampleWorkbench>()
				.Register();
		}
	}
}