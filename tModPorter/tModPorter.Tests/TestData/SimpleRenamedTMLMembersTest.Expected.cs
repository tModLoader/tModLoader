using Terraria.ModLoader;
using MonoMod.RuntimeDetour.HookGen;

public class SimpleRenamedTMLMembersTest
{
	void Method() {
		bool textureExists = ModContent.HasAsset("1");
		textureExists = ModContent.HasAsset("1" + "2");

		var mod = new Mod();
		textureExists = mod.HasAsset("1");
		textureExists = mod.HasAsset("1" + "2");
	}

	void HookEndpointManagerMethods()
	{
		MonoModHooks.Add(null, null);
		MonoModHooks.Modify(null, null);
		HookEndpointManager.Clear(); // counter-case, not meant to change
	}
}