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
		public Texture2D barTexture;

		/// <summary>
		/// The screen position of the center of the bar.
		/// </summary>
		public Vector2 barCenter;
		//No barColor because it consists of 6 separate frames with different things on each frame. Easier to supply a custom texture.

		/// <summary>
		/// The displayed icon texture.
		/// </summary>
		public Texture2D iconTexture;

		/// <summary>
		/// The icon textures frame.
		/// </summary>
		public Rectangle iconFrame;

		/// <summary>
		/// The tint of the icon.
		/// </summary>
		public Color iconColor;

		/// <summary>
		/// The % the bar should be filled with.
		/// </summary>
		public float lifePercentToShow;

		/// <summary>
		/// The % the shield bar should be filled with. Defaults to 0f (no shield drawn).
		/// </summary>
		public float shieldPercentToShow;

		/// <summary>
		/// The scale the icon is drawn with. Defaults to 1f, modify if icon is bigger or smaller than 26x28.
		/// </summary>
		public float iconScale;

		public BossBarDrawParams(Texture2D barTexture, Vector2 barCenter, Texture2D iconTexture, Rectangle iconFrame, Color iconColor, float lifePercentToShow, float shieldPercentToShow = 0f, float iconScale = 1f) {
			this.barTexture = barTexture;
			this.barCenter = barCenter;
			this.iconTexture = iconTexture;
			this.iconFrame = iconFrame;
			this.iconColor = iconColor;
			this.lifePercentToShow = lifePercentToShow;
			this.shieldPercentToShow = shieldPercentToShow;
			this.iconScale = iconScale;
		}

		public void Deconstruct(out Texture2D barTexture, out Vector2 barCenter, out Texture2D iconTexture, out Rectangle iconFrame, out Color iconColor, out float lifePercentToShow, out float shieldPercentToShow, out float iconScale) {
			barTexture = this.barTexture;
			barCenter = this.barCenter;
			iconTexture = this.iconTexture;
			iconFrame = this.iconFrame;
			iconColor = this.iconColor;
			lifePercentToShow = this.lifePercentToShow;
			shieldPercentToShow = this.shieldPercentToShow;
			iconScale = this.iconScale;
		}
	}
}
