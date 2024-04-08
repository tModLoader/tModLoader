using ExampleMod.Content.Items.Placeable;
using ExampleMod.Content.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

// This file contains ExampleSandBallProjectile, ExampleSandBallFallingProjectile, and ExampleSandBallGunProjectile.
// ExampleSandBallFallingProjectile and ExampleSandBallGunProjectile inherit from ExampleSandBallProjectile, allowing cleaner code and shared logic.
// ExampleSandBallFallingProjectile is the projectile that spawns when the ExampleSand tile falls.
// ExampleSandBallGunProjectile is the projectile that is shot by the Sandgun weapon.
// Both projectiles share the same aiStyle, ProjAIStyleID.FallingTile, but the AIType line in ExampleSandBallGunProjectile ensures that specific logic of the aiStyle is used for the sandgun projectile.
// It is possible to make a falling projectile not using ProjAIStyleID.FallingTile, but it is a lot of code.
namespace ExampleMod.Content.Projectiles
{
	public abstract class ExampleSandBallProjectile : ModProjectile
	{
		public override string Texture => "ExampleMod/Content/Projectiles/ExampleSandBallProjectile";

		public override void SetStaticDefaults() {
			ProjectileID.Sets.FallingBlockDoesNotFallThroughPlatforms[Type] = true;
			ProjectileID.Sets.ForcePlateDetection[Type] = true;
		}
	}

	public class ExampleSandBallFallingProjectile : ExampleSandBallProjectile
	{
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ProjectileID.Sets.FallingBlockTileItem[Type] = new(ModContent.TileType<ExampleSand>(), ModContent.ItemType<ExampleSandBlock>());
		}

		public override void SetDefaults() {
			// The falling projectile when compared to the sandgun projectile is hostile.
			Projectile.CloneDefaults(ProjectileID.EbonsandBallFalling);
		}
	}

	public class ExampleSandBallGunProjectile : ExampleSandBallProjectile
	{
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ProjectileID.Sets.FallingBlockTileItem[Type] = new(ModContent.TileType<ExampleSand>());
		}

		public override void SetDefaults() {
			// The sandgun projectile when compared to the falling projectile has a ranged damage type, isn't hostile, and has extraupdates = 1.
			// Note that EbonsandBallGun has infinite penetration, unlike SandBallGun
			Projectile.CloneDefaults(ProjectileID.EbonsandBallGun);
			AIType = ProjectileID.EbonsandBallGun; // This is needed for some logic in the ProjAIStyleID.FallingTile code.
		}
	}
}