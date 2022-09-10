using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Terraria.GameContent.UI.ResourceSets;

namespace Terraria.ModLoader
{
	public static class ResourceOverlayLoader
	{
		public static int OverlayCount => overlays.Count;

		internal static readonly IList<ModResourceOverlay> overlays = new List<ModResourceOverlay>();

		internal static int Add(ModResourceOverlay overlay) {
			overlays.Add(overlay);
			return OverlayCount - 1;
		}

		public static ModResourceOverlay GetOverlay(int type) {
			return type >= 0 && type < OverlayCount ? overlays[type] : null;
		}

		internal static void Unload() {
			overlays.Clear();
		}

		public static bool PreDrawResource(ref ResourceOverlayDrawContext context) {
			bool result = true;

			foreach (ModResourceOverlay overlay in overlays) {
				result &= overlay.PreDrawResource(ref context);
			}

			return result;
		}

		public static void PostDrawResource(ResourceOverlayDrawContext context) {
			foreach (ModResourceOverlay overlay in overlays) {
				overlay.PostDrawResource(context);
			}
		}

		/// <summary>
		/// Draws a resource, typically life or mana
		/// </summary>
		/// <param name="player">The player instanced used for the stat snapshot</param>
		/// <param name="resourceNumber">
		/// Which resource is being drawn<br/>
		/// <b>NOTE:</b> This value is expected to start at 1, not 0
		/// </param>
		/// <param name="texture">The texture</param>
		/// <param name="drawSource">The drawing context's source</param>
		/// <param name="position">The position to draw the resource at</param>
		/// <param name="spriteBatch">The SpriteBatch used to draw this resource.  Defaults to Main.spriteBatch</param>
		/// <param name="sourceFrame">The source rectangle within the texture to draw.  Defaults to the entire texture</param>
		/// <param name="color">The color to draw the resource with.  Defaults to Color.White</param>
		/// <param name="origin">The relative location within the source frame for rotation and scaling</param>
		/// <param name="scale">The scale to draw the resource at</param>
		/// <returns>The final drawing context</returns>
		public static ResourceOverlayDrawContext DrawResource(Player player, int resourceNumber, Asset<Texture2D> texture, IResourceDrawSource drawSource, Vector2 position, SpriteBatch spriteBatch = null, Rectangle? sourceFrame = null, Color? color = null, Vector2? origin = null, Vector2? scale = null) {
			return DrawResource(new PlayerStatsSnapshot(player), resourceNumber, texture, drawSource, position, spriteBatch, sourceFrame, color, origin, scale);
		}

		/// <inheritdoc cref="DrawResource(Player, int, Asset{Texture2D}, IResourceDrawSource, Vector2, SpriteBatch, Rectangle?, Color?, Vector2?, Vector2?)"/>
		/// <param name="snapshot">A snapshot of a player's health and mana stats</param>
		public static ResourceOverlayDrawContext DrawResource(PlayerStatsSnapshot snapshot, int resourceNumber, Asset<Texture2D> texture, IResourceDrawSource drawSource, Vector2 position, SpriteBatch spriteBatch = null, Rectangle? sourceFrame = null, Color? color = null, Vector2? origin = null, Vector2? scale = null) {
			Rectangle frame = sourceFrame ?? texture.Frame();

			ResourceOverlayDrawContext drawContext = new ResourceOverlayDrawContext(snapshot, resourceNumber, texture, drawSource) {
				position = position,
				source = sourceFrame,
				color = color ?? Color.White,
				origin = origin ?? frame.Size() / 2f,
				scale = scale ?? Vector2.One,
				SpriteBatch = spriteBatch ?? Main.spriteBatch
			};

			DrawResource(ref drawContext);

			return drawContext;
		}
		
		/// <summary>
		/// Draws a resource, typically life or mana, as if it were in the Bars display set
		/// </summary>
		/// <inheritdoc cref="PrepareBarResource(PlayerStatsSnapshot, int, Asset{Texture2D}, IResourceDrawSource, Vector2, Func{PlayerStatsSnapshot, int}, Func{PlayerStatsSnapshot, float}, SpriteBatch, Color?, Vector2?, Vector2?)"/>
		public static ResourceOverlayDrawContext DrawBarResource(PlayerStatsSnapshot snapshot, int resourceNumber, Asset<Texture2D> texture, IResourceDrawSource drawSource, Vector2 position, Func<PlayerStatsSnapshot, int> getSegmentCount, Func<PlayerStatsSnapshot, float> getFillPercent, SpriteBatch spriteBatch = null, Color? color = null, Vector2? origin = null, Vector2? scale = null) {
			ResourceOverlayDrawContext drawContext = PrepareBarResource(snapshot, resourceNumber, texture, drawSource, position, getSegmentCount, getFillPercent, spriteBatch, color, origin, scale);

			DrawResource(ref drawContext);

			return drawContext;
		}

