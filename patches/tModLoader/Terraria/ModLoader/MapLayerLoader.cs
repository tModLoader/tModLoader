using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Map;

namespace Terraria.ModLoader;

public static class MapLayerLoader
{
	public static int MapLayerCount => MapLayers.Count;

	internal static readonly List<IMapLayer> MapLayers = [
		IMapLayer.Spawn,
		IMapLayer.Pylons,
		IMapLayer.Pings
	];

	internal static readonly int DefaultLayerCount = MapLayers.Count;

	internal static IMapLayer FirstVanillaLayer => MapLayers[0];

	internal static IMapLayer LastVanillaLayer => MapLayers[DefaultLayerCount - 1];

	private static IEnumerable<ModMapLayer> ModdedLayers => MapLayers.Skip(DefaultLayerCount).Cast<ModMapLayer>();

	internal static void Add(IMapLayer layer) => MapLayers.Add(layer);

	internal static void Unload()
	{
		MapLayers.RemoveRange(DefaultLayerCount, MapLayerCount - DefaultLayerCount);
	}

	internal static void ResizeArrays()
	{
		var sortingSlots = new List<ModMapLayer>[DefaultLayerCount + 1];
		for (int i = 0; i < sortingSlots.Length; ++i)
			sortingSlots[i] = [];

		foreach (ModMapLayer layer in ModdedLayers) {
			var position = layer.GetDefaultPosition();

			switch (position) {
				case ModMapLayer.After after: {
					int afterIndex = MapLayers.IndexOf(after.Layer);
					if (afterIndex >= DefaultLayerCount || afterIndex is -1)
						throw BlameMapLayerException(new ArgumentException($"ModMapLayer {layer} did not refer to a vanilla map layer in GetDefaultPosition()"));

					sortingSlots[afterIndex + 1].Add(layer);
					break;
				}
				case ModMapLayer.Before before: {
					int beforeIndex = MapLayers.IndexOf(before.Layer);
					if (beforeIndex >= DefaultLayerCount || beforeIndex is -1)
						throw BlameMapLayerException(new ArgumentException($"ModMapLayer {layer} did not refer to a vanilla map layer in GetDefaultPosition()"));

					sortingSlots[beforeIndex].Add(layer);
					break;
				}	
				default: {
					var ex = new ArgumentException($"ModMapLayer {layer} has unknown Position {position}");
					throw BlameMapLayerException(ex);
				}
			}

			Exception BlameMapLayerException(Exception ex) {
				if (layer is ModMapLayer moddedLayer)
					ex.Data["mod"] = moddedLayer.Mod.Name;
				return ex;
			}
		}

		List<IMapLayer> sortedLayers = [];

		for (int i = 0; i < DefaultLayerCount + 1; i++) {
			var elements = sortingSlots[i];
			var sort = new TopoSort<IMapLayer>(elements,
				l => (l as ModMapLayer)?.GetModdedConstraints()?.OfType<ModMapLayer.After>().Select(a => a.Layer).Where(elements.Contains) ?? [],
				l => (l as ModMapLayer)?.GetModdedConstraints()?.OfType<ModMapLayer.Before>().Select(b => b.Layer).Where(elements.Contains) ?? []);

			foreach (IMapLayer layer in sort.Sort()) {
				sortedLayers.Add(layer);
			}

			if (i < DefaultLayerCount)
				sortedLayers.Add(MapLayers[i]);
		}

		Main.MapIcons = CreateOverlayWithLayers(sortedLayers);
		Main.Pings = (PingMapLayer)IMapLayer.Pings;
	}

	private static MapIconOverlay CreateOverlayWithLayers(IEnumerable<IMapLayer> layers)
	{
		var overlay = new MapIconOverlay();
		foreach (IMapLayer layer in layers) {
			overlay.AddLayer(layer);
		}
		return overlay;
	}
}
