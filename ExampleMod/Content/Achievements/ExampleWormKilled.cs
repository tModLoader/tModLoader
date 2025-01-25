using ExampleMod.Content.Achievements.Conditions;
using ExampleMod.Content.Items.Placeable;
using Terraria;
using Terraria.Achievements;
using Terraria.GameContent.Achievements;
using Terraria.ModLoader;

namespace ExampleMod.Content.Achievements;

public class ExampleWormKilled : ModAchievement
{
	public override void SetStaticDefaults() {
		Achievement.Hidden = false;

		// The achievement can be completed like this:
		// ModContent.GetModAchievement<ExampleWormKilled>().GetCondition("EXAMPLEMOD_KILL_BOOLEAN_CONDITION").Complete();
		Achievement.AddCondition(CustomFlagCondition.Create("EXAMPLEMOD_KILL_BOOLEAN_CONDITION"));
	}

	public override void OnCompleted(Achievement achievement) {
		Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_FromThis(), ModContent.ItemType<ExampleBar>(), 5);
		base.OnCompleted(achievement);
	}
}