		/// <summary>
		/// Prepares a resource, typically life or mana, for drawing as if it were in the Bars display set
		/// </summary>
		/// <param name="snapshot">A snapshot of a player's health and mana stats</param>
		/// <param name="resourceNumber">
		/// Which resource is being drawn<br/>
		/// <b>NOTE:</b> This value is expected to start at 1, not 0
		/// </param>
		/// <param name="texture">The texture</param>
		/// <param name="drawSource">The drawing context's source</param>
		/// <param name="position">
		/// The position to draw the resource at.<br/>
		/// <b>NOTE:</b> The final result in the returned object may not be the same as this value.
		/// </param>
		/// <param name="getSegmentCount">
		/// A function which returns the total segment count.<br/>
		/// You'll typically want to use <c>snapshot.AmountLifeOfHearts</c> or <c>snapshot.AmountOfManaStars</c>
		/// </param>
		/// <param name="getFillPercent">
		/// A function which returns the fill percentage.<br/>
		/// You'll typically want to use "<c>snapshot.Life/(float)snapshot.LifeMax</c>" or "<c>snapshot.Mana/(float)snapshot.ManaMax</c>"
		/// </param>
		/// <param name="spriteBatch">The SpriteBatch used to draw this resource.  Defaults to Main.spriteBatch</param>
		/// <param name="color">The color to draw the resource with.  Defaults to Color.White</param>
		/// <param name="origin">The relative location within the source frame for rotation and scaling</param>
		/// <param name="scale">The scale to draw the resource at</param>
		/// <returns>A drawing context object for use with <see cref="DrawResource(ref ResourceOverlayDrawContext)"/></returns>
		public static ResourceOverlayDrawContext PrepareBarResource(PlayerStatsSnapshot snapshot, int resourceNumber, Asset<Texture2D> texture, IResourceDrawSource drawSource, Vector2 position, Func<PlayerStatsSnapshot, int> getSegmentCount, Func<PlayerStatsSnapshot, float> getFillPercent, SpriteBatch spriteBatch = null, Color? color = null, Vector2? origin = null, Vector2? scale = null) {
			HorizontalBarsPlayerReosurcesDisplaySet.FillBarByValues(resourceNumber, texture, getSegmentCount(snapshot), getFillPercent(snapshot), out Vector2 offset, out float drawScale, out Rectangle? sourceRect);

			position += offset;

			scale ??= Vector2.One;
			scale *= drawScale;

			Rectangle frame = sourceRect ?? texture.Frame();

			return new ResourceOverlayDrawContext(snapshot, resourceNumber, texture, drawSource) {
				position = position,
				source = sourceRect,
				color = color ?? Color.White,
				origin = origin ?? frame.Size() / 2f,
				scale = scale ?? Vector2.One,
				SpriteBatch = spriteBatch ?? Main.spriteBatch
			};
		}

		/// <summary>
		/// Draws a life or mana resource
		/// </summary>
		/// <param name="drawContext">The drawing context</param>
		public static void DrawResource(ref ResourceOverlayDrawContext drawContext) {
			if (PreDrawResource(ref drawContext))
				drawContext.Draw();
			PostDrawResource(drawContext);
		}

		// TODO: DrawBarResource -- how the bars are drawn is different from the hearts/stars/panels

		public static bool PreDrawResourceDisplay(PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife, ref Color textColor, out bool drawText) {
			bool result = true;
			drawText = true;

			foreach (ModResourceOverlay overlay in overlays) {
				result &= overlay.PreDrawResourceDisplay(snapshot, displaySet, drawingLife, ref textColor, out bool draw);
				drawText &= draw;
			}

			return result;
		}

		public static void PostDrawResourceDisplay(PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife, Color textColor, bool drawText) {
			foreach (ModResourceOverlay overlay in overlays) {
				overlay.PostDrawResourceDisplay(snapshot, displaySet, drawingLife, textColor, drawText);
			}
		}

		public static bool DisplayHoverText(PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife) {
			bool result = true;

			foreach (ModResourceOverlay overlay in overlays) {
				result &= overlay.DisplayHoverText(snapshot, displaySet, drawingLife);
			}

			return result;
		}
	}
}
