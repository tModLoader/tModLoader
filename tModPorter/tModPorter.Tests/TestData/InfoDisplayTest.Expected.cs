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
	public override void ModifyDisplayColor(InfoDisplay currentDisplay, ref Color displayColor, ref Color displayShadowColor)
	{
		if (currentDisplay == InfoDisplay.Radar)
			displayColor = Color.Red;
	}
}