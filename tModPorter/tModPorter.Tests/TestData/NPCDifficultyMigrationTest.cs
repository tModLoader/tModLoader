using Terraria;
using Terraria.DataStructures;
using SpawnParams = Terraria.NPCSpawnParams;

public class NPCDifficultyMigrationTest
{
	void Method(NPC npc) {
		NPCSpawnParams spawnParams = new NPCSpawnParams {
			strengthMultiplierOverride = 1f
		};
		NPCSpawnParams matching = new() {
			strengthMultiplierOverride = npc.strengthMultiplier,
			playerCountForMultiplayerDifficultyOverride = npc.statsAreScaledForThisManyPlayers
		};
		SpawnParams aliased = new SpawnParams {
			strengthMultiplierOverride = null
		};
		spawnParams.strengthMultiplierOverride = 2f;
		var strengthOverride = spawnParams.strengthMultiplierOverride;
		var strength = npc.strengthMultiplier;
		npc.strengthMultiplier = 1f;

		NPCSpawnParams current = new() {
			difficultyOverride = npc.difficulty,
			playerCountForMultiplayerDifficultyOverride = npc.statsAreScaledForThisManyPlayers
		};
		current.difficultyOverride = GameDifficultyLevel.Expert;
		npc.difficulty = GameDifficultyLevel.Expert;
		var difficulty = npc.difficulty;

		var other = new OtherSpawnParams {
			strengthMultiplierOverride = 1f,
			strengthMultiplier = 1f
		};
		other.strengthMultiplierOverride = null;
		other.strengthMultiplier = 2f;
		var otherOverride = other.strengthMultiplierOverride;
		var otherStrength = other.strengthMultiplier;
	}

	class OtherSpawnParams
	{
		public float? strengthMultiplierOverride;
		public float strengthMultiplier;
	}
}
