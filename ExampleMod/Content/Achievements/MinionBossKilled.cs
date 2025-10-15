using ExampleMod.Content.NPCs.MinionBoss;
using Terraria.ModLoader;

namespace ExampleMod.Content.Achievements;

// MinionBossKilled showcases an extremely simple ModAchievement.
// It is unlocked when MinionBossBody is defeated.
public class MinionBossKilled : ModAchievement
{
	public override void SetStaticDefaults() {
		// There are 4 AchievementCategory options: Slayer, Collector, Explorer, and Challenger.
		// Slayer is the default.
		// If you want to change the achievement's category, you can do this:
		// Achievement.SetCategory(AchievementCategory.Collector);

		// This achievement has only 1 condition. When MinionBossBody is defeated the achievement will be unlocked. There is no need to add code to MinionBossBody itself.
		AddNPCKilledCondition(ModContent.NPCType<MinionBossBody>());
	}

	// By default a ModAchievement will be placed at the end of the achievement ordering.
	// GetDefaultPosition is used to position a ModAchievement in relation to vanilla achievements.
	// Since MinionBoss is similar to Eye of Cthulhu, we place it after its achievement, "EYE_ON_YOU".
	public override Position GetDefaultPosition() => new After("EYE_ON_YOU");
}
