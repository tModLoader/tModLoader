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
		internal static ModMenu currentModMenu;

		private static readonly List<ModMenu> moddedMenus = new List<ModMenu>();

		internal static List<ModMenu> AvailableMenus => moddedMenus.Where(m => m.IsAvailable).ToList();

		internal static string lastUsedModMenuName = $"ModLoader/{nameof(MenutML)}";

		internal static void Add(ModMenu modMenu) {
			moddedMenus.Add(modMenu);
		}

		private static void GotoNextModMenu() {
			List<ModMenu> menus = AvailableMenus;
			int index = menus.IndexOf(currentModMenu);
			index = Utils.Repeat(index + 1, menus.Count);
			currentModMenu = menus[index];
		}

		private static void GotoPreviousModMenu() {
			List<ModMenu> menus = AvailableMenus;
			int index = menus.IndexOf(currentModMenu);
			index = Utils.Repeat(index - 1, menus.Count);
			currentModMenu = menus[index];
		}

		internal static void GotoSavedModMenu() {
			if (lastUsedModMenuName == nameof(MenuOldVanilla)) {
				Main.instance.playOldTile = true; // If the previous menu was the 1.3.5.3 one, automatically reactivate it.
			}
			List<ModMenu> menus = AvailableMenus;
			int index = menus.FindIndex(m => GetModMenuName(m) == lastUsedModMenuName);
			if (index != -1) {
				currentModMenu = menus[index];
				currentModMenu.SelectionInit();
			}
		}

		private static string GetModMenuName(ModMenu menu) => $"{menu.Mod.Name}/{menu.Name}";

		internal static void UpdateAndDrawModMenu(ModMenu currentMenu, SpriteBatch spriteBatch, GameTime gameTime, Color color, float logoRotation, float logoScale) {
			if (lastUsedModMenuName != GetModMenuName(currentMenu)) {
				currentMenu.SelectionInit();
				lastUsedModMenuName = GetModMenuName(currentMenu);
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

			if (mouseover) {
				if (Main.mouseLeftRelease && Main.mouseLeft) {
					SoundEngine.PlaySound(SoundID.MenuTick);
					GotoNextModMenu();
				}
				else if (Main.mouseRightRelease && Main.mouseRight) {
					SoundEngine.PlaySound(SoundID.MenuTick);
					GotoPreviousModMenu();
				}
			}

			if (Main.menuMode == 0) {
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, new Vector2(switchTextRect.X, switchTextRect.Y),
					switchTextRect.Contains(Main.mouseX, Main.mouseY) ? Main.OurFavoriteColor : new Color(120, 120, 120, 76), 0, Vector2.Zero, Vector2.One);
			}
		}

		internal static void Unload() {
			moddedMenus.Clear();
		}
	}
}
