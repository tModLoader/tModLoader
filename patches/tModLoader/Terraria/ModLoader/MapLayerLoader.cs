using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Map;
using static Terraria.Map.IMapLayer;

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

	internal static void Unload()
	{
		MapLayers.RemoveRange(DefaultLayerCount, MapLayerCount - DefaultLayerCount);
		
		var overlay = new MapIconOverlay();
		foreach (IMapLayer layer in MapLayers) {
			overlay.AddLayer(layer);
		}
		Main.MapIcons = overlay;
	}

	internal static void ResizeArrays()
	{
		List<IMapLayer> sortedLayers = MapLayers[..DefaultLayerCount];
		foreach (IMapLayer layer in ModdedLayers) {
			Position position = layer.GetDefaultPosition();

			switch (position) {
				case Before before: {
					int index = sortedLayers.IndexOf(before.Layer);
					if (index is not -1) {
						sortedLayers.Insert(index, layer);
					}
					else {
						sortedLayers.Add(layer);
					}

					break;
				}
				case After after: {
					int index = sortedLayers.IndexOf(after.Layer);
					if (index is not -1) {
						sortedLayers.Insert(index + 1, layer);
					}
					else {
						sortedLayers.Add(layer);
					}

					break;
				}
				case Append: {
					sortedLayers.Add(layer);
					break;
				}
				default: {
					throw new ArgumentException($"IMapLayer {layer} has unknown {position}");
				}
			}
		}

		var overlay = new MapIconOverlay();
		foreach (IMapLayer layer in sortedLayers) {
			overlay.AddLayer(layer);
		}
		Main.MapIcons = overlay;
	}
}
