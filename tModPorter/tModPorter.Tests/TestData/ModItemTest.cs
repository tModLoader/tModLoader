using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;

public class ModItemTest : ModItem
{
	public void IdentifierTest() {
		Console.Write(mod);
		item.SetDefaults(0);
		Console.Write(item);
		item.accessory = true;
		Console.Write(item.accessory);
		item.useTime += 2;
	}

	public override bool UseItem(Player player) { return true; /* comment */ }
	
	public override void NetRecieve(BinaryReader reader) { /* Empty */ }
}