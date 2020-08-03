using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public static class PlayerLayerHooks
	{
		internal static readonly IList<PlayerLayer> ModLayers = new List<PlayerLayer>();

		internal static void Add(PlayerLayer layer) => ModLayers.Add(layer);

		internal static void Unload() => ModLayers.Clear();

		public static void ModifyDrawLayers(Player drawPlayer, Dictionary<string, List<PlayerLayer>> layers, IReadOnlyList<PlayerLayer> vanillaLayers) {
			foreach (var layer in ModLayers) {
				layer.depth = 0f;

				if (layer.Setup(drawPlayer, vanillaLayers)) {
					string modName = layer.Mod.Name;

					if (!layers.TryGetValue(modName, out var list)) {
						layers[modName] = list = new List<PlayerLayer>();
					}

					list.Add(layer);
				}
			}
		}
	}
}
