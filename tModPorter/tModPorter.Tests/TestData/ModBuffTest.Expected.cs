using Terraria.ModLoader;

public class ModBuffTest : ModBuff
{
	public override void SetStaticDefaults() {
#if COMPILE_ERROR
		canBeCleared/* tModPorter Note: Removed, use BuffID.Sets.NurseCannotRemoveDebuff */ = true;
		longerExpertDebuff/* tModPorter Note: Removed, use BuffID.Sets.LongerExpertDebuff */ = true;

		bool a = BuffLoader.CanBeCleared(0)/* tModPorter Note: Removed, use BuffID.Sets.NurseCannotRemoveDebuff */;
#endif
	}
}