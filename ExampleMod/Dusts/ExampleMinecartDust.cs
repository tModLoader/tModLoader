using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Dusts
{
	public class ExampleMinecartDust : ModDust
	{
		public override void SetDefaults()
		{
			updateType = 213;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			int fade = (int)(dust.scale / 2.5f * 255f);
			return new Color(fade, fade, fade, fade);
		}
	}
}
