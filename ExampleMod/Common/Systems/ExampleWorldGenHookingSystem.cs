using MonoMod.Cil;
using System;
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
			WorldGen.ModifyPass((PassLegacy)WorldGen.VanillaGenPasses["Pyramids"], Modify_Pyramids);

			// Detouring the shinies pass (generates ore)
			WorldGen.DetourPass((PassLegacy)WorldGen.VanillaGenPasses["Shinies"], Detour_Shinies);
		}

		void Modify_Pyramids(ILContext il) {
			try {
				var c = new ILCursor(il);
				c.EmitDelegate(() => ModContent.GetInstance<ExampleMod>().Logger.Debug("(In ILHook) Generating Pyramids"));
			}
			catch (Exception) {
				MonoModHooks.DumpIL(ModContent.GetInstance<ExampleMod>(), il);
			}
		}

		// Detouring should be the same (except for one thing mentioned below), this is just an example so you can check this is actually working
		// One thing to note is that for technical reasons, the self parameter is an object type
		// You will never need to actually cast it to type WorldGen though, since it contains no instance fields or methods
		void Detour_Shinies(WorldGen.orig_GenPassDetour orig, object self, GenerationProgress progress, GameConfiguration configuration) {
			ModContent.GetInstance<ExampleMod>().Logger.Debug("(On Hook) Before Shinies");
			orig(self, progress, configuration);
			ModContent.GetInstance<ExampleMod>().Logger.Debug("(On Hook) After Shinies");
		}
	}
}
