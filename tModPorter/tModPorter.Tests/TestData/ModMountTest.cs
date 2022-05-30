using System;
using Terraria;
using Terraria.ModLoader;

public class ModMountTest : ModMountData
{
	public void IdentifierTest() {
		Console.Write(mountData);
	}

	public static Mount.MountData TypeAndMemberTest(ModMountData modMount) => modMount.mountData;

	public override void SetDefaults() { /* Empty */ }
}
