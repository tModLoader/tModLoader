using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Terraria.WorldBuilding;



namespace Terraria.ModLoader
{
	public static class GenerationHooks
	{
		internal static readonly List<ModGeneration> Generation = new List<ModGeneration>();

		internal static void Add(ModGeneration modGeneration) => Generation.Add(modGeneration);

		internal static void Unload() => Generation.Clear();

		private class HookList
		{
			public readonly MethodInfo method;

			public ModGeneration[] arr = new ModGeneration[0];

			public HookList(MethodInfo method) {
				this.method = method;
			}
		}

		private static readonly List<HookList> hooks = new List<HookList>();

		private static HookList AddHook<F>(Expression<Func<ModGeneration, F>> func) {
			var hook = new HookList(ModLoader.Method(func));

			hooks.Add(hook);

			return hook;
		}
		internal static void RebuildHooks() {
			foreach (var hook in hooks) {
				hook.arr = ModLoader.BuildGlobalHook(Generation, hook.method);
			}
		}

		private static HookList HookSetWorldGenDefaults = AddHook<Action>(s => s.SetWorldGenDefaults);
		public static void SetWorldGenDefaults() {
			foreach (ModGeneration worldGen in HookSetWorldGenDefaults.arr) {
				worldGen.SetWorldGenDefaults();
			}
		}

		private static HookList HookPreWorldGen = AddHook<Action>(s => s.PreWorldGen);
		public static void PreWorldGen() {
			foreach (ModGeneration worldGen in HookPreWorldGen.arr) {
				worldGen.PreWorldGen();
			}
		}

		private delegate void DelegateModifyWorldGenTasks(List<GenPass> passes, ref float totalWeight);
		private static HookList HookModifyWorldGenTasks = AddHook<DelegateModifyWorldGenTasks>(s => s.ModifyWorldGenTasks);
		public static void ModifyWorldGenTasks(List<GenPass> passes, ref float totalWeight) {
			foreach (ModGeneration worldGen in HookModifyWorldGenTasks.arr) {
				worldGen.ModifyWorldGenTasks(passes, ref totalWeight);
			}
		}

		private static HookList HookPostWorldGen = AddHook<Action>(s => s.PostWorldGen);
		public static void PostWorldGen() {
			foreach (ModGeneration worldGen in HookPostWorldGen.arr) {
				worldGen.PostWorldGen();
			}
		}

		private static HookList HookModifyMeteorTasks = AddHook<Action<List<GenPass>>>(s => s.ModifyMeteorTasks);
		public static void ModifyMeteorTasks(List<GenPass> passes) {
			foreach (ModGeneration worldGen in HookModifyMeteorTasks.arr) {
				worldGen.ModifyMeteorTasks(passes);
			}
		}

		private static HookList HookModifyHardmodeTasks = AddHook<Action<List<GenPass>>>(s => s.ModifyHardmodeTasks);
		public static void ModifyHardmodeTasks(List<GenPass> passes) {
			foreach (ModGeneration worldGen in HookModifyHardmodeTasks.arr) {
				worldGen.ModifyHardmodeTasks(passes);
			}
		}

		private static HookList HookModifyAltarTasks = AddHook<Action<List<GenPass>>>(s => s.ModifyAltarTasks);
		public static void ModifyAltarTasks(List<GenPass> passes) {
			foreach (ModGeneration worldGen in HookModifyAltarTasks.arr) {
				worldGen.ModifyAltarTasks(passes);
			}
		}
	}
}
