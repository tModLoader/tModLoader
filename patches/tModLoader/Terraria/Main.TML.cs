using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ModLoader.Engine;
using Terraria.ModLoader.UI;

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

		public static void InfoDisplayPageHandler(int startX, ref string mouseText, out int startingDisplay, out int endingDisplay) {
			startingDisplay = 0;
			endingDisplay = InfoDisplayLoader.InfoDisplayCount;
			if (playerInventory && InfoDisplayLoader.ActiveDisplays() > 12) {
				startingDisplay = 12 * InfoDisplayLoader.InfoDisplayPage;
				if (InfoDisplayLoader.ActiveDisplays() - startingDisplay <= 12)
					endingDisplay = InfoDisplayLoader.ActiveDisplays();
				else
					endingDisplay = startingDisplay + 12;

				if (startingDisplay >= 8)
					startingDisplay += 1;
				endingDisplay += 1;


				Texture2D buttonTexture = UICommon.InfoDisplayPageArrowTexture.Value;
				bool hovering = false;

				GetInfoAccIconPosition(11, startX, out int X, out int Y);
				Vector2 buttonPosition = new Vector2(X, Y + 20);
				if ((float)mouseX >= buttonPosition.X && (float)mouseY >= buttonPosition.Y && (float)mouseX <= buttonPosition.X + (float)buttonTexture.Width && (float)mouseY <= buttonPosition.Y + (float)buttonTexture.Height && !PlayerInput.IgnoreMouseInterface) {
					hovering = true;
					player[myPlayer].mouseInterface = true;
					if (mouseLeft && mouseLeftRelease) {
						SoundEngine.PlaySound(12);
						mouseLeftRelease = false;
						if (InfoDisplayLoader.ActivePages() != InfoDisplayLoader.InfoDisplayPage + 1)
							InfoDisplayLoader.InfoDisplayPage += 1;
						else
							InfoDisplayLoader.InfoDisplayPage = 0;
					}

					if (!Main.mouseText) {
						mouseText = (InfoDisplayLoader.ActivePages() != InfoDisplayLoader.InfoDisplayPage + 1) ? "Next Page" : "To First Page";
						Main.mouseText = true;
					}
				}
				spriteBatch.Draw(buttonTexture, buttonPosition, new Rectangle(0, 0, buttonTexture.Width, buttonTexture.Height), Color.White, 0f, default, 1f, SpriteEffects.None, 0f);
				if (hovering)
					spriteBatch.Draw(TextureAssets.InfoIcon[13].Value, buttonPosition - Vector2.One * 2f, null, OurFavoriteColor, 0f, default, 1f, SpriteEffects.None, 0f);

				hovering = false;
				GetInfoAccIconPosition(0, startX, out X, out Y);
				buttonPosition = new Vector2(X, Y + 20);
				if ((float)mouseX >= buttonPosition.X && (float)mouseY >= buttonPosition.Y && (float)mouseX <= buttonPosition.X + (float)buttonTexture.Width && (float)mouseY <= buttonPosition.Y + (float)buttonTexture.Height && !PlayerInput.IgnoreMouseInterface) {
					hovering = true;
					player[myPlayer].mouseInterface = true;
					if (mouseLeft && mouseLeftRelease) {
						SoundEngine.PlaySound(12);
						mouseLeftRelease = false;
						if (InfoDisplayLoader.InfoDisplayPage != 0)
							InfoDisplayLoader.InfoDisplayPage -= 1;
						else
							InfoDisplayLoader.InfoDisplayPage = InfoDisplayLoader.ActivePages() - 1;
					}

					if (!Main.mouseText) {
						mouseText = (InfoDisplayLoader.InfoDisplayPage != 0) ? "Previous Page" : "To Last Page";
						Main.mouseText = true;
					}
				}
				spriteBatch.Draw(buttonTexture, buttonPosition, new Rectangle(0, 0, buttonTexture.Width, buttonTexture.Height), Color.White, 0f, default, 1f, SpriteEffects.FlipHorizontally, 0f);
				if (hovering)
					spriteBatch.Draw(TextureAssets.InfoIcon[13].Value, buttonPosition - Vector2.One * 2f, null, OurFavoriteColor, 0f, default, 1f, SpriteEffects.None, 0f);
			}
		}
	}
}
