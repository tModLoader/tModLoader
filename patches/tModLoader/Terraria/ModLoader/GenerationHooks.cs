using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Terraria.WorldBuilding;

namespace Terraria.ModLoader
{
	public static class GenerationHooks
	{
		internal static readonly List<Generation> generations = new List<Generation>();
		internal static readonly List<GlobalGeneration> globalGenerations = new List<GlobalGeneration>();

		internal static void Unload() => generations.Clear();

		private class HookList
		{
			public readonly MethodInfo method;
			public GlobalGeneration[] arr = new GlobalGeneration[0];

			public HookList(MethodInfo method) {
				this.method = method;
			}
		}

		private delegate void DelegateModifyGenerationTasks(Generation generation, List<GenPass> passes, ref float totalWeight);

		private static readonly List<HookList> hooks = new List<HookList>();
		//HookLists
		private static readonly HookList HookSetWorldGenDefaults = AddHook<Action<Generation>>(s => s.SetWorldGenDefaults);
		private static readonly HookList HookPreWorldGen = AddHook<Action<Generation>>(s => s.PreWorldGen);
		private static readonly HookList HookModifyGenerationTasks = AddHook<DelegateModifyGenerationTasks>(s => s.ModifyGenerationTasks);
		private static readonly HookList HookPostWorldGen = AddHook<Action<Generation>>(s => s.PostWorldGen);

		private static HookList AddHook<F>(Expression<Func<GlobalGeneration, F>> func) {
			var hook = new HookList(ModLoader.Method(func));

			hooks.Add(hook);

			return hook;
		}

		internal static void RebuildHooks() {
			foreach (var hook in hooks) {
				hook.arr = ModLoader.BuildGlobalHook(globalGenerations, hook.method);
			}
		}

		public static void SetWorldGenDefaults(Generation generation) {
			foreach (var globalGeneration in HookSetWorldGenDefaults.arr) {
				globalGeneration.SetWorldGenDefaults(generation);
			}
		}

		public static void PreWorldGen(Generation generation) {
			foreach (var globalGeneration in HookPreWorldGen.arr) {
				globalGeneration.PreWorldGen(generation);
			}
		}

		public static List<GenPass> GetGenerationTasks(Generation generation) {
			var passes = new List<GenPass>();
			float totalWeight = 0f;

			generation.ModifyGenerationTasks(passes, ref totalWeight);

			foreach (var globalGeneration in HookModifyGenerationTasks.arr) {
				globalGeneration.ModifyGenerationTasks(generation, passes, ref totalWeight);
			}

			return passes;
		}

		public static void PostWorldGen(Generation generation) {
			foreach (var globalGeneration in HookPostWorldGen.arr) {
				globalGeneration.PostWorldGen(generation);
			}
		}

		public static void RunGeneration(Generation generation) {
			foreach (var task in GetGenerationTasks(generation)) {
				task.Apply(null, null);
			}
		}
	}
}
