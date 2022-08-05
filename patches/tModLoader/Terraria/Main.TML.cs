﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Engine;
using Terraria.ModLoader.UI;
using Terraria.Social;

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
		public static double desiredWorldEventsUpdateRate = 1; // dictates the speed at which world events (falling stars, fairy spawns, sandstorms, etc.) can change/happen
		public static double timePass; // used to account for more precise time rates when deciding when to update weather

		internal static TMLContentManager AlternateContentManager;
		public static List<TitleLinkButton> tModLoaderTitleLinks = new List<TitleLinkButton>();

		public static Color DiscoColor => new Color(DiscoR, DiscoG, DiscoB);
		public static Color MouseTextColorReal => new Color(mouseTextColor / 255f, mouseTextColor / 255f, mouseTextColor / 255f, mouseTextColor / 255f);
		public static bool PlayerLoaded => CurrentFrameFlags.ActivePlayersCount > 0;

		// Used to prevent situations where when going from borderless Fullscreen display to windowed display the width and height don't get resized to be able to access key window functions
		// Does not effect resizing the window manually. May not be perfect, but will at least be sufficient to provide room to manually address this on the end user side
		// Magic constant comes from default windows border settings: ~ 1377 / 1440 and 1033 / 1080.
		private static int BorderedHeight(int height, bool state) => (int)(height * (state ? 1 : 0.95625));

		private static Player _currentPlayerOverride;

		/// <summary>
		/// A replacement for `Main.LocalPlayer` which respects whichever player is currently running hooks on the main thread.
		/// This works in the player select screen, and in multiplayer (when other players are updating)
		/// </summary>
		public static Player CurrentPlayer => _currentPlayerOverride ?? LocalPlayer;

		/// <summary>
		/// Checks if a tile at the given coordinates counts towards tile coloring from the Spelunker buff, and is detected by various pets.
		/// </summary>
		public static bool IsTileSpelunkable(int tileX, int tileY) {
			Tile tile = Main.tile[tileX, tileY];
			return IsTileSpelunkable(tileX, tileY, tile.type, tile.frameX, tile.frameY);
		}

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
				GetInfoAccIconPosition(0, startX, out X, out int _);
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

		//Mirrors code used in UpdateTime
		/// <summary>
		/// Syncs rain state if <see cref="StartRain"/> or <see cref="StopRain"/> were called in the same tick and caused a change to <seealso cref="maxRaining"/>.
		/// <br>Can be called on any side, but only the server will actually sync it.</br>
		/// </summary>
		public static void SyncRain() {
			if (maxRaining != oldMaxRaining) {
				if (netMode == 2)
					NetMessage.SendData(7);

				oldMaxRaining = maxRaining;
			}
		}

		public ref struct CurrentPlayerOverride
		{
			private Player _prevPlayer;

			public CurrentPlayerOverride(Player player) {
				_prevPlayer = _currentPlayerOverride;
				_currentPlayerOverride = player;
			}

			public void Dispose() {
				_currentPlayerOverride = _prevPlayer;
			}
		}

		internal void InitTMLContentManager() {
			if (dedServ) {
				return;
			}

			string vanillaContentFolder;
			if (SocialAPI.Mode == SocialMode.Steam) {
				vanillaContentFolder = Path.Combine(Steam.GetSteamTerrariaInstallDir(), "Content");
			}
			else {
				vanillaContentFolder = "../Terraria/Content"; // Side-by-Side Manual Install

				if (!Directory.Exists(vanillaContentFolder)) {
					vanillaContentFolder = "../Content"; // Nested Manual Install
				}
			}

			if (!Directory.Exists(vanillaContentFolder)) {
				ErrorReporting.FatalExit(Language.GetTextValue("tModLoader.ContentFolderNotFound"));
			}

			if (Directory.Exists(Path.Combine("Content", "Images")))
				AlternateContentManager = new TMLContentManager(Content.ServiceProvider, "Content", null);

			base.Content = new TMLContentManager(Content.ServiceProvider, vanillaContentFolder, AlternateContentManager);
		}
		
		private static void DrawtModLoaderSocialMediaButtons(Microsoft.Xna.Framework.Color menuColor, float upBump) {
			List<TitleLinkButton> titleLinks = tModLoaderTitleLinks;
			Vector2 anchorPosition = new Vector2(18f, (float)(screenHeight - 26 - 22) - upBump);
			for (int i = 0; i < titleLinks.Count; i++) {
				titleLinks[i].Draw(spriteBatch, anchorPosition);
				anchorPosition.X += 30f;
			}
		}

		/// <summary>
		/// Wait for an action to be performed on the main thread.
		/// </summary>
		/// <param name="action"></param>
		public static Task RunOnMainThread(Action action) {
			var tcs = new TaskCompletionSource();

			QueueMainThreadAction(() => {
				action();
				tcs.SetResult();
			});

			return tcs.Task;
		}

		/// <summary>
		/// Wait for an action to be performed on the main thread.
		/// </summary>
		/// <param name="func"></param>
		public static Task<T> RunOnMainThread<T>(Func<T> func) {
			var tcs = new TaskCompletionSource<T>();
			QueueMainThreadAction(() => tcs.SetResult(func()));
			return tcs.Task;
		}
	}
}
