using Terraria;
using Terraria.DataStructures;
using SpawnParams = Terraria.NPCSpawnParams;

public class NPCSpawnParamsTest
{
	void Method() {
#if COMPILE_ERROR
		NPCSpawnParams spawnParams = new NPCSpawnParams {
			gameModeData/* tModPorter Note: Removed. Use difficultyOverride with a GameDifficultyLevel value, or leave it null to use Main.Difficulty. Review custom GameModeData settings manually. */ = null
		};
		NPCSpawnParams fromWorldMode = new NPCSpawnParams {
			gameModeData/* tModPorter Note: Removed. Use difficultyOverride with a GameDifficultyLevel value, or leave it null to use Main.Difficulty. Review custom GameModeData settings manually. */ = Main.GameModeInfo
		};
		NPCSpawnParams targetTyped = new() {
			gameModeData/* tModPorter Note: Removed. Use difficultyOverride with a GameDifficultyLevel value, or leave it null to use Main.Difficulty. Review custom GameModeData settings manually. */ = null
		};
		SpawnParams aliased = new SpawnParams {
			gameModeData/* tModPorter Note: Removed. Use difficultyOverride with a GameDifficultyLevel value, or leave it null to use Main.Difficulty. Review custom GameModeData settings manually. */ = null
		};
		spawnParams.gameModeData/* tModPorter Note: Removed. Use difficultyOverride with a GameDifficultyLevel value, or leave it null to use Main.Difficulty. Review custom GameModeData settings manually. */ = null;
		var gameMode = spawnParams.gameModeData/* tModPorter Note: Removed. Use difficultyOverride with a GameDifficultyLevel value, or leave it null to use Main.Difficulty. Review custom GameModeData settings manually. */;
#endif

		NPCSpawnParams current = new() {
			difficultyOverride = GameDifficultyLevel.Expert
		};
		current.difficultyOverride = null;
		var difficulty = current.difficultyOverride;

		var other = new OtherSpawnParams {
			gameModeData = null
		};
		other.gameModeData = null;
		var otherGameMode = other.gameModeData;
	}

	class OtherSpawnParams
	{
		public object gameModeData;
	}
}
