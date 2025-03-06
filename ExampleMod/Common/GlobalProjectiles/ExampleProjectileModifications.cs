using ExampleMod.Common.GlobalNPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalProjectiles
{
	// Here is a class dedicated to showcasing projectile modifications
	public class ExampleProjectileModifications : GlobalProjectile
	{
		public override bool InstancePerEntity => true;
		public bool applyBuffOnHit;
		public bool sayTimesHitOnThirdHit;
		// These are set when the user specifies that they want a trail.
		private Color trailColor;
		private bool trailActive;
		public static LocalizedText HitMessage { get; private set; }

		// Here, a method is provided for setting the above fields.
		public void SetTrail(Color color) {
			trailColor = color;
			trailActive = true;
		}

		public override void SetStaticDefaults() {
			HitMessage = Mod.GetLocalization($"{nameof(ExampleProjectileModifications)}.HitMessage");
		}

		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
			if (sayTimesHitOnThirdHit) {
				ProjectileModificationGlobalNPC globalNPC = target.GetGlobalNPC<ProjectileModificationGlobalNPC>();
				if (globalNPC.timesHitByModifiedProjectiles % 3 == 0) {
					Main.NewText(HitMessage.Format(globalNPC.timesHitByModifiedProjectiles));
				}
				target.GetGlobalNPC<ProjectileModificationGlobalNPC>().timesHitByModifiedProjectiles += 1;
			}

			if (applyBuffOnHit) {
				target.AddBuff(BuffID.OnFire, 50);
			}
		}

		public override void PostAI(Projectile projectile) {
			if (trailActive) {
				Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.TintableDustLighted, default, default, default, trailColor);
			}
		}
	}
}
