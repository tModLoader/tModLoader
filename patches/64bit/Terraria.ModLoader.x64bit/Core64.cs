using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour.HookGen;
using ReLogic.Graphics;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.Engine;
using Terraria.ModLoader.UI;
using Terraria.Utilities;

namespace Terraria.ModLoader.x64bit
{
	//TODO: Split up this class into multiple file
	class Core64
	{
		/// <summary>
		/// Boolean that will allow you to switch between vanilla and 64bit mode, so you can play with vanilla friends!
		/// </summary>
		internal static bool vanillaMode;
		internal static bool liteMode = false;

		internal static readonly string vanillaVersion = "Terraria" + 194;

		internal static bool betaMode => false && !liteMode;

		internal const int vanillaChestLimit = 1000;
		internal static readonly int maxChest = 2000;

		internal static readonly string current64BitInternalVersion = "0.11.8";
		internal static string last64bitVersionLaunched = "";

		internal static void SetupVariable() {
			Main.chest = new Chest[maxChest];
			Main.tile = new Tile[16801, 4801];

			LoadChests_Hook += LoadChestHook;
			SaveChests_Hook += SaveChestsHook;
		}

		internal static void LoadVanillaPath() {

			Main.WorldPath = Path.Combine(Main.SavePath, "Worlds");
			Main.PlayerPath = Path.Combine(Main.SavePath, "Players");
		}

		internal static void LoadModdedPath() {
			Main.WorldPath = Path.Combine(Main.SavePath, "ModLoader", "Worlds");
			Main.PlayerPath = Path.Combine(Main.SavePath, "ModLoader", "Players");
		}

