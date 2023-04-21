using Terraria;
using Terraria.ModLoader;

public class ModPrefixTest : ModPrefix
{
#if COMPILE_ERROR
	public override void AutoStaticDefaults()/* tModPorter Note: Removed. Nothing to override anymore. Use hjson files to adjust localization */ { /* Empty */ }
#endif

	// public override SetStaticDefaults() => DisplayName.SetDefault("Test");

	void Method() {
		ModPrefix modPrefix = PrefixLoader.GetPrefix(Type);
		modPrefix = PrefixLoader.GetPrefix(Type);
	}

#if COMPILE_ERROR
	public override bool AllStatChangesHaveEffectOn(Item item) { }
#endif
}