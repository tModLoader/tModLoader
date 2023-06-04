using Terraria;
using Terraria.ModLoader;
using MonoMod.RuntimeDetour.HookGen;

public class SimpleRenamedTMLMembersTest
{
	void Method() {
		bool textureExists = ModContent.TextureExists("1");
		textureExists = ModContent.TextureExists("1" + "2");

		var mod = new Mod();
		textureExists = mod.TextureExists("1");
		textureExists = mod.TextureExists("1" + "2");

		Projectile p = null;
		p.ContinuouslyUpdateDamage = p.ContinuouslyUpdateDamage;
	}

	void HookEndpointManagerMethods()
	{
		HookEndpointManager.Add(null, null);
		HookEndpointManager.Modify(null, null);
		HookEndpointManager.Clear(); // counter-case, not meant to change
	}
}