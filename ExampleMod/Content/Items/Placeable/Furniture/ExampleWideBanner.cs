using ExampleMod.Content.Tiles.Furniture;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable.Furniture;

/// <summary>
/// Item that places <see cref="ExampleWideBannerTile"/>
/// </summary>
public class ExampleWideBanner : ModItem {
   public override void SetDefaults() {
      Item.DefaultToPlaceableTile(ModContent.TileType<ExampleWideBannerTile>());
      Item.value = Terraria.Item.buyPrice(copper: 10);
   }
}