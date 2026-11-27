using Terraria;
using Terraria.DataStructures;
using SpawnParams = Terraria.NPCSpawnParams;

public class NPCSpawnParamsTest
{
	void Method() {
		NPCSpawnParams spawnParams = new NPCSpawnParams {
			gameModeData = null
		};
		NPCSpawnParams fromWorldMode = new NPCSpawnParams {
			gameModeData = Main.GameModeInfo
		};
		NPCSpawnParams targetTyped = new() {
			gameModeData = null
		};
		SpawnParams aliased = new SpawnParams {
			gameModeData = null
		};
		spawnParams.gameModeData = null;
		var gameMode = spawnParams.gameModeData;

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
