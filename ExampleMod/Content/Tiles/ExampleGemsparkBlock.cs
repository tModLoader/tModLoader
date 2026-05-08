using ExampleMod.Content.Biomes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles
{
	// This is an example of a gemspark block tile. https://terraria.wiki.gg/wiki/Gemspark_Blocks
	// Gemspark blocks have a few special properties that are demonstrated in this ModTile:
	// - Gemspark blocks have "on" and "off" variants that are swapped between when hit by a wire signal. The "on" variant emits light and has a shiny appearance, while the "off" variant does not emit light and has a dull appearance. This example uses Autoload to load both variants and set up the relationship between them.
	// - Gemspark blocks have special framing rules that are handled in the TileFrame method. This special framing allows gemspark blocks to consider tiles in diagonal directions instead of just 4 directions, which is necessary for allowing the edges and corners of the dark outline to properly connect visually. Note that the sprite for this tile has extra frames in it for these additional framing options.
	// - Gemspark blocks have a custom waterfall style that is handled in the ChangeWaterfallStyle method. This allows the waterfall to visually fit with the gemspark block when the waterfall is flowing over it.
	public class ExampleGemsparkBlock : ModTile, ICustomAutoload
	{
		public static void Autoload(Mod mod) {
			// This code loads both the "on" and "off" variants of the gemspark block. The variants need to know the tile type of each other so that they can swap between each other when hit by a wire signal (see HitWire below).
			var onVariant = new ExampleGemsparkBlock(onVariant: true);
			var offVariant = new ExampleGemsparkBlock(onVariant: false);
			mod.AddContent(onVariant);
			mod.AddContent(offVariant);
			onVariant.otherVariantTileType = offVariant.Type;
			offVariant.otherVariantTileType = onVariant.Type;
		}

		bool isOnVariant;
		int otherVariantTileType;

		public override string Name => base.Name + (isOnVariant ? "On" : "Off");

		public ExampleGemsparkBlock(bool onVariant) {
			this.isOnVariant = onVariant;
		}

		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			if (isOnVariant) {
				// These are omitted for the "off" variant
				Main.tileLighted[Type] = true;
				Main.tileShine2[Type] = true;
			}

			// These relate to what tiles this tile will merge with
			Main.tileBrick[Type] = true;
			TileID.Sets.GemsparkFramingTypes[Type] = Type;
			TileID.Sets.ForcedDirtMerging[Type] = true;

			// This is necessary to avoid visual issues with half-blocks.
			TileID.Sets.AllBlocksWithSmoothBordersToResolveHalfBlockIssue[Type] = true;

			// Since the item that places this tile only places the "on" variant, we need to set the item drop here so the "off" variant will also drop the expected item.
			RegisterItemDrop(ModContent.ItemType<Items.Placeable.ExampleGemsparkBlock>());
			DustType = DustID.GemDiamond;
			VanillaFallbackOnModDeletion = isOnVariant ? TileID.DiamondGemspark : TileID.DiamondGemsparkOff;

			AddMapEntry(new Color(200, 200, 200));
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			// Do the special 8 way framing. Return false to prevent the game from doing its normal framing after this.
			Framing.SelfFrame8Way(i, j, Main.tile[i, j], resetFrame);
			return false;
		}

		public override void HitWire(int i, int j) {
			Tile tile = Main.tile[i, j];
			if (!tile.HasActuator) {
				// Gemspark blocks swap between the "on" and "off" variants when hit by a wire signal.
				tile.TileType = (ushort)otherVariantTileType;

				WorldGen.SquareTileFrame(i, j);
				NetMessage.SendTileSquare(-1, i, j);
			}
		}

		public override void ChangeWaterfallStyle(ref int style) {
			style = ModContent.GetInstance<ExampleWaterfallStyle>().Slot;
			// ExampleWaterfallStyle is not pink like this tile, this is just an example.
		}
	}
}