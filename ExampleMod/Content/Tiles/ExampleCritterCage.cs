using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles
{
	public class ExampleCritterCage : ModTile
	{
		public override void SetStaticDefaults() {
			// Here we just copy a bunch of values from the frog cage tile
			TileID.Sets.CritterCageLidStyle[Type] = TileID.Sets.CritterCageLidStyle[TileID.FrogCage]; // This is how vanilla draws the roof of the cage
			Main.tileFrameImportant[Type] = Main.tileFrameImportant[TileID.FrogCage];
			Main.tileLavaDeath[Type] = Main.tileLavaDeath[TileID.FrogCage];
			Main.tileSolidTop[Type] = Main.tileSolidTop[TileID.FrogCage];
			Main.tileTable[Type] = Main.tileTable[TileID.FrogCage];
			AdjTiles = [TileID.FrogCage, TileID.GoldFrogCage]; // Just in case another mod uses the frog cage to craft
			AnimationFrameHeight = 36;

			// We can copy the TileObjectData directly from an existing tile to copy changes, if any, made to the TileObjectData template the original tile copied from.
			// In this case, the original FrogCage tile is an exact copy of TileObjectData.StyleSmallCage, so either approach works here.
			TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.FrogCage, 0));
			// or TileObjectData.newTile.CopyFrom(TileObjectData.StyleSmallCage);
			TileObjectData.addTile(Type);

			// Since this tile is only used for a single item, we can reuse the item localization for the map entry.
			AddMapEntry(new Color(122, 217, 232), ModContent.GetInstance<ExampleCritterCageItem>().DisplayName);
		}

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			offsetY = 2; // From vanilla
			Main.critterCage = true; // Vanilla doesn't run the animation code for critters unless this is checked
		}

		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			Tile tile = Main.tile[i, j];
			// The GetSmallAnimalCageFrame method utilizes some math to stagger each individual tile. First the top left tile is found, then those coordinates are passed into some math to stagger an index into Main.snail2CageFrame
			// Main.frogCageFrame is used since we want the same animation, but if we wanted a different frame count or a different animation timing, we could write our own by adapting vanilla code and placing the code in AnimateTile
			int tileCageFrameIndex = TileDrawing.GetSmallAnimalCageFrame(i, j, tile.TileFrameX, tile.TileFrameY);
			frameYOffset = Main.frogCageFrame[tileCageFrameIndex] * AnimationFrameHeight;
		}

		// Here is an example of what manually implementing the critter cage frames for a unique animation would look like.
		// This is the worm cage animation code, which is the simplest vanilla example.
		// See the Main.AnimateTiles_CritterCages source code and https://terraria.wiki.gg/wiki/Cages#Action_patterns to work out how existing animations are implemented if you wish to implement a custom critter animation.
		/*
		public static int[] cageFrames = new int[Main.cageFrames];
		public static int[] cageFrameCounters = new int[Main.cageFrames];

		public override void AnimateTile(ref int frame, ref int frameCounter) {
			for (int i = 0; i < Main.cageFrames; i++) { // The for loop updates the frames for each of the staggered animations.
				cageFrameCounters[i]++;
				if (cageFrameCounters[i] < Main.rand.Next(30, 91)) // Adds randomness to the animation speed
					continue;

				cageFrameCounters[i] = 0;
				if (!Main.rand.NextBool(4))
					continue;

				// Initially the frame is in the 'O' animation, frames 0 - 8
				cageFrames[i]++;
				if (cageFrames[i] == 9 && Main.rand.NextBool(2))
					cageFrames[i] = 0; // 50% chance to restart the 'O' animation, otherwise proceed to '8' animation (frames 9 - 18)

				// Once the '8' animation is finished, either restart the '0' or '8' animation
				if (cageFrames[i] > 18) {
					if (Main.rand.NextBool(2))
						cageFrames[i] = 9;
					else
						cageFrames[i] = 0;
				}
			}
		}

		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			Tile tile = Main.tile[i, j];
			int tileCageFrameIndex = TileDrawing.GetSmallAnimalCageFrame(i, j, tile.TileFrameX, tile.TileFrameY);
			frameYOffset = cageFrames[tileCageFrameIndex] * AnimationFrameHeight;
		}
		*/
	}

	public class ExampleCritterCageItem : ModItem
	{
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.FrogCage);
			Item.createTile = ModContent.TileType<ExampleCritterCage>();
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.Terrarium)
				.AddIngredient(ModContent.ItemType<NPCs.ExampleCritterItem>())
				.SortAfterFirstRecipesOf(ItemID.FrogCage) // places the recipe right after vanilla frog cage recipe.
				.Register();
		}
	}
}