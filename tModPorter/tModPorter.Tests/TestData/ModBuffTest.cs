using Terraria.ModLoader;

public class ModBuffTest : ModBuff
{
	public override void SetDefaults() {
#if COMPILE_ERROR
		canBeCleared = false;
		longerExpertDebuff = true;

		bool a = BuffLoader.CanBeCleared(0);
#endif
	}
}