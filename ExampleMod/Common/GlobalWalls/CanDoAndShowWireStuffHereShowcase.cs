using ExampleMod.Content.Walls;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalWalls
{
	// Showcases preventing the player from seeing or modifying wires under specific wall or condition.
	// Vanilla uses this to prevent wires from being seen or modified in the Jungle Temple until Golem is defeated.
	public class CanDoAndShowWireStuffHereShowcase : GlobalWall
	{
		public override bool? CanDoWireStuffHere(int i, int j, int type) {
			// Prevents the player from modifying wires placed on ExampleWallAdvanced.
			// This could be useful for a wall that is part of a puzzle or trap that the player shouldn't be able to bypass by removing wires. ModWall.KillWall would be used to prevent the player from breaking the wall to bypass this.
			if (type == ModContent.WallType<ExampleWallAdvanced>())
				return false;

			// All other cases follow vanilla rules.
			return null;
		}

		public override bool? CanShowWireStuffHere(int i, int j, int type) {
			// Prevents the player from seeing wires on ExampleWallAdvanced.
			if (type == ModContent.WallType<ExampleWallAdvanced>())
				return false;

			// Allows the player to see wires within the Jungle Temple if at low health, bypassing the normal check for downedGolem.
			if (Main.LocalPlayer.statLife < Main.LocalPlayer.statLifeMax2 / 4 && type == WallID.LihzahrdBrickUnsafe)
				return true;

			// All other cases follow vanilla rules.
			return null;
		}
	}
}
