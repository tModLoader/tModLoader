using Terraria.Localization;
using Terraria.ModLoader;

public class ModTranslationTest : Mod
{
	void Method() {
		Mod mod = this;
		Language.GetOrRegister(mod, "");
		Language.GetOrRegister(this, "");

#if COMPILE_ERROR
		mod.AddTranslation(null)/* tModPorter Note: Removed. Use Language.GetOrRegister */;
		AddTranslation(null)/* tModPorter Note: Removed. Use Language.GetOrRegister */;
#endif

		// 1.4.3 -> 1.4.4 removal of ModTranslation
#if COMPILE_ERROR
		LocalizationLoader.AddTranslation(null)/* tModPorter Note: Removed. Use Language.GetOrRegister */;
#endif
		Language.GetOrRegister("A.B.C");
		Language.GetOrRegister(mod, "suffix");
	}
}