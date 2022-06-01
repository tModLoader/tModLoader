using Terraria.ModLoader;

public class SimpleRenamedTMLMembersTest
{
	void Method() {
		bool textureExists = ModContent.HasAsset("1");
		textureExists = ModContent.HasAsset("1" + "2");
	}
}