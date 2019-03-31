using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Dusts
{
	public class Negative : ModDust
	{
		public override void OnSpawn(Dust dust) {
			dust.noGravity = true;
		}
	}
}