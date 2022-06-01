using Terraria;
using Terraria.ModLoader;

// Misc moved classes that don't fit in any self-contained test
// TODO NPCSpawnHelper (This mainly affects SpawnConditions): Terraria.ModLoader -> Terraria.ModLoader.Utilities
// TODO RecipeGroupHelper: Terraria.ModLoader -> Terraria.ModLoader.Utilities
// TODO PlayerDrawInfo -> PlayerDrawSet? Handled in ModPlayerTest
public class SimpleMovedRenamedTMLClassesTest
{
	void Method() {
		var player = new Player();
		PlayerHooks.ResetEffects(player); // PlayerHooks -> PlayerLoader
	}
}