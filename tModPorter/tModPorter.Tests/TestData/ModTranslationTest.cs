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

	void SetDefaultRemoval(ModTranslation t)
	{
		t.SetDefault("String");
		t.SetDefault("Line 1" +
			"Line 2");
		t.SetDefault("""
Line 1
Line 2
""");
		t.SetDefault(@"Line 1
Line 2");
	}

	ModTranslation t2;
	void ExpressionBodyTest() => t2.SetDefault("Test");

	void NonExpressionBodyTest()
	{
		t2.SetDefault("Test");
	}
}