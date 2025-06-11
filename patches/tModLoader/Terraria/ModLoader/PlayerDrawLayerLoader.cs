using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using static Terraria.ModLoader.PlayerDrawLayer;

namespace Terraria.ModLoader;

public static class PlayerDrawLayerLoader
{
	private static List<PlayerDrawLayer> _layers = new List<PlayerDrawLayer>(PlayerDrawLayers.VanillaLayers);
	public static IReadOnlyList<PlayerDrawLayer> Layers => _layers;

	private static PlayerDrawLayer[] _drawOrder;
	public static IReadOnlyList<PlayerDrawLayer> DrawOrder => _drawOrder;

	internal static void Add(PlayerDrawLayer layer) => _layers.Add(layer);

	internal static void Unload()
	{
		_layers = new List<PlayerDrawLayer>(PlayerDrawLayers.VanillaLayers);
		foreach (var layer in _layers) {
			layer.ClearChildren();
		}
	}

	internal static void ResizeArrays()
	{
		var positions = Layers.ToDictionary(l => l, l => l.GetDefaultPosition());

		PlayerLoader.ModifyDrawLayerOrdering(positions);

		var betweens = new Dictionary<PlayerDrawLayer, Between>();

		foreach (var (layer, pos) in positions.ToArray()) {
			switch (pos) {
				case Between b:
					betweens.Add(layer, b);
					break;
				case BeforeParent b:
					b.Parent.AddChildBefore(layer);
					break;
				case AfterParent a:
					a.Parent.AddChildAfter(layer);
					break;
				case Multiple m:
					int slot = 0;
					foreach (var (b, cond) in m.Positions)
						betweens.Add(new PlayerDrawLayerSlot(layer, cond, slot++), b);
					break;
				default:
					throw new ArgumentException($"PlayerDrawLayer {layer} has unknown Position type {pos}");
			}
		}

		foreach (var (layer, b) in betweens) {
			if (b.Layer1 is { } after && !betweens.ContainsKey(after))
				throw new ArgumentException($"{layer.FullName} cannot be positioned after {after.FullName} because {after.FullName} does not have a fixed position. Consider using AfterParent or referring to a different layer (or null)") { Data = { ["mod"] = layer.Mod.Name } };
			if (b.Layer2 is { } before && !betweens.ContainsKey(before))
				throw new ArgumentException($"{layer.FullName} cannot be positioned before {before.FullName} because {before.FullName} does not have a fixed position. Consider using BeforeParent or referring to a different layer (or null)") { Data = { ["mod"] = layer.Mod.Name } };
		}

		var sort = new TopoSort<PlayerDrawLayer>(betweens.Keys,
			l => betweens[l].Layer1 is { } v ? [v] : [],
			l => betweens[l].Layer2 is { } v ? [v] : []
		);

		_drawOrder = sort.Sort().ToArray();
	}

	/// <summary>
	/// Note, not threadsafe
	/// </summary>
	public static PlayerDrawLayer[] GetDrawLayers(PlayerDrawSet drawInfo)
	{
		foreach (var layer in _drawOrder) {
			layer.ResetVisibility(drawInfo);
		}

		PlayerLoader.HideDrawLayers(drawInfo);

		return _drawOrder;
	}
}
