using Terraria.ModLoader;

public class ModBuffTest : ModBuff
{
	public override void SetDefaults() {
		canBeCleared = false;
		longerExpertDebuff = true;

		bool a = BuffLoader.CanBeCleared(0);
	}
}