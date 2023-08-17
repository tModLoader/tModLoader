using Terraria;
using Terraria.ModLoader;

public class ModPrefixTest : ModPrefix
{
	public override void AutoDefaults() { /* Empty */ }

	public override SetStaticDefaults() => DisplayName.SetDefault("Test");

	void Method() {
		ModPrefix modPrefix = ModPrefix.GetPrefix(Type);
		modPrefix = GetPrefix(Type);
	}

	public override void ValidateItem(Item item, ref bool invalid) { }
}