using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace ExampleMod.Common.Systems
{
	// This ModSystem will demonstrate how to IL Edit and Detour the methods used in world generation
	// TODO: elaborate + explain why they can't be done the normal way. Also, aren't detours be as simple as removing the genpass? maybe they have a usecase outside of method swaps
	// TODO: also make a wiki page and remove the old part about how to il edit worldgen
	public class ExampleWorldGenHookingSystem : ModSystem
	{
		/*/ TODO this is where world generation tasks are modified
		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
			var task = tasks.Find(t => t.Name == "Living Trees"); // Find the task we want to modify, in this case the "Living Trees" pass
			WorldGen.ModifyTask(task, ModifyLivingTrees); // Then we call WorldGen.ModifyTask, which IL edits the pass

			var task2 = tasks.Find(t => t.Name == "Living Trees");
			//WorldGen.DetourTask(task2, DetourShinies);
		}

		// The IL editing shouldn't be different
		private void ModifyLivingTrees(ILContext il) {
			var c = new ILCursor(il); // Create an ILCursor

			// Find where to apply the patch
			if (!c.TryGotoNext(i => i.MatchStloc(2))) {
				// Can't apply the patch
				Mod.Logger.Debug("Unable to apply patch for generation pass \"Living Trees\"");
				return;
			}

			// Applying the patch
			// This pops the original number of living trees off the stack, and then we add 1 in case it is 0 then multiply it by 5
			c.EmitDelegate(delegate (int originalNumberOfLivingTrees) {
				return (originalNumberOfLivingTrees + 1) * 5;
			});
		}

		private void DetourShinies(WorldGenLegacyMethod orig, GenerationProgress progress, GameConfiguration configuration) {
			progress.Message = "Skipped Shinies";
			Thread.Sleep(5000);
			//orig(progress, configuration);
		}*/
	}
}
