using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

	private static IEnumerable<IMapLayer> ModdedLayers => MapLayers.Skip(DefaultLayerCount);

	internal static void Add(IMapLayer layer) => MapLayers.Add(layer);

	internal static void Unload()
	{
		MapLayers.RemoveRange(DefaultLayerCount, MapLayerCount - DefaultLayerCount);
	}

	internal static void ResizeArrays()
	{
		var sortingSlots = new List<IMapLayer>[DefaultLayerCount + 1];
		for (int i = 0; i < sortingSlots.Length; ++i)
			sortingSlots[i] = [];

		foreach (IMapLayer layer in ModdedLayers) {
			var position = layer.GetDefaultPosition();

			switch (position) {
				case IMapLayer.After after: {
					int afterParent = MapLayers.IndexOf(after.Layer) is int index and not -1 ? index + 1 : 0;
					sortingSlots[afterParent].Add(layer);

					break;
				}
				case IMapLayer.Before before: {
					int beforeParent = MapLayers.IndexOf(before.Layer) is int index and not -1 ? index : sortingSlots.Length - 1;
					sortingSlots[beforeParent].Add(layer);

					break;
				}	
				default: {
					var ex = new ArgumentException($"IMapLayer {layer} has unknown Position {position}");
					if (layer is ModMapLayer modLayer)
						ex.Data["mod"] = modLayer.Mod.Name;
					throw ex;
				}
			}
		}

		List<IMapLayer> sortedLayers = [];

		for (int i = 0; i < DefaultLayerCount + 1; i++) {
			var elements = sortingSlots[i];
			var sort = new TopoSort<IMapLayer>(elements,
				l => l.GetModdedConstraints()?.OfType<IMapLayer.After>().Select(a => a.Layer).Where(elements.Contains) ?? [],
				l => l.GetModdedConstraints()?.OfType<IMapLayer.Before>().Select(b => b.Layer).Where(elements.Contains) ?? []);

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
