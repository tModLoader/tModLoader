using System;
using System.Collections;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class represents a DrawLayer for the player, and uses PlayerDrawInfo as its InfoType. Drawing should be done by adding Terraria.DataStructures.DrawData objects to Main.playerDrawData.
	/// </summary>
	[Autoload]
	public abstract partial class PlayerDrawLayer : ModType 
	{
		public abstract class Position
		{}

		public sealed class Between : Position
		{
			public PlayerDrawLayer Layer1 { get; }
			public PlayerDrawLayer Layer2 { get; }

			public Between(PlayerDrawLayer layer1, PlayerDrawLayer layer2) {
				Layer1 = layer1;
				Layer2 = layer2;
			}

			public Between() {
			}
		}

		public class Multiple : Position, IEnumerable
		{
			public delegate bool Condition(PlayerDrawSet drawInfo);
			public IList<(Between, Condition)> Positions { get; } = new List<(Between, Condition)>();

			public void Add(Between position, Condition condition) => Positions.Add((position, condition));

			public IEnumerator GetEnumerator() => Positions.GetEnumerator();
		}

		public class Before : Position
		{
			public PlayerDrawLayer Layer { get; }

			public Before(PlayerDrawLayer layer) {
				Layer = layer;
			}
		}

		public class After : Position
		{
			public PlayerDrawLayer Layer { get; }

			public After(PlayerDrawLayer layer) {
				Layer = layer;
			}
		}

		public abstract class Transformation {
			public virtual Transformation Parent { get; }

			/// <summary>
			/// Add a transformation to the drawInfo
			/// </summary>
			protected abstract void PreDraw(ref PlayerDrawSet drawInfo);

			/// <summary>
			/// Reverse a transformation from the drawInfo
			/// </summary>
			protected abstract void PostDraw(ref PlayerDrawSet drawInfo);

			public void PreDrawRecursive(ref PlayerDrawSet drawInfo) {
				Parent?.PreDrawRecursive(ref drawInfo);
				PreDraw(ref drawInfo);
			}

			public void PostDrawRecursive(ref PlayerDrawSet drawInfo) {
				PostDraw(ref drawInfo);
				Parent?.PostDrawRecursive(ref drawInfo);
			}
		}

		public virtual Transformation Transform { get; }


		private readonly List<PlayerDrawLayer> _childrenBefore = new List<PlayerDrawLayer>();
		public IReadOnlyList<PlayerDrawLayer> ChildrenBefore => _childrenBefore;

		private readonly List<PlayerDrawLayer> _childrenAfter = new List<PlayerDrawLayer>();
		public IReadOnlyList<PlayerDrawLayer> ChildrenAfter => _childrenAfter;

		internal void AddChildBefore(PlayerDrawLayer child) => _childrenBefore.Add(child);
		internal void AddChildAfter(PlayerDrawLayer child) => _childrenAfter.Add(child);

		internal void ClearChildren() {
			_childrenBefore.Clear();
			_childrenAfter.Clear();
		}

		public bool Visible { get; private set; }

		public void Hide() => Visible = false;

		/// <summary> Returns the layer's default visibility. This is usually called as a layer is queued for drawing, but modders can call it too for information. </summary>
		/// <returns> Whether or not this layer will be visible by default. Modders can hide and unhide layers later, if needed.</returns>
		public virtual bool GetDefaultVisiblity(PlayerDrawSet drawInfo) => true;

		internal void ResetVisiblity(PlayerDrawSet drawInfo) {
			foreach (var child in ChildrenBefore)
				child.ResetVisiblity(drawInfo);

			Visible = GetDefaultVisiblity(drawInfo);

			foreach (var child in ChildrenAfter)
				child.ResetVisiblity(drawInfo);
		}

		public abstract Position GetDefaultPosition();

		/// <summary> Returns whether or not this layer should be rendered for the minimap icon. </summary>
		public virtual bool IsHeadLayer => false;

		/// <summary> Draws this layer. </summary>
		protected abstract void Draw(ref PlayerDrawSet drawInfo);

		public void DrawWithTransformationAndChildren(ref PlayerDrawSet drawInfo) {
			if (!Visible)
				return;

			Transform?.PreDrawRecursive(ref drawInfo);

			foreach (var child in ChildrenBefore)
				child.DrawWithTransformationAndChildren(ref drawInfo);

			Draw(ref drawInfo);

			foreach (var child in ChildrenAfter)
				child.DrawWithTransformationAndChildren(ref drawInfo);

			Transform?.PostDrawRecursive(ref drawInfo);
		}

		protected override void Register() {
			ModTypeLookup<PlayerDrawLayer>.Register(this);
			PlayerDrawLayerHooks.Add(this);
		}

		public override string ToString() => Name;
	}
}
