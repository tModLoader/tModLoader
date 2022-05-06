using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria.DataStructures
{
	/// <summary>
	/// Holds data required for boss bar drawing.
	/// </summary>
	public struct BossBarDrawParams
	{
		/// <summary>
		/// The texture with fixed dimensions (516x348) containing all the necessary parts.
		/// </summary>
		public Texture2D BarTexture;

		/// <summary>
		/// The screen position of the center of the bar.
		/// </summary>
		public Vector2 BarCenter;
		//No barColor because it consists of 6 separate frames with different things on each frame. Easier to supply a custom texture.

		/// <summary>
		/// The displayed icon texture.
		/// </summary>
		public Texture2D IconTexture;

		/// <summary>
		/// The icon textures frame.
		/// </summary>
		public Rectangle IconFrame;

		/// <summary>
		/// The tint of the icon.
		/// </summary>
		public Color IconColor;

		/// <summary>
		/// The % the bar should be filled with.
		/// </summary>
		public float LifePercentToShow;

		/// <summary>
		/// The % the shield bar should be filled with. Defaults to 0f (no shield drawn).
		/// </summary>
		public float ShieldPercentToShow;

		/// <summary>
		/// The scale the icon is drawn with. Defaults to 1f, modify if icon is bigger or smaller than 26x28.
		/// </summary>
		public float IconScale;

		public BossBarDrawParams(Texture2D barTexture, Vector2 barCenter, Texture2D iconTexture, Rectangle iconFrame, Color iconColor, float lifePercentToShow, float shieldPercentToShow = 0f, float iconScale = 1f) {
			BarTexture = barTexture;
			BarCenter = barCenter;
			IconTexture = iconTexture;
			IconFrame = iconFrame;
			IconColor = iconColor;
			LifePercentToShow = lifePercentToShow;
			ShieldPercentToShow = shieldPercentToShow;
			IconScale = iconScale;
		}

		public void Deconstruct(out Texture2D barTexture, out Vector2 barCenter, out Texture2D iconTexture, out Rectangle iconFrame, out Color iconColor, out float lifePercentToShow, out float shieldPercentToShow, out float iconScale) {
			barTexture = BarTexture;
			barCenter = BarCenter;
			iconTexture = IconTexture;
			iconFrame = IconFrame;
			iconColor = IconColor;
			lifePercentToShow = LifePercentToShow;
			shieldPercentToShow = ShieldPercentToShow;
			iconScale = IconScale;
		}
	}
}
