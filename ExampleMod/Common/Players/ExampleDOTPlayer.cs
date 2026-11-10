using ExampleMod.Content.Buffs;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	public class ExampleDOTPlayer : ModPlayer
	{
		public bool amplifyDebuffs;

		public override void ResetEffects() {
			amplifyDebuffs = false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (amplifyDebuffs) {
				// The debuff carries the amplification; it does not need a lasting reference to its attacker.
				target.AddBuff(ModContent.BuffType<ExampleDOTAmplification>(), 300);
			}
		}
	}
}
