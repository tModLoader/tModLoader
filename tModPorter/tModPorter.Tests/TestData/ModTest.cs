using Terraria.ModLoader;

public class ModTest : Mod
{
	public ModTest() {
		Properties = new ModProperties() {
			Autoload = true,
			AutoloadBackgrounds = true,
			AutoloadGores = true,
			AutoloadSounds = true
		};
	}
}