using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using System.Collections.Generic;
using System.Reflection;
using System;
using Terraria.GameContent.Generation;
using Terraria.WorldBuilding;
using Terraria.IO;
using System.Collections.ObjectModel;

namespace Terraria;

public partial class WorldGen
{
	private static double _timePass = 0.0; // Used to account for more precise time rates.

	internal static void ClearGenerationPasses() => _generator?._passes.Clear();

	internal static Dictionary<string, GenPass> _vanillaGenPasses = new();
	public static IReadOnlyDictionary<string, GenPass> VanillaGenPasses = new ReadOnlyDictionary<string, GenPass>(_vanillaGenPasses);

	public static void ModifyPass(PassLegacy pass, ILContext.Manipulator callback)
	{
		HookEndpointManager.Modify(GetGenPassMethod(pass), callback);
	}

	// TODO: Self cannot be WorldGen, since the one used in the actual method is WorldGen+<>c, which is compiler generated or something like that
	// The self parameter should be removed, since world gen is an instanced class, but has no instance fields or methods (it could be made static in another pr)
	public delegate void GenPassDetour(orig_GenPassDetour orig, object self, GenerationProgress progress, GameConfiguration configuration);
	public delegate void orig_GenPassDetour(object self, GenerationProgress progress, GameConfiguration configuration);

	public static void DetourPass(PassLegacy pass, GenPassDetour hookDelegate)
	{
		HookEndpointManager.Add(GetGenPassMethod(pass), hookDelegate);
	}

	internal static MethodInfo GetGenPassMethod(PassLegacy pass)
	{
		var methodField = typeof(PassLegacy).GetField("_method", BindingFlags.NonPublic | BindingFlags.Instance);
		object methodFieldValue = methodField.GetValue(pass);
		if (methodFieldValue is WorldGenLegacyMethod legacyMethod) {
			return legacyMethod.Method;
		}
		else {
			throw new Exception("Method is not a WorldGenLegacyMethod");
		}
	}
}
