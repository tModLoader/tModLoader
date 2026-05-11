using Microsoft.Xna.Framework;
using Terraria.ModLoader;

public class TooltipLineTests
{
	void Method() {
		TooltipLine line = new TooltipLine(null, "", "");
		string mod = line.Mod;
		line.Text = "";
#if COMPILE_ERROR
		line.IsModifier/* tModPorter Note: Removed. Set Color = Terraria.ID.Colors.PrefixGood instead */ = true;
		line.IsModifierBad/* tModPorter Note: Removed. Set Color = Terraria.ID.Colors.PrefixBad instead */ = false;
		line.Color = null;
#endif

		line = new TooltipLine(null, "", "") {
			Text = "",
#if COMPILE_ERROR
			IsModifier/* tModPorter Note: Removed. Set Color = Terraria.ID.Colors.PrefixGood instead */ = true,
			IsModifierBad/* tModPorter Note: Removed. Set Color = Terraria.ID.Colors.PrefixBad instead */ = false,
			Color = null
#endif
		};

#if COMPILE_ERROR
		line.IsModifier/* tModPorter Note: Removed. Set Color = Terraria.ID.Colors.PrefixGood instead */ = true;
		line.IsModifierBad/* tModPorter Note: Removed. Set Color = Terraria.ID.Colors.PrefixBad instead */ = false;
		line.Color = null;
#endif

		DrawableTooltipLine drawableTooltipLine = new DrawableTooltipLine(line, 0, 0, 0, Color.White);
#if COMPILE_ERROR
		drawableTooltipLine.IsModifier/* tModPorter Note: Removed. Set Color = Terraria.ID.Colors.PrefixGood instead */ = true;
		drawableTooltipLine.IsModifierBad/* tModPorter Note: Removed. Set Color = Terraria.ID.Colors.PrefixBad instead */ = false;
		drawableTooltipLine.Color = null;
#endif
		drawableTooltipLine.Color = Color.White;
	}
}