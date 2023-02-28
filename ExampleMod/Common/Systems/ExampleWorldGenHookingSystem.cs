using MonoMod.Cil;
using System.Linq;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ModLoader;
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
			GenPass pyramidsPass = WorldGen.VanillaGenPasses.FirstOrDefault(t => t.Name == "Pyramids");// Locate the generation pass
			if (pyramidsPass is PassLegacy pass1)// Check if the pass is a PassLegacy, since PassLegacys are the generation passes that can't normally be edited
				WorldGen.ModifyPass(pass1, Modify_Pyramids);// IL edit the pass
			else
				Mod.Logger.Warn("Unable to modify pyramids pass");// Log an error if the pass can't be located

			// Detouring the shinies pass (generates ore)
			GenPass shiniesPass = WorldGen.VanillaGenPasses.FirstOrDefault(t => t.Name == "Shinies");// Locate the generation pass
			if (shiniesPass is PassLegacy pass2)// Check if the pass is a PassLegacy, since PassLegacys are the generation passes that can't normally be edited
				WorldGen.DetourPass(pass2, Detour_Shinies);// Detour the pass
			else
				Mod.Logger.Warn("Unable to detour shinies pass");// Log an error if the pass can't be located
		}

		// IL editing should be the same, this is just an example so you can check this is actually working
		void Modify_Pyramids(ILContext il) {
			var c = new ILCursor(il);

			c.EmitDelegate(delegate () {
				WorldGen.Pyramid(Main.maxTilesX / 2, Main.maxTilesY / 2);
			});
		}

		// Detouring should be the same (except for one thing mentioned below), this is just an example so you can check this is actually working
		// One thing to note is that for techincal reasons, the self parameter is an object type
		// You will never need to actually cast it to type WorldGen though, since it contains no instance fields or methods
		void Detour_Shinies(WorldGen.orig_GenPassDetour orig, object self, GenerationProgress progress, GameConfiguration configuration) {
			// orig(self, progress, configuration); This stops underground ore generating by not calling the original method
		}
	}
}
