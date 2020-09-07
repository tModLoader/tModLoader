using System.Collections.Generic;

namespace Terraria.ModLoader
{
	/// <summary> This class represents a layer of the drawing of an object, using a certain type of InfoType to help with its drawing. </summary>
	/// <typeparam name="InfoType"></typeparam>
	public abstract class DrawLayer<InfoType> : ModType
	{
		/// <summary> Layer constraints are used to control locations of draw layers, and point to a position before or after a specified layer. </summary>
		public struct LayerConstraint
		{
			/// <summary> Whether or not layers that use this constraint should be placed before or after the referenced layer. </summary>
			public DrawLayer<InfoType> layer;
			/// <summary> The layer this constraint points to. </summary>
			public bool before;

			/// <summary> Creates a new Layer constraint, which can be used to control locations of draw layers. </summary>
			/// <param name="layer"> The layer this constraint points to. </param>
			/// <param name="before"> Whether or not layers that use this constraint should be placed before or after the referenced layer. </param>
			public LayerConstraint(DrawLayer<InfoType> layer, bool before) {
				this.layer = layer;
				this.before = before;
			}
		}

		/// <summary> The delegate used for draw layers. </summary>
		public delegate void LayerFunction(ref InfoType info);

		/// <summary> Whether or not this DrawLayer should be drawn. For vanilla layers, this will be set to true before all drawing-related hooks are called. For modded layers, you must set this to true or false yourself. </summary>
		public bool visible = true;

		/// <summary> If this is true, this layer will appear before the parent. </summary>
		public LayerConstraint constraint;

		/// <summary> The parent of this DrawLayer. Affects layer sorting, and if the parent is not drawn, this layer will not be drawn either. Defaults to null, which skips the parent check.</summary>
		public virtual DrawLayer<InfoType> Parent { get; set; }

		protected DrawLayer() {
		}

		/// <summary>
		/// Whether or not this layer should be drawn. Returns false if visible is false. If layerList is of type List<DrawLayer<InfoType>> and parent is not null, this will also return false if the parent is not drawn.
		/// </summary>
		public bool ShouldDraw<T>(IList<T> layers) where T : DrawLayer<InfoType> {
			if (!visible) {
				return false;
			}

			DrawLayer<InfoType> parentLayer = Parent;

			while (parentLayer != null) {
				if (!parentLayer.visible || !layers.Contains((T)parentLayer)) {
					return false;
				}

				parentLayer = parentLayer.Parent;
			}

			return true;
		}

		/// <summary> Draws this layer. </summary>
		public abstract void Draw(ref InfoType drawInfo);

		public override string ToString() => Name;
	}
}
