using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Projectiles.Pets
{
	public class ExampleLightPet : ModProjectile
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Annoying Light");
			Main.projFrames[projectile.type] = 1;
			Main.projPet[projectile.type] = true;
			ProjectileID.Sets.TrailingMode[projectile.type] = 2;
			ProjectileID.Sets.LightPet[projectile.type] = true;
		}

		public override void SetDefaults() {
			projectile.width = 30;
			projectile.height = 30;
			projectile.penetrate = -1;
			projectile.netImportant = true;
			projectile.timeLeft *= 5;
			projectile.friendly = true;
			projectile.ignoreWater = true;
			projectile.scale = 0.8f;
			projectile.tileCollide = false;
		}

		private const int fadeInTicks = 30;
		private const int fullBrightTicks = 200;
		private const int fadeOutTicks = 30;
		private const int range = 500;
		private readonly int rangeHypoteneus = (int)Math.Sqrt(range * range + range * range);

		public override void AI() {
			Player player = Main.player[projectile.owner];
			ExamplePlayer modPlayer = player.GetModPlayer<ExamplePlayer>();
			if (!player.active) {
				projectile.active = false;
				return;
			}
			if (player.dead) {
				modPlayer.exampleLightPet = false;
			}
			if (modPlayer.exampleLightPet) {
				projectile.timeLeft = 2;
			}
			projectile.ai[1]++;
			if (projectile.ai[1] > 1000 && (int)projectile.ai[0] % 100 == 0) {
				for (int i = 0; i < Main.maxNPCs; i++) {
					if (Main.npc[i].active && !Main.npc[i].friendly && player.Distance(Main.npc[i].Center) < rangeHypoteneus) {
						Vector2 vectorToEnemy = Main.npc[i].Center - projectile.Center;
						projectile.velocity += 10f * Vector2.Normalize(vectorToEnemy);
						projectile.ai[1] = 0f;
						Main.PlaySound(SoundLoader.customSoundType, -1, -1, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/WatchOut"));
						break;
					}
				}
			}
			projectile.rotation += projectile.velocity.X / 20f;
			Lighting.AddLight(projectile.Center, (255 - projectile.alpha) * 0.9f / 255f, (255 - projectile.alpha) * 0.1f / 255f, (255 - projectile.alpha) * 0.3f / 255f);
			if (projectile.velocity.Length() > 1f) {
				projectile.velocity *= .98f;
			}
			if (projectile.velocity.Length() == 0) {
				projectile.velocity = Vector2.UnitX.RotatedBy(Main.rand.NextFloat() * Math.PI * 2);
				projectile.velocity *= 2f;
			}
			projectile.ai[0]++;
			if (projectile.ai[0] < fadeInTicks) {
				projectile.alpha = (int)(255 - 255 * projectile.ai[0] / fadeInTicks);
			}
			else if (projectile.ai[0] < fadeInTicks + fullBrightTicks) {
				projectile.alpha = 0;
				if (Main.rand.NextBool(6)) {
					int num145 = Dust.NewDust(projectile.position, projectile.width, projectile.height, 73, 0f, 0f, 200, default(Color), 0.8f);
					Main.dust[num145].velocity *= 0.3f;
				}
			}
			else if (projectile.ai[0] < fadeInTicks + fullBrightTicks + fadeOutTicks) {
				projectile.alpha = (int)(255 * (projectile.ai[0] - fadeInTicks - fullBrightTicks) / fadeOutTicks);
			}
			else {
				projectile.Center = new Vector2(Main.rand.Next((int)player.Center.X - range, (int)player.Center.X + range), Main.rand.Next((int)player.Center.Y - range, (int)player.Center.Y + range));
				projectile.ai[0] = 0;
				Vector2 vectorToPlayer = player.Center - projectile.Center;
				projectile.velocity = 2f * Vector2.Normalize(vectorToPlayer);
			}
			if (Vector2.Distance(player.Center, projectile.Center) > rangeHypoteneus) {
				projectile.Center = new Vector2(Main.rand.Next((int)player.Center.X - range, (int)player.Center.X + range), Main.rand.Next((int)player.Center.Y - range, (int)player.Center.Y + range));
				projectile.ai[0] = 0;
				Vector2 vectorToPlayer = player.Center - projectile.Center;
				projectile.velocity += 2f * Vector2.Normalize(vectorToPlayer);
			}
			if ((int)projectile.ai[0] % 100 == 0) {
				projectile.velocity = projectile.velocity.RotatedByRandom(MathHelper.ToRadians(90));
			}
		}
	}
}