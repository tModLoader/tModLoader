using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExamplePressurePlate : ModItem
	{
		public override void SetStaticDefaults() {
			ItemID.Sets.SortingPriorityWiring[Type] = 87;

			// Note that code in ExampleRecipes.AddRecipeGroups shows how to add this item to the pressure plate recipe group (if it were a non-weighted pressure plate)
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.ExamplePressurePlate>());
			Item.rare = ItemRarityID.Blue;

			// Shows wires and actuators when held.
			Item.mech = true;
		}
	}
}