		internal static void DrawPatreon(SpriteBatch sb, int num109, int num110, int num111, bool hasFocus, Color color12) {
			string patreonShortURL = !vanillaMode ? "Switch to vanilla mode" : "Switch to TML";
			string tmlModeString = liteMode ? "Disable lite mode" : "Enable lite mode";
			bool showPatreon = Main.menuMode == 0;
			string architecture = $"(Running in {(Environment.Is64BitProcess ? 64.ToString() : 32.ToString())} bit mode)";
			string GoG = InstallVerifier.IsGoG ? "GoG" : "Steam";
			string drawVersion;
			if (vanillaMode) {
				drawVersion = Main.versionNumber;
			}
			else {
				drawVersion = Main.versionNumber + Environment.NewLine + ModLoader.versionedName + $" - {architecture} {GoG} {PlatformUtilities.RunningPlatform()}";
			}

			Vector2 origin3 = Main.fontMouseText.MeasureString(drawVersion);
			origin3.X *= 0.5f;
			origin3.Y *= 0.5f;
			Main.spriteBatch.DrawString(Main.fontMouseText, drawVersion, new Vector2(origin3.X + num110 + 10f, Main.screenHeight - origin3.Y - num111 + 2f), color12, 0f, origin3, 1f, SpriteEffects.None, 0f);
			if (num109 == 4) {
				color12 = new Microsoft.Xna.Framework.Color(127, 191, 191, 76);
			}

			if (Main.menuMode == 10002 && vanillaMode) {
				Main.menuMode = 0;
			}

			if (showPatreon) {
				Vector2 urlSize = Main.fontMouseText.MeasureString(patreonShortURL);
				Main.spriteBatch.DrawString(Main.fontMouseText, patreonShortURL, new Vector2(num110 + 10f, Main.screenHeight - origin3.Y - 50f - num111 + 2f), color12, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
				if (num109 == 4 && Main.mouseLeftRelease && Main.mouseLeft && new Microsoft.Xna.Framework.Rectangle((int)(num110 + 10f), (int)(Main.screenHeight - origin3.Y - 50f - num111 + 2f), (int)urlSize.X, (int)origin3.Y).Contains(new Microsoft.Xna.Framework.Point(Main.mouseX, Main.mouseY)) && hasFocus) {
					Main.PlaySound(SoundID.MenuOpen);
					//vanillaMode = !vanillaMode;
					//Main.SaveSettings();
					//Interface.infoMessage.Show("You'll need to restart the game so that the necessary change can apply.", 0, null, "Restart", () => Environment.Exit(0));
					Interface.infoMessage.Show("This feature is note completely done and will be released at a further time.\n\n\n-Dradonhunter11", 0);
				}

				if (!vanillaMode) {
					urlSize = Main.fontMouseText.MeasureString(tmlModeString);
					Main.spriteBatch.DrawString(Main.fontMouseText, tmlModeString, new Vector2(num110 + 10f, Main.screenHeight - origin3.Y - 72f - num111 + 2f), color12, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
					if (num109 == 4 && Main.mouseLeftRelease && Main.mouseLeft && new Microsoft.Xna.Framework.Rectangle((int)(num110 + 10f), (int)(Main.screenHeight - origin3.Y - 72f - num111 + 2f), (int)urlSize.X, (int)origin3.Y).Contains(new Microsoft.Xna.Framework.Point(Main.mouseX, Main.mouseY)) && hasFocus) {
						string message = liteMode ? "Your game will be switched to the full tML 64bit version. You will need to restart your game completely to have the effect applied.\n" +
													  "In this mode you can generate an XL world, and allows for a chest limit of 2000. However, you can't play with 32bit anymore unless you switch back to tML64bit lite mode." :
							"Your game will be switched to the tML 64bit lite mode. You will need to restart your game completely to have the effect applied.\n" +
							"In this mode, you can play with other 32bit tModLoader clients and other tModLoader 64 bit lite users, but cannot play with tML64bit users in full mode. You can always switch back to the full tML64bit.";

						Main.PlaySound(SoundID.MenuOpen);
						Interface.infoMessage.SpecialShow(message, 0, null, "Back", "Apply", () => {
							liteMode = !liteMode;
							Main.SaveSettings();
							Environment.Exit(0);
						});
					}
				}
			}
		}

		private static int SaveChestsHook(orig_SaveChests orig, BinaryWriter writer) {

			int returnAnswer = orig(writer);
			string extraChestFile = Path.ChangeExtension(Main.ActiveWorldFileData.Path, ".twldexchest");
			using (MemoryStream stream = new MemoryStream()) {
				using (BinaryWriter extraChestWriter = new BinaryWriter(stream)) {
					SaveExtraChests(extraChestWriter);
					FileUtilities.WriteAllBytes(extraChestFile, stream.ToArray(), Main.ActiveWorldFileData.IsCloudSave);
				}
			}

			return returnAnswer;
		}

		private static void SaveExtraChests(BinaryWriter writer) {
			short numberOfChest = 0;
			for (int i = 1000; i < maxChest; i++) {
				Chest chest = Main.chest[i];
				if (chest != null) {
					bool flag = false;
					for (int j = chest.x; j <= chest.x + 1; j++) {
						for (int k = chest.y; k <= chest.y + 1; k++) {
							if (j < 0 || k < 0 || j >= Main.maxTilesX || k >= Main.maxTilesY) {
								flag = true;
								break;
							}

							Tile tile = Main.tile[j, k];
							if (!tile.active() || !Main.tileContainer[tile.type] && tile.type != ModContent.TileType<MysteryTile>()) {
								flag = true;
								break;
							}
						}
					}

					if (flag) {
						Main.chest[i] = null;
					}
					else {
						numberOfChest += 1;
					}
				}
			}

			writer.Write(numberOfChest);
			writer.Write((short)40);
			for (int i = 1000; i < maxChest; i++) {
				Chest chest = Main.chest[i];
				if (chest != null) {
					writer.Write(chest.x);
					writer.Write(chest.y);
					writer.Write(chest.name);
					for (int l = 0; l < 40; l++) {
						Item item = chest.item[l];
						if (item == null || item.modItem != null) {
							writer.Write((short)0);
						}
						else {
							if (item.stack > item.maxStack) {
								item.stack = item.maxStack;
							}

							if (item.stack < 0) {
								item.stack = 1;
							}

							writer.Write((short)item.stack);
							if (item.stack > 0) {
								writer.Write(item.netID);
								writer.Write(item.prefix);
							}
						}
					}
				}
			}

			Console.Write($"Number of chest in this world {1000 + numberOfChest}");
		}

		private static void LoadChestHook(orig_LoadChests orig, BinaryReader reader) {
			orig(reader);
			string extraChestFile = Path.ChangeExtension(Main.ActiveWorldFileData.Path, ".twldexchest");
			if (File.Exists(extraChestFile)) {
				byte[] fileByte = FileUtilities.ReadAllBytes(Path.ChangeExtension(Main.ActiveWorldFileData.Path, ".twldexchest"), Main.ActiveWorldFileData.IsCloudSave);
				using (MemoryStream stream = new MemoryStream(fileByte)) {
					using (BinaryReader extraChestReader = new BinaryReader(stream)) {
						LoadExtraChests(extraChestReader);
					}
				}
			}
		}

		private static void LoadExtraChests(BinaryReader reader) {
			int numberOfChest = reader.ReadInt16();
			int num2 = reader.ReadInt16();
			int num3;
			int num4;
			if (num2 < 40) {
				num3 = num2;
				num4 = 0;
			}
			else {
				num3 = 40;
				num4 = num2 - 40;
			}

			int i;
			for (i = 1000; i < 1000 + numberOfChest; i++) {
				Chest chest = new Chest(false);
				chest.x = reader.ReadInt32();
				chest.y = reader.ReadInt32();
				chest.name = reader.ReadString();
				for (int j = 0; j < num3; j++) {
					short num5 = reader.ReadInt16();
					Item item = new Item();
					if (num5 > 0) {
						item.netDefaults(reader.ReadInt32());
						item.stack = num5;
						item.Prefix(reader.ReadByte());
					}
					else if (num5 < 0) {
						item.netDefaults(reader.ReadInt32());
						item.Prefix(reader.ReadByte());
						item.stack = 1;
					}

					chest.item[j] = item;
				}

				for (int j = 0; j < num4; j++) {
					short num5 = reader.ReadInt16();
					if (num5 > 0) {
						reader.ReadInt32();
						reader.ReadByte();
					}
				}

				Main.chest[i] = chest;
			}

			List<Point16> list = new List<Point16>();
			for (int k = 0; k < i; k++) {
				if (Main.chest[k] != null) {
					Point16 item2 = new Point16(Main.chest[k].x, Main.chest[k].y);
					if (list.Contains(item2)) {
						Main.chest[k] = null;
					}
					else {
						list.Add(item2);
					}
				}
			}

			while (i < 1000) {
				Main.chest[i] = null;
				i++;
			}
		}

		private delegate int orig_SaveChests(BinaryWriter writer);

		private delegate int hook_SaveChests(orig_SaveChests orig, BinaryWriter writer);

		private static event hook_SaveChests SaveChests_Hook {
			add {
				Type type = typeof(WorldFile);
				if (type != null) {
					HookEndpointManager.Add<hook_SaveChests>(type.GetMethod("SaveChests", BindingFlags.Static | BindingFlags.NonPublic), value);
				}
			}
			remove {
				Type type = typeof(WorldFile);
				if (type != null) {
					HookEndpointManager.Remove<hook_SaveChests>(type.GetMethod("SaveChests", BindingFlags.Static | BindingFlags.NonPublic), value);
				}
			}
		}

		private delegate void orig_LoadChests(BinaryReader reader);

		private delegate void hook_LoadChests(orig_LoadChests orig, BinaryReader reader);

		private static event hook_LoadChests LoadChests_Hook {
			add {
				Type type = typeof(WorldFile);
				if (type != null) {
					HookEndpointManager.Add<hook_LoadChests>(type.GetMethod("LoadChests", BindingFlags.Static | BindingFlags.NonPublic), value);
				}
			}
			remove {
				Type type = typeof(WorldFile);
				if (type != null) {
					HookEndpointManager.Remove<hook_LoadChests>(type.GetMethod("LoadChests", BindingFlags.Static | BindingFlags.NonPublic), value);
				}
			}
		}

		public static void DoClientSizeChanged(Object sender, EventArgs e)
		{
			var window = sender as GameWindow;
			// We remove the event in case SetResolution changes the window size again.
			window.ClientSizeChanged -= DoClientSizeChanged;
			if (Main.graphics.IsFullScreen) {
				Main.SetResolution(Main.PendingResolutionWidth, Main.PendingResolutionHeight);
			}
			else {
				Main.SetResolution(window.ClientBounds.Width, window.ClientBounds.Height);
			}
			window.ClientSizeChanged += DoClientSizeChanged;
		}
	}
}
