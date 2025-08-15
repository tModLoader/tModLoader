using ExampleMod.Content.NPCs;
using Terraria.ModLoader;

namespace ExampleMod.Content.Achievements;

// ExampleWormKilled showcases an extremely simple ModAchievement.
// It is unlocked when ExampleWormHead is defeated.
public class ExampleWormKilled : ModAchievement
{
	public override void SetStaticDefaults() {
		// There are 4 AchievementCategory options: Slayer, Collector, Explorer, and Challenger.
		// Slayer is the default.
		// If you want to change the achievement's category, you can do this:
		// Achievement.SetCategory(AchievementCategory.Collector);

		// This achievement has only 1 condition. When ExampleWormHead is defeated the achievement will be unlocked
		AddNPCKilledCondition(ModContent.NPCType<ExampleWormHead>());
	}
}
