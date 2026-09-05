using ExampleMod.Content.Walls;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	// Showcases preventing the player from seeing or modifying wires under specific wall or condition.
	public class ExampleCanDoAndShowWireStuffHerePlayer : ModPlayer
	{
		public override bool? CanDoWireStuffHere(int x, int y) {
			Tile tile = Framing.GetTileSafely(x, y);

			// Allows the player to modify wires within the Jungle Temple regardless of conditions.
			if (tile.WallType == WallID.LihzahrdBrickUnsafe)
				return true;

			// Prevents the player from modifying wires placed on ExampleWall.
			if (tile.WallType == ModContent.WallType<ExampleWall>())
				return false;

			// All other cases follow vanilla rules.
			return null;
		}
		public override bool? CanShowWireStuffHere(int x, int y) {
			Tile tile = Framing.GetTileSafely(x, y);

			// Prevents the player from seeing wires on ExampleWall.
			if (tile.WallType == ModContent.WallType<ExampleWall>())
				return false;

			// Unlike the above function, this returns true to show all wires except those on ExampleWall.
			return true;
		}
	}
}
