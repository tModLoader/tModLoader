using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class ModItemTest : ModItem
{
	public void IdentifierTest() {
		Console.Write(mod);
		item.SetDefaults(0);
		Console.Write(item);
		item.accessory = true;
		Console.Write(item.accessory);
		item.useTime += 2;

		Console.WriteLine(item.IsCandidateForReforge);
		item.CloneWithModdedDataFrom(item);

		SacrificeTotal = 1;
	}

	public override void SetStaticDefaults()
	{
		Tooltip.SetDefault(
			"This tooltip\n" +
			"Has multiple lines");
		Terraria.ID.AmmoID.Sets.IsRocket[Type] = true;
	}

	public override bool IgnoreDamageModifiers => false;

	public override bool OnlyShootOnSwing => false;

	public override bool CloneNewInstances => false;

	public override ModItem Clone() { return null; }

	public override bool NewPreReforge() { return false; /* comment */ }

	public override bool UseItem(Player player) { return true; /* comment */ }

	public override void HoldStyle(Player player) { /* comment */ }

	public override void UseStyle(Player player) { /* comment */ }

	public override bool CanEquipAccessory(Player player, int slot) { return true; /* comment */ }

	public override void NetRecieve(BinaryReader reader) { /* Empty */ }

	public override void GetWeaponKnockback(Player player, ref float knockback) { /* Empty */ }

	public override void GetWeaponCrit(Player player, ref int crit) { /* Empty */ }

	public override void ModifyWeaponDamage(Player player, ref float add, ref float mult, ref float flat) {
		add += 0.1f;
		mult *= 0.2f;
		flat += 4;
	}

	public override void OnCreate(ItemCreationContext context) {
		if (context is RecipeCreationContext) { }
		else if (context is InitializationContext) { }
	}

	public override bool DrawHead() { return true; /* Empty */ }

	public override bool DrawBody() { return true; /* Empty */ }

	public override bool DrawLegs() { return true; /* Empty */ }

	public override void DrawHands(ref bool drawHands, ref bool drawArms) { /* Empty */ }

	public override void DrawHair(ref bool drawHair, ref bool drawAltHair) { /* Empty */ }

	public override bool? CanBurnInLava() => null;

	public override void Load(TagCompound tag) { /* Empty */ }

	public override TagCompound Save() => new TagCompound();

	public override void ExtractinatorUse(ref int resultType, ref int resultStack) { /* Empty */ }

	public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit) { }
	public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit) { }
	public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit) { }
	public override void OnHitPvp(Player player, Player target, int damage, bool crit) { }

	public override void AutoLightSelect(ref bool dryTorch, ref bool wetTorch, ref bool glowstick)
	{
		dryTorch = true;
	}
}