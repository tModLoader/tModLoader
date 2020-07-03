using Microsoft.Xna.Framework;

namespace Terraria.ModLoader
{
	/// <summary>
	/// Represents a style of water that gets drawn, based on factors such as the background. This is used to determine the color of the water, as well as other things as determined by the hooks below.
	/// </summary>
	public abstract class ModWaterStyle
	{
		/// <summary>
		/// The mod that added this style of water.
		/// </summary>
		public Mod mod {
			get;
			internal set;
		}

		/// <summary>
		/// The internal name of this water style.
		/// </summary>
		public string Name {
			get;
			internal set;
		}

		/// <summary>
		/// The ID of the water style.
		/// </summary>
		public int Type {
			get;
			internal set;
		}

		internal string texture;
		internal string blockTexture;

		/// <summary>
		/// Allows you to automatically add a ModWaterStyle instead of using Mod.AddWaterStyle. Return true to allow autoloading; by default returns the mod's autoload property. Name is initialized to the overriding class name, and texture is initialized to the namespace and overriding class name with periods replaced with slashes. BlockTexture is initialized to texture with "_Block" added at the end. Use this to either force or stop an autoload, change the name that identifies this type of ModWaterStyle, and/or change the texture paths used by this ModWaterStyle.
		/// </summary>
		public virtual bool Autoload(ref string name, ref string texture, ref string blockTexture) {
			return mod.Properties.Autoload;
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
	public class ModWaterfallStyle
	{
		/// <summary>
		/// The mod that added this style of waterfall.
		/// </summary>
		public Mod mod {
			get;
			internal set;
		}

		/// <summary>
		/// The internal name of this waterfall style.
		/// </summary>
		public string Name {
			get;
			internal set;
		}

		/// <summary>
		/// The ID of this waterfall style.
		/// </summary>
		public int Type {
			get;
			internal set;
		}

		internal string texture;

		/// <summary>
		/// Allows you to automatically add a ModWaterfallStyle instead of using Mod.AddWaterfallStyle. Return true to allow autoloading; by default returns the mod's autoload property. Name is initialized to the overriding class name, and texture is initialized to the namespace and overriding class name with periods replaced with slashes. Use this to either force or stop an autoload, change the name that identifies this type of ModWaterStyle, or change the texture path used by this ModWaterfallStyle.
		/// </summary>
		public virtual bool Autoload(ref string name, ref string texture) {
			return mod.Properties.Autoload;
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
