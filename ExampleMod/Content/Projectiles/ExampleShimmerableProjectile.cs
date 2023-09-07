using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles;

public class ExampleShimmerableProjectile : ModProjectile, IModShimmerable
{
	public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.ThornBall}";
	public override void Load() {
		ShimmerTransformation.AddAsKnownType<ExampleShimmerableProjectile>();
	}

	public override void SetStaticDefaults() {
		new ShimmerTransformation<ExampleShimmerableProjectile>()
			.AddResult(new ThornBallShimmerResult(9999, 1))
			.AddNPCResult(NPCID.Frog, 2)
			.Register();
	}

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

	private const float maxShimmerTime = 300;
	private float shimmerTime = maxShimmerTime;

	public override void AI() {
		if (Projectile.shimmerWet) {
			shimmerTime--;
		}
	}

	public void Remove(int amount) {
		Projectile.active = false;
	}

	public override Color? GetAlpha(Color lightColor) {
		return Color.Lerp(Color.Red, lightColor, shimmerTime / maxShimmerTime);
	}

	/// <inheritdoc cref="TypedShimmerResult(int, int)"/>
	public record class ThornBallShimmerResult(int Damage, int Knockback) : TypedShimmerResult(ProjectileID.ThornBall, 1)
	{
		public override bool IsSameResultType(ModShimmerResult result) {
			return result is ThornBallShimmerResult r && r.Type == Type;
		}

		public override void Spawn(IModShimmerable shimmerable, int allowedStack, List<IModShimmerable> spawned) {
			int spawnTotal = Count * allowedStack;
			while (spawnTotal > 0) {
				Projectile.NewProjectileDirect(shimmerable.GetSource_ForShimmer(), shimmerable.Center, shimmerable.Velocity, Type, Damage, Knockback);
				//spawned.Add(newNPC); TODO: ¯\_(ツ)_/¯
				spawnTotal--;
			}
		}
	}
}
