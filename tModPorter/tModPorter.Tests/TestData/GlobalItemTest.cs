using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class GlobalItemTest : GlobalItem
{
	public override bool CloneNewInstances => true;

	public override bool UseItem(Item item, Player player) => false;

	public override bool NewPreReforge(Item item) { return false; /* comment */ }

	public override void HoldStyle(Item item, Player player) { /* comment */ }

	public override void UseStyle(Item item, Player player) { /* comment */ }

	public override bool CanEquipAccessory(Item item, Player player, int slot) { return true; /* comment */ }

	public override void GetWeaponKnockback(Item item, Player player, ref float knockback) { /* Empty */ }

	public override void GetWeaponCrit(Item item, Player player, ref int crit) { /* Empty */ }

	public override void ModifyWeaponDamage(Item item, Player player, ref float add, ref float mult, ref float flat) {
		add += 0.1f;
		mult *= 0.2f;
		flat += 4;
	}

	public override void OnCreate(Item item, ItemCreationContext context) { }

	public override bool DrawHead(int head) { return true; /* Empty */ }

	public override bool DrawBody(int body) { return true; /* Empty */ }

	public override bool DrawLegs(int legs, int shoes) { return true; /* Empty */ }

	public override void DrawHands(int body, ref bool drawHands, ref bool drawArms) { /* Empty */ }

	public override void DrawHair(int head, ref bool drawHair, ref bool drawAltHair) { /* Empty */ }

	public override bool? CanBurnInLava(Item item) => null;

	public override void Load(Item item, TagCompound tag) { /* Empty */ }

	public override TagCompound Save(Item item) => new TagCompound();

	public override void ExtractinatorUse(int extractType, ref int resultType, ref int resultStack) { /* Empty */ }

	public override void ModifyHitNPC(Item item, Player player, NPC target, ref int damage, ref float knockBack, ref bool crit) { }
	public override void OnHitNPC(Item item, Player player, NPC target, int damage, float knockBack, bool crit) { }
	public override void ModifyHitPvp(Item item, Player player, Player target, ref int damage, ref bool crit) { }
	public override void OnHitPvp(Item item, Player player, Player target, int damage, bool crit) { }
}