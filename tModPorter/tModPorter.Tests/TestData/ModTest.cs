using Terraria.ModLoader;

public class ModTest : Mod
{
	public ModTest() {
#if COMPILE_ERROR
		Properties = new ModProperties() {
			Autoload = true,
			AutoloadBackgrounds = true,
			AutoloadGores = true,
			AutoloadSounds = true
		};
#endif
	}
}
