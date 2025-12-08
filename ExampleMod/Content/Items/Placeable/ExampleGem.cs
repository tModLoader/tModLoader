using ExampleMod.Content.Tiles;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	// This is an example of a gem item. Gems are involved in many different pieces of content, but this example will focus on content that requires unique examples rather than attempting to mirror all vanilla gem content.

	// The following aspects are implemented:
	// - ExampleExposedGem is an example of placed gems, it has unique tile framing logic

	// The following aspects are not demonstrated but can be implemented by learning from other examples:
	// - Gem terrain tile (gem inside stone tile)
	// - Gem stash pile tile and corresponding rubblemaker support
	// - Shimmer transmutation
	// - Gem trees and Gemcorn (will be implemented later when supported)
	// - Gem locks and Large Gem item
	// - Gemstone cave worldgen support
	// - Extractinator output support
	// - Other content (Gem critters, torches, staff, hook, phasesaber/blade, robe (see Player.hasGemRobe), gemspark block, stained glass)
	public class ExampleGem : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<ExampleExposedGem>(), 0);
			Item.alpha = 50;
			Item.value = 7500;
		}
	}
}
