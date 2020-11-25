using System.Collections.Generic;
using Terraria.DataStructures;

namespace Terraria.ModLoader
{
	[Autoload(false)]
	internal sealed class LegacyPlayerDrawLayer : PlayerDrawLayer
	{
		/// <summary> The delegate of this method, which can either do the actual drawing or add draw data, depending on what kind of layer this is. </summary>
		public delegate void LayerFunction(ref PlayerDrawSet info);
		private readonly LayerFunction Layer;

		/// <summary> The delegate of this method, which can either do the actual drawing or add draw data, depending on what kind of layer this is. </summary>
		public delegate bool LayerCondition(PlayerDrawSet info);
		private readonly LayerCondition Condition;

		private readonly string CustomName;
		private readonly bool HeadLayer;

		private PhysicalGroup _group;
		public override PhysicalGroup Group => _group;

		public override string Name => CustomName;
		public override bool IsHeadLayer => HeadLayer;

		/// <summary> Creates a LegacyPlayerLayer with the given mod name, identifier name, and drawing action. </summary>
		public LegacyPlayerDrawLayer(string name, LayerFunction layer, PhysicalGroup group = null, bool isHeadLayer = false, LayerCondition condition = null) {
			CustomName = name;
			Layer = layer;
			_group = group;
			HeadLayer = isHeadLayer;
			Condition = condition;
		}

		public override bool GetDefaultVisiblity(PlayerDrawSet drawInfo) => Condition?.Invoke(drawInfo) ?? true;

		protected override void Draw(ref PlayerDrawSet drawInfo) => Layer(ref drawInfo);

		public override IEnumerable<LayerConstraint> GetConstraints() {
			// poor man's IndexOf because IReadOnlyList doesn't expose the method
			int index = 0;
			while (PlayerDrawLayerHooks.VanillaLayers[index] != this)
				index++;

			if (index > 0)
				yield return LayerConstraint.After(PlayerDrawLayerHooks.VanillaLayers[index - 1]);
			if (index < PlayerDrawLayerHooks.VanillaLayers.Count - 1)
				yield return LayerConstraint.Before(PlayerDrawLayerHooks.VanillaLayers[index + 1]);
		}
	}
}
