using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terraria.ModLoader.Default
{
    internal class UnloadedLiquid : ModLiquid
    {
        public override System.String Texture => "ModLoader/UnloadLiquid";

        public override void SetStaticDefaults()
        {
			// Do IO stuff here
            base.SetStaticDefaults();
        }
    }
}
