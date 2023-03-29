using MonoMod.Cil;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.WorldBuilding;
using Terraria.IO;
using MonoMod.RuntimeDetour;
using System.Collections.Concurrent;

namespace Terraria;

public partial class WorldGen
{
	internal static void ClearGenerationPasses()
	{
		_generator?._passes.Clear();
		_hookRefs.Clear();
	}

	internal static Dictionary<string, GenPass> _vanillaGenPasses = new();
	public static IReadOnlyDictionary<string, GenPass> VanillaGenPasses => _vanillaGenPasses;

	// To prevent hooks being unloaded early due to garbage collection
	private static ConcurrentBag<object> _hookRefs = new();

	public static void ModifyPass(PassLegacy pass, ILContext.Manipulator callback)
	{
		_hookRefs.Add(new ILHook(pass._method.Method, callback));
	}

	// TODO: Self cannot be WorldGen, since the one used in the actual method is WorldGen+<>c, which is compiler generated or something like that
	// The self parameter should be removed, since world gen is an instanced class, but has no instance fields or methods (it could be made static in another pr)
	public delegate void GenPassDetour(orig_GenPassDetour orig, object self, GenerationProgress progress, GameConfiguration configuration);
	public delegate void orig_GenPassDetour(object self, GenerationProgress progress, GameConfiguration configuration);

	public static void DetourPass(PassLegacy pass, GenPassDetour hookDelegate)
	{
		_hookRefs.Add(new Hook(pass._method.Method, hookDelegate));
	}
}
