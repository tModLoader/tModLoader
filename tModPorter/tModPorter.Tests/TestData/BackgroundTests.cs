using Terraria.ModLoader;

public class ModSurfaceBgStyleTest : ModSurfaceBgStyle
{
#if COMPILE_ERROR
	public override bool ChooseBgStyle() { return true; }
#endif

	void Method() {
		var a = mod;
		var slot = mod.GetBackgroundSlot("Backgrounds/File");
	}

	// Mandatory
	public override void ModifyFarFades(float[] fades, float transitionSpeed) { /* Empty */ }
}

public class ModUgBgStyleTest : ModUgBgStyle
{
#if COMPILE_ERROR
	public override bool ChooseBgStyle() { return true; }
#endif

	void Method() {
		var a = mod;
		var slot = mod.GetBackgroundSlot("Backgrounds/File");
	}

	// Mandatory
	public override void FillTextureArray(int[] textureSlots) { /* Empty */ }
}
