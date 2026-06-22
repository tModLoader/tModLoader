using ExampleMod.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	// Example Kite is a adaptation of a basic vanilla kite. The projectile has the rest of the code to customize the kite further.
	public class ExampleKite : ModItem
	{
		public override void SetStaticDefaults() {
			ItemID.Sets.IsAKite[Type] = true;
			ItemID.Sets.PlaceTileOnAltUse[Type] = true;
			ItemID.Sets.SortingPriorityToolsKites[Type] = 5; // Sort this kite with other kites, at the same order as yellow kite after all the typical kites
			ItemID.Sets.HasAProjectileThatHasAUsabilityCheck[Type] = true;
			ItemID.Sets.HasRightFire[Type] = true;
		}


		public override void SetDefaults() {
			Item.DefaultToKite(ModContent.ProjectileType<ExampleKiteProjectile>());
		}

		public override bool CanUseItem(Player player) {
			return player.ownedProjectileCounts[Item.shoot] == 0;
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