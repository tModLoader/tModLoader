using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public static class BuilderToggleLoader
	{
		public static int BuilderToggleCount => BuilderToggles.Count;

		internal static readonly List<BuilderToggle> BuilderToggles = new List<BuilderToggle>() {
			BuilderToggle.RulerLine,
			BuilderToggle.RulerGrid,
			BuilderToggle.AutoActuate,
			BuilderToggle.AutoPaint,
			BuilderToggle.RedWireVisibility,
			BuilderToggle.GreenWireVisibility,
			BuilderToggle.BlueWireVisibility,
			BuilderToggle.YellowWireVisibility,
			BuilderToggle.HideAllWires,
			BuilderToggle.ActuatorsVisibility,
			BuilderToggle.BlockSwap,
			BuilderToggle.TorchBiome
		};

		private static readonly int DefaultDisplayCount = BuilderToggles.Count;

		public static int InfoDisplayPage = 0;

		internal static readonly IList<GlobalBuilderToggle> globalBuilderToggles = new List<GlobalBuilderToggle>();

		static BuilderToggleLoader() {
			RegisterDefaultToggles();
		}

		internal static int Add(BuilderToggle builderToggle) {
			BuilderToggles.Add(builderToggle);
			return BuilderToggles.Count - 1;
		}

		internal static void Unload() {
			BuilderToggles.RemoveRange(DefaultDisplayCount, BuilderToggles.Count - DefaultDisplayCount);
			globalBuilderToggles.Clear();
		}

		internal static void RegisterDefaultToggles() {
			int i = 0;
			foreach (var builderToggle in BuilderToggles) {
				builderToggle.Type = i++;
				ContentInstance.Register(builderToggle);
				ModTypeLookup<BuilderToggle>.Register(builderToggle);
			}
		}

		public static int ActiveBuilderToggles() {
			int activeDisplays = 0;
			for (int i = 0; i < BuilderToggles.Count; i++) {
				if (BuilderToggles[i].Active())
					activeDisplays += 1;
			}

			return activeDisplays;
		}

		public static void AddGlobalBuilderToggles(GlobalBuilderToggle globalBuilderToggle)
		{
			globalBuilderToggles.Add(globalBuilderToggle);
			ModTypeLookup<GlobalBuilderToggle>.Register(globalBuilderToggle);
		}

		public static int ActivePages() {
			int activePages = 1;
			int activeToggles = ActiveBuilderToggles();

			while (activeToggles > 12) {
				activePages += 1;
				activeToggles -= 12;
			}

			return activePages;
		}


		public static bool Active(BuilderToggle builderToggle) {
			bool active = builderToggle.Active();
			foreach (GlobalBuilderToggle global in globalBuilderToggles) {
				if (global.Active(builderToggle).HasValue)
					active &= global.Active(builderToggle).Value;
			}

			return active;
		}

		public static void ModifyDisplayValue(BuilderToggle builderToggle, ref string displayValue) {
			foreach (GlobalBuilderToggle global in globalBuilderToggles) {
				global.ModifyDisplayValue(builderToggle, ref displayValue);
			}
		}
	}
}