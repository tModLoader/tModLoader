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

namespace Terraria;

public partial class WorldGen
{
	private static double _timePass = 0.0; // Used to account for more precise time rates.

	internal static void ClearGenerationPasses() => _generator?._passes.Clear();

	internal static List<Tuple<GenPass, ILContext.Manipulator>> genPassILEdits;
	internal static List<Tuple<GenPass, GenPassDetour>> genPassDetours;

	public delegate void GenPassDetour(WorldGenLegacyMethod orig, GenerationProgress progress, GameConfiguration configuration);

	/// <summary>
	/// IL edits a PassLegacy during world generation. Use in ModifyWorldGenerationTasks or ModifyHardmodeTasks.
	/// </summary>
	/// <param name="task">The task to be IL edited</param>
	/// <param name="callback">The method that IL edits the pass</param>
	// TODO: Add support for hardmode tasks
	public static void ModifyTask(GenPass task, ILContext.Manipulator callback)
	{
		HookEndpointManager.Modify(GetGenPassMethod(task), callback);
		genPassILEdits.Add(Tuple.Create(task, callback));
	}

	/// <summary>
	/// Detours a PassLegacy during world generation. Use in ModifyWorldGenerationTasks or ModifyHardmodeTasks.
	/// </summary>
	/// <param name="task">The task to be detoured</param>
	/// <param name="hookDelegate">The detour method</param>
	// TODO: make detours work
	public static void DetourTask(GenPass task, GenPassDetour hookDelegate)
	{
		HookEndpointManager.Add(GetGenPassMethod(task), hookDelegate);
		genPassDetours.Add(Tuple.Create(task, hookDelegate));
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

	// TODO: tidy up
	internal static MethodInfo GetGenPassMethod(GenPass task)
	{
		if (task is PassLegacy pass) {
			var methodField = typeof(PassLegacy).GetField("_method", BindingFlags.NonPublic | BindingFlags.Instance);
			object method = methodField.GetValue(pass);
			if (method is WorldGenLegacyMethod legacyMethod) {
				return legacyMethod.Method;
			}
			else {
				throw new Exception("Method is not a WorldGenLegacyMethod");
				return null;
			}
		}
		else {
			throw new Exception("Task is not a PassLegacy");
			return null;
		}
	}

	// TODO: Remove after testing and create DumpHooks for detours
	internal static void DumpILHooks()
	{
		var ilHookList = typeof(HookEndpointManager).GetField("ILHooks", BindingFlags.NonPublic | BindingFlags.Static);
		var list = ilHookList.GetValue(null);
		if (list is Dictionary<(MethodBase, Delegate), ILHook> dict) {
			Logging.tML.Debug("ILHooks Dump:");
			foreach (var item in dict) {
				Logging.tML.Debug(item.Key + ": " + item.Value);
			}
		}
		else {
			throw new Exception($"Failed to get HookEndpointManager.ILHooks: Type is {list.GetType()}");
		}
	}
}
