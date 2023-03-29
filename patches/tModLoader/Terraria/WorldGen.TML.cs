using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.WorldBuilding;
using Terraria.IO;

namespace Terraria;

public partial class WorldGen
{
	internal static void ClearGenerationPasses() => _generator?._passes.Clear();

	internal static Dictionary<string, GenPass> _vanillaGenPasses = new();
	public static IReadOnlyDictionary<string, GenPass> VanillaGenPasses => _vanillaGenPasses;

	public static void ModifyPass(PassLegacy pass, ILContext.Manipulator callback)
	{
		HookEndpointManager.Modify(pass._method.Method, callback);
	}

	// TODO: Self cannot be WorldGen, since the one used in the actual method is WorldGen+<>c, which is compiler generated or something like that
	// The self parameter should be removed, since world gen is an instanced class, but has no instance fields or methods (it could be made static in another pr)
	public delegate void GenPassDetour(orig_GenPassDetour orig, object self, GenerationProgress progress, GameConfiguration configuration);
	public delegate void orig_GenPassDetour(object self, GenerationProgress progress, GameConfiguration configuration);

	public static void DetourPass(PassLegacy pass, GenPassDetour hookDelegate)
	{
		HookEndpointManager.Add(pass._method.Method, hookDelegate);
	}
}
