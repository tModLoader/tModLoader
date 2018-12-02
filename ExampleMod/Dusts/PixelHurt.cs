using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Dusts
{
	public class PixelHurt : ModDust
	{
		public override bool Autoload(ref string name, ref string texture) {
			texture = "ExampleMod/Dusts/Pixel";
			return mod.Properties.Autoload;
		}

		public override void OnSpawn(Dust dust) {
			dust.noGravity = true;
			dust.noLight = true;
		}

		public override bool Update(Dust dust) {
			dust.position += dust.velocity;
			dust.velocity *= 0.97f;
			if (dust.velocity.Length() < 0.01) {
				dust.active = false;
			}
			return false;
		}
	}
}