using Microsoft.XNA.Framework.Graphics;
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
#endif

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
}
