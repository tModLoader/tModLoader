using System;
using Terraria;
using Terraria.ModLoader;

public class ModGoreTest : ModGore
{
	public void IdentifierTest() {
		Console.Write(updateType);
	}

	public override void OnSpawn(Gore gore) { /* Empty */ }

	public override bool DrawBehind(Gore gore) { return false; }
}