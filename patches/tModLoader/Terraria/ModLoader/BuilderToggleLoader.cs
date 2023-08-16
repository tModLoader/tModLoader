using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Terraria.ModLoader;

public static class BuilderToggleLoader
{
	public static int BuilderToggleCount => BuilderToggles.Count;

	internal static readonly List<BuilderToggle> BuilderToggles = new List<BuilderToggle>() {
		BuilderToggle.BlockSwap,
		BuilderToggle.TorchBiome,
		BuilderToggle.HideAllWires,
		BuilderToggle.ActuatorsVisibility,
		BuilderToggle.RulerLine,
		BuilderToggle.RulerGrid,
		BuilderToggle.AutoActuate,
		BuilderToggle.AutoPaint,
		BuilderToggle.RedWireVisibility,
		BuilderToggle.BlueWireVisibility,
		BuilderToggle.GreenWireVisibility,
		BuilderToggle.YellowWireVisibility
	};

	private static readonly int DefaultDisplayCount = BuilderToggles.Count;

	public static int BuilderTogglePage = 0;

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
		int[] defaultTogglesShowOrder = new[] {10, 11, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7};
		int i = 0;
		foreach (var builderToggle in BuilderToggles) {
			builderToggle.Type = defaultTogglesShowOrder[i++];
			ContentInstance.Register(builderToggle);
			ModTypeLookup<BuilderToggle>.Register(builderToggle);
		}
	}

	internal static List<BuilderToggle> ActiveBuilderTogglesList() {
		List<BuilderToggle> activeToggles = new List<BuilderToggle>(BuilderToggleCount);
		for (int i = 0; i < BuilderToggles.Count; i++) {
			if (BuilderToggles[i].Active())
				activeToggles.Add(BuilderToggles[i]);
		}

		return activeToggles;
	}

	public static int ActiveBuilderToggles() => ActiveBuilderTogglesList().Count;

	public static void AddGlobalBuilderToggles(GlobalBuilderToggle globalBuilderToggle)
	{
		globalBuilderToggles.Add(globalBuilderToggle);
		ModTypeLookup<GlobalBuilderToggle>.Register(globalBuilderToggle);
	}

	public static bool Active(BuilderToggle builderToggle) {
		bool active = builderToggle.Active();
		foreach (GlobalBuilderToggle global in globalBuilderToggles) {
			if (global.Active(builderToggle).HasValue)
				active &= global.Active(builderToggle).Value;
		}

		return active;
	}

	public static void ModifyNumberOfStates(BuilderToggle builderToggle, ref int numberOfStates) {
		foreach (GlobalBuilderToggle global in globalBuilderToggles) {
			global.ModifyNumberOfStates(builderToggle, ref numberOfStates);
		}
	}

	public static void ModifyDisplayValue(BuilderToggle builderToggle, ref string displayValue) {
		foreach (GlobalBuilderToggle global in globalBuilderToggles) {
			global.ModifyDisplayValue(builderToggle, ref displayValue);
		}
	}

	public static void ModifyDisplayColor(BuilderToggle builderToggle, ref Color displayColor) {
		foreach (GlobalBuilderToggle global in globalBuilderToggles) {
			global.ModifyDisplayColor(builderToggle, ref displayColor);
		}
	}

	public static void ModifyDisplayTexture(BuilderToggle builderToggle, ref Texture2D texture, ref Rectangle rectangle) {
		foreach (GlobalBuilderToggle global in globalBuilderToggles) {
			global.ModifyDisplayTexture(builderToggle, ref texture, ref rectangle);
		}
	}
}