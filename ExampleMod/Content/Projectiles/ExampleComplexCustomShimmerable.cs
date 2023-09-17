using ExampleMod.Content.Items;
using ExampleMod.Content.NPCs;
using ExampleMod.Content.Tiles.Furniture;
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
			.AddResult(new EvilGrenadeShimmerResult(9999, 10))
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
		maxTransformTime = 0;
	}

	private float maxTransformTime;

	public override void AI() { // Does not let Projectile.shimmerTransformTime decrement
		maxTransformTime = Projectile.shimmerTransformTime = MathF.Max(maxTransformTime, Projectile.shimmerTransformTime);
	}

	public override Color? GetAlpha(Color lightColor) {
		return Color.Lerp(Color.Red, lightColor, Projectile.shimmerTransformTime);
	}

	/// <inheritdoc cref="ProjectileShimmerResult(int, int, int, bool, bool, int)"/>
	public record class EvilGrenadeShimmerResult(int Damage, int Knockback) : ProjectileShimmerResult(ProjectileID.Grenade, Damage, Knockback, true, false, 5)
	{
		public override IEnumerable<Projectile> SpawnFrom(IModShimmerable shimmerable, int allowedStack) {
			Player closestPlayer = Main.player[Player.FindClosest(shimmerable.Center, 1, 1)];
			foreach (Projectile projectile in base.SpawnFrom(shimmerable, allowedStack)) {
				Vector2 velocityMod = GetShimmerSpawnVelocityModifier();
				projectile.velocity = (shimmerable.Velocity / 2) + velocityMod + 5 * Vector2.Normalize(closestPlayer.Center - shimmerable.Center);
				yield return projectile;
			}
		}
	}
}

// This shoots the above item
public class ExampleComplexCustomShimmerableItem : ModItem
{
	public override string Texture => "ExampleMod/Content/Items/ExampleItem";

	public override void SetStaticDefaults() {
		Item.ResearchUnlockCount = 99;
	}

	public override void SetDefaults() {
		Item.damage = 18;
		Item.DamageType = DamageClass.Ranged;
		Item.width = Item.height = 8;
		Item.maxStack = Item.CommonMaxStack;
		Item.consumable = true;
		Item.knockBack = 1.5f;
		Item.value = Item.buyPrice(silver: 10);
		Item.rare = ItemRarityID.Purple;
		Item.shoot = ModContent.ProjectileType<ExampleComplexCustomShimmerable>();
		Item.shootSpeed = 4f;
		Item.ammo = AmmoID.Bullet;
	}

	public override void AddRecipes() {
		CreateRecipe()
			.AddIngredient<ExampleItem>()
			.AddTile<ExampleWorkbench>()
			.Register();
	}
}