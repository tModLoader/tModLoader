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

		private static List<ModMenu> lastAvailableMenus = new List<ModMenu>();

		internal static List<ModMenu> AvailableMenus => moddedMenus.Where(m => m.IsAvailable).ToList();

		internal static string lastUsedModMenuName = $"ModLoader/{nameof(MenutML)}";

		internal static void Add(ModMenu modMenu) {
			moddedMenus.Add(modMenu);
		}

		private static void OffsetModMenu(int offset) {
			var menus = AvailableMenus;

			currentModMenu = menus[Utils.Repeat(menus.IndexOf(currentModMenu) + offset, menus.Count)];
		}

		internal static void GotoSavedModMenu() {
			if (lastUsedModMenuName == nameof(MenuOldVanilla)) {
				Main.instance.playOldTile = true; // If the previous menu was the 1.3.5.3 one, automatically reactivate it.
			}
			List<ModMenu> menus = AvailableMenus;
			int index = menus.FindIndex(m => GetModMenuName(m) == lastUsedModMenuName);
			if (index == -1) {
				index = 0;
			}
			currentModMenu = menus[index];
			currentModMenu.SelectionInit();
			lastAvailableMenus = AvailableMenus;
		}

		private static string GetModMenuName(ModMenu menu) => $"{menu.Mod.Name}/{menu.Name}";

		internal static void UpdateAndDrawModMenu(ModMenu menu, SpriteBatch spriteBatch, GameTime gameTime, Color color, float logoRotation, float logoScale) {
			if (lastUsedModMenuName != GetModMenuName(menu)) {
				menu.SelectionInit();
				lastUsedModMenuName = GetModMenuName(menu);
			}

			menu.userInterface?.Update(gameTime);
			menu.userInterface?.Draw(spriteBatch, gameTime);
			menu.Update(Main.menuMode == 0);

			Texture2D logo = menu.Logo.Value;

			Vector2 logoDrawPos = new Vector2(Main.screenWidth / 2, 100f);
			float scale = 1;

			if (menu is MenuJourneysEnd) {
				Color color2 = new Color((byte)(color.R * (Main.LogoA / 255f)), (byte)(color.G * (Main.LogoA / 255f)), (byte)(color.B * (Main.LogoA / 255f)), (byte)(color.A * (Main.LogoA / 255f)));
				Color color3 = new Color((byte)(color.R * (Main.LogoB / 255f)), (byte)(color.G * (Main.LogoB / 255f)), (byte)(color.B * (Main.LogoB / 255f)), (byte)(color.A * (Main.LogoB / 255f)));
				spriteBatch.Draw(TextureAssets.Logo.Value, logoDrawPos, new Rectangle(0, 0, TextureAssets.Logo.Width(), TextureAssets.Logo.Height()), color2, logoRotation, new Vector2(TextureAssets.Logo.Width() / 2, TextureAssets.Logo.Height() / 2), logoScale, SpriteEffects.None, 0f);
				spriteBatch.Draw(TextureAssets.Logo2.Value, logoDrawPos, new Rectangle(0, 0, TextureAssets.Logo.Width(), TextureAssets.Logo.Height()), color3, logoRotation, new Vector2(TextureAssets.Logo.Width() / 2, TextureAssets.Logo.Height() / 2), logoScale, SpriteEffects.None, 0f);
			}
			else if (menu is MenuOldVanilla) {
				Color color2 = new Color((byte)(color.R * (Main.LogoA / 255f)), (byte)(color.G * (Main.LogoA / 255f)), (byte)(color.B * (Main.LogoA / 255f)), (byte)(color.A * (Main.LogoA / 255f)));
				Color color3 = new Color((byte)(color.R * (Main.LogoB / 255f)), (byte)(color.G * (Main.LogoB / 255f)), (byte)(color.B * (Main.LogoB / 255f)), (byte)(color.A * (Main.LogoB / 255f)));
				spriteBatch.Draw(TextureAssets.Logo3.Value, logoDrawPos, new Rectangle(0, 0, TextureAssets.Logo.Width(), TextureAssets.Logo.Height()), color2, logoRotation, new Vector2(TextureAssets.Logo.Width() / 2, TextureAssets.Logo.Height() / 2), logoScale, SpriteEffects.None, 0f);
				spriteBatch.Draw(TextureAssets.Logo4.Value, logoDrawPos, new Rectangle(0, 0, TextureAssets.Logo.Width(), TextureAssets.Logo.Height()), color3, logoRotation, new Vector2(TextureAssets.Logo.Width() / 2, TextureAssets.Logo.Height() / 2), logoScale, SpriteEffects.None, 0f);
			}
			else {
				if (menu.PreDrawLogo(spriteBatch, ref logoDrawPos, ref logoRotation, ref scale, ref color)) {
					spriteBatch.Draw(logo, logoDrawPos, new Rectangle(0, 0, logo.Width, logo.Height), color, logoRotation, new Vector2(logo.Width * 0.5f, logo.Height * 0.5f), logoScale * (menu is MenutML ? 0.84f : scale), SpriteEffects.None, 0f);
				}
				menu.PostDrawLogo(spriteBatch, logoDrawPos, logoRotation, scale, color);
			}

			if (currentModMenu.isNew) {
				currentModMenu.isNew = false;
			}

			int newMenus = AvailableMenus.Count(m => m.isNew);

			string text = $"{Language.GetTextValue("tModLoader.ModMenuSwap")}: {menu.DisplayName}{(newMenus == 0 ? "" : ModLoader.notifyNewMainMenuThemes ? $" ({newMenus} New)" : "")}";

			Vector2 size = FontAssets.MouseText.Value.MeasureString(text);

			Rectangle switchTextRect = Main.menuMode == 0 ? new Rectangle((int)(Main.screenWidth / 2 - (size.X / 2)), (int)(Main.screenHeight - 2 - size.Y), (int)size.X, (int)size.Y) : Rectangle.Empty;
			Rectangle logoRect = new Rectangle((int)logoDrawPos.X - (logo.Width / 2), (int)logoDrawPos.Y - (logo.Height / 2), logo.Width, logo.Height);

			bool mouseover = switchTextRect.Contains(Main.mouseX, Main.mouseY) || logoRect.Contains(Main.mouseX, Main.mouseY);

			if (mouseover) {
				if (Main.mouseLeftRelease && Main.mouseLeft) {
					SoundEngine.PlaySound(SoundID.MenuTick);
					OffsetModMenu(1);
				}
				else if (Main.mouseRightRelease && Main.mouseRight) {
					SoundEngine.PlaySound(SoundID.MenuTick);
					OffsetModMenu(-1);
				}
			}

			if (Main.menuMode == 0) {
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, new Vector2(switchTextRect.X, switchTextRect.Y),
					switchTextRect.Contains(Main.mouseX, Main.mouseY) ? Main.OurFavoriteColor : new Color(120, 120, 120, 76), 0, Vector2.Zero, Vector2.One);
			}

			foreach (ModMenu modMenu in AvailableMenus) {
				if (!lastAvailableMenus.Contains(modMenu)) {
					modMenu.isNew = true;
				}
			}

			lastAvailableMenus = AvailableMenus;
		}

		internal static void Unload() {
			moddedMenus.Clear();
		}
	}
}
