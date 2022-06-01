using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

// Misc moved classes that don't fit in any self-contained test
// TODO PlayerDrawInfo -> PlayerDrawSet? Handled in ModPlayerTest
public class SimpleMovedRenamedTMLClassesTest
{
	void Method() {
		var player = new Player();
		PlayerLoader.ResetEffects(player); // PlayerHooks -> PlayerLoader

		SpawnCondition condition = SpawnCondition.TownWaterCritter; // SpawnCondition namespace: Terraria.ModLoader.Utilities
	}
}