using Terraria.ModLoader;
using Terraria.ID;

public class ModBuffTest : ModBuff
{
	public override void SetDefaults() {
		canBeCleared = false;
		longerExpertDebuff = true;
		BuffID.Sets.LongerExpertDebuff[Type] = true;

		bool a = BuffLoader.CanBeCleared(0);

		BuffID.Sets.IsAnNPCWhipDebuff[Type] = true;

		BuffID.Sets.BasicMountData[Type] = new BuffID.Sets.BuffMountData() {
			mountID = ModContent.MountType<ExampleMinecartMount>()
		};
	}

	public override void ModifyBuffTip(ref string tip, ref int rare) { }
}