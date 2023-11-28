using Microsoft.Xna.Framework;
using Terraria.ModLoader;

public class InfoDisplayTest143 : InfoDisplay {
	public override string DisplayValue(ref Color displayColor, ref Color displayShadowColor)/* tModPorter Suggestion: Set displayColor to InactiveInfoTextColor if your display value is "zero"/shows no valuable information */ {
		return "";
	}
}

public class InfoDisplayTest144 : InfoDisplay {
	public override string DisplayValue(ref Color displayColor, ref Color displayShadowColor)/* tModPorter Suggestion: Set displayColor to InactiveInfoTextColor if your display value is "zero"/shows no valuable information */ {
		return "";
	}
}

public class GlobalInfoDisplayTest : GlobalInfoDisplay
{
	public override void ModifyDisplayParameters(InfoDisplay currentDisplay, ref string displayValue, ref string displayName, ref Color displayColor, ref Color displayShadowColor)/* tModPorter ModifyDisplayValue, ModifyDisplayName, and ModifyDisplayColor are all combined into ModifyDisplayParameters now. */
	{
		if (currentDisplay == InfoDisplay.Radar)
			displayColor = Color.Red;
	}
}

public class GlobalInfoDisplayTest2 : GlobalInfoDisplay
{
	public override void ModifyDisplayParameters(InfoDisplay currentDisplay, ref string displayValue, ref string displayName, ref Color displayColor, ref Color displayShadowColor)/* tModPorter ModifyDisplayValue, ModifyDisplayName, and ModifyDisplayColor are all combined into ModifyDisplayParameters now. */
	{
	}
}