using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader.Config.UI;

// ATTENTION: Below this point is custom config UI element.
// Be aware that mods using custom config elements will break with the next few tModLoader updates until their design is finalized.
// You will need to be very active in updating your mod if you use these as they can break in any update.

// This file defines a custom ConfigElement based on Float data type
// with custom slider coloring method implemented that can be used in ModConfig classes.
namespace ExampleMod.Common.Configs.CustomUI
{

	class CustomFloatElement : FloatElement
	{
		public CustomFloatElement() {
			ColorMethod = new Utils.ColorLerpMethod((percent) => Color.Lerp(Color.BlueViolet, Color.Aquamarine, percent));
		}
	}
}
