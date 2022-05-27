using Terraria.ModLoader; 

public class TooltipLineTests
{
	void Method() {
		TooltipLine line = new TooltipLine(null, "", "");
		line.text = "";
		line.isModifier = true;
		line.isModifierBad = false;
		line.overrideColor = null;

		line = new TooltipLine(null, "", "") {
			text = "",
			isModifier = true,
			isModifierBad = false,
			overrideColor = null
		};
	}
}