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

namespace Terraria;

public partial class WorldGen
{
	private static double _timePass = 0.0; // Used to account for more precise time rates.

	internal static void ClearGenerationPasses() => _generator?._passes.Clear();

	/*public static ReadOnlyCollection<GenPass> VanillaGenPasses = vanillaGenPasses_internal.AsReadOnly();
	internal static List<GenPass> vanillaGenPasses_internal = new List<GenPass>();

	internal static List<Tuple<PassLegacy, ILContext.Manipulator>> genPassILEdits;
	internal static List<Tuple<PassLegacy, GenPassDetour>> genPassDetours;

	public delegate void GenPassDetour(WorldGenLegacyMethod orig, GenerationProgress progress, GameConfiguration configuration);

	// TODO: XML comments
	// TODO: Add support for hardmode tasks
	public static void ModifyTask(PassLegacy pass, ILContext.Manipulator callback)
	{
		HookEndpointManager.Modify(GetGenPassMethod(pass), callback);
		genPassILEdits.Add(Tuple.Create(pass, callback));
	}

	/// <summary>
	/// Detours a PassLegacy during world generation. Use in ModifyWorldGenerationTasks or ModifyHardmodeTasks.
	/// </summary>
	/// <param name="task">The task to be detoured</param>
	/// <param name="hookDelegate">The detour method</param>
	// TODO: make detours work
	public static void DetourTask(PassLegacy pass, GenPassDetour hookDelegate)
	{
		HookEndpointManager.Add(GetGenPassMethod(pass), hookDelegate);
		genPassDetours.Add(Tuple.Create(pass, hookDelegate));
	}

	internal static void RemoveGenPassILEditsAndDetours()
	{
		foreach (var ilEdit in genPassILEdits) {
			HookEndpointManager.Unmodify(GetGenPassMethod(ilEdit.Item1), ilEdit.Item2);
		}

		foreach (var detour in genPassDetours) {
			HookEndpointManager.Remove(GetGenPassMethod(detour.Item1), detour.Item2);
		}
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
	}*/
}
