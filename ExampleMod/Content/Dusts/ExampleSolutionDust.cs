using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Dusts
{
	public class ExampleSolutionDust : ModDust
	{
		public override void SetStaticDefaults() {
			UpdateType = DustID.PureSpray;
		}
	}
}