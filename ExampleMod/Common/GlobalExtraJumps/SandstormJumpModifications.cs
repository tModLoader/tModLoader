using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalExtraJumps
{
	// Showcases modifying the extra jump from the Sandstorm in a Bottle
	public class SandstormJumpModifications : GlobalExtraJump
	{
		public override void ModifyJumpDuration(ExtraJump jump, Player player, ref float duration) {
			// Make the jump duration last for 2x longer than normal
			if (jump == ExtraJump.SandstormInABottle)
				duration *= 2f;
		}
	}
}
