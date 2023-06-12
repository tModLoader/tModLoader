using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class GlobalItemTest : GlobalItem
{
	protected override bool CloneNewInstances => true;

	public override bool? UseItem(Item item, Player player)/* tModPorter Suggestion: Return null instead of false */ => false;

#if COMPILE_ERROR
	public override void PreReforge(Item item)/* tModPorter Note: Use CanReforge instead for logic determining if a reforge can happen. */ { return false; /* comment */ }
#endif

	public override void HoldStyle(Item item, Player player, Rectangle heldItemFrame) { /* comment */ }

	public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) { /* comment */ }

	public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)/* tModPorter Suggestion: Consider using new hook CanAccessoryBeEquippedWith */ { return true; /* comment */ }

	public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback) { /* Empty */ }

	public override void ModifyWeaponCrit(Item item, Player player, ref float crit) { /* Empty */ }

	public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) {
		// not-yet-implemented
		damage += 0.1f;
		damage *= 0.2f;
		damage.Flat += 4;
		// instead-expect
#if COMPILE_ERROR
		add += 0.1f;
		mult *= 0.2f;
		flat += 4;
#endif
	}

	public override void OnCreated(Item item, ItemCreationContext context) { }

#if COMPILE_ERROR
	public override bool DrawHead(int head)/* tModPorter Note: Removed. In SetStaticDefaults, use ArmorIDs.Head.Sets.DrawHead[head] = false if you returned false */ { return true; /* Empty */ }

	public override bool DrawBody(int body)/* tModPorter Note: Removed. In SetStaticDefaults, use ArmorIDs.Body.Sets.HidesTopSkin[body] = true if you returned false */ { return true; /* Empty */ }

	public override bool DrawLegs(int legs, int shoes)/* tModPorter Note: Removed. In SetStaticDefaults, use ArmorIDs.Legs.Sets.HidesBottomSkin[legs] = true, and ArmorIDs.Shoe.Sets.OverridesLegs[shoes] = true */ { return true; /* Empty */ }

	public override void DrawHands(int body, ref bool drawHands, ref bool drawArms)/* tModPorter Note: Removed. In SetStaticDefaults, use ArmorIDs.Body.Sets.HidesHands[body] = false if you had drawHands set to true. If you had drawArms set to true, you don't need to do anything */ { /* Empty */ }

	public override void DrawHair(int head, ref bool drawHair, ref bool drawAltHair)/* tModPorter Note: Removed. In SetStaticDefaults, use ArmorIDs.Head.Sets.DrawFullHair[head] = true if you had drawHair set to true, and ArmorIDs.Head.Sets.DrawHatHair[head] = true if you had drawAltHair set to true */ { /* Empty */ }

	public override bool? CanBurnInLava(Item item)/* tModPorter Note: Removed. Use ItemID.Sets.IsLavaImmuneRegardlessOfRarity or add a method hook to On_Item.CheckLavaDeath */ => null;
#endif

	public override void LoadData(Item item, TagCompound tag) { /* Empty */ }

#if COMPILE_ERROR
	public override void SaveData(Item item, TagCompound tag)/* tModPorter Suggestion: Edit tag parameter instead of returning new TagCompound */ => new TagCompound();
#endif

	public override void ExtractinatorUse(int extractType, int extractinatorBlockType, ref int resultType, ref int resultStack) { /* Empty */ }

	public override void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers) { }
	public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone) { }
	public override void ModifyHitPvp(Item item, Player player, Player target, ref Player.HurtModifiers modifiers) { }
	public override void OnHitPvp(Item item, Player player, Player target, Player.HurtInfo hurtInfo) { }
}