using ExampleMod.Content.Items.Weapons;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	/// <summary>
	/// This the class that clones the vanilla Meowmere projectile using CloneDefaults().
	/// Make sure to check out <see cref="ExampleCloneWeapon" />, which fires this projectile; it itself is a cloned version of the Meowmere.
	/// </summary>
	public class ExampleCloneProjectile : ModProjectile
	{
		public override void SetDefaults() {
			// This method right here is the backbone of what we're doing here; by using this method, we copy all of
			// the Meowmere Projectile's SetDefault stats (such as projectile.friendly and projectile.penetrate) on to our projectile,
			// so we don't have to go into the source and copy the stats ourselves. It saves a lot of time and looks much cleaner;
			// if you're going to copy the stats of a projectile, use CloneDefaults().

			Projectile.CloneDefaults(ProjectileID.Meowmere);

			// To further the Cloning process, we can also copy the ai of any given projectile using AIType, since we want
			// the projectile to essentially behave the same way as the vanilla projectile.
			AIType = ProjectileID.Meowmere;

			// After CloneDefaults has been called, we can now modify the stats to our wishes, or keep them as they are.
			// For the sake of example, lets make our projectile penetrate enemies a few more times than the vanilla projectile.
			// This can be done by modifying projectile.penetrate
			Projectile.penetrate += 3;
		}

		// While there are several different ways to change how our projectile could behave differently, lets make it so
		// when our projectile finally dies, it will explode into 4 regular Meowmere projectiles.
		public override void OnKill(int timeLeft) {
			Vector2 launchVelocity = new Vector2(-4, 0); // Create a velocity moving the left.
			for (int i = 0; i < 4; i++) {
				// Every iteration, rotate the newly spawned projectile by the equivalent 1/4th of a circle (MathHelper.PiOver4)
				// (Remember that all rotation in Terraria is based on Radians, NOT Degrees!)
				launchVelocity = launchVelocity.RotatedBy(MathHelper.PiOver4);

				// Spawn a new projectile with the newly rotated velocity, belonging to the original projectile owner. The new projectile will inherit the spawning source of this projectile.
				Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center, launchVelocity, ProjectileID.Meowmere, Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
			}
		}

		// Now, using CloneDefaults() and aiType doesn't copy EVERY aspect of the projectile. In Vanilla, several other methods
		// are used to generate different effects that aren't included in AI. For the case of the Meowmere projectile, since the
		// ricochet sound is not included in the AI, we must add it ourselves:
		public override bool OnTileCollide(Vector2 oldVelocity) {
			// Since there are two ricochet sounds for the Meowmere, we can randomly choose between them like this:

			SoundEngine.PlaySound(Main.rand.NextBool() ? SoundID.Item57 : SoundID.Item58, Projectile.position);

			// Essentially, using ? and : is a glorified and shortened method of creating a simple if statement in
			// a single line. If Main.rand.NextBool() returns true, it plays SoundID.Item57. If it returns false, then it
			// will play SoundID.Item58. The condition goes before the ? and the two possibilities follow, separated by a :

			// This line calls the base (empty) implementation of this hook method to return its default value, which in its case is always 'true'.
			// Hover on the method below in VS to see its summary.
			return base.OnTileCollide(oldVelocity);
		}
	}
}
