using Terraria.ModLoader;

public class ModBuffTest : ModBuff
{
	public override void SetDefaults() {
		canBeCleared = false;
		longerExpertDebuff = true;

		bool a = BuffLoader.CanBeCleared(0);
	}

	public override void ModifyBuffTip(ref string tip, ref int rare) { }
}