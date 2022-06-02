using ExampleMod.Content.Biomes;
using ExampleMod.Content.Tiles;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	public class ExampleChooseBestTorch : ModPlayer
	{
		// ChooseBestTorch gives you control over the type and style of a torch if it's manipulated by Torch God's Favor.
		// Technically, you can use this to manipulate the placement of other tiles if you're so inclined, which is why
		// we need to check if the player has the Torch God's Favor enabled here.
		public override bool? ChooseBestTorch(ref int type, ref int style) {
			// As per the comment above, we must check for whether Torch God's Favor is being used to ensure vanilla parity,
			// since we're modifying the tile that gets placed.
			bool torchGodsFavor = Player.UsingBiomeTorches && type == TileID.Torches && style == 0;

			// Check if the player is in any Example biome.
			bool inExampleSurface = Player.InModBiome(ModContent.GetInstance<ExampleSurfaceBiome>());
			bool inExampleUnderground = Player.InModBiome(ModContent.GetInstance<ExampleUndergroundBiome>());

			// If the player is in any Example biome, set the tile style to zero and the tile type to the *placeable* ExampleTorch tile.
			if (torchGodsFavor && (inExampleSurface || inExampleUnderground)) {
				type = ModContent.TileType<ExampleTorch>();
				style = 0;
			}

			// Since this method expects a bool? return value, we can return "true", "false", or "null".
			// Returning true causes the Torch God's Favor to always execute the vanilla behavior as well, which may cause your torch's style to get overridden.
			// Returning false causes the Torch God's Favor to never execute the vanilla behavior.
			// Returning null causes the Torch God's Favor to use vanilla's conditions for executing behavior.
			return false;
		}
	}
}