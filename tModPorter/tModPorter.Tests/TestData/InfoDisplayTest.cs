using Terraria.ModLoader;

public class InfoDisplayTest143 : InfoDisplay {
	public override string DisplayValue() {
		return "";
	}
}

public class InfoDisplayTest144 : InfoDisplay {
	public override string DisplayValue(ref Color displayColor) {
		return "";
	}
}

public class GlobalInfoDisplayTest : GlobalInfoDisplay
{
	public override void ModifyDisplayColor(InfoDisplay currentDisplay, ref Color displayColor)
	{
		if (currentDisplay == InfoDisplay.Radar)
			displayColor = Color.Red;
	}
}

public class GlobalInfoDisplayTest2 : GlobalInfoDisplay
{
	public override void ModifyDisplayName(InfoDisplay currentDisplay, ref string displayName)
	{
	}
}