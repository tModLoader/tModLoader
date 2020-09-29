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
	}
}
