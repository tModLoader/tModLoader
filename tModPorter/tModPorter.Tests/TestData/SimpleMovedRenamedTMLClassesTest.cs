using Terraria;
using Terraria.ModLoader;

// Misc moved tml classes that don't fit in any self-contained test
public class SimpleMovedRenamedTMLClassesTest
{
	void Method() {
		var player = new Player();
		PlayerHooks.ResetEffects(player); // PlayerHooks -> PlayerLoader

		SpawnCondition condition = SpawnCondition.TownWaterCritter; // SpawnCondition namespace: Terraria.ModLoader.Utilities

		MusicPriority priority = MusicPriority.BiomeLow; // MusicPriority -> SceneEffectPriority
	}
}