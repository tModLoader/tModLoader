using Microsoft.Xna.Framework;
using System;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles;
public class ExampleShimmerableProjectile : ModProjectile, IModShimmerable
{
	public override void SetDefaults() {
		// Duplicate Plantera's thorn ball, but friendly
		Projectile.CloneDefaults(ProjectileID.ThornBall);
		AIType = ProjectileID.ThornBall;
		Projectile.hostile = false;
		Projectile.friendly = true;
	}
	public Vector2 Center { get => Projectile.Center; set => Projectile.Center = value; }
	public Rectangle Hitbox { get => Projectile.Hitbox; set => Projectile.Hitbox = value; }
	public Vector2 Velocity { get => Projectile.velocity; set => Projectile.velocity = value; }

	public IEntitySource GetSource_ForShimmer() {
		return Projectile.GetSource_Misc("shimmer");
	}

	public override void AI() {
		if (Projectile.shimmerWet) {
		}
	}

	public void Remove(int amount) {
		throw new NotImplementedException();
	}
}
