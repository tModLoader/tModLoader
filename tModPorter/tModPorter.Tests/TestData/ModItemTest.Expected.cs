using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

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

#if COMPILE_ERROR
	public override bool IgnoreDamageModifiers/* Suggestion: Removed. If you returned true, concider not setting Item.DamageType, or make a custom DamageClass which returns StatInheritanceData.None in GetModifierInheritance */ => false;

	public override bool OnlyShootOnSwing/* Suggestion: Removed. If you returned true, set Item.useTime to a multiple of Item.useAnimation */ => false;
#endif

	protected override bool CloneNewInstances => false;

	public override ModItem Clone(Item newEntity) { return null; }

	public override bool PreReforge() { return false; /* comment */ }

	public override bool? UseItem(Player player)/* Suggestion: Return null instead of false */ { return true; /* comment */ }

	public override void HoldStyle(Player player, Rectangle heldItemFrame) { /* comment */ }

	public override void UseStyle(Player player, Rectangle heldItemFrame) { /* comment */ }

	public override void EquipFrameEffects(Player player, EquipType type) { /* comment */ }

	public override void NetReceive(BinaryReader reader) { /* Empty */ }

	public override void ModifyWeaponKnockback(Player player, ref StatModifier knockback) { /* Empty */ }

	public override void ModifyWeaponCrit(Player player, ref float crit) { /* Empty */ }

	public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
		damage += 0.1f;
		damage *= 0.2f;
		damage.Flat += 4;
	}

	public override void LoadData(TagCompound tag) { /* Empty */ }

#if COMPILE_ERROR
	public override void SaveData(TagCompound tag)/* Suggestion: Edit tag parameter rather than returning new TagCompound */ => new TagCompound();
#endif
}