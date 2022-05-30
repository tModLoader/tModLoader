using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;

public class ModItemTest : ModItem
{
	public void IdentifierTest() {
		Console.Write(Mod);
		Item.SetDefaults(0);
		Console.Write(Item);
		Item.accessory = true;
		Console.Write(Item.accessory);
		Item.useTime += 2;
	}

	public override bool? UseItem(Player player) { return true; /* comment */ }
	
	public override void NetReceive(BinaryReader reader) { /* Empty */ }
}