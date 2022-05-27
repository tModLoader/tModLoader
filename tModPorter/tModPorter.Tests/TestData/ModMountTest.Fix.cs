using System;
using Terraria;
using Terraria.ModLoader;

public class ModMountTest : ModMount
{
	public void IdentifierTest() {
		Console.Write(MountData);
	}

	public static Mount.MountData TypeAndMemberTest(ModMount modMount) => modMount.MountData;

	public override void SetStaticDefaults() { /* Empty */ }
}
