using Terraria.ModLoader;

public class ModTranslationTest : Mod
{
	void Method() {
		Mod mod = this;
		mod.CreateTranslation("");
		CreateTranslation("");

		mod.AddTranslation(null);
		AddTranslation(null);

		// 1.4.3 -> 1.4.4 removal of ModTranslation
		LocalizationLoader.AddTranslation(null);
		LocalizationLoader.CreateTranslation("A.B.C");
		LocalizationLoader.GetOrCreateTranslation(mod, "suffix");
	}
}