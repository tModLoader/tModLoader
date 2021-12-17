using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Dusts
{
	public class ExampleBubble : ModDust
	{
		public override void OnSpawn(Dust dust) {
			dust.noGravity = true;
			dust.frame = new Rectangle(0, 0, 30, 30);
			// If our texture had 3 different dust on top of each other (a 30x90 pixel image), we might do this:
			// dust.frame = new Rectangle(0, Main.rand.Next(3) * 30, 30, 30);
		}

		public override bool Update(Dust dust) {
			// Move the dust based on its velocity and reduce its size to then remove it, as the 'return false;' at the end will prevent vanilla logic.
			dust.position += dust.velocity;
			dust.scale -= 0.01f;

			if (dust.scale < 0.75f)
				dust.active = false;

			return false;
		}
	}
}
