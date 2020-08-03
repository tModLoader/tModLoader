using Terraria.DataStructures;

namespace Terraria.ModLoader
{
	/// <summary> This class represents a DrawLayer for the player's map icon, and uses PlayerDrawHeadInfo as its InfoType. Drawing should be done directly through drawInfo.spriteBatch. </summary>
	[Autoload(false)]
	public sealed class LegacyPlayerHeadLayer : PlayerHeadLayer
	{
		/// <summary> The delegate of this method, which can either do the actual drawing or add draw data, depending on what kind of layer this is. </summary>
		public readonly LayerFunction Layer;

		private readonly string CustomName;

		public override string Name => CustomName;

		/// <summary> Creates a LegacyPlayerLayer with the given mod name, identifier name, and drawing action. </summary>
		public LegacyPlayerHeadLayer(Mod mod, string name, LayerFunction layer) {
			Mod = mod;
			CustomName = name;
			Layer = layer;
		}

		/// <summary> Creates a LegacyPlayerLayer with the given mod name, identifier name, parent layer, and drawing action. </summary>
		public LegacyPlayerHeadLayer(Mod mod, string name, PlayerHeadLayer parent, LayerFunction layer) : this(mod, name, layer) {
			Parent = parent;
		}

		public override void Draw(ref PlayerDrawHeadSet drawInfo) => Layer(ref drawInfo);
	}
}
