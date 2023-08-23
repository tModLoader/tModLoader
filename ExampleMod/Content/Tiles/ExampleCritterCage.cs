using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Tiles;

public class ExampleCritterCage : ModTile
{
	public override void SetStaticDefaults() {
		//Here we just copy a bunch of values from the frog cage
		TileID.Sets.CritterCageLidStyle[Type] = TileID.Sets.CritterCageLidStyle[TileID.FrogCage]; // This is how vanilla draws the roof of the cage
		Main.tileFrameImportant[Type] = Main.tileFrameImportant[TileID.FrogCage];
		Main.tileLavaDeath[Type] = Main.tileLavaDeath[TileID.FrogCage];
		Main.tileSolidTop[Type] = Main.tileSolidTop[TileID.FrogCage];
		Main.tileTable[Type] = Main.tileTable[TileID.FrogCage];
		AdjTiles = new int[] { TileID.FrogCage, TileID.GoldFrogCage }; // Just in case another mod uses the frog cage to craft

		// These two lines are functionally identical, here we use the first to keep the style of copy from the frog, use the second if you're making an entirely new tile
		TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.FrogCage, 0));
		//TileObjectData.newTile.CopyFrom(TileObjectData.StyleSmallCage);

		TileObjectData.addTile(Type);
		AddMapEntry(new Color(122, 217, 232), CreateMapEntryName());
	}

	private static int tileCageFrameHolder;

	public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
		offsetY = 2; //From vanilla
		Main.critterCage = true; // Vanilla doesn't run the animation code for critters unless this is checked
		tileCageFrameHolder = TileDrawing.GetSmallAnimalCageFrame(i, j, tileFrameX, tileFrameY);
	}

	public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
		frameYOffset = Main.frogCageFrame[tileCageFrameHolder] * 36;
	}

	// The below code should still work, but the above code is less verbose and a little friendlier due to using vanilla code 
	//public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
	//	// This code utilizes some math to stagger each individual tile. First the top left tile is found, then those coordinates are passed into some math to stagger an index into Main.snail2CageFrame
	//	// Main.snail2CageFrame is used since we want the same animation, but if we wanted a different frame count or a different animation timing, we could write our own by adapting vanilla code and placing the code in AnimateTile
	//	Tile tile = Main.tile[i, j];
	//	Main.critterCage = true;
	//	int left = i - frameXOffset / 18;
	//	int top = j - frameYOffset / 18;
	//	int offset = left / 3 * (top / 3);
	//	offset %= Main.cageFrames;
	//	frameYOffset = Main.snail2CageFrame[offset] * AnimationFrameHeight;
	//}
}

public class ExampleCritterCageItem : ModItem
{
	public override void SetDefaults() {
		Item.CloneDefaults(TileID.FrogCage);
		Item.createTile = TileType<ExampleCritterCage>();
	}

	public override void AddRecipes() {
		CreateRecipe()
			.AddIngredient(ItemID.Terrarium)
			.AddIngredient(ItemType<NPCs.ExampleCritterItem>())
			.Register();
	}
}
