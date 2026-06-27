using Terraria.ModLoader;
using Terraria.ID;

public class ModBuffTest : ModBuff
{
	public override void SetStaticDefaults() {
#if COMPILE_ERROR
		canBeCleared/* tModPorter Note: Removed. Use BuffID.Sets.NurseCannotRemoveDebuff instead, and invert the logic */ = false;
		longerExpertDebuff/* tModPorter Note: Removed. Use BuffID.Sets.BuffTimeIsExtendedWithGameDifficulty instead */ = true;
#endif
		BuffID.Sets.BuffTimeIsExtendedWithGameDifficulty[Type] = true;

#if COMPILE_ERROR
		bool a = BuffLoader.CanBeCleared(0)/* tModPorter Note: Removed. Use !BuffID.Sets.NurseCannotRemoveDebuff instead */;
#endif

		BuffID.Sets.IsATagBuff[Type] = true;

#if COMPILE_ERROR
		BuffID.Sets.BasicMountData/* tModPorter Note: Removed. Replace with BuffID.Sets.MountType[Type] = ModContent.MountType<MyMount>(); */[Type] = new BuffID.Sets.BuffMountData() {
			mountID = ModContent.MountType<ExampleMinecartMount>()
		};
#endif
	}

	public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare) { }
}