using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace ExampleMod.Content.Liquids
{
	public class ExampleLiquid : ModLiquid
	{
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			WaterfallLength = 15;
			DefaultOpacity = 1;
		}
	}
}
