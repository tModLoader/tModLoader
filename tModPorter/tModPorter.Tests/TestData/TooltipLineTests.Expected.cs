using Terraria.ModLoader; 

public class TooltipLineTests
{
	void Method() {
		TooltipLine line = new TooltipLine(null, "", "");
		string mod = line.Mod;
		line.Text = "";
#if COMPILE_ERROR
		line.IsModifier = true;
		line.IsModifierBad = false;
#endif
		line.OverrideColor = null;

		line = new TooltipLine(null, "", "") {
			Text = "",
#if COMPILE_ERROR
			IsModifier = true,
			IsModifierBad = false,
#endif
			OverrideColor = null
		};
	}
}