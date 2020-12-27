using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExampleMod.Common.GlobalNPCs;

namespace ExampleMod.Common.GlobalProjectiles
{
	// Here is a class dedicated to showcasing projectile modifications 
	public class ExampleProjectileModifications : GlobalProjectile
	{
		public override bool InstancePerEntity => true;
		public override bool CloneNewInstances => true;
		public bool applyBuffOnHit;
		public bool sayTimesHitOnThirdHit;
		// These are set when the user specifies that they want a trail.
		private Color trailColor;
		private bool trailActive;

		// Here I have a method for setting the above fields.
		public void SetTrail(Color color) {
			trailColor = color;
			trailActive = true;
		}
		public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit) {
			if(sayTimesHitOnThirdHit) {
				ProjectileModificationGlobalNPC globalNPC = target.GetGlobalNPC<ProjectileModificationGlobalNPC>();
				if (globalNPC.timesHitByModifiedProjectiles % 3 == 0) {
					Main.NewText($"This NPC has been hit with a modified projectile {globalNPC.timesHitByModifiedProjectiles} times.");
				}
				target.GetGlobalNPC<ProjectileModificationGlobalNPC>().timesHitByModifiedProjectiles += 1;
			}

			if(applyBuffOnHit) {
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
