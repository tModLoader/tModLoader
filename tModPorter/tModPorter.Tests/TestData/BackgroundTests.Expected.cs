using Terraria.ModLoader;

public class ModSurfaceBgStyleTest : ModSurfaceBackgroundStyle
{
#if COMPILE_ERROR
	public override bool ChooseBgStyle()/* tModPorter Note: Removed. Create a ModBiome (or ModSceneEffect) class and override SurfaceBackgroundStyle property to return this object through Mod/ModContent.Find, then move this code into IsBiomeActive (or IsSceneEffectActive) */ { return true; }
#endif

	void Method() {
		var a = Mod;
		var slot = BackgroundTextureLoader.GetBackgroundSlot(Mod, "Backgrounds/File");
	}

	// Mandatory
	public override void ModifyFarFades(float[] fades, float transitionSpeed) { /* Empty */ }
}

public class ModUgBgStyleTest : ModUndergroundBackgroundStyle
{
#if COMPILE_ERROR
	public override bool ChooseBgStyle()/* tModPorter Note: Removed. Create a ModBiome (or ModSceneEffect) class and override UndergroundBackgroundStyle property to return this object through Mod/ModContent.Find, then move this code into IsBiomeActive (or IsSceneEffectActive) */ { return true; }
#endif

	void Method() {
		var a = Mod;
		var slot = BackgroundTextureLoader.GetBackgroundSlot(Mod, "Backgrounds/File");
	}

	// Mandatory
	public override void FillTextureArray(int[] textureSlots) { /* Empty */ }
}