using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
    public class UnloadedLiquid : ModLiquid
    {
        public override string Texture => "ModLoader/UnloadedLiquid";

        public override void SetStaticDefaults()
        {
			this.WaterfallLength = 0;
			this.DefaultOpacity = 1;
			TileIO.Liquids.unloadedTypes.Add(Type);
		}
    }
}
