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
	public class DynamicLabelColorProvider : ModConfigStylesProvider
	{
		public override void ModifyLabelColor(Func<object> getObject, bool isMouseHovering, bool canWrite, ref Color labelColor) {
			labelColor = Main.DiscoColor;
		}
	}
}
