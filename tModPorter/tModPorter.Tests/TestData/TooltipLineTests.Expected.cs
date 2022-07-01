using Terraria.ModLoader; 

public class TooltipLineTests
{
	void Method() {
		TooltipLine line = new TooltipLine(null, "", "");
		string mod = line.Mod;
		line.Text = "";
		line.IsModifier = true;
		line.IsModifierBad = false;
		line.OverrideColor = null;

		line = new TooltipLine(null, "", "") {
			Text = "",
			IsModifier = true,
			IsModifierBad = false,
			OverrideColor = null
		};
	}
}