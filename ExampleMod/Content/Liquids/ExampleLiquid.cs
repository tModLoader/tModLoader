using Microsoft.Xna.Framework;
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

			// We can add a map entry to our liquid, by doing so we can show where our liquid is on the map.
			// Unlike vanilla, we can also add a map entry name, which will display a name if the liquid is being selected on the map.
			AddMapEntry(new Color(200, 200, 200), DisplayName);
		}

		// ModifyLight allows the liquid to emit light similarly to any tile or wall.
		// You can use this to emit light similarly to lava or shimmer.
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			// Here we make the liquid just emit a bright white light by setting R, G, and B to 1.
			r = 1f;
			g = 1f;
			b = 1f;
		}
	}
}
