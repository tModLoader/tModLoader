using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;

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

	public override bool PreReforge() { return false; /* comment */ }

	public override bool? UseItem(Player player) { return true; /* comment */ }

	public override void UseStyle(Player player, Rectangle heldItemFrame) { /* comment */ }

	public override void NetReceive(BinaryReader reader) { /* Empty */ }

	public override void ModifyWeaponKnockback(Player player, ref StatModifier knockback) { /* Empty */ }

	public override void ModifyWeaponCrit(Player player, ref float crit) { /* Empty */ }

	public override void ModifyWeaponDamage(Player player, ref StatModifier damage) { /* Empty */ }

	public override void LoadData(TagCompound tag) { /* Empty */ }

	public override void SaveData(TagCompound tag)/* Edit tag parameter rather than returning new TagCompound */ => new TagCompound();
}