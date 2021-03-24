using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ModLoader.Engine;

namespace Terraria
{
	public partial class Main
	{
		public static int soundError;
		public static int ambientError;
		public static bool mouseMiddle;
		public static bool mouseXButton1;
		public static bool mouseXButton2;
		public static bool mouseMiddleRelease;
		public static bool mouseXButton1Release;
		public static bool mouseXButton2Release;
		public static Point16 trashSlotOffset;
		public static bool hidePlayerCraftingMenu;
		public static bool showServerConsole;
		public static bool Support8K = true; // provides an option to disable 8k (but leave 4k)

		internal static TMLContentManager AlternateContentManager;

		public static Color DiscoColor => new Color(DiscoR, DiscoG, DiscoB);
		public static Color MouseTextColorReal => new Color(mouseTextColor / 255f, mouseTextColor / 255f, mouseTextColor / 255f, mouseTextColor / 255f);
		public static bool PlayerLoaded => CurrentFrameFlags.ActivePlayersCount > 0;

		internal static Dictionary<int, ModInvasion.ModInvasionProgressDisplay> invasionDisplays = new Dictionary<int, ModInvasion.ModInvasionProgressDisplay>();

		public static void DrawModdedInvasionUI() {
			int i = 0;

			foreach (ModInvasion modInvasion in ModEventLoader.ModInvasions) {
				if (!invasionDisplays.TryGetValue(modInvasion.Type, out ModInvasion.ModInvasionProgressDisplay progressDisplay))
					continue;

				if (!modInvasion.Active && (progressDisplay == null || progressDisplay.Alpha == 0))
					continue;

				if (!gamePaused && !modInvasion.Active)
					progressDisplay.DisplayLeft--;

				if (progressDisplay.DisplayLeft > 0)
					progressDisplay.Alpha += 0.05f;
				else
					progressDisplay.Alpha -= 0.05f;

				if (progressDisplay.Alpha <= 0f)
					continue;

				if (progressDisplay.Alpha >= 1f)
					progressDisplay.Alpha = 1f;

				Texture2D icon = modInvasion.Icon;

				string title = modInvasion.Title;
				float alpha = progressDisplay.Alpha;
				float scale = 0.5f + alpha * 0.5f;

				int bottomPanelWidth = (int)(200f * scale);
				int bottomPanelHeight = (int)(45f * scale);

				Vector2 uiOffset = (bottomPanelHeight + 50) * i * Vector2.UnitY;

				if (invasionType != 0 && invasionProgressAlpha > 0f)
					uiOffset.Y += bottomPanelHeight + 50;

				Vector2 bottomUICenter = new Vector2(screenWidth - 120, screenHeight - 40) - uiOffset;
				Color bottomUIColor = new Color(63, 65, 151, 255) * 0.785f;
				Rectangle bottomUIRect = new Rectangle((int)bottomUICenter.X - bottomPanelWidth / 2, (int)bottomUICenter.Y - bottomPanelHeight / 2, bottomPanelWidth, bottomPanelHeight);

				Vector2 progressBarOutlineCenter = bottomUICenter;

				string progressText = modInvasion.ProgressText;
				float progress = modInvasion.Progress;
				float progressBarWidth = 169f * scale;
				float progressBarHeight = 8f * scale;
				Vector2 progressBarPos = bottomUICenter + Vector2.UnitY * progressBarHeight + Vector2.UnitX * 1f;

				if (modInvasion.PreDrawBottomPanel(spriteBatch, ref bottomUIRect, ref bottomUIColor))
					Utils.DrawInvBG(spriteBatch, bottomUIRect, bottomUIColor);

				modInvasion.PostDrawBottomPanel(spriteBatch, bottomUIRect, bottomUIColor);

				float progressTextScale = 1f;
				float progressTextAnchorX = 0.5f;
				float progressTextAnchorY = 1f;
				Color progressTextColor = Color.White * alpha;
				Vector2 progressTextPos = progressBarPos;

				if (modInvasion.PreDrawProgressText(spriteBatch, ref progressTextPos, ref progressTextColor, ref progressTextScale, ref progressTextAnchorX, ref progressTextAnchorY))
					Utils.DrawBorderString(spriteBatch, progressText, progressBarPos, progressTextColor, scale * progressTextScale, progressTextAnchorX, progressTextAnchorY);

				modInvasion.PostDrawProgressText(spriteBatch, progressBarPos, progressTextColor, scale * progressTextScale, progressTextAnchorX, progressTextAnchorY);

				Texture2D progressBarOutline = TextureAssets.ColorBar.Value;
				Vector2 outlineOrigin = new Vector2(progressBarOutline.Width / 2, 0f);
				Color outlineColor = Color.White * alpha;
				float outlineRotation = 0f;
				float outlineScale = 1f;

				if (modInvasion.PreDrawProgressBarOutline(spriteBatch, ref progressBarOutlineCenter, ref outlineColor, ref outlineRotation, ref outlineOrigin, ref outlineScale))
					spriteBatch.Draw(progressBarOutline, bottomUICenter, null, outlineColor, outlineRotation, outlineOrigin, scale * outlineScale, SpriteEffects.None, 0f);

				modInvasion.PostDrawProgressBarOutline(spriteBatch, progressBarOutlineCenter, outlineColor, outlineRotation, outlineOrigin, outlineScale);

				progressBarPos += Vector2.UnitX * (progress - 0.5f) * progressBarWidth;

				Texture2D magicPixel = TextureAssets.MagicPixel.Value;
				Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
				Color progressBarColor = new Color(255, 241, 51);

				if (modInvasion.PreDrawProgressBar(spriteBatch, ref progressBarPos, ref progressBarWidth, ref progressBarHeight, ref progressBarColor)) {
					spriteBatch.Draw(magicPixel, progressBarPos, sourceRect, progressBarColor * alpha, 0f, new Vector2(1f, 0.5f), new Vector2(progressBarWidth * progress, progressBarHeight), SpriteEffects.None, 0f);
					spriteBatch.Draw(magicPixel, progressBarPos, sourceRect, new Color(255, 165, 0, 127) * alpha, 0f, new Vector2(1f, 0.5f), new Vector2(2f, progressBarHeight), SpriteEffects.None, 0f);
					spriteBatch.Draw(magicPixel, progressBarPos, sourceRect, Color.Black * alpha, 0f, new Vector2(0f, 0.5f), new Vector2(progressBarWidth * (1f - progress), progressBarHeight), SpriteEffects.None, 0f);
				}

				Vector2 invasionTitleSize = FontAssets.MouseText.Value.MeasureString(title);
				float titleUIWidth = 120f;

				if (invasionTitleSize.X > 200f)
					titleUIWidth += invasionTitleSize.X - 200f;

				Rectangle topPanelRect = Utils.CenteredRectangle(new Vector2(screenWidth - titleUIWidth, screenHeight - 80) - uiOffset, (invasionTitleSize + new Vector2(icon.Width + 12, 6f)) * scale);
				Color topPanelColor = new Color(63, 65, 151, 255) * 0.785f;

				if (modInvasion.PreDrawTopPanel(spriteBatch, ref topPanelRect, ref topPanelColor))
					Utils.DrawInvBG(spriteBatch, topPanelRect, topPanelColor);

				modInvasion.PostDrawTopPanel(spriteBatch, topPanelRect, topPanelColor);

				Vector2 iconPos = topPanelRect.Left() + Vector2.UnitX * scale * 8f;
				Vector2 iconOrigin = new Vector2(0f, icon.Height / 2);
				Color iconColor = Color.White * alpha;
				float iconRotation = 0f;
				float iconScale = 1f;
				SpriteEffects iconEffects = SpriteEffects.None;

				if (modInvasion.PreDrawIcon(spriteBatch, ref iconPos, ref iconColor, ref iconRotation, ref iconOrigin, ref iconScale, ref iconEffects))
					spriteBatch.Draw(icon, iconPos, null, iconColor, iconRotation, iconOrigin, scale * 0.8f * iconScale, iconEffects, 0f);

				modInvasion.PostDrawIcon(spriteBatch, iconPos, iconColor, iconRotation, iconOrigin, iconScale, iconEffects);

				Vector2 titlePos = topPanelRect.Right() + Vector2.UnitX * scale * -22f;
				Color titleColor = Color.White * alpha;
				float titleScale = 1f;
				float titleAnchorX = 1f;
				float titleAnchorY = 0.4f;

				if (modInvasion.PreDrawTitle(spriteBatch, ref titlePos, ref titleColor, ref titleScale, ref titleAnchorX, ref titleAnchorY))
					Utils.DrawBorderString(spriteBatch, title, titlePos, titleColor, scale * 0.9f * titleScale, titleAnchorX, titleAnchorY);

				modInvasion.PostDrawTitle(spriteBatch, titlePos, titleColor, titleScale, titleAnchorX, titleAnchorY);
				i++;
			}
		}
	}
}
