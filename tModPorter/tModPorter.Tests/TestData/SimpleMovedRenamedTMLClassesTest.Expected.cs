using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

// Misc moved tml classes that don't fit in any self-contained test
public class SimpleMovedRenamedTMLClassesTest
{
	void Method() {
		var player = new Player();
		PlayerLoader.ResetEffects(player); // PlayerHooks -> PlayerLoader

		SpawnCondition condition = SpawnCondition.TownWaterCritter; // SpawnCondition namespace: Terraria.ModLoader.Utilities

		SceneEffectPriority priority = SceneEffectPriority.BiomeLow; // MusicPriority -> SceneEffectPriority
	}
}