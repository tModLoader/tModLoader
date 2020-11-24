using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameInput;
using Terraria.Graphics;
using Terraria.UI;

namespace Terraria.ModLoader
{
	internal static class ModHooks
	{
		//in Terraria.Main.UpdateMusic before updating music boxes call ModHooks.UpdateMusic(ref this.newMusic);
		internal static void UpdateMusic(ref int music, ref MusicPriority priority) {
			foreach (Mod mod in ModLoader.Mods) {
				int modMusic = -1;
				MusicPriority modPriority = MusicPriority.BiomeLow;

				mod.UpdateMusic(ref modMusic, ref modPriority);

				if (modMusic >= 0 && modPriority >= priority) {
					music = modMusic;
					priority = modPriority;
				}
			}
		}

		// Pretty much deprecated. 
		internal static void HotKeyPressed() {
			foreach (var modHotkey in HotKeyLoader.HotKeys) {
				if (PlayerInput.Triggers.Current.KeyStatus[modHotkey.uniqueName]) {
					modHotkey.mod.HotKeyPressed(modHotkey.name);
				}
			}
		}

		internal static void ModifyTransformMatrix(ref SpriteViewMatrix Transform) {
			foreach (Mod mod in ModLoader.Mods) {
				mod.ModifyTransformMatrix(ref Transform);
			}
		}

		internal static void ModifySunLight(ref Color tileColor, ref Color backgroundColor) {
			if (Main.gameMenu)
				return;

			foreach (Mod mod in ModLoader.Mods) {
				mod.ModifySunLightColor(ref tileColor, ref backgroundColor);
			}
		}

		internal static void ModifyLightingBrightness(ref float negLight, ref float negLight2) {
			float scale = 1f;

			foreach (Mod mod in ModLoader.Mods) {
				mod.ModifyLightingBrightness(ref scale);
			}

			if (Lighting.NotRetro) {
				negLight *= scale;
				negLight2 *= scale;
			}
			else {
				negLight -= (scale - 1f) / 2.307692307692308f;
				negLight2 -= (scale - 1f) / 0.75f;
			}

			negLight = Math.Max(negLight, 0.001f);
			negLight2 = Math.Max(negLight2, 0.001f);
		}

		internal static void PostDrawFullscreenMap(ref string mouseText) {
			foreach (Mod mod in ModLoader.Mods) {
				mod.PostDrawFullscreenMap(ref mouseText);
			}
		}

		internal static void UpdateUI(GameTime gameTime) {
			if (Main.gameMenu)
				return;

			foreach (Mod mod in ModLoader.Mods) {
				mod.UpdateUI(gameTime);
			}
		}

		public static void PreUpdateEntities() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.PreUpdateEntities();
			}
		}

		public static void MidUpdatePlayerNPC() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.MidUpdatePlayerNPC();
			}
		}

		public static void MidUpdateNPCGore() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.MidUpdateNPCGore();
			}
		}

		public static void MidUpdateGoreProjectile() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.MidUpdateGoreProjectile();
			}
		}

		public static void MidUpdateProjectileItem() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.MidUpdateProjectileItem();
			}
		}

		public static void MidUpdateItemDust() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.MidUpdateItemDust();
			}
		}

		public static void MidUpdateDustTime() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.MidUpdateDustTime();
			}
		}

		public static void MidUpdateTimeWorld() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.MidUpdateTimeWorld();
			}
		}

		public static void MidUpdateInvasionNet() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.MidUpdateInvasionNet();
			}
		}

		public static void PostUpdateEverything() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.PostUpdateEverything();
			}
		}

		internal static void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			foreach (GameInterfaceLayer layer in layers) {
				layer.Active = true;
			}

			foreach (Mod mod in ModLoader.Mods) {
				mod.ModifyInterfaceLayers(layers);
			}
		}

		internal static void PostDrawInterface(SpriteBatch spriteBatch) {
			foreach (Mod mod in ModLoader.Mods) {
				mod.PostDrawInterface(spriteBatch);
			}
		}

		internal static void PostUpdateInput() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.PostUpdateInput();
			}
		}

		internal static void PreSaveAndQuit() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.PreSaveAndQuit();
			}
		}

		internal static List<MenuButton> ModifyMenuButtons(ref int numButtons, ref bool[] readonlyText, ref bool[] unhoverableText, ref bool[] loweredAlpha, ref int[] yOffsetPos, ref int[] xOffsetPos, ref byte[] color, ref float[] scale, ref bool[] noCenterOffset, ref string[] text, Color defaultColor, out Color[] buttonColor, out Action[] onClick, out Action[] onHover) {
			List<MenuButton> buttons = new List<MenuButton>();
			for (int i = 0; i < numButtons; i++) {
				if (text[i] == null)
					continue;

				MenuButton button = new MenuButton($"{Main.menuMode}_{i}Button", text[i]);
				button.colorByte = color[i];
				switch (button.colorByte) {
					case 0:
						button.color = defaultColor;
						break;
					case 1:
						button.color = Main.mcColor;
						break;
					case 2:
						button.color = Main.hcColor;
						break;
					case 3:
						button.color = Main.highVersionColor;
						break;
					case 4:
					case 5:
					case 6:
						button.color = Main.errorColor;
						break;
					default:
						button.color = defaultColor;
						break;
				}
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
			foreach (Mod mod in ModLoader.Mods)
				mod.ModifyMenuButtons(buttons);

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
			onClick = new Action[numButtons];
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
				onClick[i] = buttons[i].OnClick;
				onHover[i] = buttons[i].OnHover;
			}
			
			return buttons;
		}
	}
}
