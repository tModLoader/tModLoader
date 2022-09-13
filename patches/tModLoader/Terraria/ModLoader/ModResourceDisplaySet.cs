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

		/// <summary>
		/// An invalid snapshot instance copied for use in the various helper methods in this class
		/// </summary>
		public static readonly PlayerStatsSnapshot MaxedSnapshot = new PlayerStatsSnapshot() {
			AmountOfLifeHearts = 20,
			AmountOfManaStars = 20,
			Life = 400,
			LifeMax = 400,
			Mana = 200,
			ManaMax = 200,
			LifeFruitCount = 20
		};

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
		/// Creates a drawing context for drawing a life resource from the Classic display set
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="heartNumber">Which life to draw, ranging from 1 to at most 20</param>
		/// <param name="position">The position to draw the life at</param>
		/// <param name="sourceFrame">The area within the life texture to draw.  Defaults to the entire texture</param>
		/// <param name="color">The color to draw the life with.  Defaults to <see cref="Color.White"/></param>
		/// <param name="rotation">The rotation to draw the life with.  Defaults to 0 radians</param>
		/// <param name="origin">The relative center within the frame for rotation and scaling.  Defaults to the center of the frame</param>
		/// <param name="scale">The scale to draw the life at.  Defaults to <see cref="Vector2.One"/></param>
		/// <param name="pulse">Whether the life's scale should be modified by the "pulse" effect.  Defaults to <see langword="false"/></param>
		/// <returns><see langword="null"/> if the parameters are invalid, otherwise the created context</returns>
		public static ResourceOverlayDrawContext? GetClassicHeart(SpriteBatch spriteBatch, int heartNumber, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null, bool pulse = false) {
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
		/// Creates a drawing context for drawing a mana resource from the Classic display set
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
		/// <returns><see langword="null"/> if the parameters are invalid, otherwise the created context</returns>
		public static ResourceOverlayDrawContext? GetClassicStar(SpriteBatch spriteBatch, int starNumber, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null, bool pulse = false) {
			var stats = PlayerStats;

			if (stats.AmountOfManaStars < starNumber)
				return null;

			if (starNumber < 1 || starNumber > 20)
				return null;

			scale ??= Vector2.One;

			if (pulse)
				scale += new Vector2(Main.cursorScale - 1f);

			return ResourceOverlayLoader.PrepareResource(stats, starNumber, TextureAssets.Mana, new ResourceDrawSource_ClassicMana(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}

		/// <summary>
		/// Creates a drawing context for drawing a life resource from the Fancy display set
		/// </summary>
		/// <inheritdoc cref="GetClassicHeart(SpriteBatch, int, Vector2, Rectangle?, Color?, float, Vector2?, Vector2?, bool)"/>
		public static ResourceOverlayDrawContext? GetFancyHeart(SpriteBatch spriteBatch, int heartNumber, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null, bool pulse = false) {
			var stats = PlayerStats;

			if (stats.AmountOfLifeHearts < heartNumber)
				return null;

			if (heartNumber < 1 || heartNumber > 20)
				return null;

			string str = "Images/UI/PlayerResourceSets/FancyClassic/HeartFill";

			if (heartNumber <= stats.LifeFruitCount)
				str += "_B";

			var asset = Main.Assets.Request<Texture2D>(str, AssetRequestMode.ImmediateLoad);

			scale ??= Vector2.One;

			if (pulse)
				scale += new Vector2(Main.cursorScale - 1f);

			return ResourceOverlayLoader.PrepareResource(stats, heartNumber, asset, new ResourceDrawSource_FancyLife(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}

		/// <summary>
		/// Creates a drawing context for drawing a life panel resource from the Fancy display set
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="heartNumber">Which panel to draw, ranging from 1 to at most 20</param>
		/// <param name="position">The position to draw the panel at</param>
		/// <param name="sourceFrame">The area within the panel texture to draw.  Defaults to the entire texture</param>
		/// <param name="color">The color to draw the panel with.  Defaults to <see cref="Color.White"/></param>
		/// <param name="rotation">The rotation to draw the panel with.  Defaults to 0 radians</param>
		/// <param name="origin">The relative center within the frame for rotation and scaling.  Defaults to the center of the frame</param>
		/// <param name="scale">The scale to draw the panel at.  Defaults to <see cref="Vector2.One"/></param>
		/// <returns><see langword="null"/> if the parameters are invalid, otherwise the created context</returns>
		public static ResourceOverlayDrawContext? GetFancyLifePanel(SpriteBatch spriteBatch, int heartNumber, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null) {
			var stats = PlayerStats;

			if (stats.AmountOfLifeHearts < heartNumber)
				return null;

			if (heartNumber < 1 || heartNumber > 20)
				return null;

			string texture = "Images/UI/PlayerResourceSets/FancyClassic/Heart_";

			if (heartNumber == stats.AmountOfLifeHearts) {
				// Final panel to draw has a special "Fancy" variant.  Determine whether it has panels to the left of it
				if (heartNumber % 10 == 1) {
					// First and only panel in this panel's row
					texture += "Single_Fancy";
				} else {
					// Other panels existed in this panel's row
					texture += "Right_Fancy";
				}
			} else if (heartNumber % 10 == 1) {
				// First panel in this row
				texture += "Left";
			} else if (heartNumber % 10 <= 9) {
				// Any panel that has a panel to its left AND right
				texture += "Middle";
			} else {
				// Final panel in the first row
				texture += "Right";
			}

			var asset = Main.Assets.Request<Texture2D>(texture, AssetRequestMode.ImmediateLoad);

			return ResourceOverlayLoader.PrepareResource(stats, heartNumber, asset, new ResourceDrawSource_FancyLifePanel(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}

		/// <returns></returns>
		/// <inheritdoc cref="GetFancyLifePanel(SpriteBatch, int, Vector2, Rectangle?, Color?, float, Vector2?, Vector2?)"/>
		public static ResourceOverlayDrawContext GetFancyLifePanelLeft(SpriteBatch spriteBatch, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null) {
			var asset = Main.Assets.Request<Texture2D>("Images/UI/PlayerResourceSets/FancyClassic/Heart_Left", AssetRequestMode.ImmediateLoad);

			return ResourceOverlayLoader.PrepareResource(MaxedSnapshot, 1, asset, new ResourceDrawSource_FancyLifePanel(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}

		/// <summary>
		/// Creates a drawing context for drawing a life panel resource from the Fancy display set
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="heartNumber">Which panel to draw, ranging from 2 to at most 19, excluding 10 and 11</param>
		/// <param name="position">The position to draw the panel at</param>
		/// <param name="sourceFrame">The area within the panel texture to draw.  Defaults to the entire texture</param>
		/// <param name="color">The color to draw the panel with.  Defaults to <see cref="Color.White"/></param>
		/// <param name="rotation">The rotation to draw the panel with.  Defaults to 0 radians</param>
		/// <param name="origin">The relative center within the frame for rotation and scaling.  Defaults to the center of the frame</param>
		/// <param name="scale">The scale to draw the panel at.  Defaults to <see cref="Vector2.One"/></param>
		/// <returns><see langword="null"/> if the parameters are invalid, otherwise the created context</returns>
		public static ResourceOverlayDrawContext GetFancyLifePanelMiddle(SpriteBatch spriteBatch, int heartNumber, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null) {
			var asset = Main.Assets.Request<Texture2D>("Images/UI/PlayerResourceSets/FancyClassic/Heart_Middle", AssetRequestMode.ImmediateLoad);

			// Ensure that the context actually gets a middle life panel
			heartNumber = Utils.Clamp(heartNumber, 2, 19);
			if (heartNumber == 10)
				heartNumber = 9;
			else if (heartNumber == 11)
				heartNumber = 12;

			return ResourceOverlayLoader.PrepareResource(MaxedSnapshot, heartNumber, asset, new ResourceDrawSource_FancyLifePanel(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}

		/// <inheritdoc cref="GetFancyLifePanelLeft(SpriteBatch, Vector2, Rectangle?, Color?, float, Vector2?, Vector2?)"/>
		public static ResourceOverlayDrawContext GetFancyLifePanelRight(SpriteBatch spriteBatch, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null) {
			var asset = Main.Assets.Request<Texture2D>("Images/UI/PlayerResourceSets/FancyClassic/Heart_Right", AssetRequestMode.ImmediateLoad);

			return ResourceOverlayLoader.PrepareResource(MaxedSnapshot, 10, asset, new ResourceDrawSource_FancyLifePanel(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}

		/// <inheritdoc cref="GetFancyLifePanelLeft(SpriteBatch, Vector2, Rectangle?, Color?, float, Vector2?, Vector2?)"/>
		public static ResourceOverlayDrawContext GetFancyLifePanelSingle(SpriteBatch spriteBatch, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null) {
			var asset = Main.Assets.Request<Texture2D>("Images/UI/PlayerResourceSets/FancyClassic/Heart_Single_Fancy", AssetRequestMode.ImmediateLoad);

			return ResourceOverlayLoader.PrepareResource(MaxedSnapshot with { AmountOfLifeHearts = 1 }, 1, asset, new ResourceDrawSource_FancyLifePanel(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}

		/// <inheritdoc cref="GetFancyLifePanelLeft(SpriteBatch, Vector2, Rectangle?, Color?, float, Vector2?, Vector2?)"/>
		public static ResourceOverlayDrawContext GetFancyLifePanelRightFancy(SpriteBatch spriteBatch, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null) {
			var asset = Main.Assets.Request<Texture2D>("Images/UI/PlayerResourceSets/FancyClassic/Heart_Right_Fancy", AssetRequestMode.ImmediateLoad);

			return ResourceOverlayLoader.PrepareResource(MaxedSnapshot, 20, asset, new ResourceDrawSource_FancyLifePanel(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}

		/// <summary>
		/// Creates a drawing context for drawing a mana resource from the Fancy display set
		/// </summary>
		/// <inheritdoc cref="GetClassicStar(SpriteBatch, int, Vector2, Rectangle?, Color?, float, Vector2?, Vector2?, bool)"/>
		public static ResourceOverlayDrawContext? GetFancyStar(SpriteBatch spriteBatch, int starNumber, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null, bool pulse = false) {
			var stats = PlayerStats;

			if (stats.AmountOfManaStars < starNumber)
				return null;

			if (starNumber < 1 || starNumber > 20)
				return null;

			var asset = Main.Assets.Request<Texture2D>("Images/UI/PlayerResourceSets/FancyClassic/Star_Fill", AssetRequestMode.ImmediateLoad);

			scale ??= Vector2.One;

			if (pulse)
				scale += new Vector2(Main.cursorScale - 1f);

			return ResourceOverlayLoader.PrepareResource(stats, starNumber, asset, new ResourceDrawSource_FancyMana(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}

		/// <summary>
		/// Creates a drawing context for drawing a mana panel resource from the Fancy display set
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="starNumber">Which panel to draw, ranging from 1 to at most 20</param>
		/// <param name="position">The position to draw the panel at</param>
		/// <param name="sourceFrame">The area within the panel texture to draw.  Defaults to the entire texture</param>
		/// <param name="color">The color to draw the panel with.  Defaults to <see cref="Color.White"/></param>
		/// <param name="rotation">The rotation to draw the panel with.  Defaults to 0 radians</param>
		/// <param name="origin">The relative center within the frame for rotation and scaling.  Defaults to the center of the frame</param>
		/// <param name="scale">The scale to draw the panel at.  Defaults to <see cref="Vector2.One"/></param>
		/// <returns><see langword="null"/> if the parameters are invalid, otherwise the created context</returns>
		public static ResourceOverlayDrawContext? GetFancyManaPanel(SpriteBatch spriteBatch, int starNumber, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null) {
			var stats = PlayerStats;

			if (stats.AmountOfManaStars < starNumber)
				return null;

			if (starNumber < 1 || starNumber > 20)
				return null;

			string texture = "Images/UI/PlayerResourceSets/FancyClassic/Star_";

			if (starNumber == stats.AmountOfManaStars) {
				//Final panel in the column.  Determine whether it has panels above it
				if (starNumber == 1) {
					// First and only panel
					texture += "Single";
				} else {
					// Other panels existed above this panel
					texture += "C";
				}
			} else if (starNumber == 1) {
				// First panel in the column
				texture += "A";
			} else {
				// Any panel that has a panel above AND below it
				texture += "B";
			}

			var asset = Main.Assets.Request<Texture2D>(texture, AssetRequestMode.ImmediateLoad);

			return ResourceOverlayLoader.PrepareResource(stats, starNumber, asset, new ResourceDrawSource_FancyManaPanel(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}

		/// <returns></returns>
		/// <inheritdoc cref="GetFancyManaPanel(SpriteBatch, int, Vector2, Rectangle?, Color?, float, Vector2?, Vector2?)"/>
		public static ResourceOverlayDrawContext GetFancyManaPanelTop(SpriteBatch spriteBatch, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null) {
			var asset = Main.Assets.Request<Texture2D>("Images/UI/PlayerResourceSets/FancyClassic/Star_A", AssetRequestMode.ImmediateLoad);

			return ResourceOverlayLoader.PrepareResource(MaxedSnapshot, 1, asset, new ResourceDrawSource_FancyManaPanel(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}

		/// <summary>
		/// Creates a drawing context for drawing a mana panel resource from the Fancy display set
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="starNumber">Which star to draw, ranging from 2 to at most 19</param>
		/// <param name="position">The position to draw the star at</param>
		/// <param name="sourceFrame">The area within the star texture to draw.  Defaults to the entire texture</param>
		/// <param name="color">The color to draw the star with.  Defaults to <see cref="Color.White"/></param>
		/// <param name="rotation">The rotation to draw the star with.  Defaults to 0 radians</param>
		/// <param name="origin">The relative center within the frame for rotation and scaling.  Defaults to the center of the frame</param>
		/// <param name="scale">The scale to draw the star at.  Defaults to <see cref="Vector2.One"/></param>
		public static ResourceOverlayDrawContext GetFancyManaPanelMiddle(SpriteBatch spriteBatch, int starNumber, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null) {
			var asset = Main.Assets.Request<Texture2D>("Images/UI/PlayerResourceSets/FancyClassic/Star_B", AssetRequestMode.ImmediateLoad);

			// Ensure that the context actually gets a middle star panel
			starNumber = Utils.Clamp(starNumber, 2, 19);

			return ResourceOverlayLoader.PrepareResource(MaxedSnapshot, starNumber, asset, new ResourceDrawSource_FancyManaPanel(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}

		/// <inheritdoc cref="GetFancyManaPanelTop(SpriteBatch, Vector2, Rectangle?, Color?, float, Vector2?, Vector2?)"/>
		public static ResourceOverlayDrawContext GetFancyManaPanelBottom(SpriteBatch spriteBatch, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null) {
			var asset = Main.Assets.Request<Texture2D>("Images/UI/PlayerResourceSets/FancyClassic/Star_C", AssetRequestMode.ImmediateLoad);

			return ResourceOverlayLoader.PrepareResource(MaxedSnapshot, 20, asset, new ResourceDrawSource_FancyManaPanel(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}

		/// <inheritdoc cref="GetFancyManaPanelTop(SpriteBatch, Vector2, Rectangle?, Color?, float, Vector2?, Vector2?)"/>
		public static ResourceOverlayDrawContext GetFancyManaPanelSingle(SpriteBatch spriteBatch, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null) {
			var asset = Main.Assets.Request<Texture2D>("Images/UI/PlayerResourceSets/FancyClassic/Star_Single", AssetRequestMode.ImmediateLoad);

			return ResourceOverlayLoader.PrepareResource(MaxedSnapshot with { AmountOfManaStars = 1 }, 1, asset, new ResourceDrawSource_FancyManaPanel(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}

		/// <summary>
		/// Creates a drawing context for drawing a life resource from the Bars display set
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
		/// <returns><see langword="null"/> if the parameters are invalid, otherwise the created context</returns>
		public static ResourceOverlayDrawContext? GetLifeBarFill(SpriteBatch spriteBatch, int lifeBarNumber, float fillPercent, Vector2 position, BarResourceFillMode mode = BarResourceFillMode.RightToLeft, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null) {
			var stats = PlayerStats;

			if (stats.AmountOfLifeHearts < lifeBarNumber)
				return null;

			if (lifeBarNumber < 1 || lifeBarNumber > 20)
				return null;

			string str = "Images/UI/PlayerResourceSets/HorizontalBars/HP_Fill";

			if (lifeBarNumber > stats.AmountOfLifeHearts - stats.LifeFruitCount)
				str += "_Honey";

			var asset = Main.Assets.Request<Texture2D>(str, AssetRequestMode.ImmediateLoad);

			return ResourceOverlayLoader.PrepareBarResource(stats, lifeBarNumber, fillPercent, asset, new ResourceDrawSource_BarsLife(), position, mode, spriteBatch, color, rotation, origin, scale);
		}

		/// <summary>
		/// Creates a drawing context for drawing a life panel resource from the Bars display set
		/// </summary>
		/// <inheritdoc cref="GetFancyLifePanelLeft(SpriteBatch, Vector2, Rectangle?, Color?, float, Vector2?, Vector2?)"/>
		public static ResourceOverlayDrawContext GetLifeBarPanelLeft(SpriteBatch spriteBatch, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null) {
			var asset = Main.Assets.Request<Texture2D>("Images/UI/PlayerResourceSets/HorizontalBars/Panel_Left", AssetRequestMode.ImmediateLoad);

			return ResourceOverlayLoader.PrepareResource(MaxedSnapshot, 1, asset, new ResourceDrawSource_BarsLifePanel(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}

		/// <summary>
		/// Creates a drawing context for drawing a life panel resource from the Bars display set
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="heartNumber">Which panel to draw, ranging from 2 to at most 19</param>
		/// <param name="position">The position to draw the panel at</param>
		/// <param name="sourceFrame">The area within the panel texture to draw.  Defaults to the entire texture</param>
		/// <param name="color">The color to draw the panel with.  Defaults to <see cref="Color.White"/></param>
		/// <param name="rotation">The rotation to draw the panel with.  Defaults to 0 radians</param>
		/// <param name="origin">The relative center within the frame for rotation and scaling.  Defaults to the center of the frame</param>
		/// <param name="scale">The scale to draw the panel at.  Defaults to <see cref="Vector2.One"/></param>
		/// <returns><see langword="null"/> if the parameters are invalid, otherwise the created context</returns>
		public static ResourceOverlayDrawContext GetLifeBarPanelMiddle(SpriteBatch spriteBatch, int heartNumber, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null) {
			var asset = Main.Assets.Request<Texture2D>("Images/UI/PlayerResourceSets/HorizontalBars/HP_Panel_Middle", AssetRequestMode.ImmediateLoad);

			heartNumber = Utils.Clamp(heartNumber, 2, 19);

			return ResourceOverlayLoader.PrepareResource(MaxedSnapshot, heartNumber + 1, asset, new ResourceDrawSource_BarsLifePanel(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}

		/// <summary>
		/// Creates a drawing context for drawing a life panel resource from the Bars display set
		/// </summary>
		/// <inheritdoc cref="GetFancyLifePanelLeft(SpriteBatch, Vector2, Rectangle?, Color?, float, Vector2?, Vector2?)"/>
		public static ResourceOverlayDrawContext GetLifeBarPanelRight(SpriteBatch spriteBatch, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null) {
			var asset = Main.Assets.Request<Texture2D>("Images/UI/PlayerResourceSets/HorizontalBars/HP_Panel_Right", AssetRequestMode.ImmediateLoad);

			return ResourceOverlayLoader.PrepareResource(MaxedSnapshot, 22, asset, new ResourceDrawSource_BarsLifePanel(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}

		/// <summary>
		/// Creates a drawing context for drawing a mana resource from the Bars display set
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
		/// <returns><see langword="null"/> if the parameters are invalid, otherwise the created context</returns>
		public static ResourceOverlayDrawContext? GetManaBarFill(SpriteBatch spriteBatch, int starBarNumber, float fillPercent, Vector2 position, BarResourceFillMode mode = BarResourceFillMode.RightToLeft, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null) {
			var stats = PlayerStats;

			if (stats.AmountOfLifeHearts < starBarNumber)
				return null;

			if (starBarNumber < 1 || starBarNumber > 20)
				return null;

			var asset = Main.Assets.Request<Texture2D>("Images/UI/PlayerResourceSets/HorizontalBars/MP_Fill", AssetRequestMode.ImmediateLoad);

			return ResourceOverlayLoader.PrepareBarResource(stats, starBarNumber, fillPercent, asset, new ResourceDrawSource_BarsMana(), position, mode, spriteBatch, color, rotation, origin, scale);
		}

		/// <summary>
		/// Creates a drawing context for drawing a mana panel resource from the Bars display set
		/// </summary>
		/// <inheritdoc cref="GetFancyManaPanelTop(SpriteBatch, Vector2, Rectangle?, Color?, float, Vector2?, Vector2?)"/>
		public static ResourceOverlayDrawContext GetManaBarPanelLeft(SpriteBatch spriteBatch, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null) {
			var asset = Main.Assets.Request<Texture2D>("Images/UI/PlayerResourceSets/HorizontalBars/Panel_Left", AssetRequestMode.ImmediateLoad);

			return ResourceOverlayLoader.PrepareResource(MaxedSnapshot, 1, asset, new ResourceDrawSource_BarsManaPanel(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}

		/// <summary>
		/// Creates a drawing context for drawing a mana panel resource from the Bars display set
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="heartNumber">Which panel to draw, ranging from 2 to at most 19</param>
		/// <param name="position">The position to draw the panel at</param>
		/// <param name="sourceFrame">The area within the panel texture to draw.  Defaults to the entire texture</param>
		/// <param name="color">The color to draw the panel with.  Defaults to <see cref="Color.White"/></param>
		/// <param name="rotation">The rotation to draw the panel with.  Defaults to 0 radians</param>
		/// <param name="origin">The relative center within the frame for rotation and scaling.  Defaults to the center of the frame</param>
		/// <param name="scale">The scale to draw the panel at.  Defaults to <see cref="Vector2.One"/></param>
		/// <returns><see langword="null"/> if the parameters are invalid, otherwise the created context</returns>
		public static ResourceOverlayDrawContext GetManaBarPanelMiddle(SpriteBatch spriteBatch, int heartNumber, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null) {
			var asset = Main.Assets.Request<Texture2D>("Images/UI/PlayerResourceSets/HorizontalBars/HP_Panel_Middle", AssetRequestMode.ImmediateLoad);

			heartNumber = Utils.Clamp(heartNumber, 2, 19);

			return ResourceOverlayLoader.PrepareResource(MaxedSnapshot, heartNumber + 1, asset, new ResourceDrawSource_BarsManaPanel(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}

		/// <summary>
		/// Creates a drawing context for drawing a mana panel resource from the Bars display set
		/// </summary>
		/// <inheritdoc cref="GetFancyManaPanelTop(SpriteBatch, Vector2, Rectangle?, Color?, float, Vector2?, Vector2?)"/>
		public static ResourceOverlayDrawContext GetManaBarPanelRight(SpriteBatch spriteBatch, Vector2 position, Rectangle? sourceFrame = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null) {
			var asset = Main.Assets.Request<Texture2D>("Images/UI/PlayerResourceSets/HorizontalBars/HP_Panel_Right", AssetRequestMode.ImmediateLoad);

			return ResourceOverlayLoader.PrepareResource(MaxedSnapshot, 22, asset, new ResourceDrawSource_BarsManaPanel(), position, spriteBatch, sourceFrame, color, rotation, origin, scale);
		}
	}
}
