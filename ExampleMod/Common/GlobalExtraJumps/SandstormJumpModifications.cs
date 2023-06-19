using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalExtraJumps
{
	// Showcases modifying the extra jump from the Sandstorm in a Bottle
	public class SandstormJumpModifications : GlobalExtraJump
	{
		public override void ModifyJumpDuration(ModExtraJump jump, Player player, ref float duration) {
			// Make the jump duration last for 2x longer than normal
			if (jump == ModExtraJump.SandstormInABottle)
				duration *= 2f;
		}
	}
}
