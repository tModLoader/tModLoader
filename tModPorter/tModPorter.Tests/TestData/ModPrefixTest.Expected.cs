using Terraria.ModLoader;

public class ModPrefixTest : ModPrefix
{
	public override void AutoStaticDefaults() { /* Empty */ }

	void Method() {
		ModPrefix modPrefix = PrefixLoader.GetPrefix(Type);
		modPrefix = PrefixLoader.GetPrefix(Type);
	}
}