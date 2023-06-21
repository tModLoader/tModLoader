using ExampleMod.Content.Tiles;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalItems
{
	public class RubblemakerGlobalItem : GlobalItem
	{
		public override void AddToRubblemaker(FlexibleTileWand rubblemakerSmall, FlexibleTileWand rubblemakerMedium, FlexibleTileWand rubblemakerLarge)
		{
			rubblemakerSmall.AddVariations(ModContent.ItemType<Content.Items.Placeable.ExampleBlock>(), ModContent.TileType<ExampleRubbleSmall>(), 0, 1, 2, 3, 4, 5);
			rubblemakerMedium.AddVariations(ModContent.ItemType<Content.Items.Placeable.ExampleBlock>(), ModContent.TileType<ExampleRubbleMedium>(), 0, 1, 2, 3, 4, 5);
			rubblemakerLarge.AddVariations(ModContent.ItemType<Content.Items.Placeable.ExampleBlock>(), ModContent.TileType<ExampleRubbleLarge>(), 0, 1, 2, 3, 4, 5);
		}
	}
}
