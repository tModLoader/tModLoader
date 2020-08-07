using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Default;
using Terraria.UI.Chat;

namespace Terraria.ModLoader
{
	internal static class MenuLoader
	{
		internal static readonly List<ModMenu> moddedMenus = new List<ModMenu>();

		internal static List<ModMenu> AvailableMenus => moddedMenus.Where(m => m.IsAvailable).ToList();

		internal static int currentModMenuIndex;

		internal static string lastUsedModMenuName = nameof(MenutML);

		internal static void GotoSavedModMenu() {
			if (lastUsedModMenuName == nameof(MenuOldVanilla)) {
				Main.instance.playOldTile = true; // If the previous menu was the 1.3.5.3 one, automatically reactivate it.
			}
			int index = AvailableMenus.FindIndex(m => m.GetType().Name == lastUsedModMenuName);
			if (index != -1) {
				currentModMenuIndex = index;
				AvailableMenus[currentModMenuIndex].SelectionInit();
			}
		}

		internal static void UpdateAndDrawModMenu(ModMenu currentMenu, SpriteBatch spriteBatch, GameTime gameTime, Color color, float logoRotation, float logoScale, int count) {
			if (lastUsedModMenuName != currentMenu.GetType().Name) {
				currentMenu.SelectionInit();
				lastUsedModMenuName = currentMenu.GetType().Name;
			}

			currentMenu.userInterface?.Update(gameTime);
			currentMenu.userInterface?.Draw(spriteBatch, gameTime);
			currentMenu.Update(Main.menuMode == 0);

			Texture2D logo = currentMenu.Logo.Value;

			float scale = 1;
			Vector2 logoDrawPos = new Vector2(Main.screenWidth / 2, 100f);

			if (currentMenu.PreDrawLogo(spriteBatch, ref logoDrawPos, ref logoRotation, ref scale, ref color)) {
				spriteBatch.Draw(logo, logoDrawPos, new Rectangle(0, 0, logo.Width, logo.Height), color, logoRotation, new Vector2(logo.Width * 0.5f, logo.Height * 0.5f), logoScale * scale, SpriteEffects.None, 0f);
			}
			currentMenu.PostDrawLogo(spriteBatch, logoDrawPos, logoRotation, scale, color);

			string modName = currentMenu.NameOnMenu ?? currentMenu.Mod?.DisplayName ?? "tModLoader";
			string text = $"{Language.GetTextValue("tModLoader.ModMenuSwap")}: {modName}";

			Vector2 size = FontAssets.MouseText.Value.MeasureString(text);

			Rectangle switchTextRect = Main.menuMode == 0 ? new Rectangle((int)(Main.screenWidth / 2 - (size.X / 2)), (int)(Main.screenHeight - 2 - size.Y), (int)size.X, (int)size.Y) : Rectangle.Empty;
			Rectangle logoRect = new Rectangle((int)logoDrawPos.X - (logo.Width / 2), (int)logoDrawPos.Y - (logo.Height / 2), logo.Width, logo.Height);

			bool mouseover = switchTextRect.Contains(Main.mouseX, Main.mouseY) || logoRect.Contains(Main.mouseX, Main.mouseY);

			if (mouseover && count > 1) {
				if (Main.mouseLeftRelease && Main.mouseLeft) {
					SoundEngine.PlaySound(SoundID.MenuTick);
					if (++currentModMenuIndex > count - 1) {
						currentModMenuIndex = 0;
					}
				}
				else if (Main.mouseRightRelease && Main.mouseRight) {
					SoundEngine.PlaySound(SoundID.MenuTick);
					if (--currentModMenuIndex < 0) {
						currentModMenuIndex = count - 1;
					}
				}
			}

			if (count > 1 && Main.menuMode == 0) {
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, new Vector2(switchTextRect.X, switchTextRect.Y),
					switchTextRect.Contains(Main.mouseX, Main.mouseY) ? Main.OurFavoriteColor : new Color(120, 120, 120, 76), 0, Vector2.Zero, Vector2.One);
			}
		}
	}
}
