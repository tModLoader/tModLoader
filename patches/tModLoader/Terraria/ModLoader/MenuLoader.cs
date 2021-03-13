using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
	public static class MenuLoader
	{
		internal static readonly MenutML MenutML = new MenutML();
		internal static readonly MenuJourneysEnd MenuJourneysEnd = new MenuJourneysEnd();
		internal static readonly MenuOldVanilla MenuOldVanilla = new MenuOldVanilla();

		private static readonly List<ModMenu> menus = new List<ModMenu>() {
			MenutML,
			MenuJourneysEnd,
			MenuOldVanilla
		};

		private static readonly int DefaultMenuCount = menus.Count;

		private static ModMenu currentMenu = MenutML;

		public static ModMenu CurrentMenu => currentMenu;

		private static ModMenu switchToMenu = null;

		private static bool loading = true;

		internal static string LastSelectedModMenu = MenutML.FullName;
		internal static string KnownMenuSaveString = string.Join(",", menus.Select(m => m.FullName));

		private static string[] KnownMenus => KnownMenuSaveString.Split(',');

		private static void AddKnownMenu(string name) {
			var newSaveString = string.Join(",", KnownMenus.Concat(new [] { name }).Distinct());
			if (newSaveString != KnownMenuSaveString) {
				KnownMenuSaveString = newSaveString;
				Main.SaveSettings();
			}
		}

		internal static void Add(ModMenu modMenu) {
			lock (menus) {
				menus.Add(modMenu);
			}
		}

		private static void OffsetModMenu(int offset) {
			lock (menus) {
				switchToMenu = currentMenu;
				do {
					switchToMenu = menus[Utils.Repeat(menus.IndexOf(switchToMenu) + offset, menus.Count)];
				} while (!switchToMenu.IsAvailable);
			}
		}

		internal static void GotoSavedModMenu() {
			if (LastSelectedModMenu == MenuOldVanilla.FullName) {
				Main.instance.playOldTile = true; // If the previous menu was the 1.3.5.3 one, automatically reactivate it.
			}

			switchToMenu = menus.SingleOrDefault(m => m.FullName == LastSelectedModMenu && m.IsAvailable) ?? MenutML;
			loading = false;
		}

		public static void ActivateOldVanillaMenu() {
			switchToMenu = MenuOldVanilla;
		}

		internal static void UpdateAndDrawModMenu(SpriteBatch spriteBatch, GameTime gameTime, Color color, float logoRotation, float logoScale) {
			if (switchToMenu != null && switchToMenu != currentMenu) {
				currentMenu.OnDeselected();
				currentMenu = switchToMenu;
				currentMenu.OnSelected();
				if (currentMenu.IsNew) {
					currentMenu.IsNew = false;
					AddKnownMenu(currentMenu.FullName);
				}
			}
			switchToMenu = null;

			if (!loading && currentMenu.FullName != LastSelectedModMenu) {
				LastSelectedModMenu = currentMenu.FullName;
				Main.SaveSettings();
			}

			currentMenu.UserInterface.Update(gameTime);
			currentMenu.UserInterface.Draw(spriteBatch, gameTime);
			currentMenu.Update(Main.menuMode == 0);

			Texture2D logo = currentMenu.Logo.Value;
			Vector2 logoDrawPos = new Vector2(Main.screenWidth / 2, 100f);
			float scale = 1;

			if (currentMenu.PreDrawLogo(spriteBatch, ref logoDrawPos, ref logoRotation, ref scale, ref color)) {
				spriteBatch.Draw(logo, logoDrawPos, new Rectangle(0, 0, logo.Width, logo.Height), color, logoRotation, new Vector2(logo.Width * 0.5f, logo.Height * 0.5f), logoScale * (currentMenu is MenutML ? 0.84f : scale), SpriteEffects.None, 0f);
			}
			currentMenu.PostDrawLogo(spriteBatch, logoDrawPos, logoRotation, scale, color);

			int newMenus;
			lock (menus) {
				var knownMenus = KnownMenus;
				foreach (ModMenu menu in menus) {
					menu.IsNew = menu.IsAvailable && !knownMenus.Contains(menu.FullName);
				}
				newMenus = menus.Count(m => m.IsNew);
			}

			string text = $"{Language.GetTextValue("tModLoader.ModMenuSwap")}: {currentMenu.DisplayName}{(newMenus == 0 ? "" : ModLoader.notifyNewMainMenuThemes ? $" ({newMenus} New)" : "")}";

			Vector2 size = FontAssets.MouseText.Value.MeasureString(text);

			Rectangle switchTextRect = Main.menuMode == 0 ? new Rectangle((int)(Main.screenWidth / 2 - (size.X / 2)), (int)(Main.screenHeight - 2 - size.Y), (int)size.X, (int)size.Y) : Rectangle.Empty;
			Rectangle logoRect = new Rectangle((int)logoDrawPos.X - (logo.Width / 2), (int)logoDrawPos.Y - (logo.Height / 2), logo.Width, logo.Height);

			bool mouseover = switchTextRect.Contains(Main.mouseX, Main.mouseY) || logoRect.Contains(Main.mouseX, Main.mouseY);

			if (mouseover && !Main.alreadyGrabbingSunOrMoon) {
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
		}

		internal static List<MenuButton> ModifyMenuButtons(ref int numButtons, ref bool[] readonlyText, ref bool[] unhoverableText, ref bool[] loweredAlpha, ref int[] yOffsetPos, ref int[] xOffsetPos, ref byte[] color, ref float[] scale, ref bool[] noCenterOffset, ref string[] text, Color defaultColor, out Color[] buttonColor, out Action[] onLeftClick, out Action[] onRightClick, out Action[] onHover) {
			List<MenuButton> buttons = new List<MenuButton>();

			for (int i = 0; i < numButtons; i++) {
				if (text[i] == null)
					continue;

				string buttonText = text[i]; // Can't use ref params in lambdas

				// Find the respective localization key for a button
				// Fallback to <MenuMode>_<NumberInArray>Button if, for some reason, no value is found
				string buttonName = LanguageManager.Instance._localizedTexts.Values.FirstOrDefault(x => x.Value == buttonText)?.Key ?? $"{Main.menuMode}_{i}Button";

				// Create a vanilla menu button instance
				MenuButton button = new MenuButton(buttonName, text[i]);

				// Copy all data over to the newly-created MenuButton instance
				button.color = GetColorFromByte(button.colorByte = color[i], defaultColor); // Find the button's color based on the vanilla color byte
				button.loweredAlpha = loweredAlpha[i];
				button.noCenterOffset = noCenterOffset[i];
				button.readonlyText = readonlyText[i];
				button.scale = scale[i];
				button.text = text[i];
				button.unhoverableText = unhoverableText[i];
				button.xOffsetPos = xOffsetPos[i];
				button.yOffsetPos = yOffsetPos[i];
				buttons.Add(button);
			}

			// Modify buttons based on the currently-selected menu
			currentMenu.ModifyMenuButtons(buttons);

			// Resize and repopulate arrays with the new data
			numButtons = buttons.Count;
			readonlyText = new bool[numButtons];
			unhoverableText = new bool[numButtons];
			loweredAlpha = new bool[numButtons];
			yOffsetPos = new int[numButtons];
			xOffsetPos = new int[numButtons];
			color = new byte[numButtons];
			buttonColor = new Color[numButtons];
			scale = new float[numButtons];
			noCenterOffset = new bool[numButtons];
			text = new string[numButtons];
			onLeftClick = new Action[numButtons];
			onRightClick = new Action[numButtons];
			onHover = new Action[numButtons];
			for (int i = 0; i < numButtons; i++) {
				readonlyText[i] = buttons[i].readonlyText;
				unhoverableText[i] = buttons[i].readonlyText;
				loweredAlpha[i] = buttons[i].loweredAlpha;
				yOffsetPos[i] = buttons[i].yOffsetPos;
				xOffsetPos[i] = buttons[i].xOffsetPos;
				color[i] = buttons[i].colorByte;
				buttonColor[i] = buttons[i].color;
				scale[i] = buttons[i].scale;
				noCenterOffset[i] = buttons[i].noCenterOffset;
				text[i] = buttons[i].text;
				onLeftClick[i] = buttons[i].onLeftClick;
				onRightClick[i] = buttons[i].onRightClick;
				onHover[i] = buttons[i].onHover;
			}

			return buttons;
		}

		/// <summary>
		/// Returns a <see cref="Color"/> identical whatever color vanilla would use. <br />
		/// This replaces the code vanilla uses for getting <see cref="MenuButton"/> colors.
		/// </summary>
		private static Color GetColorFromByte(byte color, Color defaultColor) {
			switch (color) {
				default:
					return defaultColor;

				case 1:
					return Main.mcColor;

				case 2:
					return Main.hcColor;

				case 3:
					return Main.highVersionColor;

				case 4:
				case 5:
				case 6:
					return Main.errorColor;
			}
		}

		internal static void Unload() {
			loading = true;
			if (menus.IndexOf(currentMenu) >= DefaultMenuCount) {
				switchToMenu = MenutML;
			}

			lock (menus) {
				menus.RemoveRange(DefaultMenuCount, menus.Count - DefaultMenuCount);
			}
		}
	}
}
