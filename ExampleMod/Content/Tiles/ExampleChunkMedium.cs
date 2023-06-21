using ExampleMod.Content.Items;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles
{
	public abstract class ExampleChunkMediumBase : ModTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = false;
			Main.tileSolidTop[Type] = false;
			Main.tileTable[Type] = false;
			Main.tileNoAttach[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;

			DustType = DustID.Stone;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.addTile(Type);

			TileID.Sets.DisableSmartCursor[Type] = true;
			AddMapEntry(new Color(152, 171, 198));
		}
	}

	// Ambient tile created by Rubblemaker
	public class ExampleChunkMediumRubble : ExampleChunkMediumBase {

		//Override texture path, shared with natural variant
		public override string Texture => "ExampleMod/Content/Tiles/ExampleChunkMedium";

		public override void SetStaticDefaults() {
			// Call to base SetStaticDefaults. Must inherit static defaults from base type 
			base.SetStaticDefaults();

			// Add rubble variant, all existing styles, to Rubblemaker, allowing to place this tile by consuming ExampleItems
			FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<ExampleItem>(), Type, 0, 1, 2, 3, 4, 5);
		}
	}

	// Natural ambient tile (i.e. created on WorldGen, not by Rubblemaker) 
	public class ExampleChunkMediumNatural : ExampleChunkMediumBase {

		//Override texture path, shared with rubble variant
		public override string Texture => "ExampleMod/Content/Tiles/ExampleChunkMedium";

		// Drops 2 ExampleItems on tile break
		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, ModContent.ItemType<ExampleItem>(), Stack: 2);
		}

		//Adds a 1/6 chance to drop worms 
		public override void DropCritterChance(int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance) {
			wormChance = 6;
		}
	}
}