using System;
using Terraria;
using Terraria.ModLoader;

public class ModMountTest : ModMount
{
	public void IdentifierTest() {
		Console.Write(MountData);
	}

	public override void JumpHeight(Player mountedPlayer, ref int jumpHeight, float xVelocity) { /* Empty */ }

	public override void JumpSpeed(Player mountedPlayer, ref float jumpSeed, float xVelocity) { /* Empty */ }

	public static Mount.MountData TypeAndMemberTest(ModMount modMount) => modMount.MountData;

	public override void SetStaticDefaults() { /* Empty */ }
}