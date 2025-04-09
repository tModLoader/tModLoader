using System.Collections;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace Terraria.ModLoader;

public abstract partial class PlayerDrawLayer
{
	/// <summary>
	/// A PlayerDrawLayer's position in the player rendering draw order. When a player is drawn, each "layer" is drawn from back to front.
	/// </summary>
	public abstract class Position { }

	/// <summary>
	/// Order between the two provided layers. This will cause the layer to draw after layer1 but before layer2, meaning this layer will draw over layer1 and be drawn over by layer2.
	/// <para/> <see langword="null"/> can be used to indicate positions before the 1st layer (layer1) or after the last layer (layer2). This can be used to indicate that a layer should be before/after a provided layer, but it doesn't matter where exactly it ends up in the ordering aside from being before/after the provided layer. For ordering before or after all vanilla layers, the helper properties <see cref="PlayerDrawLayers.BeforeFirstVanillaLayer"/> and <see cref="PlayerDrawLayers.AfterLastVanillaLayer"/> can be used. 
	/// <para/> The layer parameters used must have fixed positions, meaning that a layer registered using <see cref="Multiple"/> is not valid. For vanilla layers, this includes <see cref="PlayerDrawLayers.FrontAccFront"/> and <see cref="PlayerDrawLayers.HeldItem"/>. If ordering in relation to these layers, consider either using <see cref="AfterParent"/> or <see cref="BeforeParent"/> to draw at whatever positions that layer is actually drawn, or using a different layer for ordering instead.
	/// </summary>
	public sealed class Between : Position
	{
		public PlayerDrawLayer Layer1 { get; }
		public PlayerDrawLayer Layer2 { get; }

		/// <inheritdoc cref="Between"/>
		public Between(PlayerDrawLayer layer1, PlayerDrawLayer layer2)
		{
			Layer1 = layer1;
			Layer2 = layer2;
		}

		public Between()
		{
		}
	}

	/// <summary>
	/// Orders this layer into multiple <see cref="Position"/>s, allowing this layer to draw conditionally at multiple different layer positions. Use this for layers that might need to be drawn at different layers rather than making multiple <see cref="PlayerDrawLayer"/>.
	/// <para/> An example of this can be seen in <see href="https://github.com/tModLoader/tModLoader/blob/stable/patches/tModLoader/Terraria/DataStructures/PlayerDrawLayers.tML.cs#L158">PlayerDrawLayers.tML.cs</see>. Take note how the condition logic for FrontAccFront and HeldItem both ensure that the layer will only be drawn once for a given player because of the checks against the player draw data, despite being placed at multiple layer positions. 
	/// </summary>
	public class Multiple : Position, IEnumerable
	{
		/// <inheritdoc cref="Multiple"/>
		public Multiple()
		{
		}

		public delegate bool Condition(PlayerDrawSet drawInfo);
		public IList<(Between, Condition)> Positions { get; } = new List<(Between, Condition)>();

		public void Add(Between position, Condition condition) => Positions.Add((position, condition));

		public IEnumerator GetEnumerator() => Positions.GetEnumerator();
	}

	/// <summary>
	/// Order immediately before the provided parent layer. This will cause the layer to draw immediately behind the parent layer. The visibility and draw order of the layer is also bound to the parent layer, if the parent layer is moved or hidden, this layer will also be moved or hidden.
	/// </summary>
	public class BeforeParent : Position
	{
		public PlayerDrawLayer Parent { get; }

		/// <inheritdoc cref="BeforeParent"/>
		public BeforeParent(PlayerDrawLayer parent)
		{
			Parent = parent;
		}
	}

	/// <summary>
	/// Order immediately after the provided parent layer. This will cause the layer to draw immediately in front of the parent layer. The visibility and draw order of the layer is also bound to the parent layer, if the parent layer is moved or hidden, this layer will also be moved or hidden.
	/// </summary>
	public class AfterParent : Position
	{
		public PlayerDrawLayer Parent { get; }

		/// <inheritdoc cref="AfterParent"/>
		public AfterParent(PlayerDrawLayer parent)
		{
			Parent = parent;
		}
	}
}
