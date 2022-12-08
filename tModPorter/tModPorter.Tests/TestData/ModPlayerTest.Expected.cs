using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class ModPlayerTest : ModPlayer
{
	public void IdentifierTest() {
		Console.Write(Player);
	}

	public override void ModifyWeaponKnockback(Item item, ref StatModifier knockback) { /* Empty */ }

	public override void ModifyWeaponCrit(Item item, ref float crit) { /* Empty */ }

	public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
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

	public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter) => true;
	public override void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter) { }
	public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter) { }

	public override void LoadData(TagCompound tag) { /* Empty */ }

#if COMPILE_ERROR
	public override void SaveData(TagCompound tag)/* tModPorter Suggestion: Edit tag parameter instead of returning new TagCompound */ {
		return new TagCompound();
	}

	public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)/* tModPorter Suggestion: Return an Item array to add to the players starting items. Use ModifyStartingInventory for modifying them if needed */ {
		items.Add(9);
	}

	public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)/* tModPorter Suggestion: Return an Item array to add to the players starting items. Use ModifyStartingInventory for modifying them if needed */ {
		items.Add(9);
	}

	public override Texture2D SetMapBackgroundImage()/* tModPorter Note: Removed. Create a ModBiome (or ModSceneEffect) class and override MapBackground property to return this object through Mod/ModContent.Find, then move this code into IsBiomeActive (or IsSceneEffectActive) */ {
		return null
	}
#endif

	public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) { /* Empty */ }

	public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
		// The following are 1.3 parameters written out using new 1.4 syntax
		// not-yet-implemented
		Item fishingRodLocal = attempt.playerFishingConditions.Pole;
		Item baitLocal = attempt.playerFishingConditions.Bait;
		int powerLocal = attempt.fishingLevel;
		int liquidTypeLocal = attempt.inHoney ? 2 : attempt.inLava ? 1 : 0;
		int poolSizeLocal = attempt.waterTilesCount;
		int worldLayerLocal = attempt.heightLevel;
		int questFishLocal = attempt.questFish;
		ref int caughtTypeLocal = ref itemDrop;
		// instead-expect
#if COMPILE_ERROR
		Item fishingRodLocal = fishingRod;
		Item baitLocal = bait;
		int powerLocal = power;
		int liquidTypeLocal = liquidType;
		int poolSizeLocal = poolSize;
		int worldLayerLocal = worldLayer;
		int questFishLocal = questFish;
		ref int caughtTypeLocal = ref caughtType;
#endif
		// ref int junkLocal = ref junk; // Can't really be transformed, unless you check for fisher.rolledItemDrop = Main.rand.Next(2337, 2340);
	}

	public void UseQuickSpawnItem() {
		Item item = new Item(22);
		Player.QuickSpawnItem(null, item);
	}
}
