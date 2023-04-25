using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Hairs
{
	// Based on Player_Hair_88 and Player_Hair_98
	// Note that internal hair ids are 1 less than the texture filename
	public class ExampleHair : ModHair
	{
		public override bool IsMale => false;
		
		public override void SetStaticDefaults() {
			HairID.Sets.DrawBackHair[Type] = true;
		}

		public override IEnumerable<Condition> GetUnlockConditions() {
			return Enumerable.Empty<Condition>();
			// TODO: throws exception on player creation menu, biome flags not available on player instance?
			//yield return ExampleConditions.InExampleBiome;
		}
	}
}
