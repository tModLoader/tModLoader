using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	// ExampleFlail and ExampleFlailProjectile show the minimum amount of code needed for a flail using the existing vanilla code and behavior. ExampleAdvancedFlail and ExampleAdvancedFlailProjectile need to be consulted if more advanced customization is desired, or if you want to learn more advanced modding techniques.
	// ExampleFlailProjectile is a copy of the Sunfury flail projectile.
	internal class ExampleFlailProjectile : ModProjectile
	{
		public override void SetStaticDefaults() {
			ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.netImportant = true; // This ensures that the projectile is synced when other players join the world.
			Projectile.width = 22; // The width of your projectile
			Projectile.height = 22; // The height of your projectile
			Projectile.friendly = true; // Deals damage to enemies
			Projectile.penetrate = -1; // Infinite pierce
			Projectile.DamageType = DamageClass.Melee; // Deals melee damage
			Projectile.scale = 0.8f;
			Projectile.usesLocalNPCImmunity = true; // Used for hit cooldown changes in the ai hook
			Projectile.localNPCHitCooldown = 10; // This facilitates custom hit cooldown logic

			// Here we reuse the flail projectile aistyle and set the aitype to the Sunfury. These lines will get our projectile to behave exactly like Sunfury would. This only affects the AI code, you'll need to adapt other code for the other behaviors you wish to use.
			Projectile.aiStyle = ProjAIStyleID.Flail;
			AIType = ProjectileID.Sunfury;

			// These help center the projectile as it rotates since its hitbox and scale doesn't match the actual texture size
			DrawOffsetX = -6;
			DrawOriginOffsetY = -6;
		}

		// All of the following methods are additional behaviors of Sunfury that are not automatically inherited by ExampleFlailProjectile through the use of Projectile.aiStyle and AIType. You'll need to find corresponding code in the decompiled source code if you wish to clone a different vanilla projectile as a starting point.

		// Draw the projectile in full brightness, ignoring lighting conditions.
		public override Color? GetAlpha(Color lightColor) {
			return Color.White;
		}

		// In PreDrawExtras, we trick the game into thinking the projectile is actually a Sunfury projectile. After PreDrawExtras, the Terraria code will draw the chain. Drawing the chain ourselves is quite complicated, ExampleAdvancedFlailProjectile has an example of that. Then, in PreDraw, we restore the Projectile.type back to normal so we don't break anything.
		public override bool PreDrawExtras() {
			Projectile.type = ProjectileID.Sunfury;
			return base.PreDrawExtras();
		}
		public override bool PreDraw(ref Color lightColor) {
			Projectile.type = ModContent.ProjectileType<ExampleFlailProjectile>();

			// This code handles the after images.
			if (Projectile.ai[0] == 1f) {
				Texture2D projectileTexture = TextureAssets.Projectile[Type].Value;
				Vector2 drawPosition = Projectile.position + new Vector2(Projectile.width, Projectile.height) / 2f + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
				Vector2 drawOrigin = new Vector2(projectileTexture.Width, projectileTexture.Height) / 2f;
				Color drawColor = Projectile.GetAlpha(lightColor);
				drawColor.A = 127;
				drawColor *= 0.5f;
				int launchTimer = (int)Projectile.ai[1];
				if (launchTimer > 5) {
					launchTimer = 5;
				}

				SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

				for (float transparency = 1f; transparency >= 0f; transparency -= 0.125f) {
					float opacity = 1f - transparency;
					Vector2 drawAdjustment = Projectile.velocity * -launchTimer * transparency;
					Main.EntitySpriteDraw(projectileTexture, drawPosition + drawAdjustment, null, drawColor * opacity, Projectile.rotation, drawOrigin, Projectile.scale * 1.15f * MathHelper.Lerp(0.5f, 1f, opacity), spriteEffects, 0);
				}
			}

			return base.PreDraw(ref lightColor);
		}

		// Another thing that won't automatically be inherited by using Projectile.aiStyle and AIType are effects that happen when the projectile hits something. Here we see the code responsible for applying the OnFire debuff to players and enemies.
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Main.rand.NextBool(2)) {
				target.AddBuff(BuffID.OnFire, 300);
			}
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			if (Main.rand.NextBool(4)) {
				target.AddBuff(BuffID.OnFire, 180, quiet: false);
			}
		}

		// Finally, you can slightly customize the AI if you read and understand the vanilla aiStyle source code. You can't customize the range, retract speeds, or anything else. If you need to customize those things, you'll need to follow ExampleAdvancedFlailProjectile. This example spawns a Grenade right when the flail starts to retract.
		public override void AI() {
			// The only reason this code works is because the author read the vanilla code and comprehended it well enough to tack on additional logic.
			if (Main.myPlayer == Projectile.owner && Projectile.ai[0] == 2f && Projectile.ai[1] == 0f) {
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, ProjectileID.Grenade, Projectile.damage, Projectile.knockBack, Main.myPlayer);
				Projectile.ai[1]++;
			}
		}
	}
}
