using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using MonoMod.RuntimeDetour;
using System.Collections.Generic;
using System.Reflection;
using System;
using Terraria.GameContent.Generation;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Steamworks;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.IO;
using System.Collections.ObjectModel;
using MonoMod.Utils;
using MonoMod.Core.Platforms;
using System.Linq;

namespace Terraria;

public partial class WorldGen
{
	private static double _timePass = 0.0; // Used to account for more precise time rates.

	internal static void ClearGenerationPasses() => _generator?._passes.Clear();

	internal static List<GenPass> vanillaGenPasses_internal = new();
	public static ReadOnlyCollection<GenPass> VanillaGenPasses = vanillaGenPasses_internal.AsReadOnly();

	internal static List<Tuple<PassLegacy, ILContext.Manipulator>> genPassILEdits = new();
	internal static List<Tuple<PassLegacy, GenPassDetour>> genPassDetours = new();

	public static void ModifyTask(PassLegacy pass, ILContext.Manipulator callback)
	{
		HookEndpointManager.Modify(GetGenPassMethod(pass), callback);
		genPassILEdits.Add(Tuple.Create(pass, callback));
	}

	// TODO: Self cannot be WorldGen, since the one used in the actual method is WorldGen+<>c, which is compiler generated or something like that
	// the self parameter should be removed, since world gen is an instanced class, but has no instance fields or methods (it could be made static in another pr)
	public delegate void GenPassDetour(orig_GenPassDetour orig, object self, GenerationProgress progress, GameConfiguration configuration);
	public delegate void orig_GenPassDetour(object self, GenerationProgress progress, GameConfiguration configuration);

	public static void DetourTask(PassLegacy pass, GenPassDetour hookDelegate)
	{
		HookEndpointManager.Add(GetGenPassMethod(pass), hookDelegate);
		genPassDetours.Add(Tuple.Create(pass, hookDelegate));
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

	// TODO: Unload it in MonoModHooks.cs?
	internal static void RemoveGenPassILEditsAndDetours()
	{
		foreach (var ilEdit in genPassILEdits) {
			HookEndpointManager.Unmodify(GetGenPassMethod(ilEdit.Item1), ilEdit.Item2);
		}

		foreach (var detour in genPassDetours) {
			HookEndpointManager.Remove(GetGenPassMethod(detour.Item1), detour.Item2);
		}

		genPassILEdits.Clear();
		genPassDetours.Clear();
	}
}
