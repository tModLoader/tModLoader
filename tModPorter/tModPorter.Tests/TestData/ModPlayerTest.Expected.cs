using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class ModPlayerTest : ModPlayer
{
	public void IdentifierTest() {
		Console.Write(Player);
	}

	public override void ModifyWeaponKnockback(Item item, ref StatModifier knockback) { /* Empty */ }

	public override void ModifyWeaponCrit(Item item, ref float crit) { /* Empty */ }

	public override void ModifyWeaponDamage(Item item, ref StatModifier damage) { /* Empty */ }

	public override void LoadData(TagCompound tag) { /* Empty */ }

	public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) { /* Empty */ }
}
