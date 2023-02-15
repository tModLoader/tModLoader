using Microsoft.Xna.Framework;
using Terraria.ModLoader;

public class InfoDisplayTest : InfoDisplay {
	public override string DisplayValue(ref Color displayColor, Color inactiveColor)/* tModPorter Suggestion: Set displayColor to inactiveColor if your display value is "zero"/shows no valuable information */ {
		return "";
	}
}
