using ExampleMod.Content.NPCs;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles;

public class ExampleComplexCustomShimmerable : ModProjectile
{
	public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.ThornBall}";
	public override void SetStaticDefaults() {
		CreateShimmerTransformation()
			.AddResult(new MultiGrenadeShimmerResult(9999, 10))
			.AddNPCResult(NPCID.Frog, 2)
			.AddModifyShimmerCallBack(NPCShimmerShowcase.ModifyShimmer_GildFrogs)
			.DisableChainedShimmers()
			.Register();
	}

	public override void SetDefaults() {
		// Duplicate Plantera's thorn ball, but friendly
		Projectile.CloneDefaults(ProjectileID.ThornBall);
		AIType = ProjectileID.ThornBall;
		Projectile.hostile = false;
		Projectile.friendly = true;
	}
	private float maxTransformTime;
	public override void AI() { // Does not let Projectile.shimmerTransformTime decrement
		maxTransformTime = Projectile.shimmerTransformTime = MathF.Max(maxTransformTime, Projectile.shimmerTransformTime);
	}
	public override Color? GetAlpha(Color lightColor) {
		return Color.Lerp(Color.Red, lightColor, Projectile.shimmerTransformTime);
	}

	/// <inheritdoc cref="ProjectileShimmerResult.ProjectileShimmerResult(int, int, int, bool, bool, int)"/>
	//TODO: Better behaviour here
	public record class MultiGrenadeShimmerResult(int Damage, int Knockback) : ProjectileShimmerResult(ProjectileID.Grenade, Damage, Knockback, true, false, 5)
	{
		public override IEnumerable<IModShimmerable> SpawnFrom(IModShimmerable shimmerable, int allowedStack) {
			int spawnTotal = Count * allowedStack;
			Player closestPlayer = Main.player[Player.FindClosest(shimmerable.Center, 1, 1)];

			while (spawnTotal > 0) {
				Vector2 velocityMod = GetShimmerSpawnVelocityModifier();
				Projectile projectile = Projectile.NewProjectileDirect(shimmerable.GetSource_Shimmer(null), shimmerable.Center,
					shimmerable.Velocity + velocityMod + velocityMod * 2 * (Vector2.Normalize(closestPlayer.Center - shimmerable.Center)),
					Type, Damage, Knockback);
				projectile.position -= projectile.Size / 2;
				projectile.hostile = Hostile;
				projectile.friendly = Friendly;
				projectile.netUpdate = true;
				yield return projectile;

				spawnTotal--;
			}
		}
	}
}
