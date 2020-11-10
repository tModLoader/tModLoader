using System.Collections.Generic;

namespace Terraria.ModLoader
{
	/// <summary> This class represents a layer of the drawing of an object, using a certain type of InfoType to help with its drawing. </summary>
	/// <typeparam name="LayerType">The base layer type (for constraints/parent fields)</typeparam>
	/// <typeparam name="InfoType">The DrawData struct type</typeparam>
	public abstract class DrawLayer<LayerType, InfoType> : ModType 
		where LayerType : DrawLayer<LayerType, InfoType> 
		where InfoType : struct {

		/// <summary> Layer constraints are used to control locations of draw layers, and point to a position before or after a specified layer. </summary>
		public struct LayerConstraint {
			/// <summary> Whether or not layers that use this constraint should be placed before or after the referenced layer. </summary>
			public LayerType layer;
			/// <summary> The layer this constraint points to. </summary>
			public bool before;

			/// <summary> Creates a new Layer constraint, which can be used to control locations of draw layers. </summary>
			/// <param name="layer"> The layer this constraint points to. </param>
			/// <param name="before"> Whether or not layers that use this constraint should be placed before or after the referenced layer. </param>
			public LayerConstraint(LayerType layer, bool before) {
				this.layer = layer;
				this.before = before;
			}

			public static LayerConstraint Before(LayerType layer) => new LayerConstraint(layer, true);
			public static LayerConstraint After(LayerType layer) => new LayerConstraint(layer, false);

			public static LayerConstraint Before<T>() where T : LayerType => Before(ModContent.GetInstance<T>());
			public static LayerConstraint After<T>() where T : LayerType => After(ModContent.GetInstance<T>());
		}

		/// <summary>
		/// Use a physical group to apply transformations to the layers being rendered
		/// </summary>
		public abstract class PhysicalGroup {
			public virtual PhysicalGroup Parent { get; }

			/// <summary>
			/// Add a transformation to the drawInfo
			/// </summary>
			protected abstract void PreDraw(ref InfoType drawInfo);

			/// <summary>
			/// Reverse a transformation from the drawInfo
			/// </summary>
			protected abstract void PostDraw(ref InfoType drawInfo);

			public void PreDrawRecursive(ref InfoType drawInfo) {
				Parent?.PreDrawRecursive(ref drawInfo);
				PreDraw(ref drawInfo);
			}

			public void PostDrawRecursive(ref InfoType drawInfo) {
				PostDraw(ref drawInfo);
				Parent?.PostDrawRecursive(ref drawInfo);
			}
		}

		/// <summary> Whether or not this DrawLayer should be drawn. This will be set to the value of GetDefaultVisiblity in preparation for a draw pass. </summary>
		public bool visible = true;

		/// <summary> The parent of this DrawLayer. If the parent is not drawn, this layer will not be drawn either. Defaults to null, which skips the parent check.</summary>
		public virtual LayerType Parent { get; }

		public virtual PhysicalGroup Group { get; }

		/// <summary>
		/// Checks this layer's visibility, and parent layer visibility
		/// </summary>
		public bool IsVisible() => visible && (Parent?.IsVisible() ?? true);

		/// <summary> Returns the layer's default visibility. This is usually called as a layer is queued for drawing, but modders can call it too for information. </summary>
		/// <returns> Whether or not this layer will be visible by default. Modders can hide and unhide layers later, if needed.</returns>
		public virtual bool GetDefaultVisiblity(InfoType drawInfo) => true;

		/// <summary>
		/// yield LayerConstraint.Before/LayerConstraint.After to position this layer with respect to others.
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerable<LayerConstraint> GetConstraints();

		/// <summary> Draws this layer. </summary>
		protected abstract void Draw(ref InfoType drawInfo);

		/// <summary>
		/// Draws this layer, if IsVisible, applying the offsets/effects from Group (if any)
		/// </summary>
		/// <param name="drawInfo"></param>
		public void PositionAndDraw(ref InfoType drawInfo) {
			if (!IsVisible())
				return;

			Group?.PreDrawRecursive(ref drawInfo);
			Draw(ref drawInfo);
			Group?.PostDrawRecursive(ref drawInfo);
		}

		public override string ToString() => Name;
	}
}
