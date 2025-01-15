using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader.Config;

namespace ExampleMod.Common.Configs.CustomUI
{
	public class FloatToPercentProvider : ModConfigStylesProvider
	{
		public override void ModifyLabel(Func<object> getObject, string configElementLabel, ref string label) {
			label = configElementLabel + ": " + ((float)getObject()).ToString("0%");
		}
	}
}
