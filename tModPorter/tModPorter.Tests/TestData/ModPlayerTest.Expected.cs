using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;

public class ModPlayerTest : ModPlayer
{
	public void IdentifierTest() {
		Console.Write(Player);
	}

	public override void ModifyWeaponKnockback(Item item, ref StatModifier knockback) { /* Empty */ }

	public override void ModifyWeaponCrit(Item item, ref float crit) { /* Empty */ }

	public override void ModifyWeaponDamage(Item item, ref StatModifier damage) { /* Empty */ }

	public override void LoadData(TagCompound tag) { /* Empty */ }

#if COMPILE_ERROR
	public override void SaveData(TagCompound tag)/* Edit tag parameter rather than returning new TagCompound */ {
		return new TagCompound();
	}
#endif

	public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) { /* Empty */ }

	public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
		// The following are 1.3 parameters written out using new 1.4 syntax
		Item fishingRodLocal = attempt.playerFishingConditions.Pole;
		Item baitLocal = attempt.playerFishingConditions.Bait;
		int powerLocal = attempt.fishingLevel;
		int liquidTypeLocal = attempt.inHoney ? 2 : attempt.inLava ? 1 : 0;
		int poolSizeLocal = attempt.waterTilesCount;
		int worldLayerLocal = attempt.heightLevel;
		int questFishLocal = attempt.questFish;
		ref int caughtTypeLocal = ref itemDrop;
		// ref int junkLocal = ref junk; // Can't really be transformed, unless you check for fisher.rolledItemDrop = Main.rand.Next(2337, 2340);
	}
}
