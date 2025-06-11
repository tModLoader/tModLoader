using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class ModPlayerTest : ModPlayer
{
	public void IdentifierTest() {
		Console.Write(player);
	}

	public override void GetWeaponKnockback(Item item, ref float knockback) { /* Empty */ }

	public override void GetWeaponCrit(Item item, ref int crit) { /* Empty */ }

	public override void ModifyWeaponDamage(Item item, ref float add, ref float mult, ref float flat) {
		add += 0.1f;
		mult *= 0.2f;
		flat += 4;
	}

	public override void Load(TagCompound tag) { /* Empty */ }

	public override TagCompound Save() {
		return new TagCompound();
	}

	public override void SetupStartInventory(IList<Item> items, bool mediumcoreDeath) {
		items.Add(9);
	}

	public override void SetupStartInventory(IList<Item> items) {
		items.Add(9);
	}

	public override Texture2D SetMapBackgroundImage() {
		return null
	}

	public override void DrawEffects(PlayerDrawInfo drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) { /* Empty */ }

	public override void CatchFish(Item fishingRod, Item bait, int power, int liquidType, int poolSize, int worldLayer, int questFish, ref int caughtType, ref bool junk) {
		// The following are 1.3 parameters written out using new 1.4 syntax
		Item fishingRodLocal = fishingRod;
		Item baitLocal = bait;
		int powerLocal = power;
		int liquidTypeLocal = liquidType;
		int poolSizeLocal = poolSize;
		int worldLayerLocal = worldLayer;
		int questFishLocal = questFish;
		ref int caughtTypeLocal = ref caughtType;
		// ref int junkLocal = ref junk; // Can't really be transformed, unless you check for fisher.rolledItemDrop = Main.rand.Next(2337, 2340);
	}

	public override void clientClone(ModPlayer clientClone)
	{ }

	public override void PlayerConnect(Player player) { }
	public override void PlayerDisconnect(Player player) { }
	public override void OnEnterWorld(Player player) { }
	public override void OnRespawn(Player player) { }

	public void UseQuickSpawnItem() {
		Item item = new Item(22);
		Player.QuickSpawnClonedItem(null, item);
	}

	public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit,
		ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter)
	{
		return false;
	}
	public override void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter) { }
	public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter) { }

	public override void ModifyHitByNPC(NPC npc, ref int damage, ref bool crit) { }
	public override void OnHitByNPC(NPC npc, int damage, bool crit) { }
	public override void ModifyHitByProjectile(Projectile proj, ref int damage, ref bool crit) { }
	public override void OnHitByProjectile(Projectile proj, int damage, bool crit) { }

	public override bool? CanHitNPC(Item item, NPC target) => null;
	public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit) { }
	public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit) { }
	public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) { }
	public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit) { }
	public override void ModifyHitPvp(Item item, Player target, ref int damage, ref bool crit) { }
	public override void OnHitPvp(Item item, Player target, int damage, bool crit) { }
	public override void ModifyHitPvpWithProj(Projectile proj, Player target, ref int damage, ref bool crit) { }
	public override void OnHitPvpWithProj(Projectile proj, Player target, int damage, bool crit) { }

	public override bool FreeDodge(PlayerDeathReason damageSource, int cooldownCounter) => false;
	public override bool ConsumableDodge(PlayerDeathReason damageSource, int cooldownCounter) => false;
}
