using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExamplePlanterBox : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Content.Tiles.ExamplePlanterBox>());
			Item.width = 12;
			Item.height = 12;
			Item.value = Item.buyPrice(silver: 1);
		}
	}
}