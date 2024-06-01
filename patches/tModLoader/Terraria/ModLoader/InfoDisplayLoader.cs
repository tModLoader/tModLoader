using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Terraria.ModLoader;

public static class InfoDisplayLoader
{
	public static int InfoDisplayCount => InfoDisplays.Count;

	internal static readonly List<InfoDisplay> InfoDisplays = new List<InfoDisplay>() {
		InfoDisplay.Watches,
		InfoDisplay.WeatherRadio,
		InfoDisplay.Sextant,
		InfoDisplay.FishFinder,
		InfoDisplay.MetalDetector,
		InfoDisplay.LifeformAnalyzer,
		InfoDisplay.Radar,
		InfoDisplay.TallyCounter,
		InfoDisplay.Dummy,
		InfoDisplay.DPSMeter,
		InfoDisplay.Stopwatch,
		InfoDisplay.Compass,
		InfoDisplay.DepthMeter
	};

	internal static readonly int DefaultDisplayCount = InfoDisplays.Count;

	public static int InfoDisplayPage = 0;

	internal static readonly IList<GlobalInfoDisplay> globalInfoDisplays = new List<GlobalInfoDisplay>();

	static InfoDisplayLoader()
	{
		RegisterDefaultDisplays();
	}

	internal static int Add(InfoDisplay infoDisplay)
	{
		InfoDisplays.Add(infoDisplay);
		return InfoDisplays.Count - 1;
	}

	internal static void Unload()
	{
		InfoDisplays.RemoveRange(DefaultDisplayCount, InfoDisplays.Count - DefaultDisplayCount);
		globalInfoDisplays.Clear();
	}

	internal static void RegisterDefaultDisplays()
	{
		int i = 0;
		foreach (var infoDisplay in InfoDisplays) {
			infoDisplay.Type = i++;
			ContentInstance.Register(infoDisplay);
			ModTypeLookup<InfoDisplay>.Register(infoDisplay);
		}
	}

	public static int ActiveDisplays()
	{
		int activeDisplays = 0;
		for (int i = 0; i < InfoDisplays.Count; i++) {
			if (InfoDisplays[i].Active())
				activeDisplays += 1;
		}
		return activeDisplays;
	}

	public static void AddGlobalInfoDisplay(GlobalInfoDisplay globalInfoDisplay)
	{
		globalInfoDisplays.Add(globalInfoDisplay);
		ModTypeLookup<GlobalInfoDisplay>.Register(globalInfoDisplay);
	}

	public static int ActivePages()
	{
		int activePages = 1;
		int activeDisplays = ActiveDisplays();

		while (activeDisplays > 12) {
			activePages += 1;
			activeDisplays -= 12;
		}
		return activePages;
	}


	public static bool Active(InfoDisplay info)
	{
		bool active = info.Active();
		foreach (GlobalInfoDisplay global in globalInfoDisplays) {
			bool? val = global.Active(info);
			if (val.HasValue)
				active &= val.Value;
		}
		return active;
	}

	// Remove in 2023_10
	[Obsolete("Use ModifyDisplayParameters instead")]
	public static void ModifyDisplayName(InfoDisplay info, ref string displayName)
	{
		foreach (GlobalInfoDisplay global in globalInfoDisplays) {
			global.ModifyDisplayName(info, ref displayName);
		}
	}

	// Remove in 2023_10
	[Obsolete("Use ModifyDisplayParameters instead")]
	public static void ModifyDisplayValue(InfoDisplay info, ref string displayName)
	{
		foreach (GlobalInfoDisplay global in globalInfoDisplays) {
			global.ModifyDisplayValue(info, ref displayName);
		}
	}

	// Remove in 2023_10
	[Obsolete("Use ModifyDisplayParameters instead")]
	public static void ModifyDisplayColor(InfoDisplay info, ref Color displayColor, ref Color displayShadowColor)
	{
		foreach (GlobalInfoDisplay global in globalInfoDisplays) {
			global.ModifyDisplayColor(info, ref displayColor, ref displayShadowColor);
		}
	}

	public static void ModifyDisplayParameters(InfoDisplay info, ref string displayValue, ref string displayName, ref Color displayColor, ref Color displayShadowColor)
	{
		foreach (GlobalInfoDisplay global in globalInfoDisplays) {
			global.ModifyDisplayParameters(info, ref displayValue, ref displayName, ref displayColor, ref displayShadowColor);
		}
	}
}
