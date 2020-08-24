using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Terraria.GameContent.Liquid;

namespace Terraria.ModLoader
{
	/// <summary>
	/// Represents a style of water that gets drawn, based on factors such as the background. This is used to determine the color of the water, as well as other things as determined by the hooks below.
	/// </summary>
	public abstract class ModWaterStyle:ModTexturedType
	{
		/// <summary>
		/// The ID of the water style.
		/// </summary>
		public int Type {get;internal set;}

		public virtual string BlockTexture => Texture + "_Block";

		protected sealed override void Register() {
			Type = WaterStyleLoader.ReserveStyle();

			ModTypeLookup<ModWaterStyle>.Register(this);
			WaterStyleLoader.waterStyles.Add(this);
		}

		public override void SetupContent() {
			LiquidRenderer.Instance._liquidTextures[Type] = ModContent.GetTexture(Texture);
			TextureAssets.Liquid[Type] = ModContent.GetTexture(BlockTexture);
		}

		/// <summary>
		/// Whether the conditions have been met for this water style to be used. Typically Main.bgStyle is checked to determine whether a water style should be used. Returns false by default.
		/// </summary>
		public virtual bool ChooseWaterStyle() {
			return false;
		}

		/// <summary>
		/// The ID of the waterfall style the game should use when this water style is in use.
		/// </summary>
		public abstract int ChooseWaterfallStyle();

		/// <summary>
		/// The ID of the dust that is created when anything splashes in water.
		/// </summary>
		public abstract int GetSplashDust();

		/// <summary>
		/// The ID of the gore that represents droplets of water falling down from a block.
		/// </summary>
		public abstract int GetDropletGore();

		/// <summary>
		/// Allows you to modify the light levels of the tiles behind the water. The light color components will be multiplied by the parameters.
		/// </summary>
		public virtual void LightColorMultiplier(ref float r, ref float g, ref float b) {
			r = 0.88f;
			g = 0.96f;
			b = 1.015f;
		}

		/// <summary>
		/// Allows you to change the hair color resulting from the biome hair dye when this water style is in use.
		/// </summary>
		public virtual Color BiomeHairColor() {
			return new Color(28, 216, 94);
		}
	}

	/// <summary>
	/// Represents a style of waterfalls that gets drawn. This is mostly used to determine the color of the waterfall.
	/// </summary>
	public class ModWaterfallStyle:ModTexturedType
	{
		/// <summary>
		/// The ID of this waterfall style.
		/// </summary>
		public int Type {get;internal set;}

		protected sealed override void Register() {
			Type = WaterfallStyleLoader.ReserveStyle();

			ModTypeLookup<ModWaterfallStyle>.Register(this);
			WaterfallStyleLoader.waterfallStyles.Add(this);
		}

		public override void SetupContent() {
			Main.instance.waterfallManager.waterfallTexture[Type] = ModContent.GetTexture(Texture);
		}

		/// <summary>
		/// Allows you to create light at a tile occupied by a waterfall of this style.
		/// </summary>
		public virtual void AddLight(int i, int j) {
		}

		/// <summary>
		/// Allows you to determine the color multiplier acting on waterfalls of this style. Useful for waterfalls whose colors change over time.
		/// </summary>
		public virtual void ColorMultiplier(ref float r, ref float g, ref float b, float a) {
		}
	}
}
