using Terraria;
using Terraria.DataStructures;
using SpawnParams = Terraria.NPCSpawnParams;

public class NPCDifficultyMigrationTest
{
	void Method(NPC npc) {
#if COMPILE_ERROR
		NPCSpawnParams spawnParams = new NPCSpawnParams {
			strengthMultiplierOverride/* tModPorter Note: Removed. Use difficultyOverride instead. It specifies effective difficulty, not a strength multiplier. Review old values and calculations manually. */ = 1f
		};
		NPCSpawnParams matching = new() {
			strengthMultiplierOverride/* tModPorter Note: Removed. Use difficultyOverride instead. It specifies effective difficulty, not a strength multiplier. Review old values and calculations manually. */ = npc.strengthMultiplier/* tModPorter Note: Removed. Use difficulty instead. It records effective difficulty, not a strength multiplier. Review old values and calculations manually. */,
			playerCountForMultiplayerDifficultyOverride = npc.statsAreScaledForThisManyPlayers
		};
		SpawnParams aliased = new SpawnParams {
			strengthMultiplierOverride/* tModPorter Note: Removed. Use difficultyOverride instead. It specifies effective difficulty, not a strength multiplier. Review old values and calculations manually. */ = null
		};
		spawnParams.strengthMultiplierOverride/* tModPorter Note: Removed. Use difficultyOverride instead. It specifies effective difficulty, not a strength multiplier. Review old values and calculations manually. */ = 2f;
		var strengthOverride = spawnParams.strengthMultiplierOverride/* tModPorter Note: Removed. Use difficultyOverride instead. It specifies effective difficulty, not a strength multiplier. Review old values and calculations manually. */;
		var strength = npc.strengthMultiplier/* tModPorter Note: Removed. Use difficulty instead. It records effective difficulty, not a strength multiplier. Review old values and calculations manually. */;
		npc.strengthMultiplier/* tModPorter Note: Removed. Use difficulty instead. It records effective difficulty, not a strength multiplier. Review old values and calculations manually. */ = 1f;
#endif

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
