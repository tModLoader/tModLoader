using System;
using Terraria;
using Terraria.ModLoader;

public class ModMountTest : ModMountData
{
	public void IdentifierTest() {
		Console.Write(mountData);
	}

	public override void JumpHeight(ref int jumpHeight, float xVelocity) { /* Empty */ }

	public override void JumpSpeed(ref float jumpSeed, float xVelocity) { /* Empty */ }

	public static Mount.MountData TypeAndMemberTest(ModMountData modMount) => modMount.mountData;

	public override void SetDefaults() { /* Empty */ }
}