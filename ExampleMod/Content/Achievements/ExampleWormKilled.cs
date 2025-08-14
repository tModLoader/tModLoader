using ExampleMod.Content.Items.Placeable;
using Terraria;
using Terraria.Achievements;
using Terraria.GameContent.Achievements;
using Terraria.ModLoader;

namespace ExampleMod.Content.Achievements;

public class ExampleWormKilled : ModAchievement
{
	// The CustomFlagCondition used for this Achievement.
	public CustomFlagCondition Condition { get; private set; }

	public override void SetStaticDefaults() {
		Achievement.Hidden = false;

		// There are 4 AchievementCategory options: Slayer, Collector, Explorer, and Challenger.
		// Slayer is the default.
		// If you want to change the achievement's category, you can do this:
		// Achievement.SetCategory(AchievementCategory.Collector);

		// The achievement can be completed like this:
		// ModContent.GetInstance<ExampleWormKilled>().Condition.Complete();
		Condition = AddCondition();
	}

	public override void OnCompleted(Achievement achievement) {
		Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_FromThis(), ModContent.ItemType<ExampleBar>(), 5);
	}
}
