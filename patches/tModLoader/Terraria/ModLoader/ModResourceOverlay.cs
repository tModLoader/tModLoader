using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.ResourceSets;

namespace Terraria.ModLoader
{
	/// <summary>
	/// A class that is used to customize how the vanilla resource displays (Classic, Fancy and Bars) are drawn
	/// </summary>
	public abstract class ModResourceOverlay : ModType
	{
		public int Type { get; internal set; }

		protected sealed override void Register() {
			ModTypeLookup<ModResourceOverlay>.Register(this);
			Type = ResourceOverlayLoader.Add(this);
		}

		public sealed override void SetupContent() => SetStaticDefaults();

		/// <summary>
		/// Allows you to modify how a heart in the Classic display set is drawn
		/// </summary>
		/// <param name="context">Contains the drawing data for the heart being drawn.  You should use the ResourceOverlayDrawContext.Draw method for all drawing</param>
		/// <returns><see langword="true"/> if the intended heart sprite should draw, <see langword="false"/> otherwise.</returns>
		public virtual bool PreDrawClassicLifeHeart(ref ResourceOverlayDrawContext context) {
			return true;
		}

		/// <summary>
		/// Allows you to draw on top of a heart in the Classic display set
		/// </summary>
		/// <param name="context">Contains the drawing data for the heart being drawn.  You should use the ResourceOverlayDrawContext.Draw method for all drawing</param>
		public virtual void PostDrawClassicLifeHeart(ResourceOverlayDrawContext context) {
		}

		/// <summary>
		/// Allows you to modify how a mana star in the Classic display set is drawn
		/// </summary>
		/// <param name="context">Contains the drawing data for the mana star being drawn.  You should use the ResourceOverlayDrawContext.Draw method for all drawing</param>
		/// <returns><see langword="true"/> if the intended mana star sprite should draw, <see langword="false"/> otherwise.</returns>
		public virtual bool PreDrawClassicManaStar(ref ResourceOverlayDrawContext context) {
			return true;
		}

		/// <summary>
		/// Allows you to draw on top of a mana star in the Classic display set
		/// </summary>
		/// <param name="context">Contains the drawing data for the heart being drawn.  You should use the ResourceOverlayDrawContext.Draw method for all drawing</param>
		public virtual void PostDrawClassicManaStar(ResourceOverlayDrawContext context) {
		}

		/// <summary>
		/// Allows you to draw before the hearts in the Classic display set are drawn
		/// </summary>
		/// <param name="snapshot">A snapshot of the stats from Main.LocalPlayer</param>
		/// <param name="lifeTextColor">The color to draw the text above the hearts with</param>
		/// <param name="drawText">Whether the text above the hearts should draw</param>
		/// <returns>Whether the hearts in the Classic display set are drawn</returns>
		public virtual bool PreDrawClassicLifeDisplay(PlayerStatsSnapshot snapshot, ref Color lifeTextColor, out bool drawText) {
			drawText = true;
			return true;
		}

		/// <summary>
		/// Allows you to draw after the hearts in the Classic display set are drawn
		/// </summary>
		/// <param name="snapshot">A snapshot of the stats from Main.LocalPlayer</param>
		/// <param name="lifeTextColor">The color the text above the hearts was drawn with</param>
		/// <param name="drawText">Whether the text above the hearts was drawn</param>
		public virtual void PostDrawClassicLifeDisplay(PlayerStatsSnapshot snapshot, Color lifeTextColor, bool drawText) {
		}

		/// <summary>
		/// Allows you to draw before the mana stars in the Classic display set are drawn
		/// </summary>
		/// <param name="snapshot">A snapshot of the stats from Main.LocalPlayer</param>
		/// <param name="lifeTextColor">The color to draw the text above the mana stars with</param>
		/// <param name="drawText">Whether the text above the mana stars should draw</param>
		/// <returns>Whether the mana stars in the Classic display set are drawn</returns>
		public virtual bool PreDrawClassicManaDisplay(PlayerStatsSnapshot snapshot, ref Color lifeTextColor, out bool drawText) {
			drawText = true;
			return true;
		}

		/// <summary>
		/// Allows you to draw after the mana stars in the Classic display set are drawn
		/// </summary>
		/// <param name="snapshot">A snapshot of the stats from Main.LocalPlayer</param>
		/// <param name="lifeTextColor">The color the text above the mana stars was drawn with</param>
		/// <param name="drawText">Whether the text above the mana stars was drawn</param>
		public virtual void PostDrawClassicManaDisplay(PlayerStatsSnapshot snapshot, Color lifeTextColor, bool drawText) {
		}
	}
}
