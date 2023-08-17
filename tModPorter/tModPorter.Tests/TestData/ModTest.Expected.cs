using Terraria.ModLoader;

public class ModTest : Mod
{
	public ModTest() {
#if COMPILE_ERROR
		Properties/* tModPorter Note: Removed. Instead, assign the properties directly (ContentAutoloadingEnabled, GoreAutoloadingEnabled, MusicAutoloadingEnabled, and BackgroundAutoloadingEnabled) */ = new ModProperties() {
			Autoload = true,
			AutoloadBackgrounds = true,
			AutoloadGores = true,
			AutoloadSounds = true
		};
#endif
	}
}