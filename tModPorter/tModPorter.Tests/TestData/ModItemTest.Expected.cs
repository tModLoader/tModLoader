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
	public override bool IgnoreDamageModifiers/* tModPorter Note: Removed. If you returned true, consider leaving Item.DamageType as DamageClass.Default, or make a custom DamageClass which returns StatInheritanceData.None in GetModifierInheritance */ => false;

	public override bool OnlyShootOnSwing/* tModPorter Note: Removed. If you returned true, set Item.useTime to a multiple of Item.useAnimation */ => false;
#endif

	protected override bool CloneNewInstances => false;

	public override ModItem Clone(Item newEntity) { return null; }

	public override bool PreReforge() { return false; /* comment */ }

	public override bool? UseItem(Player player)/* tModPorter Suggestion: Return null instead of false */ { return true; /* comment */ }

	public override void HoldStyle(Player player, Rectangle heldItemFrame) { /* comment */ }

	public override void UseStyle(Player player, Rectangle heldItemFrame) { /* comment */ }

	public override bool CanEquipAccessory(Player player, int slot, bool modded)/* tModPorter Suggestion: Consider using new hook CanAccessoryBeEquippedWith */ { return true; /* comment */ }

	public override void NetReceive(BinaryReader reader) { /* Empty */ }

	public override void ModifyWeaponKnockback(Player player, ref StatModifier knockback) { /* Empty */ }

	public override void ModifyWeaponCrit(Player player, ref float crit) { /* Empty */ }

	public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
		damage += 0.1f;
		damage *= 0.2f;
		damage.Flat += 4;
	}

#if COMPILE_ERROR
	public override bool DrawHead()/* tModPorter Note: Removed. In SetStaticDefaults, use ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false if you returned false */ { return true; /* Empty */ }

	public override bool DrawBody()/* tModPorter Note: Removed. In SetStaticDefaults, use ArmorIDs.Body.Sets.HidesTopSkin[Item.bodySlot] = true if you returned false */ { return true; /* Empty */ }

	public override bool DrawLegs()/* tModPorter Note: Removed. In SetStaticDefaults, use ArmorIDs.Legs.Sets.HidesBottomSkin[Item.legSlot] = true if you returned false for an accessory of EquipType.Legs, and ArmorIDs.Shoe.Sets.OverridesLegs[Item.shoeSlot] = true if you returned false for an accessory of EquipType.Shoes */ { return true; /* Empty */ }

	public override void DrawHands(ref bool drawHands, ref bool drawArms)/* tModPorter Note: Removed. In SetStaticDefaults, use ArmorIDs.Body.Sets.HidesHands[Item.bodySlot] = false if you had drawHands set to true. If you had drawArms set to true, you don't need to do anything */ { /* Empty */ }

	public override void DrawHair(ref bool drawHair, ref bool drawAltHair)/* tModPorter Note: Removed. In SetStaticDefaults, use ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true if you had drawHair set to true, and ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true if you had drawAltHair set to true */ { /* Empty */ }
#endif

	public override void LoadData(TagCompound tag) { /* Empty */ }

#if COMPILE_ERROR
	public override void SaveData(TagCompound tag)/* tModPorter Suggestion: Edit tag parameter instead of returning new TagCompound */ => new TagCompound();
#endif
}