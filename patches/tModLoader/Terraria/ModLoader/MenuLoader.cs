using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Default;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Terraria.ModLoader;

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

	internal static bool loading = true;

	internal static string LastSelectedModMenu = MenutML.FullName;
	internal static string KnownMenuSaveString = string.Join(",", menus.Select(m => m.FullName));

	private static string[] KnownMenus => KnownMenuSaveString.Split(',');

	private static void AddKnownMenu(string name)
	{
		var newSaveString = string.Join(",", KnownMenus.Concat(new[] { name }).Distinct());
		if (newSaveString != KnownMenuSaveString) {
			KnownMenuSaveString = newSaveString;
			Main.SaveSettings();
		}
	}

	internal static void Add(ModMenu modMenu)
	{
		lock (menus) {
			menus.Add(modMenu);
			ModTypeLookup<ModMenu>.Register(modMenu);
		}
	}

	private static void OffsetModMenu(int offset)
	{
		lock (menus) {
			switchToMenu = currentMenu;
			do {
				switchToMenu = menus[Utils.Repeat(menus.IndexOf(switchToMenu) + offset, menus.Count)];
			} while (!switchToMenu.IsAvailable);
		}
	}

	internal static void GotoSavedModMenu()
	{
		if (LastSelectedModMenu == MenuOldVanilla.FullName) {
			Main.instance.playOldTile = true; // If the previous menu was the 1.3.5.3 one, automatically reactivate it.
		}

		switchToMenu = MenutML;
		if (ModContent.TryFind(LastSelectedModMenu, out ModMenu value) && value.IsAvailable)
			switchToMenu = value;
		if (LastSelectedModMenu == MenuJourneysEnd.FullName)
			switchToMenu = MenuJourneysEnd;

		loading = false;
	}

	public static void ActivateOldVanillaMenu()
	{
		switchToMenu = MenuOldVanilla;
	}

	internal static void UpdateAndDrawModMenu(SpriteBatch spriteBatch, GameTime gameTime, Color color, float logoRotation, float logoScale)
	{
		var activeInterface = UserInterface.ActiveInstance;

		// Rendering an interface makes it override ActiveInstance, so this block restores the previous value to prevent issues from that.
		try {
			UpdateAndDrawModMenuInner(spriteBatch, gameTime, color, logoRotation, logoScale);
		}
		finally {
			// We don't call Use() to avoid triggering recalculations.
			UserInterface.ActiveInstance = activeInterface;
		}
	}

	private static void UpdateAndDrawModMenuInner(SpriteBatch spriteBatch, GameTime gameTime, Color color, float logoRotation, float logoScale)
	{
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
		// Prevent Recalculate() spam due to Use() in UserInterface.Draw().
		UserInterface.ActiveInstance = currentMenu.UserInterface;
		currentMenu.UserInterface.Draw(spriteBatch, gameTime);

		currentMenu.Update(Main.menuMode == 0);

		Texture2D logo = currentMenu.Logo.Value;
		Vector2 logoDrawPos = new Vector2(Main.screenWidth / 2, 100f);
		float scale = logoScale;

		if (currentMenu.PreDrawLogo(spriteBatch, ref logoDrawPos, ref logoRotation, ref scale, ref color)) {
			spriteBatch.Draw(logo, logoDrawPos, new Rectangle(0, 0, logo.Width, logo.Height), color, logoRotation, new Vector2(logo.Width * 0.5f, logo.Height * 0.5f), scale, SpriteEffects.None, 0f);
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

		Vector2 size = ChatManager.GetStringSize(FontAssets.MouseText.Value, ChatManager.ParseMessage(text, color).ToArray(), Vector2.One);

		Rectangle switchTextRect = Main.menuMode == 0 ? new Rectangle((int)(Main.screenWidth / 2 - (size.X / 2)), (int)(Main.screenHeight - 2 - size.Y), (int)size.X, (int)size.Y) : Rectangle.Empty;
		//Rectangle logoRect = new Rectangle((int)logoDrawPos.X - (logo.Width / 2), (int)logoDrawPos.Y - (logo.Height / 2), logo.Width, logo.Height);

		bool mouseover = switchTextRect.Contains(Main.mouseX, Main.mouseY); // || logoRect.Contains(Main.mouseX, Main.mouseY);

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

	internal static void Unload()
	{
		loading = true;
		// Prevent asset disposed exceptions by disallowing modded menus during the unload process.
		if (menus.IndexOf(currentMenu, 0, DefaultMenuCount) == -1) {
			switchToMenu = MenutML;
			while (currentMenu != MenutML) {
				Thread.Yield();
			}
		}

		lock (menus) {
			menus.RemoveRange(DefaultMenuCount, menus.Count - DefaultMenuCount);
		}
	}
}
