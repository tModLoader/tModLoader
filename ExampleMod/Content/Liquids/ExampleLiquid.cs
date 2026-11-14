using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Liquids
{
	public class ExampleLiquid : ModLiquid
	{
		//SetStaticDefaults are the defaults added when the game initially loads.
		//Here we set a few settings that this liquid will have.
		public override void SetStaticDefaults() {
			VanillaFallbackOnModDeletion = (ushort)LiquidID.Water;
		}
	}
}
