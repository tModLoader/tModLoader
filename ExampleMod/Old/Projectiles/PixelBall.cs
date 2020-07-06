using ExampleMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Projectiles
{
	public class PixelBall : ElementBall
	{
		public override string Texture => mod.Name + "/Projectiles/ElementBall";

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Pixel Ball");
		}

		public override void CreateDust() {
			Color? color = GetColor();
			if (color.HasValue) {
				int dust = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustType<Pixel>(), 0f, 0f, 0, color.Value);
				Main.dust[dust].velocity += projectile.velocity;
				Main.dust[dust].scale = 0.9f;
			}
		}

		public override void PlaySound() {
			SoundEngine.PlaySound(SoundID.Item33, projectile.position);
		}

		public override string GetName() {
			if (projectile.ai[0] == 24f) {
				return "Fire Sprite";
			}
			if (projectile.ai[0] == 44f) {
				return "Frost Sprite";
			}
			if (projectile.ai[0] == BuffType<Buffs.EtherealFlames>()) {
				return "Spirit Sprite";
			}
			if (projectile.ai[0] == 70f) {
				return "Infestation Sprite";
			}
			if (projectile.ai[0] == 69f) {
				return "Ichor Sprite";
			}
			return "Doom Bubble";
		}
	}
}