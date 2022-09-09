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
		/// Draws a life or mana resource
		/// </summary>
		/// <param name="snapshot">The snapshot of a player's health and mana stats</param>
		/// <param name="texture">The default texture to use</param>
		/// <param name="resourceNumber">Which resource number is being drawn.  For hearts/stars/bars, this ranges from 1 to 20.  For bar panels, this ranges from 1 to 22.</param>
		/// <param name="position">The default position of the resource</param>
		/// <param name="alpha">The transparency for the resource</param>
		/// <param name="scale">The scale to draw the resource at</param>
		/// <param name="drawSource">The drawing source</param>
		public static void DrawResource(PlayerStatsSnapshot snapshot, Asset<Texture2D> texture, int resourceNumber, Vector2 position, int alpha, Vector2 scale, IResourceDrawSource drawSource) {
			ResourceOverlayDrawContext drawContext = new ResourceOverlayDrawContext(snapshot, resourceNumber, texture, drawSource) {
				position = position,
				color = new Color(alpha, alpha, alpha, (int)(alpha * 0.9f)),
				origin = texture.Value.Size() / 2f,
				scale = scale
			};

			if (PreDrawResource(ref drawContext))
				drawContext.Draw();
			PostDrawResource(drawContext);
		}

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
	}
}
