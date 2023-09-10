using Microsoft.CodeAnalysis;
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
	public override void SetStaticDefaults() {
		new ShimmerTransformation<ExampleShimmerableProjectile>()
			.AddResult(new MultiGrenadeShimmerResult(9999, 10))
			.AddNPCResult(NPCID.Frog, 2)
			.Register(Type);
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

	private const float maxShimmerTime = 150;
	private float shimmerTime = maxShimmerTime;

	public override void AI() {
		if (Projectile.shimmerWet) {
			shimmerTime--;
			ShimmerTransformation<ExampleShimmerableProjectile>.TryModShimmer(this);
		}
	}

	public bool CanShimmer() {
		return shimmerTime < 0;
	}

	public void Remove(int amount) {
		Projectile.active = false;
		Projectile.netUpdate = true;
	}

	public override Color? GetAlpha(Color lightColor) {
		return Color.Lerp(Color.Red, lightColor, MathF.Max((maxShimmerTime - shimmerTime) / maxShimmerTime, 0));
	}

	/// <inheritdoc cref="TypedShimmerResult(int, int)"/>
	public record class MultiGrenadeShimmerResult(int Damage, int Knockback) : TypedShimmerResult(ProjectileID.Grenade, 5)
	{
		public override bool IsSameResultType(ModShimmerResult result) {
			return result is MultiGrenadeShimmerResult;
		}

		public override void Spawn(IModShimmerable shimmerable, int allowedStack, List<IModShimmerable> spawned) {
			int spawnTotal = Count * allowedStack;
			while (spawnTotal > 0) {
				Projectile projectile = Projectile.NewProjectileDirect(shimmerable.GetSource_ForShimmer(), shimmerable.Center, shimmerable.Velocity + GetShimmerSpawnVelocityModifier(), Type, Damage, Knockback);
				projectile.position -= projectile.Size / 2;
				projectile.hostile = false;
				projectile.friendly = true;
				projectile.netUpdate = true;

				//spawned.Add(newNPC); TODO: ¯\_(ツ)_/¯ could be an issue?
				spawnTotal--;
			}
		}
	}
}
