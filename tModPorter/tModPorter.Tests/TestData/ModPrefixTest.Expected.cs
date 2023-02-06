using Terraria;
using Terraria.ModLoader;

public class ModPrefixTest : ModPrefix
{
#if COMPILE_ERROR
	public override void AutoStaticDefaults()/* tModPorter Note: Removed. Nothing to override anymore. Use hjson files and/or override DisplayName to adjust localization */ { /* Empty */ }
#endif

	void Method() {
		ModPrefix modPrefix = PrefixLoader.GetPrefix(Type);
		modPrefix = PrefixLoader.GetPrefix(Type);
	}

#if COMPILE_ERROR
	public override bool AllStatChangesHaveEffectOn(Item item) { }
#endif
}