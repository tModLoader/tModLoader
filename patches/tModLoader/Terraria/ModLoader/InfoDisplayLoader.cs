using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
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

		private static readonly int DefaultDisplayCount = InfoDisplays.Count;

		public static int InfoDisplayPage = 0;

		static InfoDisplayLoader() {
			RegisterDefaultDisplays();
		}

		internal static int Add(InfoDisplay infoDisplay) {
			InfoDisplays.Add(infoDisplay);
			return InfoDisplays.Count - 1;
		}

		internal static void Unload() {
			InfoDisplays.RemoveRange(DefaultDisplayCount, InfoDisplays.Count - DefaultDisplayCount);
		}

		internal static void RegisterDefaultDisplays() {
			int i = 0;
			foreach (var infoDisplay in InfoDisplays) {
				infoDisplay.Type = i++;
				ContentInstance.Register(infoDisplay);
				ModTypeLookup<InfoDisplay>.Register(infoDisplay);
			}
		}

		public static int ActiveDisplays() {
			int activeDisplays = 0;
			for (int i = 0; i < InfoDisplays.Count; i++) {
				if (InfoDisplays[i].Active())
					activeDisplays += 1;
			}
			return activeDisplays;
		}

		public static int ActivePages() {
			int activePages = 1;
			int activeDisplays = ActiveDisplays();

			keepChecking:
			if (activeDisplays > 12) {
				activePages += 1;
				activeDisplays -= 12;
				goto keepChecking;
			}
			return activePages;
		}
	}
}
