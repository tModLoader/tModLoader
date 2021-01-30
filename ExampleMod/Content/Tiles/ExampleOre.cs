using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace ExampleMod.Content.Tiles
{
	public class ExampleOre : ModTile
	{
		public override void SetDefaults() {
			TileID.Sets.Ore[Type] = true;
			Main.tileSpelunker[Type] = true; // The tile will be affected by spelunker highlighting
			Main.tileOreFinderPriority[Type] = 410; // Metal Detector value, see https://terraria.gamepedia.com/Metal_Detector
			Main.tileShine2[Type] = true; // Modifies the draw color slightly.
			Main.tileShine[Type] = 975; // How often tiny dust appear off this tile. Larger is less frequently
			Main.tileMergeDirt[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("ExampleOre");
			AddMapEntry(new Color(152, 171, 198), name);

			DustType = 84;
			ItemDrop = ModContent.ItemType<Items.Placeable.ExampleOre>();
			SoundType = SoundID.Tink;
			SoundStyle = 1;
			//mineResist = 4f;
			//minPick = 200;
		}
	}

	public class ExampleOreSystem : ModSystem
	{
		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight) {
			// Because world generation is like layering several images ontop of each other, we need to do some steps between the original world generation steps.

			// The first step is an Ore. Most vanilla ores are generated in a step called "Shinies", so for maximum compatibility, we will also do this.
			// First, we find out which step "Shinies" is.
			int ShiniesIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));

			if (ShiniesIndex != -1) {
				// Next, we insert our pass directly after the original "Shinies" pass. 
				// ExampleOrePass is a class seen bellow
				tasks.Insert(ShiniesIndex + 1, new ExampleOrePass("Example Mod Ores", 237.4298f));
			}
		}
	}

	public class ExampleOrePass : GenPass
	{
		public ExampleOrePass(string name, float loadWeight) : base(name, loadWeight) {
		}

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
			// progress.Message is the message shown to the user while the following code is running.
			// Try to make your message clear. You can be a little bit clever, but make sure it is descriptive enough for troubleshooting purposes. 
			progress.Message = "Example Mod Ores";

			// Ores are quite simple, we simply use a for loop and the WorldGen.TileRunner to place splotches of the specified Tile in the world.
			// "6E-05" is "scientific notation". It simply means 0.00006 but in some ways is easier to read.
			for (int k = 0; k < (int)(Main.maxTilesX * Main.maxTilesY * 6E-05); k++) {
				// The inside of this for loop corresponds to one single splotch of our Ore.
				// First, we randomly choose any coordinate in the world by choosing a random x and y value.
				int x = WorldGen.genRand.Next(0, Main.maxTilesX);

				// WorldGen.worldSurfaceLow is actually the highest surface tile. In practice you might want to use WorldGen.rockLayer or other WorldGen values.
				int y = WorldGen.genRand.Next((int)WorldGen.worldSurfaceLow, Main.maxTilesY);

				// Then, we call WorldGen.TileRunner with random "strength" and random "steps", as well as the Tile we wish to place.
				// Feel free to experiment with strength and step to see the shape they generate.
				WorldGen.TileRunner(x, y, WorldGen.genRand.Next(3, 6), WorldGen.genRand.Next(2, 6), ModContent.TileType<ExampleOre>());

				// Alternately, we could check the tile already present in the coordinate we are interested.
				// Wrapping WorldGen.TileRunner in the following condition would make the ore only generate in Snow.
				// Tile tile = Framing.GetTileSafely(x, y);
				// if (tile.active() && tile.type == TileID.SnowBlock)
				// {
				// 	WorldGen.TileRunner(.....);
				// }
			}
		}
	}
}