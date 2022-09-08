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

		public static bool PreDrawClassicLifeHeart(ref ResourceOverlayDrawContext context) {
			bool result = true;

			foreach (ModResourceOverlay overlay in overlays) {
				result &= overlay.PreDrawClassicLifeHeart(ref context);
			}

			return result;
		}

		public static void PostDrawClassicLifeHeart(ResourceOverlayDrawContext context) {
			foreach (ModResourceOverlay overlay in overlays) {
				overlay.PostDrawClassicLifeHeart(context);
			}
		}

		public static void DrawClassicLifeHeart(PlayerStatsSnapshot snapshot, Asset<Texture2D> texture, int heartNumber, Vector2 position, int alpha, float scale) {
			ResourceOverlayDrawContext context = new ResourceOverlayDrawContext(snapshot, heartNumber) {
				texture = texture,
				position = position,
				color = new Color(alpha, alpha, alpha, (int)(alpha * 0.9f)),
				origin = texture.Value.Size() / 2f,
				scale = new Vector2(scale)
			};

			if (PreDrawClassicLifeHeart(ref context))
				context.Draw();
			PostDrawClassicLifeHeart(context);
		}

		public static bool PreDrawClassicManaStar(ref ResourceOverlayDrawContext context) {
			bool result = true;

			foreach (ModResourceOverlay overlay in overlays) {
				result &= overlay.PreDrawClassicManaStar(ref context);
			}

			return result;
		}

		public static void PostDrawClassicManaStar(ResourceOverlayDrawContext context) {
			foreach (ModResourceOverlay overlay in overlays) {
				overlay.PostDrawClassicManaStar(context);
			}
		}

		public static void DrawClassicManaStar(PlayerStatsSnapshot snapshot, Asset<Texture2D> texture, int starNumber, Vector2 position, int alpha, float scale) {
			ResourceOverlayDrawContext context = new ResourceOverlayDrawContext(snapshot, starNumber) {
				texture = texture,
				position = position,
				color = new Color(alpha, alpha, alpha, (int)(alpha * 0.9f)),
				origin = texture.Value.Size() / 2f,
				scale = new Vector2(scale)
			};

			if (PreDrawClassicManaStar(ref context))
				context.Draw();
			PostDrawClassicManaStar(context);
		}

		public static bool PreDrawClassicLifeDisplay(PlayerStatsSnapshot snapshot, ref Color lifeTextColor, out bool drawText) {
			bool result = true;
			drawText = true;

			foreach (ModResourceOverlay overlay in overlays) {
				result &= overlay.PreDrawClassicLifeDisplay(snapshot, ref lifeTextColor, out bool draw);
				drawText &= draw;
			}

			return result;
		}

		public static void PostDrawClassicLifeDisplay(PlayerStatsSnapshot snapshot, Color lifeTextColor, bool drawText) {
			foreach (ModResourceOverlay overlay in overlays) {
				overlay.PostDrawClassicLifeDisplay(snapshot, lifeTextColor, drawText);
			}
		}

		public static bool PreDrawClassicManaDisplay(PlayerStatsSnapshot snapshot, ref Color manaTextColor, out bool drawText) {
			bool result = true;
			drawText = true;

			foreach (ModResourceOverlay overlay in overlays) {
				result &= overlay.PreDrawClassicManaDisplay(snapshot, ref manaTextColor, out bool draw);
				drawText &= draw;
			}

			return result;
		}

		public static void PostDrawClassicManaDisplay(PlayerStatsSnapshot snapshot, Color manaTextColor, bool drawText) {
			foreach (ModResourceOverlay overlay in overlays) {
				overlay.PostDrawClassicManaDisplay(snapshot, manaTextColor, drawText);
			}
		}
	}
}
