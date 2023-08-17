using Terraria.ModLoader;

public class ModSurfaceBgStyleTest : ModSurfaceBgStyle
{
	public override bool ChooseBgStyle() { return true; }

	void Method() {
		var a = mod;
		var slot = mod.GetBackgroundSlot("Backgrounds/File");
	}

	// Mandatory
	public override void ModifyFarFades(float[] fades, float transitionSpeed) { /* Empty */ }
}

public class ModUgBgStyleTest : ModUgBgStyle
{
	public override bool ChooseBgStyle() { return true; }

	void Method() {
		var a = mod;
		var slot = mod.GetBackgroundSlot("Backgrounds/File");
	}

	// Mandatory
	public override void FillTextureArray(int[] textureSlots) { /* Empty */ }
}