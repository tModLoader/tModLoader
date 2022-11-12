﻿using Microsoft.Xna.Framework.Graphics;
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

	public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit,
			ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) => true;
	public override void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit) { }
	public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit) { }

	public override void Load(TagCompound tag) { /* Empty */ }

#if COMPILE_ERROR
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
#endif

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

	public void UseQuickSpawnItem() {
		Item item = new Item(22);
		Player.QuickSpawnClonedItem(null, item);
	}
}
