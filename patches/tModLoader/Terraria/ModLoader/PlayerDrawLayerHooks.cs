using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Terraria.DataStructures;

namespace Terraria.ModLoader
{
	public static class PlayerDrawLayerHooks
	{
		internal static readonly IList<PlayerDrawLayer> ModLayers = new List<PlayerDrawLayer>();

		internal static void Add(PlayerDrawLayer layer) => ModLayers.Add(layer);

		internal static void Unload() => ModLayers.Clear();

		public static void AddDrawLayers(PlayerDrawSet drawInfo, Dictionary<string, List<PlayerDrawLayer>> layers) {
			foreach (var layer in ModLayers) {
				layer.GetDefaults(drawInfo, out layer.visible, out layer.constraint);

				string modName = layer.Mod.Name;

				if (!layers.TryGetValue(modName, out var list)) {
					layers[modName] = list = new List<PlayerDrawLayer>();
				}

				list.Add(layer);
			}
		}

		public static IEnumerable<PlayerDrawLayer> GetDrawLayers(PlayerDrawSet drawInfo, List<PlayerDrawLayer> vanillaLayers) {
			for (int i = 0; i < vanillaLayers.Count; i++) {
				var layer = (LegacyPlayerDrawLayer)vanillaLayers[i];

				layer.visible = true;
				layer.constraint = i > 0 ? new PlayerDrawLayer.LayerConstraint(vanillaLayers[i - 1], false) : default;
			}

			var layers = new Dictionary<string, List<PlayerDrawLayer>> {
				{ "Terraria", vanillaLayers }
			};

			//Add OOP layers.
			AddDrawLayers(drawInfo, layers);

			//Actually make layer lists readonly
			var readonlyLayersBackingDictionary = new Dictionary<string, IReadOnlyList<PlayerDrawLayer>>();
			var readonlyLayers = new ReadOnlyDictionary<string, IReadOnlyList<PlayerDrawLayer>>(readonlyLayersBackingDictionary);

			foreach (var pair in layers) {
				readonlyLayersBackingDictionary[pair.Key] = pair.Value.AsReadOnly();
			}

			//Modify draw layers, but not the collections.
			PlayerHooks.ModifyDrawLayers(drawInfo, readonlyLayers);

			var array = layers
				.SelectMany(pair => pair.Value)
				.ToArray();

			//TODO: Sort the array based on layer constraints.

			return array;
		}
	}
}
