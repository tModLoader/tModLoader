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
	/// Places this layer between the two provided layers. This layer will draw over layer1 and under layer2.
	/// <para/> <see langword="null"/> can be used to indicate that the exact position of the layer doesn't matter aside from being before/after the other non-null argument. For example <c>new Between(PlayerDrawLayers.HairBack, null)</c> would place the layer anywhere after <c>HairBack</c>, while <c>new Between(null, PlayerDrawLayers.HairBack)</c> would be anywhere before <c>HairBack</c>. For ordering before or after all vanilla layers, the helper properties <see cref="PlayerDrawLayers.BeforeFirstVanillaLayer"/> and <see cref="PlayerDrawLayers.AfterLastVanillaLayer"/> can be used directly.
	/// <para/> The layer parameters used must have fixed positions, meaning that layers registered using either <see cref="Multiple"/>, <see cref="BeforeParent"/>, or <see cref="AfterParent"/> are not valid. For vanilla layers, this includes <see cref="PlayerDrawLayers.FrontAccFront"/> and <see cref="PlayerDrawLayers.HeldItem"/>. If ordering in relation to these layers, consider either using <see cref="AfterParent"/> or <see cref="BeforeParent"/> to draw at whatever positions that layer is actually drawn, or referencing a different layer for ordering instead.
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
	/// Places this layer into multiple <see cref="Position"/>s. Use this as a helper for layers that need to move around in the draw order rather than making multiple <see cref="PlayerDrawLayer"/> 'slots' manually.
	/// <para/> An example of this can be seen in <see href="https://github.com/tModLoader/tModLoader/blob/stable/patches/tModLoader/Terraria/DataStructures/PlayerDrawLayers.tML.cs#L158">PlayerDrawLayers.tML.cs</see>. Note how the conditions for <c>FrontAccFront</c> and <c>HeldItem</c> are mutually exclusive, ensuring that the layer will only be drawn once for a given player.
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
	/// Places this layer immediately before (under) the provided parent layer. The visibility and draw order of the layer is also bound to the parent layer, if the parent layer is moved or hidden, this layer will also be moved or hidden.
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
	/// Places this layer immediately after (over) the provided parent layer. The visibility and draw order of the layer is also bound to the parent layer, if the parent layer is moved or hidden, this layer will also be moved or hidden.
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
