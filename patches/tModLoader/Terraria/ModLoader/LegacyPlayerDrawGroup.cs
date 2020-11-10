using System.Collections.Generic;
using Terraria.DataStructures;

namespace Terraria.ModLoader
{
	[Autoload(false)]
	internal sealed class LegacyPlayerDrawGroup : PlayerDrawLayer.PhysicalGroup
	{
		/// <summary> The delegate of this method, which can either do the actual drawing or add draw data, depending on what kind of layer this is. </summary>
		public delegate void LayerFunction(ref PlayerDrawSet info);
		private readonly LayerFunction PreDrawFunc;
		private readonly LayerFunction PostDrawFunc;

		private PlayerDrawLayer.PhysicalGroup _parent;
		public override PlayerDrawLayer.PhysicalGroup Parent => _parent;

		/// <summary> Creates a LegacyPlayerLayer with the given mod name, identifier name, and drawing action. </summary>
		public LegacyPlayerDrawGroup(LayerFunction preDraw, LayerFunction postDraw, PlayerDrawLayer.PhysicalGroup parent = null) {
			PreDrawFunc = preDraw;
			PostDrawFunc = postDraw;
			_parent = parent;
		}

		protected override void PreDraw(ref PlayerDrawSet drawInfo) => PreDrawFunc(ref drawInfo);

		protected override void PostDraw(ref PlayerDrawSet drawInfo) => PostDrawFunc(ref drawInfo);
	}
}
