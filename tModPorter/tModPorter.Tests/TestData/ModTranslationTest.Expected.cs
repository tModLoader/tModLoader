using Terraria.ModLoader;

public class ModTranslationTest : Mod
{
	void Method() {
		Mod mod = this;
		LocalizationLoader.AddTranslation(null);
		LocalizationLoader.CreateTranslation(mod, "");

		LocalizationLoader.AddTranslation(null);
		LocalizationLoader.CreateTranslation(this, "");
	}
}