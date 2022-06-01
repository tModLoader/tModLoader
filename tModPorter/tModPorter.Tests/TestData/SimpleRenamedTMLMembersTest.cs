using Terraria.ModLoader;

public class SimpleRenamedTMLMembersTest
{
	void Method() {
		bool textureExists = ModContent.TextureExists("1");
		textureExists = ModContent.TextureExists("1" + "2");
	}
}