using ExampleMod.Content.NPCs;
using Terraria;
using Terraria.GameContent.Achievements;
using Terraria.ModLoader;

namespace ExampleMod.Content.Achievements;

// ManyExampleWormsKilled is a more complicated example than MinionBossKilled.
// It is unlocked when ExampleWormHead has been defeated 5 times rather than just once.
public class ManyExampleWormsKilled : ModAchievement
{
	public CustomIntCondition Condition { get; private set; }

	// This achievement is "hidden", meaning its name and description will both appear as "???" in the achievements menu, until at least 1 ExampleWormHead has been killed.
	public override bool Hidden => Condition.Value == 0;

	public override void SetStaticDefaults() {
		// There are 4 AchievementCategory options: Slayer, Collector, Explorer, and Challenger.
		// Slayer is the default.
		// If you want to change the achievement's category, you can do this:
		// Achievement.SetCategory(AchievementCategory.Collector);

		// Unlike MinionBossKilled, which uses AddNPCKilledCondition, this ModAchievement uses AddIntCondition to track the 5 kills. This is necessary because AddNPCKilledCondition only supports tracking a single kill.
		Condition = AddIntCondition(5);

		// This approach requires manually incrementing Condition.Value to track the kill count. To do this, we subscribe to the AchievementsHelper.OnNPCKilled event, then in NPCKilledListener we increment our Condition by 1. See the NPCKilledListener method below.
		// We can't use the ExampleWormHead.OnKill method for this because that doesn't run on multiplayer clients.
		// The AchievementsHelper.OnNPCKilled event, however, properly runs for all clients that participated in a fight and damaged the NPC.
		AchievementsHelper.OnNPCKilled += NPCKilledListener;

		// Other AchievementCondition options include: AddCondition, AddFloatCondition, AddItemCraftCondition, AddItemPickupCondition, AddNPCKilledCondition, and AddTileDestroyedCondition.
		// If making a custom condition (AddCondition, AddFloatCondition, AddItemCraftCondition), make sure the logic is correct for multiplayer. Syncing effects through ModPacket might be required. Achievements are not loaded on the server so be sure to only increment custom condition values on clients to avoid null exceptions.
	}

	public override void Unload() {
		AchievementsHelper.OnNPCKilled -= NPCKilledListener;
	}

	private void NPCKilledListener(Player player, short npcId) {
		if (player.whoAmI != Main.myPlayer) {
			return;
		}

		if (npcId == ModContent.NPCType<ExampleWormHead>()) {
			// Int conditions will automatically complete once you've incremented it enough. There is no need to call the Complete method manually.
			Condition.Value++;
		}
	}
}
