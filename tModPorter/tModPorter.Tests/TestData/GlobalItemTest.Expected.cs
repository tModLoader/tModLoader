using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class GlobalItemTest : GlobalItem
{
	protected override bool CloneNewInstances => true;

	public override bool? UseItem(Item item, Player player)/* tModPorter Suggestion: Return null instead of false */ => false;

	public override bool PreReforge(Item item) { return false; /* comment */ }

	public override void HoldStyle(Item item, Player player, Rectangle heldItemFrame) { /* comment */ }

	public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) { /* comment */ }

	public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)/* tModPorter Suggestion: Consider using new hook CanAccessoryBeEquippedWith */ { return true; /* comment */ }

	public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback) { /* Empty */ }

	public override void ModifyWeaponCrit(Item item, Player player, ref float crit) { /* Empty */ }

	public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) {
		damage += 0.1f;
		damage *= 0.2f;
		damage.Flat += 4;
	}

#if COMPILE_ERROR
	public override bool DrawHead(int head)/* tModPorter Note: Removed. In SetStaticDefaults, use ArmorIDs.Head.Sets.DrawHead[head] = false if you returned false */ { return true; /* Empty */ }

	public override bool DrawBody(int body)/* tModPorter Note: Removed. In SetStaticDefaults, use ArmorIDs.Body.Sets.HidesTopSkin[body] = true if you returned false */ { return true; /* Empty */ }

	public override bool DrawLegs(int legs, int shoes)/* tModPorter Note: Removed. In SetStaticDefaults, use ArmorIDs.Legs.Sets.HidesBottomSkin[legs] = true, and ArmorIDs.Shoe.Sets.OverridesLegs[shoes] = true */ { return true; /* Empty */ }

	public override void DrawHands(int body, ref bool drawHands, ref bool drawArms)/* tModPorter Note: Removed. In SetStaticDefaults, use ArmorIDs.Body.Sets.HidesHands[body] = false if you had drawHands set to true. If you had drawArms set to true, you don't need to do anything */ { /* Empty */ }

	public override void DrawHair(int head, ref bool drawHair, ref bool drawAltHair)/* tModPorter Note: Removed. In SetStaticDefaults, use ArmorIDs.Head.Sets.DrawFullHair[head] = true if you had drawHair set to true, and ArmorIDs.Head.Sets.DrawHatHair[head] = true if you had drawAltHair set to true */ { /* Empty */ }
#endif

	public override void LoadData(Item item, TagCompound tag) { /* Empty */ }

#if COMPILE_ERROR
	public override void SaveData(Item item, TagCompound tag)/* tModPorter Suggestion: Edit tag parameter instead of returning new TagCompound */ => new TagCompound();
#endif
}