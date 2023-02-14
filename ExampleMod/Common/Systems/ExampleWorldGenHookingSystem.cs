using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace ExampleMod.Common.Systems
{
	// This ModSystem will demonstrate how to IL edit and Detour world generation passes
	// Since world generation passes are anonymous methods (they don't have a name), they can't be edited the standard way (using IL_xx or On_xx)
	public class ExampleWorldGenHookingSystem : ModSystem
	{
		// All of the registration should take place in load
		// Generation pass hooks are unloaded manually, so no Unload method is needed
		public override void Load() {
			// IL editing the pyramids pass
			GenPass pyramidsPass = WorldGen.VanillaGenPasses.First(t => t.Name == "Pyramids");// Locate the generation pass
			if (pyramidsPass is PassLegacy pass1)// Check if the pass is a PassLegacy, since PassLegacys are the generation passes that can't normally be edited
				WorldGen.ModifyTask(pass1, Modify_Pyramids);// IL edit the pass
			else
				Mod.Logger.Warn("Unable to modify pyramids pass");// Log an error if the pass can't be located

			// Detouring the shinies pass (generates ore)
			GenPass shiniesPass = WorldGen.VanillaGenPasses.First(t => t.Name == "Shinies");// Locate the generation pass
			if (shiniesPass is PassLegacy pass2)// Check if the pass is a PassLegacy, since PassLegacys are the generation passes that can't normally be edited
				WorldGen.DetourTask(pass2, Detour_Shinies);// Detour the pass
			else
				Mod.Logger.Warn("Unable to detour shinies pass");// Log an error if the pass can't be located
		}

		// IL editing should be the same, this is just an example so you can check this is actually working
		void Modify_Pyramids(ILContext il) {
			var c = new ILCursor(il);

			c.EmitDelegate(delegate () {
				if (WorldGen.genRand.NextBool())
					WorldGen.Pyramid(Main.spawnTileX, Main.spawnTileY);
			});
		}

		// Detouring should be the same, this is just an example so you can check this is actually working
		void Detour_Shinies(WorldGen.orig_GenPassDetour orig, object self, GenerationProgress progress, GameConfiguration configuration) {
			// orig(self, progress, configuration); This stops underground ore generating by not calling the original method
		}
	}
}
