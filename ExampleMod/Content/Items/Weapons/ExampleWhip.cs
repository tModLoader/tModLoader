using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExampleMod.Content.Projectiles;

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleWhip : ModItem
	{
		public override void SetStaticDefaults() {
			Item.SacrificeTotal = 1;
		}

		public override void SetDefaults() {
			// This method quickly sets the whip's properties.
			// Mouse over to see its parameters.
			Item.DefaultToWhip(ModContent.ProjectileType<ExampleWhipProjectile>(), 20, 2, 4); 
			Item.rare = ItemRarityID.Green;
			Item.channel = true;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
		
		// Makes the whip receive melee prefixes
		public override bool MeleePrefix() {
			return true;
		}
	}
}
