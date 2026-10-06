using ExampleMod.Content.Walls;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	// Showcases preventing the player from seeing or modifying wires under specific wall or condition.
	// Vanilla uses this to prevent wires from being seen or modified in the Jungle Temple until Golem is defeated.
	public class ExampleCanDoAndShowWireStuffHerePlayer : ModPlayer
	{
		public override bool? CanDoWireStuffHere(int i, int j) {
			Tile tile = Main.tile[i, j];

			// Prevents the player from modifying wires placed on ExampleWall.
			if (tile.WallType == ModContent.WallType<ExampleWall>())
				return false;

			// All other cases follow vanilla rules.
			return null;
		}
		public override bool? CanShowWireStuffHere(int i, int j) {
			Tile tile = Main.tile[i, j];

			// Prevents the player from seeing wires on ExampleWall.
			if (tile.WallType == ModContent.WallType<ExampleWall>())
				return false;

			// Allows the player to see wires within the Jungle Temple if at low health, bypassing the normal check for downedGolem.
			if (Player.statLife < Player.statLifeMax2 / 4 && tile.WallType == WallID.LihzahrdBrickUnsafe)
				return true;

			// All other cases follow vanilla rules.
			return null;
		}
	}
}
