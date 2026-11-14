using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Liquids
{
	public class ExampleLiquid : ModLiquid
	{
		//SetStaticDefaults are the defaults added when the game initially loads.
		//Here we set a few settings that this liquid will have.
		public override void SetStaticDefaults() {
			//This ID set controls what items classify as a sponge when trying to suck up this liquid
			//Here we remove the Ultra Absorbant sponge, Allow the Lava Absorbant sponge and the staff of regrowth to suck up this liquid
			LiquidID.Sets.CanBeAbsorbedBy[Type].Remove(ItemID.UltraAbsorbantSponge);
			LiquidID.Sets.CanBeAbsorbedBy[Type].Add(ItemID.LavaAbsorbantSponge);
			LiquidID.Sets.CanBeAbsorbedBy[Type].Add(ItemID.StaffofRegrowth); //Here is an example of turning a regular item into a sponge thats capable of sucking up our liquid

			VanillaFallbackOnModDeletion = (ushort)LiquidID.Water;
		}
	}
}
