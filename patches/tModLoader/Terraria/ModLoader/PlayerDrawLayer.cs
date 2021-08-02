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
		public abstract class Transformation
		{
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

		public bool Visible { get; private set; } = true;

		public virtual Transformation Transform { get; }


		private readonly List<PlayerDrawLayer> _childrenBefore = new List<PlayerDrawLayer>();
		public IReadOnlyList<PlayerDrawLayer> ChildrenBefore => _childrenBefore;

		private readonly List<PlayerDrawLayer> _childrenAfter = new List<PlayerDrawLayer>();
		public IReadOnlyList<PlayerDrawLayer> ChildrenAfter => _childrenAfter;

		/// <summary> Returns whether or not this layer should be rendered for the minimap icon. </summary>
		public virtual bool IsHeadLayer => false;

		public void Hide() => Visible = false;

		internal void AddChildBefore(PlayerDrawLayer child) => _childrenBefore.Add(child);
		internal void AddChildAfter(PlayerDrawLayer child) => _childrenAfter.Add(child);

		internal void ClearChildren() {
			_childrenBefore.Clear();
			_childrenAfter.Clear();
		}

		/// <summary> Returns the layer's default visibility. This is usually called as a layer is queued for drawing, but modders can call it too for information. </summary>
		/// <returns> Whether or not this layer will be visible by default. Modders can hide layers later, if needed.</returns>
		public virtual bool GetDefaultVisiblity(PlayerDrawSet drawInfo) => true;

		public abstract Position GetDefaultPosition();

		internal void ResetVisiblity(PlayerDrawSet drawInfo) {
			foreach (var child in ChildrenBefore)
				child.ResetVisiblity(drawInfo);

			Visible = GetDefaultVisiblity(drawInfo);

			foreach (var child in ChildrenAfter)
				child.ResetVisiblity(drawInfo);
		}

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

		protected sealed override void Register() {
			ModTypeLookup<PlayerDrawLayer>.Register(this);
			PlayerDrawLayerLoader.Add(this);
		}

		public sealed override void SetupContent() => SetStaticDefaults();

		public override string ToString() => Name;
	}
}
