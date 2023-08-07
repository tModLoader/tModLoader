using Terraria.ModLoader;
using Terraria.ID;

public class ModBuffTest : ModBuff
{
	public override void SetStaticDefaults() {
#if COMPILE_ERROR
		canBeCleared/* tModPorter Note: Removed. Use BuffID.Sets.NurseCannotRemoveDebuff instead, and invert the logic */ = false;
		longerExpertDebuff/* tModPorter Note: Removed. Use BuffID.Sets.LongerExpertDebuff instead */ = true;

		bool a = BuffLoader.CanBeCleared(0)/* tModPorter Note: Removed. Use !BuffID.Sets.NurseCannotRemoveDebuff instead */;
#endif

		BuffID.Sets.IsATagBuff[Type] = true;
	}

	public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare) { }
}