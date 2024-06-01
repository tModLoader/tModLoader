using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	// Showcases modifying the extra jump from the Sandstorm in a Bottle
	public class ExampleExtraJumpModificationPlayer : ModPlayer
	{
		public override void ModifyExtraJumpDurationMultiplier(ExtraJump jump, ref float duration) {
			// Make the jump duration last for 2x longer than normal
			if (jump == ExtraJump.SandstormInABottle)
				duration *= 2f;
		}
	}
}
