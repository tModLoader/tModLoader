using ExampleMod.Content.Tiles;
using Terraria.Enums;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	/// <summary>
	/// The coupled item that places the Advanced Example Pylon tile. For more information on said tile,
	/// see <seealso cref="ExamplePylonTileAdvanced"/>.
	/// </summary>
	public class ExamplePylonItemAdvanced : ModItem
	{
		public override void SetDefaults() {
			// Basically, this a just a shorthand method that will set all default values necessary to place
			// the passed in tile type; in this case, the Advanced Example Pylon tile.
			Item.DefaultToPlaceableTile(ModContent.TileType<ExamplePylonTileAdvanced>());

			// Another shorthand method that will set the rarity and how much the item is worth.
			Item.SetShopValues(ItemRarityColor.LightRed4, Terraria.Item.buyPrice(gold: 20));
		}
	}
}
