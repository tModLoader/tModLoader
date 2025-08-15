using ExampleMod.Content.Items.Placeable;
using Terraria;
using Terraria.Achievements;
using Terraria.GameContent.Achievements;
using Terraria.ModLoader;

namespace ExampleMod.Content.Achievements;

// ManyExampleWormsKilled is a more complicated example than ExampleWormKilled.
// It is unlocked when ExampleWormHead has been defeated 5 times rather than just once.
public class ManyExampleWormsKilled : ModAchievement
{
	public CustomIntCondition Condition { get; private set; }

	public override void SetStaticDefaults() {
		Achievement.Hidden = true;

		// There are 4 AchievementCategory options: Slayer, Collector, Explorer, and Challenger.
		// Slayer is the default.
		// If you want to change the achievement's category, you can do this:
		// Achievement.SetCategory(AchievementCategory.Collector);

		// Unlike ExampleWormKilled, which uses AddNPCKilledCondition, this ModAchievement uses AddIntCondition to track the 5 kills. This is necessary because AddNPCKilledCondition only supports tracking a single kill.
		// This approach also requires manually incrementing Condition.Value to track the kill count, as seen in the ExampleWormHead.OnKill method: ModContent.GetInstance<ManyExampleWormsKilled>().Condition.Value++;
		// Int conditions will automatically complete once you've incremented it enough.
		Condition = AddIntCondition(5);

		// Other AchievementCondition options include: AddFloatCondition, AddItemCraftCondition, AddItemPickupCondition, AddNPCKilledCondition, and AddTileDestroyedCondition. AddCondition can be used for custom AchievementCondition classes
	}

	public override void OnCompleted(Achievement achievement) {
		// TODO: Fireworks?
		Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_FromThis(), ModContent.ItemType<ExampleBar>(), 15);
	}
}
