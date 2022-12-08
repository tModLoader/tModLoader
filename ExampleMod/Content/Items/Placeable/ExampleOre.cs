using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExampleOre : ModItem
	{
		public override void SetStaticDefaults() {
			Item.SacrificeTotal = 100;
			ItemID.Sets.SortingPriorityMaterials[Item.type] = 58;
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.ExampleOre>());
			Item.width = 12;
			Item.height = 12;
			Item.value = 3000;
		}
	}
}