using MonoMod.Cil;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.WorldBuilding;
using Terraria.IO;
using Terraria.ModLoader;

namespace Terraria;

public partial class WorldGen
{
	internal static void ClearGenerationPasses()
	{
		_generator?._passes.Clear();
	}

	internal static Dictionary<string, GenPass> _vanillaGenPasses = new();
	public static IReadOnlyDictionary<string, GenPass> VanillaGenPasses => _vanillaGenPasses;

	public static void ModifyPass(PassLegacy pass, ILContext.Manipulator callback)
	{
		MonoModHooks.Modify(pass._method.Method, callback);
	}

	// The self reference has to be object, because the actual type is a compiler generated closure class
	// The self reference isn't useful anyway, since the closure doesn't capture any method locals or an enclosing class instance
	// We might think to omit the self parameter from mod delegates, and register a wrapper which propogates self via a closure, but then MonoModHooks will attribute the hook to tModLoader rather than the original mod.
	public delegate void GenPassDetour(orig_GenPassDetour orig, object self, GenerationProgress progress, GameConfiguration configuration);
	public delegate void orig_GenPassDetour(object self, GenerationProgress progress, GameConfiguration configuration);

	public static void DetourPass(PassLegacy pass, GenPassDetour hookDelegate)
	{
		MonoModHooks.Add(pass._method.Method, hookDelegate);
	}
}
