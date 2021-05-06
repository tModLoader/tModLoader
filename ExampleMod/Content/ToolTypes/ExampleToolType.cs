using Terraria.ModLoader;
using Terraria.ID;
using Terraria;
using System;

namespace ExampleMod.Content.ToolTypes
{
	public class ExampleToolType : ToolType
	{
		public override void SetupContent() {
			//Makes it so tools that have this ToolType have a tooltip of 'X% example power'.
			ToolName.SetDefault("% example power");
		}

		// Makes it so this ToolType's behavior runs after the Vanilla block mining ones.
		public override ToolPriority Priority => ToolPriority.MineBlock;

		// Use this to tell the game this ToolType affects blocks, so it properly checks if those blocks can be affected.
		public override bool AffectsBlocks => true;

		// Use this to prevent this ToolType from mining specific tiles. This ToolType will only mine snow and ice (but not slush).
		// Note that there are other restrictions applied depending on a ToolType's priority.
		public override bool CanUseTool(Player player, Item item, Tile tile, int x, int y) {
			return TileID.Sets.IcesSnow[tile.type];
		}

		// Use this to program how this ToolType behaves, whether it is mining tiles or something else.
		// This one will mine tiles in a 3x3 area, pushed 1 tile away from the player.
		public override bool UseTool(Player player, Item item, Tile tile, int x, int y, int power) {
			// First we get the direction from the player's center, in tiles.
			int dirX = x - (int)player.Center.X / 16;
			int dirY = y - (int)player.Center.Y / 16;

			// Then we decide the direction in which to push the area, based on the character's height and width, so it's centered when the selected tile isn't diagonal from the player.
			if (Math.Abs(dirX) > Math.Ceiling(player.width / 2f / 16f))
				dirX = dirX > 0 ? 1 : -1;
			else
				dirX = 0;

			if (Math.Abs(dirY) > Math.Ceiling(player.height / 2f / 16f))
				dirY = dirY > 0 ? 1 : -1;
			else
				dirY = 0;

			// Lastly we mine each tile in the area
			for (int i = x + dirX - 1; i <= x + dirX + 1; i++) {
				for (int j = y + dirY - 1; j <= y + dirY + 1; j++) {
					Tile otherTile = Framing.GetTileSafely(i, j);

					// For a tool that mines tiles in an area, we need to properly check if those tiles can be mined.
					// To do this we call ToolTypeLoader.CanUseTool, which returns a bool value depending on whether mods allow the tile to be mined and the tool to mine it.
					// Make sure this is skipped for the original x and y coordinates, as this already ran for those.
					if ((i != x || j != y) && !ToolTypeLoader.CanUseTool(this, player, item, otherTile, i, j))
						continue;

					// Note that we are calling the vanilla method for mining with a pickaxe.
					// This has the advantage of handling all mining logic and being compatible with mods.
					// However, it also means that mining resistances from tiles that apply to pickaxes will apply to this ToolType too.
					player.PickTile(i, j, power, item);
				}
			}

			// We return true to tell the game this ToolType did something. This stops other ToolTypes with lower priority from running their code.
			// This is required if you want there to be a delay between each hit (use time).
			return true;
		}
	}
}