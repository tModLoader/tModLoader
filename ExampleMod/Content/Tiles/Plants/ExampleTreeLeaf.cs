using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles
{
	public class ExampleTreeLeaf : ModGore
	{
		public override string Texture => "ExampleMod/Content/Tiles/Plants/ExampleTree_Leaf";

		public override void SetStaticDefaults() {
			
			GoreID.Sets.SpecialAI[Type] = 3;
		}
	}
}
