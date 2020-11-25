using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Graphics.Renderers;

namespace Terraria.ModLoader
{
	public static class PlayerDrawLayerHooks
	{
		public static readonly IReadOnlyList<PlayerDrawLayer> VanillaLayers = new ReadOnlyCollection<PlayerDrawLayer>(LegacyPlayerRenderer.GetVanillaLayers());

		private static readonly IList<PlayerDrawLayer> _layers = new List<PlayerDrawLayer>(VanillaLayers);
		public static readonly IReadOnlyList<PlayerDrawLayer> Layers = new ReadOnlyCollection<PlayerDrawLayer>(_layers);

		internal static void ReplaceLayers(IEnumerable<PlayerDrawLayer> newLayers) {
			_layers.Clear();
			foreach (var l in newLayers) {
				_layers.Add(l);
			}
		}

		internal static void Add(PlayerDrawLayer layer) => _layers.Add(layer);

		internal static void Unload() => ReplaceLayers(VanillaLayers);

		internal static void ResizeArrays() {
			var constraints = Layers.ToDictionary(l => l, l => l.GetConstraints().ToList());
			var readOnlyConstraints = new ReadOnlyDictionary<PlayerDrawLayer, List<PlayerDrawLayer.LayerConstraint>>(constraints);

			PlayerHooks.ModifyDrawLayerOrdering(readOnlyConstraints);

			var sort = new TopoSort<PlayerDrawLayer>(Layers,
				l => readOnlyConstraints[l].Where(c => !c.before).Select(c => c.layer),
				l => readOnlyConstraints[l].Where(c => c.before).Select(c => c.layer));

			ReplaceLayers(sort.Sort());
		}

		/// <summary>
		/// Note, not threadsafe
		/// </summary>
		public static IEnumerable<PlayerDrawLayer> GetDrawLayers(PlayerDrawSet drawInfo) {
			foreach (var layer in Layers) {
				layer.visible = layer.GetDefaultVisiblity(drawInfo);
			}

			PlayerHooks.ModifyDrawLayers(drawInfo, Layers);

			return Layers;
		}
	}
}
