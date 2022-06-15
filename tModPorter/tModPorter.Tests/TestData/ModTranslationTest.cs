using Terraria.ModLoader;

public class ModTranslationTest : Mod
{
	void Method() {
		Mod mod = this;
		mod.AddTranslation(null);
		mod.CreateTranslation("");

		AddTranslation(null);
		CreateTranslation("");
	}
}

public class SideEffectTest : ModItem
{
	Mod GetModMightHaveSideEffects() => Mod;

	void Method() {
		GetModMightHaveSideEffects().AddTranslation(null);
	}
}