using ExampleMod.Content.Items.Placeable;
using Terraria;
using Terraria.Achievements;
using Terraria.GameContent.Achievements;
using Terraria.ModLoader;

namespace ExampleMod.Content.Achievements;

public class ManyExampleWormsKilled : ModAchievement
{
	public readonly CustomIntCondition IntCondition = new("ExampleIntCondition", 5);
	public override void SetStaticDefaults() {
		Achievement.Hidden = true;

		// There are 4 AchievementCategorys: Slayer, Collector, Explorer, and Challenger.
		// Slayer is the default.
		// If you want to change the achievement's category, you can do this:
		// Achievement.SetCategory(AchievementCategory.Collector);

		// Int conditions will automatically complete once you've incremented it enough.
		// ModContent.GetInstance<ManyExampleWormsKilled>().IntCondition.Value++;
		Achievement.AddCondition(IntCondition);
		Achievement.UseTracker(IntCondition.GetAchievementTracker());
	}

	public override void OnCompleted(Achievement achievement) {
		Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_FromThis(), ModContent.ItemType<ExampleBar>(), 15);
	}
}