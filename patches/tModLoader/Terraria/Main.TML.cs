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
				invasionDisplays.TryGetValue(modInvasion.Type, out ModInvasion.ModInvasionProgressDisplay progressDisplay);

				if (!modInvasion.Active && (progressDisplay == null || progressDisplay.DisplayLeft == 0))
					continue;

				if (!gamePaused && !modInvasion.Active)
					progressDisplay.DisplayLeft--;

				if (progressDisplay.DisplayLeft > 0)
					progressDisplay.Alpha += 0.05f;
				else
					progressDisplay.Alpha -= 0.05f;

				if (progressDisplay.Alpha <= 0f)
					return;

				if (progressDisplay.Alpha >= 1f)
					progressDisplay.Alpha = 1f;

				float scale = 0.5f + progressDisplay.Alpha * 0.5f;
				Texture2D icon = modInvasion.Icon;
				string title = modInvasion.Title;

				int width = (int)(200f * scale);
				int height = (int)(45f * scale);
				Vector2 offset = (height + 50) * i * Vector2.UnitY;

				if (invasionType != 0 && invasionProgressAlpha > 0f)
					offset.Y += height + 50;

				Vector2 uiPosition = new Vector2(screenWidth - 120, screenHeight - 40) - offset;

				Utils.DrawInvBG(R: new Rectangle((int)uiPosition.X - width / 2, (int)uiPosition.Y - height / 2, width, height), sb: spriteBatch, c: modInvasion.ProgressUIColor);

				string progressText = modInvasion.ProgressText;
				Texture2D value2 = TextureAssets.ColorBar.Value;

				float progress = modInvasion.Progress;
				float progressBarWidth = 169f * scale;
				float progressBarHeight = 8f * scale;
				Vector2 progressBarPos = uiPosition + Vector2.UnitY * progressBarHeight + Vector2.UnitX * 1f;

				Utils.DrawBorderString(spriteBatch, progressText, progressBarPos, Color.White * progressDisplay.Alpha, scale, 0.5f, 1f);
				spriteBatch.Draw(value2, uiPosition, null, Color.White * progressDisplay.Alpha, 0f, new Vector2(value2.Width / 2, 0f), scale, SpriteEffects.None, 0f);
				progressBarPos += Vector2.UnitX * (progress - 0.5f) * progressBarWidth;
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, progressBarPos, new Rectangle(0, 0, 1, 1), modInvasion.ProgressBarColor * progressDisplay.Alpha, 0f, new Vector2(1f, 0.5f), new Vector2(progressBarWidth * progress, progressBarHeight), SpriteEffects.None, 0f);
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, progressBarPos, new Rectangle(0, 0, 1, 1), new Color(255, 165, 0, 127) * progressDisplay.Alpha, 0f, new Vector2(1f, 0.5f), new Vector2(2f, progressBarHeight), SpriteEffects.None, 0f);
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, progressBarPos, new Rectangle(0, 0, 1, 1), Color.Black * progressDisplay.Alpha, 0f, new Vector2(0f, 0.5f), new Vector2(progressBarWidth * (1f - progress), progressBarHeight), SpriteEffects.None, 0f);

				Vector2 invasionTitleSize = FontAssets.MouseText.Value.MeasureString(title);
				float num13 = 120f;
				if (invasionTitleSize.X > 200f)
					num13 += invasionTitleSize.X - 200f;

				Rectangle r3 = Utils.CenteredRectangle(new Vector2(screenWidth - num13, screenHeight - 80) - offset, (invasionTitleSize + new Vector2(icon.Width + 12, 6f)) * scale);
				Utils.DrawInvBG(spriteBatch, r3, modInvasion.TitleUIColor);
				spriteBatch.Draw(icon, r3.Left() + Vector2.UnitX * scale * 8f, null, Color.White * progressDisplay.Alpha, 0f, new Vector2(0f, icon.Height / 2), scale * 0.8f, SpriteEffects.None, 0f);
				Utils.DrawBorderString(spriteBatch, title, r3.Right() + Vector2.UnitX * scale * -22f, Color.White * progressDisplay.Alpha, scale * 0.9f, 1f, 0.4f);

				i++;
			}
		}
	}
}
