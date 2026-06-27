using ExampleMod.Content.NPCs;
using ExampleMod.Content.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Achievements;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Achievements;

// This example showcases more advanced features of ModAchievement.
// See MinionBossKilled and then ManyExampleWormsKilled first to learn the basics.
public class AdvancedExampleAchievement : ModAchievement
{
	// It is possible to share a texture and use Index to choose which icon is used from a texture.
	public override string TextureName => "ExampleMod/Content/Achievements/AllAchievements";
	public override int Index => 1;

	public CustomFlagCondition IronPickaxeCondition { get; private set; }
	public CustomIntCondition TotalDamageCondition { get; private set; }

	public override void SetStaticDefaults() {
		Achievement.SetCategory(AchievementCategory.Challenger);

		// This example showcases using multiple AchievementCondition in a single ModAchievement.
		// All of the conditions involve ExampleCritterNPC or Frog.

		// ExampleCritterNPC Hit once with a Iron Pickaxe
		IronPickaxeCondition = AddCondition();

		// 100 total damage dealt to ExampleCritterNPC
		TotalDamageCondition = AddIntCondition("TotalDamage", 100);

		// AddCondition, AddIntCondition, and AddFloatCondition all use the default key of "Condition". If you are using multiple of any of these, they need unique keys.
		// The other conditions will automatically have an appropriate key, but they can be customized if needed.

		// ExampleCritterNPC and Frog killed. Since these are 2 separate conditions, each needs to be killed.
		var KillExampleCritterCondition = AddNPCKilledCondition(ModContent.NPCType<ExampleCritterNPC>());
		var KillFrogCondition = AddNPCKilledCondition(NPCID.Frog);

		// The ExampleCritterNPC or Frog items being caught. Since these are in the same condition, either will satisfy this condition.
		var PickupCondition = AddItemPickupCondition([ModContent.ItemType<ExampleCritterItem>(), ItemID.Frog]);

		// The ExampleCritterCageItem or FrogCage items being crafted.
		var CraftCageCondition = AddItemCraftCondition([ModContent.ItemType<ExampleCritterCageItem>(), ItemID.FrogCage]);
		// There is a OnComplete event for AchievementCondition available, if you need it.
		CraftCageCondition.OnComplete += (condition) => Main.NewText($"You are making progress towards '{Achievement.FriendlyName}', keep at it.");

		// The ExampleCritterCageItem or FrogCage tiles being mined.
		var MineCageCondition = AddTileDestroyedCondition([ModContent.TileType<ExampleCritterCage>(), TileID.FrogCage]);

		// By default, a ModAchievement will have a suitable IAchievementTracker assigned. A ModAchievement with multiple conditions like this one will automatically use the ConditionsCompletedTracker as its IAchievementTracker. ConditionsCompletedTracker will simply count how many conditions out of the total are completed and report that as the progress.
		// Rather than that, we are using a custom IAchievementTracker that calculates the progress by weighting each condition to demonstrate how to implement and use a custom IAchievementTracker.
		Achievement.UseTracker(new CustomTracker(
			new Dictionary<AchievementCondition, float>() {
				[IronPickaxeCondition] = 1f,
				[TotalDamageCondition] = 2f,
				[KillExampleCritterCondition] = 0.5f,
				[KillFrogCondition] = 0.5f,
				[PickupCondition] = 4f,
				[CraftCageCondition] = 1f,
				[MineCageCondition] = 1f,
			}
		));
	}

	// We can use GetModdedConstraints to dictate the relative ordering of ModAchievements. (Remember that GetDefaultPosition is used to position a ModAchievement in relation to vanilla achievements.
	// Since both ManyExampleWormsKilled and AdvancedExampleAchievement do not use GetDefaultPosition, they will be at the end of the listing.
	// By default they will be in load order relative to each other, which in this case would put AdvancedExampleAchievement before ManyExampleWormsKilled since autoload happens in alphabetical order, so we can use GetModdedConstraints like this to order AdvancedExampleAchievement after ManyExampleWormsKilled.
	public override IEnumerable<Position> GetModdedConstraints() {
		yield return new After(ModContent.GetInstance<ManyExampleWormsKilled>());
	}

	public override void OnCompleted(Achievement achievement) {
		// Make some fireworks
		int fireworkProjectile = ProjectileID.RocketFireworksBoxRed + Main.rand.Next(4);
		Projectile.NewProjectile(Main.LocalPlayer.GetSource_FromThis(), Main.LocalPlayer.Top, new Vector2(Main.rand.NextFloat(-2, 2), -Main.rand.NextFloat(2f, 4f)).RotatedByRandom(0.3f), fireworkProjectile, 0, 0, Main.myPlayer);
	}
}

public class CustomTracker : ConditionFloatTracker
{
	private Dictionary<AchievementCondition, float> weightedConditions;

	public CustomTracker(Dictionary<AchievementCondition, float> weightedConditions) {
		this.weightedConditions = weightedConditions;
		foreach ((AchievementCondition condition, float weight) in weightedConditions) {
			_maxValue += weight;
			condition.OnComplete += OnConditionCompleted;
		}
	}

	private void OnConditionCompleted(AchievementCondition condition) {
		SetValue(Math.Min(_value + weightedConditions[condition], _maxValue));
	}

	protected override void Load() {
		foreach ((AchievementCondition condition, float weight) in weightedConditions) {
			if (condition.IsCompleted)
				_value += weight;
		}
	}
}
