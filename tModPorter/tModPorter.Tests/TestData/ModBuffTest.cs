using Terraria.ModLoader;
using Terraria.ID;

public class ModBuffTest : ModBuff
{
	public override void SetDefaults() {
		canBeCleared = false;
		longerExpertDebuff = true;

		bool a = BuffLoader.CanBeCleared(0);

		BuffID.Sets.IsAnNPCWhipDebuff[Type] = true;
	}

	public override void ModifyBuffTip(ref string tip, ref int rare) { }
}