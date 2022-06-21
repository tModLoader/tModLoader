using Terraria.ModLoader;

public class ModPrefixTest : ModPrefix
{
	public override void AutoDefaults() { /* Empty */ }

	void Method() {
		ModPrefix modPrefix = ModPrefix.GetPrefix(Type);
		modPrefix = GetPrefix(Type);
	}
}