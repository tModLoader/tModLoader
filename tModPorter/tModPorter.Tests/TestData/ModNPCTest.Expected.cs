using System;
using Terraria.ModLoader;

public class ModNPCTest : ModNPC
{
	public void IdentifierTest() {
		Console.Write(NPC);
	}

	public override void OnKill() {  /*empty*/ }
}
