using ExampleMod.Content.Items.Placeable;
using Terraria;
using Terraria.Achievements;
using Terraria.GameContent.Achievements;
using Terraria.ModLoader;

namespace ExampleMod.Content.Achievements;

public class ManyExampleWormsKilled : ModAchievement
{
	public override void SetStaticDefaults() {
		Achievement.Hidden = true;

		// Int conditions will automatically complete once you've incremented it enough.
		// ((CustomIntCondition)ModContent.GetModAchievement<ManyExampleWormsKilled>().GetCondition("EXAMPLEMOD_KILL_WORMS")).Value++;
		Achievement.AddCondition(CustomIntCondition.Create("EXAMPLEMOD_KILL_WORMS", 5));
		Achievement.UseTrackerFromCondition("EXAMPLEMOD_KILL_WORMS");
	}

	public override void OnCompleted(Achievement achievement) {
		Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_FromThis(), ModContent.ItemType<ExampleBar>(), 15);
		base.OnCompleted(achievement);
	}
}