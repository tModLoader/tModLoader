using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Text.RegularExpressions;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class serves as a place for you to define your own logic for drawing the player's life and mana resources.<br/>
	/// For modifying parts of the vanilla display sets, use <see cref="ModResourceOverlay"/>.
	/// </summary>
	public abstract class ModResourceDisplaySet : ModType, IPlayerResourcesDisplaySet, IConfigKeyHolder
	{
		public int Type { get; internal set; }

		public bool Selected => Main.ResourceSetsManager.ActiveSet == this;

		/// <summary>
		/// Gets the name for this resource display set based on its DisplayName and the current culture
		/// </summary>
		public string DisplayedName => DisplayName.GetTranslation(Language.ActiveCulture);

		/// <summary>
		/// Included only for completion's sake.  Returns DisplayName.Key
		/// </summary>
		public string NameKey => DisplayName.Key;

		/// <summary>
		/// The name used to get this resource display set.  Returns <see cref="ModType.FullName"/>
		/// </summary>
		public string ConfigKey => FullName;

		/// <summary>
		/// The translations for the display name of this item.
		/// </summary>
		public ModTranslation DisplayName { get; internal set; }

		/// <summary>
		/// The current snapshot of the life and mana stats for Main.LocalPlayer
		/// </summary>
		public static PlayerStatsSnapshot PlayerStats => new PlayerStatsSnapshot(Main.LocalPlayer);

		protected sealed override void Register() {
			ModTypeLookup<ModResourceDisplaySet>.Register(this);

			DisplayName = LocalizationLoader.GetOrCreateTranslation(Mod, $"ResourceDisplaySet.{Name}");

			Type = ResourceDisplaySetLoader.Add(this);
		}

		public sealed override void SetupContent() {
			AutoStaticDefaults();
			SetStaticDefaults();
		}

		/// <summary>
		/// Automatically sets certain static defaults. Override this if you do not want the properties to be set for you.
		/// </summary>
		public virtual void AutoStaticDefaults() {
			if (DisplayName.IsDefault())
				DisplayName.SetDefault(Regex.Replace(DisplayedName, "([A-Z])", " $1").Trim());
		}

		public void Draw() {
			var stats = PlayerStats;
			PreDrawResources(stats);

			Color color = new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor);
			if (ResourceOverlayLoader.PreDrawResourceDisplay(stats, this, true, ref color, out bool drawText))
				DrawLife(Main.spriteBatch);
			ResourceOverlayLoader.PostDrawResourceDisplay(stats, this, true, color, drawText);

			color = new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor);
			if (ResourceOverlayLoader.PreDrawResourceDisplay(stats, this, false, ref color, out drawText))
				DrawMana(Main.spriteBatch);
			ResourceOverlayLoader.PostDrawResourceDisplay(stats, this, false, color, drawText);
		}

		/// <summary>
		/// Allows you to initialize fields, textures, etc. before drawing occurs
		/// </summary>
		/// <param name="snapshot">A copy of <see cref="PlayerStats"/></param>
		public virtual void PreDrawResources(PlayerStatsSnapshot snapshot) { }

		/// <summary>
		/// Draw the life resources for your display set here
		/// </summary>
		/// <param name="spriteBatch"></param>
		public virtual void DrawLife(SpriteBatch spriteBatch) { }

		/// <summary>
		/// Draw the mana resources for your display set here
		/// </summary>
		/// <param name="spriteBatch"></param>
		public virtual void DrawMana(SpriteBatch spriteBatch) { }

		public void TryToHover() {
			if (PreHover(out bool hoveringLife)) {
				if (hoveringLife)
					CommonResourceBarMethods.DrawLifeMouseOver();
				else
					CommonResourceBarMethods.DrawManaMouseOver();
			}
		}

		/// <summary>
		/// Allows you to specify if the vanilla life/mana hover text should display
		/// </summary>
		/// <param name="hoveringLife">Whether the hover text should be for life (<see langword="true"/>) or mana (<see langword="false"/>)</param>
		public virtual bool PreHover(out bool hoveringLife) {
			hoveringLife = false;
			return false;
		}

		/// <summary>
		/// Draws a heart resource from the Classic display set
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="heartNumber">Which heart to draw, ranging from 1 to at most 20</param>
		/// <param name="position">The position to draw the heart at</param>
		/// <param name="sourceFrame">The area within the heart texture to draw.  Defaults to the entire texture</param>
		/// <param name="color">The color to draw the heart with.  Defaults to <see cref="Color.White"/></param>
		/// <param name="rotation">The rotation to draw the heart with.  Defaults to 0 radians</param>
		/// <param name="origin">The relative center within the frame for rotation and scaling.  Defaults to the center of the frame</param>
		/// <param name="scale">The scale to draw the heart at.  Defaults to <see cref="Vector2.One"/></param>
		/// <param name="pulse">Whether the heart's scale should be modified by the "pulse" effect.  Defaults to <see langword="false"/></param>
		/// <returns><see langword="null"/> if the parameters are invalid, otherwise the result of the drawing</returns>
		public static ResourceOverlayDrawContext? DrawClassicHeart(SpriteBatch spriteBatch, int heartNumber, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null, bool pulse = false) {
			var stats = PlayerStats;

			if (stats.AmountOfLifeHearts < heartNumber)
				return null;

			if (heartNumber < 1 || heartNumber > 20)
				return null;

			var asset = heartNumber <= stats.LifeFruitCount ? TextureAssets.Heart : TextureAssets.Heart2;

			scale ??= Vector2.One;

			if (pulse)
				scale += new Vector2(Main.cursorScale - 1f);

			return ResourceOverlayLoader.DrawResource(stats, heartNumber, asset, new ResourceDrawSource_ClassicLife(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}

		/// <summary>
		/// Draws a mana resource from the Classic display set
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="starNumber">Which star to draw, ranging from 1 to at most 20</param>
		/// <param name="position">The position to draw the star at</param>
		/// <param name="sourceFrame">The area within the star texture to draw.  Defaults to the entire texture</param>
		/// <param name="color">The color to draw the star with.  Defaults to <see cref="Color.White"/></param>
		/// <param name="rotation">The rotation to draw the star with.  Defaults to 0 radians</param>
		/// <param name="origin">The relative center within the frame for rotation and scaling.  Defaults to the center of the frame</param>
		/// <param name="scale">The scale to draw the star at.  Defaults to <see cref="Vector2.One"/></param>
		/// <param name="pulse">Whether the star's scale should be modified by the "pulse" effect.  Defaults to <see langword="false"/></param>
		/// <returns><see langword="null"/> if the parameters are invalid, otherwise the result of the drawing</returns>
		public static ResourceOverlayDrawContext? DrawClassicStar(SpriteBatch spriteBatch, int starNumber, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null, bool pulse = false) {
			var stats = PlayerStats;

			if (stats.AmountOfManaStars < starNumber)
				return null;

			if (starNumber < 1 || starNumber > 20)
				return null;

			scale ??= Vector2.One;

			if (pulse)
				scale += new Vector2(Main.cursorScale - 1f);

			return ResourceOverlayLoader.DrawResource(stats, starNumber, TextureAssets.Mana, new ResourceDrawSource_ClassicMana(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}

		/// <summary>
		/// Draws a heart resource from the Fancy display set
		/// </summary>
		/// <inheritdoc cref="DrawClassicHeart(SpriteBatch, int, Vector2, Rectangle?, Color?, float, Vector2?, Vector2?, bool)"/>
		public static ResourceOverlayDrawContext? DrawFancyHeart(SpriteBatch spriteBatch, int heartNumber, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null, bool pulse = false) {
			var stats = PlayerStats;

			if (stats.AmountOfLifeHearts < heartNumber)
				return null;

			if (heartNumber < 1 || heartNumber > 20)
				return null;

			string str = "Images/UI/PlayerResourceSets/FancyClassic/";

			if (heartNumber <= stats.LifeFruitCount)
				str += "HeartFill_B";
			else
				str += "HeartFill";

			var asset = Main.Assets.Request<Texture2D>(str, AssetRequestMode.ImmediateLoad);

			scale ??= Vector2.One;

			if (pulse)
				scale += new Vector2(Main.cursorScale - 1f);

			return ResourceOverlayLoader.DrawResource(stats, heartNumber, asset, new ResourceDrawSource_FancyLife(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}

		/// <summary>
		/// Draws a mana resource from the Fancy display set
		/// </summary>
		/// <inheritdoc cref="DrawClassicStar(SpriteBatch, int, Vector2, Rectangle?, Color?, float, Vector2?, Vector2?, bool)"/>
		public static ResourceOverlayDrawContext? DrawFancyStar(SpriteBatch spriteBatch, int starNumber, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null, bool pulse = false) {
			var stats = PlayerStats;

			if (stats.AmountOfManaStars < starNumber)
				return null;

			if (starNumber < 1 || starNumber > 20)
				return null;

			var asset = Main.Assets.Request<Texture2D>("Images/UI/PlayerResourceSets/FancyClassic/Star_Fill", AssetRequestMode.ImmediateLoad);

			scale ??= Vector2.One;

			if (pulse)
				scale += new Vector2(Main.cursorScale - 1f);

			return ResourceOverlayLoader.DrawResource(stats, starNumber, asset, new ResourceDrawSource_FancyMana(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}

		/// <summary>
		/// Draws a heart resource from the Bars display set
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="lifeBarNumber">Which bar to draw, ranging from 1 to at most 20</param>
		/// <param name="fillPercent">How much of the bar should be "filled"</param>
		/// <param name="position">The position to draw the bar at</param>
		/// <param name="mode">The mode for determining how the source area is affected by <paramref name="fillPercent"/>.  Defaults to <see cref="BarResourceFillMode.RightToLeft"/></param>
		/// <param name="color">The color to draw the bar with.  Defaults to <see cref="Color.White"/></param>
		/// <param name="rotation">The rotation to draw the bar with.  Defaults to 0 radians</param>
		/// <param name="origin">The relative center within the frame for rotation and scaling.  Defaults to the center of the frame</param>
		/// <param name="scale">The scale to draw the bar at.  Defaults to <see cref="Vector2.One"/></param>
		/// <returns><see langword="null"/> if the parameters are invalid, otherwise the result of the drawing</returns>
		public static ResourceOverlayDrawContext? DrawLifeBarFill(SpriteBatch spriteBatch, int lifeBarNumber, float fillPercent, Vector2 position, BarResourceFillMode mode = BarResourceFillMode.RightToLeft, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null) {
			var stats = PlayerStats;

			if (stats.AmountOfLifeHearts < lifeBarNumber)
				return null;

			if (lifeBarNumber < 1 || lifeBarNumber > 20)
				return null;

			string str = "Images/UI/PlayerResourceSets/HorizontalBars/";

			if (lifeBarNumber > stats.AmountOfLifeHearts - stats.LifeFruitCount)
				str += "HP_Fill_Honey";
			else
				str += "HP_Fill";

			var asset = Main.Assets.Request<Texture2D>(str, AssetRequestMode.ImmediateLoad);

			return ResourceOverlayLoader.DrawBarResource(stats, lifeBarNumber, fillPercent, asset, new ResourceDrawSource_BarsLife(), position, mode, spriteBatch, color, rotation, origin, scale);
		}

		/// <summary>
		/// Draws a mana resource from the Bars display set
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="starBarNumber">Which bar to draw, ranging from 1 to at most 20</param>
		/// <param name="fillPercent">How much of the bar should be "filled"</param>
		/// <param name="position">The position to draw the bar at</param>
		/// <param name="mode">The mode for determining how the source area is affected by <paramref name="fillPercent"/>.  Defaults to <see cref="BarResourceFillMode.RightToLeft"/></param>
		/// <param name="color">The color to draw the bar with.  Defaults to <see cref="Color.White"/></param>
		/// <param name="rotation">The rotation to draw the bar with.  Defaults to 0 radians</param>
		/// <param name="origin">The relative center within the frame for rotation and scaling.  Defaults to the center of the frame</param>
		/// <param name="scale">The scale to draw the bar at.  Defaults to <see cref="Vector2.One"/></param>
		/// <returns><see langword="null"/> if the parameters are invalid, otherwise the result of the drawing</returns>
		public static ResourceOverlayDrawContext? DrawManaBarFill(SpriteBatch spriteBatch, int starBarNumber, float fillPercent, Vector2 position, BarResourceFillMode mode = BarResourceFillMode.RightToLeft, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null) {
			var stats = PlayerStats;

			if (stats.AmountOfLifeHearts < starBarNumber)
				return null;

			if (starBarNumber < 1 || starBarNumber > 20)
				return null;

			var asset = Main.Assets.Request<Texture2D>("Images/UI/PlayerResourceSets/HorizontalBars/MP_Fill", AssetRequestMode.ImmediateLoad);

			return ResourceOverlayLoader.DrawBarResource(stats, starBarNumber, fillPercent, asset, new ResourceDrawSource_BarsMana(), position, mode, spriteBatch, color, rotation, origin, scale);
		}
	}
}
